//! Window-local close interception and Ctrl+S for WinUI Reactor 0.100, which
//! does not expose these window events. Hooks live only as long as the editor.
use super::Message;
use std::{
    cell::{Cell, RefCell},
    rc::{Rc, Weak},
};
use windows_reactor::LocalSender;
use windows_sys::Win32::{
    Foundation::*,
    System::Threading::GetCurrentThreadId,
    UI::{Input::KeyboardAndMouse::*, Shell::*, WindowsAndMessaging::*},
};

const SUBCLASS_ID: usize = 0x57564445;
struct State {
    hwnd: Cell<HWND>,
    allow_close: Cell<bool>,
    sender: LocalSender<Message>,
}
thread_local! { static EDITOR: RefCell<Weak<State>> = const { RefCell::new(Weak::new()) }; }
pub(super) struct WindowHook {
    state: Rc<State>,
    keyboard: HHOOK,
}
impl WindowHook {
    pub fn install(hwnd: HWND, sender: LocalSender<Message>) -> Result<Self, String> {
        if hwnd.is_null() {
            return Err("Couldn't initialize the editor window. Close it and try again.".into());
        }
        let state = Rc::new(State {
            hwnd: Cell::new(hwnd),
            allow_close: Cell::new(false),
            sender,
        });
        unsafe {
            if SetWindowSubclass(
                hwnd,
                Some(window_proc),
                SUBCLASS_ID,
                Rc::as_ptr(&state) as usize,
            ) == 0
            {
                return Err("Couldn't initialize unsaved-change protection. Close the editor and try again.".into());
            }
            let keyboard = SetWindowsHookExW(
                WH_GETMESSAGE,
                Some(keyboard_proc),
                std::ptr::null_mut(),
                GetCurrentThreadId(),
            );
            if keyboard.is_null() {
                RemoveWindowSubclass(hwnd, Some(window_proc), SUBCLASS_ID);
                return Err(
                    "Couldn't initialize the editor keyboard shortcuts. Close it and try again."
                        .into(),
                );
            }
            EDITOR.with(|slot| *slot.borrow_mut() = Rc::downgrade(&state));
            Ok(Self { state, keyboard })
        }
    }
    pub fn allow_close(&self) {
        self.state.allow_close.set(true);
    }
}
impl Drop for WindowHook {
    fn drop(&mut self) {
        unsafe {
            UnhookWindowsHookEx(self.keyboard);
            let hwnd = self.state.hwnd.get();
            if !hwnd.is_null() {
                RemoveWindowSubclass(hwnd, Some(window_proc), SUBCLASS_ID);
            }
        }
        EDITOR.with(|slot| *slot.borrow_mut() = Weak::new());
    }
}
unsafe extern "system" fn window_proc(
    hwnd: HWND,
    message: u32,
    w: WPARAM,
    l: LPARAM,
    _: usize,
    data: usize,
) -> LRESULT {
    let state = &*(data as *const State);
    if message == WM_CLOSE && !state.allow_close.get() && state.sender.send(Message::Close) {
        return 0;
    }
    if message == WM_NCDESTROY {
        state.hwnd.set(std::ptr::null_mut());
        RemoveWindowSubclass(hwnd, Some(window_proc), SUBCLASS_ID);
    }
    DefSubclassProc(hwnd, message, w, l)
}
unsafe extern "system" fn keyboard_proc(code: i32, w: WPARAM, l: LPARAM) -> LRESULT {
    if code == HC_ACTION as i32 && w == PM_REMOVE as usize {
        let message = &mut *(l as *mut MSG);
        if message.message == WM_KEYDOWN
            && message.wParam == VK_S as usize
            && GetKeyState(VK_CONTROL as i32) < 0
            && GetKeyState(VK_MENU as i32) >= 0
        {
            let state = EDITOR.with(|slot| slot.borrow().upgrade());
            if let Some(state) = state {
                if state.hwnd.get() == GetForegroundWindow()
                    && GetAncestor(message.hwnd, GA_ROOT) == state.hwnd.get()
                    && state.sender.send(Message::Save)
                {
                    message.message = WM_NULL;
                    message.wParam = 0;
                    message.lParam = 0;
                }
            }
        }
    }
    CallNextHookEx(std::ptr::null_mut(), code, w, l)
}
