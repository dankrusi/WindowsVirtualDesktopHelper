//! Only this module knows the undocumented COM layouts. QueryService must accept
//! an exact IID before we call its vtable; never cast an unknown OS interface.
use super::{profiles::PROFILES, registry};
use std::{ffi::c_void, marker::PhantomData, rc::Rc};
use windows_sys::{
    core::GUID,
    Win32::{System::Com::*, System::Registry::HKEY_CURRENT_USER},
};
type Ptr = *mut c_void;
pub struct Profile {
    pub name: &'static str,
    pub manager: GUID,
    pub desktop: GUID,
    pub monitor: bool,
    pub extra_current: bool,
}
struct Com(Ptr, PhantomData<Rc<()>>);
impl Com {
    fn new(p: Ptr) -> Result<Self, String> {
        if p.is_null() {
            Err("COM returned a null interface".into())
        } else {
            Ok(Self(p, PhantomData))
        }
    }
    unsafe fn slot(&self, n: usize) -> Ptr {
        *(*(self.0 as *const *const Ptr)).add(n)
    }
}
impl Drop for Com {
    fn drop(&mut self) {
        unsafe {
            let f: unsafe extern "system" fn(Ptr) -> u32 = std::mem::transmute(self.slot(2));
            f(self.0);
        }
    }
}
fn check(hr: i32) -> Result<(), String> {
    if hr < 0 {
        Err(format!("VirtualDesktopAPI HRESULT 0x{:08X}", hr as u32))
    } else {
        Ok(())
    }
}
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Desktop {
    pub id: u128,
    pub name: String,
}
#[derive(Clone, Debug)]
pub struct Snapshot {
    pub desktops: Vec<Desktop>,
    pub current: usize,
}
impl Snapshot {
    pub fn active(&self) -> &Desktop {
        &self.desktops[self.current]
    }
}
pub struct DesktopManager {
    manager: Com,
    public: Option<Com>,
    profile: &'static Profile,
}
impl DesktopManager {
    pub fn connect() -> Result<Self, String> {
        unsafe {
            let mut shell = std::ptr::null_mut();
            check(CoCreateInstance(
                &GUID::from_u128(0xc2f03a3321f547fab4bb156362a2f239),
                std::ptr::null_mut(),
                CLSCTX_LOCAL_SERVER,
                &GUID::from_u128(0x6d5140c1743611ce803400aa006009fa),
                &mut shell,
            ))?;
            let shell = Com::new(shell)?;
            let query: unsafe extern "system" fn(Ptr, *const GUID, *const GUID, *mut Ptr) -> i32 =
                std::mem::transmute(shell.slot(3));
            let build = registry::string(
                windows_sys::Win32::System::Registry::HKEY_LOCAL_MACHINE,
                r"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "CurrentBuildNumber",
            )
            .and_then(|s| s.parse::<u32>().ok())
            .unwrap_or(0);
            for profile in PROFILES.iter().rev() {
                // 21H2 and 22H2 reuse an IID despite inserting a vtable slot.
                if profile.name == "VirtualDesktopWin11_21H2" && build >= 22621 {
                    continue;
                }
                if profile.name == "VirtualDesktopWin11_22H2" && build < 22621 {
                    continue;
                }
                let mut manager = std::ptr::null_mut();
                if query(
                    shell.0,
                    &GUID::from_u128(0xc5e0cdca7b6e41b29fc4d93975cc467b),
                    &profile.manager,
                    &mut manager,
                ) >= 0
                {
                    let mut result = Self {
                        manager: Com::new(manager)?,
                        public: None,
                        profile,
                    };
                    if result.snapshot().is_ok() {
                        let mut public = std::ptr::null_mut();
                        if CoCreateInstance(
                            &GUID::from_u128(0xaa5090865ca94c258f95589d3c07b48a),
                            std::ptr::null_mut(),
                            CLSCTX_INPROC_SERVER,
                            &GUID::from_u128(0xa5cd92ff29be454c8d04d82879fb3f1b),
                            &mut public,
                        ) >= 0
                        {
                            result.public = Com::new(public).ok();
                        }
                        super::log(&format!("Connected {}", profile.name));
                        return Ok(result);
                    }
                }
            }
            Err("This Windows build exposes no supported virtual desktop interface. See app.log and report your Windows build on GitHub.".into())
        }
    }
    /// Published IVirtualDesktopManager API: attribute a foreground event to the
    /// window's actual desktop, even when delivery races with a desktop switch.
    pub fn window_desktop(
        &self,
        hwnd: windows_sys::Win32::Foundation::HWND,
    ) -> Result<u128, String> {
        let public = self
            .public
            .as_ref()
            .ok_or("Window desktop API unavailable")?;
        unsafe {
            let mut id = GUID::from_u128(0);
            let f: unsafe extern "system" fn(
                Ptr,
                windows_sys::Win32::Foundation::HWND,
                *mut GUID,
            ) -> i32 = std::mem::transmute(public.slot(4));
            check(f(public.0, hwnd, &mut id))?;
            Ok(((id.data1 as u128) << 96)
                | ((id.data2 as u128) << 80)
                | ((id.data3 as u128) << 64)
                | u64::from_be_bytes(id.data4) as u128)
        }
    }
    pub fn profile(&self) -> &str {
        self.profile.name
    }
    fn current(&self) -> Result<Com, String> {
        unsafe {
            let mut out = std::ptr::null_mut();
            let hr = if self.profile.monitor {
                let f: unsafe extern "system" fn(Ptr, Ptr, *mut Ptr) -> i32 =
                    std::mem::transmute(self.manager.slot(6));
                f(self.manager.0, std::ptr::null_mut(), &mut out)
            } else {
                let f: unsafe extern "system" fn(Ptr, *mut Ptr) -> i32 =
                    std::mem::transmute(self.manager.slot(6));
                f(self.manager.0, &mut out)
            };
            check(hr)?;
            Com::new(out)
        }
    }
    fn desktops(&self) -> Result<Vec<Com>, String> {
        unsafe {
            let mut out = std::ptr::null_mut();
            let slot = 7 + usize::from(self.profile.extra_current);
            let hr = if self.profile.monitor {
                let f: unsafe extern "system" fn(Ptr, Ptr, *mut Ptr) -> i32 =
                    std::mem::transmute(self.manager.slot(slot));
                f(self.manager.0, std::ptr::null_mut(), &mut out)
            } else {
                let f: unsafe extern "system" fn(Ptr, *mut Ptr) -> i32 =
                    std::mem::transmute(self.manager.slot(slot));
                f(self.manager.0, &mut out)
            };
            check(hr)?;
            let array = Com::new(out)?;
            let count: unsafe extern "system" fn(Ptr, *mut u32) -> i32 =
                std::mem::transmute(array.slot(3));
            let get: unsafe extern "system" fn(Ptr, u32, *const GUID, *mut Ptr) -> i32 =
                std::mem::transmute(array.slot(4));
            let mut n = 0;
            check(count(array.0, &mut n))?;
            if n == 0 || n > 4096 {
                return Err("Invalid virtual desktop count".into());
            }
            let mut result = Vec::with_capacity(n as usize);
            for i in 0..n {
                let mut p = std::ptr::null_mut();
                check(get(array.0, i, &self.profile.desktop, &mut p))?;
                result.push(Com::new(p)?);
            }
            Ok(result)
        }
    }
    fn id(desktop: &Com) -> Result<u128, String> {
        unsafe {
            let mut id = GUID::from_u128(0);
            let f: unsafe extern "system" fn(Ptr, *mut GUID) -> i32 =
                std::mem::transmute(desktop.slot(4));
            check(f(desktop.0, &mut id))?;
            Ok(((id.data1 as u128) << 96)
                | ((id.data2 as u128) << 80)
                | ((id.data3 as u128) << 64)
                | u64::from_be_bytes(id.data4) as u128)
        }
    }
    pub fn snapshot(&self) -> Result<Snapshot, String> {
        let id = Self::id(&self.current()?)?;
        let mut desktops = Vec::new();
        for (i, d) in self.desktops()?.iter().enumerate() {
            let id = Self::id(d)?;
            let hex = format!("{id:032x}");
            let key = format!(
                "{}\\Desktops\\{{{}-{}-{}-{}-{}}}",
                registry::DESKTOPS,
                &hex[..8],
                &hex[8..12],
                &hex[12..16],
                &hex[16..20],
                &hex[20..]
            );
            let name = registry::string(HKEY_CURRENT_USER, &key, "Name")
                .filter(|s| !s.is_empty())
                .unwrap_or_else(|| format!("Desktop {}", i + 1));
            desktops.push(Desktop { id, name });
        }
        let current = desktops
            .iter()
            .position(|d| d.id == id)
            .ok_or("Current desktop changed during enumeration")?;
        Ok(Snapshot { desktops, current })
    }
    pub fn switch(&self, index: usize) -> Result<(), String> {
        let desktops = self.desktops()?;
        let d = desktops
            .get(index)
            .ok_or("Desktop number is out of range")?;
        unsafe {
            let slot = 9 + usize::from(self.profile.extra_current);
            let hr = if self.profile.monitor {
                let f: unsafe extern "system" fn(Ptr, Ptr, Ptr) -> i32 =
                    std::mem::transmute(self.manager.slot(slot));
                f(self.manager.0, std::ptr::null_mut(), d.0)
            } else {
                let f: unsafe extern "system" fn(Ptr, Ptr) -> i32 =
                    std::mem::transmute(self.manager.slot(slot));
                f(self.manager.0, d.0)
            };
            check(hr)
        }
    }
}
#[derive(Default)]
pub struct History {
    visits: Vec<u128>,
}
impl History {
    pub fn record(&mut self, s: &Snapshot) {
        self.visits
            .retain(|id| s.desktops.iter().any(|d| d.id == *id));
        let id = s.active().id;
        if self.visits.last() != Some(&id) {
            self.visits.push(id);
        }
        if self.visits.len() > 20 {
            self.visits.remove(0);
        }
    }
    pub fn previous(&self, s: &Snapshot) -> Option<usize> {
        self.visits
            .iter()
            .rev()
            .find(|id| **id != s.active().id)
            .and_then(|id| s.desktops.iter().position(|d| d.id == *id))
    }
}
#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn history_survives_reorder_and_removal() {
        let mut h = History::default();
        let mut s = Snapshot {
            desktops: vec![
                Desktop {
                    id: 1,
                    name: "a".into(),
                },
                Desktop {
                    id: 2,
                    name: "b".into(),
                },
            ],
            current: 0,
        };
        h.record(&s);
        s.current = 1;
        h.record(&s);
        assert_eq!(h.previous(&s), Some(0));
        s.desktops.reverse();
        s.current = 0;
        h.record(&s);
        assert_eq!(h.previous(&s), Some(1));
        s.desktops.pop();
        h.record(&s);
        assert_eq!(h.previous(&s), None);
    }
    /// Opt-in integration check; never run on CI or a noninteractive desktop.
    #[test]
    #[ignore = "Temporarily switches to the next desktop and restores the original"]
    fn live_switch_and_registry_notification() {
        use crate::platform::{events::Watcher, wide};
        use windows_sys::Win32::{
            Foundation::*, System::LibraryLoader::GetModuleHandleW, UI::WindowsAndMessaging::*,
        };
        unsafe extern "system" fn proc(h: HWND, m: u32, w: usize, l: isize) -> isize {
            unsafe { DefWindowProcW(h, m, w, l) }
        }
        struct Apartment;
        impl Drop for Apartment {
            fn drop(&mut self) {
                unsafe {
                    CoUninitialize();
                }
            }
        }
        struct Window(HWND);
        impl Drop for Window {
            fn drop(&mut self) {
                unsafe {
                    DestroyWindow(self.0);
                }
            }
        }
        struct Restore<'a>(&'a DesktopManager, u128);
        impl Drop for Restore<'_> {
            fn drop(&mut self) {
                if let Ok(s) = self.0.snapshot() {
                    if let Some(n) = s.desktops.iter().position(|d| d.id == self.1) {
                        let _ = self.0.switch(n);
                    }
                }
            }
        }
        unsafe {
            check(CoInitializeEx(
                std::ptr::null(),
                COINIT_APARTMENTTHREADED as u32,
            ))
            .unwrap();
            let _apartment = Apartment;
            let manager = DesktopManager::connect().unwrap();
            let initial = manager.snapshot().unwrap();
            assert!(
                initial.desktops.len() > 1,
                "Create a second desktop before running this test"
            );
            let class = wide("WVDH.IntegrationTest");
            let wc = WNDCLASSW {
                hInstance: GetModuleHandleW(std::ptr::null()),
                lpfnWndProc: Some(proc),
                lpszClassName: class.as_ptr(),
                ..std::mem::zeroed()
            };
            RegisterClassW(&wc);
            let window = Window(CreateWindowExW(
                0,
                class.as_ptr(),
                class.as_ptr(),
                0,
                0,
                0,
                0,
                0,
                HWND_MESSAGE,
                std::ptr::null_mut(),
                wc.hInstance,
                std::ptr::null(),
            ));
            assert!(!window.0.is_null());
            let _watcher = Watcher::start(window.0).unwrap();
            let _restore = Restore(&manager, initial.active().id);
            let next = (initial.current + 1) % initial.desktops.len();
            manager.switch(next).unwrap();
            let start = std::time::Instant::now();
            let mut received = false;
            while start.elapsed() < std::time::Duration::from_secs(3) {
                let mut msg: MSG = std::mem::zeroed();
                while PeekMessageW(&mut msg, std::ptr::null_mut(), 0, 0, PM_REMOVE) != 0 {
                    if msg.hwnd == window.0
                        && msg.message == crate::platform::events::DESKTOP_CHANGED
                    {
                        received = true;
                    }
                    TranslateMessage(&msg);
                    DispatchMessageW(&msg);
                }
                if received {
                    break;
                }
                MsgWaitForMultipleObjects(
                    0,
                    std::ptr::null(),
                    0,
                    3000 - start.elapsed().as_millis().min(3000) as u32,
                    QS_ALLINPUT,
                );
            }
            assert!(
                received,
                "No registry notification after switching desktops"
            );
            assert_eq!(
                manager.snapshot().unwrap().active().id,
                initial.desktops[next].id
            );
        }
    }
}
