//! A dedicated WinUI window with a local draft and explicit, validated saves.
mod document;
mod native;
#[cfg(test)]
mod tests;

use crate::settings::Settings;
use document::Document;
use std::{
    cell::{Cell, RefCell},
    rc::Rc,
};
use windows_reactor::*;
use windows_sys::Win32::{Foundation::HWND, UI::WindowsAndMessaging::*};

const TITLE: &str = "Configuration \u{b7} Virtual Desktop Helper";

struct SessionState {
    latest: RefCell<Settings>,
    opened: Cell<bool>,
    hwnd: Cell<HWND>,
    refresh: RefCell<Option<LocalSender<Message>>>,
}
#[derive(Clone)]
pub(super) struct Session(Rc<SessionState>);
impl PartialEq for Session {
    fn eq(&self, other: &Self) -> bool {
        Rc::ptr_eq(&self.0, &other.0)
    }
}
impl Session {
    pub fn new(settings: &Settings) -> Self {
        Self(Rc::new(SessionState {
            latest: RefCell::new(settings.clone()),
            opened: Cell::new(false),
            hwnd: Cell::new(std::ptr::null_mut()),
            refresh: RefCell::new(None),
        }))
    }
    pub fn update(&self, settings: &Settings) {
        *self.0.latest.borrow_mut() = settings.clone();
        if let Some(sender) = self.0.refresh.borrow().as_ref() {
            sender.send(Message::Appearance);
        }
    }
    pub fn begin_open(&self) -> bool {
        if self.0.opened.replace(true) {
            let hwnd = self.0.hwnd.get();
            if !hwnd.is_null() {
                unsafe {
                    ShowWindow(hwnd, SW_RESTORE);
                    SetForegroundWindow(hwnd);
                }
            }
            false
        } else {
            true
        }
    }
    pub fn closed(&self) {
        self.0.opened.set(false);
        self.0.hwnd.set(std::ptr::null_mut());
        self.0.refresh.borrow_mut().take();
    }
}
#[derive(Clone, PartialEq)]
pub(super) struct Input {
    pub session: Session,
    pub parent: usize,
    pub saved: Callback<Settings>,
}
pub(super) enum Message {
    Ready,
    Edit(String),
    Save,
    Close,
    Confirm(ContentDialogResult),
    Appearance,
}
pub(super) struct Editor {
    input: Input,
    document: Document,
    ready: bool,
    error: String,
    confirm_close: bool,
    just_saved: bool,
    textbox: ElementRef<TextBox>,
    native: Option<native::WindowHook>,
}
impl Component for Editor {
    type Input = Input;
    type Message = Message;
    fn create(input: &Input, context: &ComponentContext<Self>) -> Self {
        *input.session.0.refresh.borrow_mut() = Some(context.sender());
        Self::new(input)
    }
    fn update(&mut self, message: Message, context: &ComponentContext<Self>) {
        match message {
            Message::Ready => {
                let hwnd = super::settings_process::window_named(TITLE);
                match native::WindowHook::install(hwnd, context.sender()) {
                    Ok(hook) => {
                        self.native = Some(hook);
                        self.input.session.0.hwnd.set(hwnd);
                        self.ready = true;
                        let _ = self.textbox.request_focus();
                    }
                    Err(error) => self.error = error,
                }
                self.caption();
            }
            Message::Edit(text) if self.ready => {
                self.edit(text);
            }
            Message::Save if !self.confirm_close => {
                self.save();
            }
            Message::Close => {
                if self.document.dirty() {
                    self.confirm_close = true;
                } else {
                    self.close(context);
                }
            }
            Message::Confirm(result) => {
                self.confirm_close = false;
                match result {
                    ContentDialogResult::Primary => {
                        if self.save() {
                            self.close(context);
                        }
                    }
                    ContentDialogResult::Secondary => self.close(context),
                    _ => {
                        let _ = self.textbox.request_focus();
                    }
                }
            }
            Message::Appearance => self.caption(),
            _ => {}
        }
    }
    fn view(&self, _: &Input, context: &mut ViewContext<Self>) -> View {
        let settings = self.input.session.0.latest.borrow();
        context.window_title(TITLE);
        context.window_visuals(
            WindowVisuals::new()
                .client_size(960., 720.)
                .constraints(WindowConstraints {
                    min_width: Some(640.),
                    min_height: Some(480.),
                    ..Default::default()
                })
                .backdrop(WindowBackdrop::Mica)
                .theme(match settings.get("general.theme") {
                    "light" => WindowTheme::Light,
                    "dark" => WindowTheme::Dark,
                    _ => WindowTheme::System,
                }),
        );
        context.on_color_scheme(context.callback(|_: ColorScheme| Message::Appearance));
        let sender = context.sender();
        context.use_effect("mount-editor", (), move || {
            sender.send(Message::Ready);
            None
        });
        let mut textbox = draft_textbox(&self.document, self.ready)
            .element_ref(&self.textbox)
            .horizontal_alignment(HorizontalAlignment::Stretch)
            .vertical_alignment(VerticalAlignment::Stretch)
            .automation_name("Configuration text")
            .grid_row(2);
        if self.ready {
            textbox = textbox.on_text_changed(context.callback(Message::Edit));
        }
        Grid::new().children((
            Grid::new()
                .margin(24.)
                .row_spacing(16.)
                .rows([
                    GridLength::Auto,
                    GridLength::Auto,
                    GridLength::STAR,
                    GridLength::Auto,
                ])
                .children((
                    Grid::new()
                        .columns([GridLength::Pixel(32.), GridLength::STAR])
                        .column_spacing(16.)
                        .children((
                            FontIcon::new()
                                .glyph("\u{E943}")
                                .vertical_alignment(VerticalAlignment::Center),
                            StackPanel::new().grid_column(1).spacing(6.).children((
                                TextBlock::new()
                                    .text("Configuration editor")
                                    .font_size(24.)
                                    .font_weight(FontWeight::SEMI_BOLD),
                                TextBlock::new()
                                    .text(settings.path.to_string_lossy())
                                    .font_size(12.)
                                    .opacity(0.7)
                                    .text_wrapping(TextWrapping::Wrap),
                            )),
                        )),
                    InfoBar::new()
                        .grid_row(1)
                        .is_open(!self.error.is_empty())
                        .is_closable(false)
                        .severity(InfoBarSeverity::Error)
                        .title("Configuration wasn't saved")
                        .message(&self.error),
                    textbox,
                    Grid::new()
                        .grid_row(3)
                        .columns([GridLength::STAR, GridLength::Auto])
                        .column_spacing(16.)
                        .children((
                            TextBlock::new()
                                .vertical_alignment(VerticalAlignment::Center)
                                .opacity(0.7)
                                .text(if self.document.dirty() {
                                    "Unsaved changes"
                                } else if self.just_saved {
                                    "Saved"
                                } else {
                                    ""
                                }),
                            StackPanel::new()
                                .grid_column(1)
                                .orientation(Orientation::Horizontal)
                                .spacing(12.)
                                .children((
                                    Button::new()
                                        .on_click(context.callback(|()| Message::Close))
                                        .content("Close"),
                                    Button::new()
                                        .style(ButtonStyle::Accent)
                                        .min_width(112.)
                                        .is_enabled(self.ready && self.document.dirty())
                                        .automation_name("Save configuration, Control S")
                                        .on_click(context.callback(|()| Message::Save))
                                        .content("Save  (Ctrl+S)"),
                                )),
                        )),
                )),
            ContentDialog::new()
                .is_open(self.confirm_close)
                .title("Save your changes?")
                .primary_button_text("Save")
                .secondary_button_text("Discard")
                .close_button_text("Keep editing")
                .on_closed(context.callback(Message::Confirm))
                .content(
                    TextBlock::new()
                        .text("You have unsaved changes to the configuration.")
                        .text_wrapping(TextWrapping::Wrap),
                ),
        ))
    }
}
impl Editor {
    fn new(input: &Input) -> Self {
        Self {
            input: input.clone(),
            document: Document::new(&input.session.0.latest.borrow()),
            ready: false,
            error: String::new(),
            confirm_close: false,
            just_saved: false,
            textbox: ElementRef::new(),
            native: None,
        }
    }
    fn edit(&mut self, text: String) {
        self.document.edit(text);
        self.just_saved = false;
        self.error.clear();
    }
    fn caption(&self) {
        super::settings_process::update_caption_for(TITLE, &self.input.session.0.latest.borrow());
    }
    fn save(&mut self) -> bool {
        if !self.ready {
            return false;
        }
        if !self.document.dirty() {
            return true;
        }
        let result = self
            .document
            .candidate(&self.input.session.0.latest.borrow())
            .and_then(|candidate| {
                super::settings_process::publish(&candidate, self.input.parent as HWND)?;
                Ok(candidate)
            });
        match result {
            Ok(candidate) => {
                self.document.saved(&candidate);
                self.input.session.update(&candidate);
                let _ = self.input.saved.call(candidate);
                self.error.clear();
                self.just_saved = true;
                self.caption();
                true
            }
            Err(error) => {
                self.error = error;
                false
            }
        }
    }
    fn close(&self, context: &ComponentContext<Self>) {
        if let Some(native) = &self.native {
            native.allow_close();
        }
        let _ = context.window().request_close();
    }
}
impl Drop for Editor {
    fn drop(&mut self) {
        self.input.session.closed();
    }
}

fn draft_textbox(document: &Document, ready: bool) -> TextBox {
    // Reactor observes native text edits before reconciling. The desired Text
    // must match that exact value; a fixed initial value rolls each edit back,
    // and normalizing newlines here forces SetText and resets selection/undo.
    // Delay initial population until AcceptsReturn has been mounted.
    TextBox::new()
        .text(if ready { &document.text } else { "" })
        .accepts_return(true)
        .text_wrapping(TextWrapping::NoWrap)
        .is_enabled(ready)
}
