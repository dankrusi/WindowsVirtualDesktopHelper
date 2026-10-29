//! Event-driven shell recovery. Timers exist only while a resource is unavailable.
use super::{
    desktop::{DesktopManager, Snapshot},
    events::Watcher,
};
use std::time::{Duration, Instant};
use windows_sys::Win32::{
    Foundation::HWND,
    UI::WindowsAndMessaging::{KillTimer, SetTimer},
};

#[derive(Default)]
struct RetrySchedule {
    delay_ms: u32,
    due: Option<Instant>,
}
impl RetrySchedule {
    fn failed(&mut self, now: Instant) -> Option<u32> {
        // Repeated change notifications must not postpone an already armed retry.
        if self.due.is_some() {
            return None;
        }
        self.delay_ms = if self.delay_ms == 0 {
            500
        } else {
            (self.delay_ms * 2).min(30_000)
        };
        self.due = Some(now + Duration::from_millis(self.delay_ms.into()));
        Some(self.delay_ms)
    }
    fn consume(&mut self, now: Instant) -> bool {
        if self.due.is_some_and(|due| now >= due) {
            self.due = None;
            true
        } else {
            false
        }
    }
}

pub struct RetryTimer {
    hwnd: HWND,
    id: usize,
    schedule: RetrySchedule,
}
impl RetryTimer {
    pub fn new(hwnd: HWND, id: usize) -> Self {
        Self {
            hwnd,
            id,
            schedule: RetrySchedule::default(),
        }
    }
    pub fn failed(&mut self) {
        if let Some(delay) = self.schedule.failed(Instant::now()) {
            if unsafe { SetTimer(self.hwnd, self.id, delay, None) } == 0 {
                self.schedule.due = None;
                super::log("Could not schedule shell recovery timer");
            }
        }
    }
    pub fn active(&self) -> bool {
        self.schedule.delay_ms != 0
    }
    pub fn reset(&mut self) {
        unsafe {
            KillTimer(self.hwnd, self.id);
        }
        self.schedule = RetrySchedule::default();
    }
    pub fn due(&mut self, id: usize) -> bool {
        // KillTimer doesn't remove WM_TIMER already in the queue. Ignore stale
        // messages after success/restart, and consume each scheduled retry once.
        if id == self.id && self.schedule.consume(Instant::now()) {
            unsafe {
                KillTimer(self.hwnd, self.id);
            }
            true
        } else {
            false
        }
    }
}
impl Drop for RetryTimer {
    fn drop(&mut self) {
        self.reset();
    }
}

/// A connection is published only after its watcher and first snapshot work.
struct Session<M, W> {
    connected: Option<(W, M)>,
}
impl<M, W> Default for Session<M, W> {
    fn default() -> Self {
        Self { connected: None }
    }
}
impl<M, W> Session<M, W> {
    fn read<S>(
        &mut self,
        connect: impl FnOnce() -> Result<M, String>,
        watch: impl FnOnce() -> Result<W, String>,
        snapshot: impl FnOnce(&M) -> Result<S, String>,
    ) -> Result<S, String> {
        let result = (|| {
            if let Some((_, manager)) = self.connected.as_ref() {
                return snapshot(manager);
            }
            let manager = connect()?;
            let watcher = watch()?;
            let next = snapshot(&manager)?;
            self.connected = Some((watcher, manager));
            Ok(next)
        })();
        if result.is_err() {
            self.connected = None;
        }
        result
    }
}

pub struct DesktopConnection {
    session: Session<DesktopManager, Watcher>,
    retry: RetryTimer,
    label: &'static str,
}
impl DesktopConnection {
    pub fn new(hwnd: HWND, timer: usize, label: &'static str) -> Self {
        Self {
            session: Session::default(),
            retry: RetryTimer::new(hwnd, timer),
            label,
        }
    }
    pub fn manager(&self) -> Option<&DesktopManager> {
        self.session.connected.as_ref().map(|(_, manager)| manager)
    }
    pub fn restart(&mut self) {
        self.session.connected = None;
        self.retry.reset();
    }
    pub fn failed(&mut self, error: &str) {
        self.session.connected = None;
        if !self.retry.active() {
            super::log(&format!(
                "{} unavailable; retrying automatically: {error}",
                self.label
            ));
        }
        self.retry.failed();
    }
    pub fn snapshot(&mut self) -> Result<Snapshot, String> {
        let hwnd = self.retry.hwnd;
        let result = self.session.read(
            DesktopManager::connect,
            || Watcher::start(hwnd),
            DesktopManager::snapshot,
        );
        match &result {
            Ok(_) => {
                if self.retry.active() {
                    super::log(&format!("{} restored", self.label));
                }
                self.retry.reset();
            }
            Err(error) => self.failed(error),
        }
        result
    }
    pub fn retry_due(&mut self, id: usize) -> bool {
        self.retry.due(id)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::{cell::Cell, rc::Rc};
    #[test]
    fn prolonged_outage_retries_indefinitely_without_postponing_or_busy_looping() {
        let mut state = RetrySchedule::default();
        let mut now = Instant::now();
        for attempt in 0..100 {
            let delay = state.failed(now).unwrap();
            assert_eq!(delay, (500u32 * 2u32.pow(attempt.min(6))).min(30_000));
            assert_eq!(state.failed(now + Duration::from_millis(1)), None);
            assert!(!state.consume(now + Duration::from_millis((delay - 1).into())));
            now += Duration::from_millis(delay.into());
            assert!(state.consume(now));
            assert!(!state.consume(now));
        }
        state = RetrySchedule::default();
        assert!(!state.consume(now + Duration::from_secs(60)));
        assert_eq!(state.failed(now), Some(500));
    }
    #[test]
    fn stale_timer_after_a_new_restart_cannot_consume_the_new_deadline() {
        let now = Instant::now();
        let mut state = RetrySchedule::default();
        state.failed(now);
        state = RetrySchedule::default();
        state.failed(now + Duration::from_millis(400));
        assert!(!state.consume(now + Duration::from_millis(500)));
        assert!(state.consume(now + Duration::from_millis(900)));
    }
    struct Resource(Rc<Cell<u32>>);
    impl Drop for Resource {
        fn drop(&mut self) {
            self.0.set(self.0.get() + 1);
        }
    }
    #[test]
    fn failed_watcher_or_snapshot_never_leaves_a_partly_connected_session() {
        let drops = Rc::new(Cell::new(0));
        let mut session = Session::<Resource, Resource>::default();
        let result = session.read(
            || Ok(Resource(drops.clone())),
            || Err("watcher unavailable".into()),
            |_| Ok(()),
        );
        assert!(result.is_err());
        assert!(session.connected.is_none());
        assert_eq!(drops.get(), 1);
        let result = session.read(
            || Ok(Resource(drops.clone())),
            || Ok(Resource(drops.clone())),
            |_| Err::<(), _>("snapshot unavailable".into()),
        );
        assert!(result.is_err());
        assert!(session.connected.is_none());
        assert_eq!(drops.get(), 3);
        session
            .read(
                || Ok(Resource(drops.clone())),
                || Ok(Resource(drops.clone())),
                |_| Ok(42),
            )
            .unwrap();
        assert!(session.connected.is_some());
        let result = session.read(
            || panic!("connected"),
            || panic!("connected"),
            |_| Err::<(), _>("Explorer restarted".into()),
        );
        assert!(result.is_err());
        assert!(session.connected.is_none());
        assert_eq!(drops.get(), 5);
        assert_eq!(
            session.read(
                || Ok(Resource(drops.clone())),
                || Ok(Resource(drops.clone())),
                |_| Ok(43)
            ),
            Ok(43)
        );
    }
}
