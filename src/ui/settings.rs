//! Actual WinUI 3 / Fluent controls, hosted only while Settings is open.
use crate::{
    platform::registry,
    settings::{Settings, DEFAULTS},
};
use std::collections::BTreeMap;
use windows_reactor::*;
use windows_sys::Win32::Foundation::HWND;

const PAGES: [(&str, &str, &str); 5] = [
    ("general", "General", "\u{E713}"),
    ("tray", "Tray icons", "\u{E9D9}"),
    ("overlays", "Desktop overlays", "\u{E7F4}"),
    ("hotkeys", "Keyboard shortcuts", "\u{E765}"),
    ("advanced", "Advanced", "\u{E90F}"),
];
const POS: [(&str, &str); 9] = [
    ("topleft", "Top left"),
    ("topcenter", "Top center"),
    ("topright", "Top right"),
    ("middleleft", "Center left"),
    ("middlecenter", "Center"),
    ("middleright", "Center right"),
    ("bottomleft", "Bottom left"),
    ("bottomcenter", "Bottom center"),
    ("bottomright", "Bottom right"),
];
const THEME: [(&str, &str); 3] = [
    ("auto", "Use Windows setting"),
    ("light", "Light"),
    ("dark", "Dark"),
];

enum Message {
    Page(Option<String>),
    Set(&'static str, String),
    Edit(String, String),
    Save(String),
    Preview(bool),
    Advanced(String),
    SaveAdvanced,
    Folder,
    Startup,
    Dismiss,
    Capture(&'static str),
    Captured(Option<String>),
    Size(WindowSize),
    Pane(bool),
    Caption,
    RefreshTray(bool),
}
struct SettingsUi {
    settings: Settings,
    parent: usize,
    page: String,
    error: String,
    edits: BTreeMap<String, String>,
    advanced: String,
    preview: super::overlay::Overlays,
    capture: Option<super::shortcut_capture::Capture>,
    capture_key: Option<&'static str>,
    narrow: bool,
    pane_open: bool,
    tray_preview: super::tray_preview::Preview,
}
impl Component for SettingsUi {
    type Input = Settings;
    type Message = Message;
    fn create(input: &Settings, context: &ComponentContext<Self>) -> Self {
        let sender = context.sender();
        Self {
            tray_preview: super::tray_preview::Preview::new(input, move |reconnect| {
                sender.send(Message::RefreshTray(reconnect));
            }),
            settings: input.clone(),
            parent: input.get("ui-parent").parse().unwrap_or(0),
            page: "general".into(),
            error: String::new(),
            edits: BTreeMap::new(),
            advanced: config_text(input),
            preview: super::overlay::Overlays::new(),
            capture: None,
            capture_key: None,
            narrow: input.get("ui-width").parse::<f64>().unwrap_or(1080.) < 1000.,
            pane_open: input.get("ui-width").parse::<f64>().unwrap_or(1080.) >= 1000.,
        }
    }
    fn update(&mut self, message: Message, context: &ComponentContext<Self>) {
        match message {
            Message::RefreshTray(reconnect) => self.tray_preview.refresh(&self.settings, reconnect),
            Message::Page(Some(page)) => {
                self.tray_preview.refresh(&self.settings, false);
                self.page = page;
                if self.narrow {
                    self.pane_open = false;
                }
                self.capture = None;
                self.capture_key = None;
            }
            Message::Page(None) => {}
            Message::Set(key, value) => {
                if self.settings.get(key) == value {
                    return;
                }
                let mut candidate = self.settings.clone();
                candidate.set(key, value);
                self.commit(candidate);
            }
            Message::Edit(key, value) => {
                self.edits.insert(key, value);
            }
            Message::Save(key) => {
                if let Some(value) = self.edits.get(&key) {
                    let mut candidate = self.settings.clone();
                    candidate.set(&key, value.clone());
                    if self.commit(candidate) {
                        self.edits.remove(&key);
                    }
                }
            }
            Message::Preview(status) => {
                self.preview.show(
                    &self.settings,
                    self.tray_preview.desktop_name(),
                    status,
                    Some(2200),
                );
            }
            Message::Advanced(text) => self.advanced = text,
            Message::SaveAdvanced => match crate::settings::parse(&self.advanced) {
                Ok(config) => {
                    let mut candidate = self.settings.clone();
                    candidate.config = config;
                    self.commit(candidate);
                }
                Err(e) => self.error = e,
            },
            Message::Folder => super::settings_process::open_folder(&self.settings),
            Message::Startup => super::settings_process::startup_settings(),
            Message::Dismiss => self.error.clear(),
            Message::Capture(key) => {
                self.capture = None;
                self.capture_key = Some(key);
                let sender = context.sender();
                match super::shortcut_capture::Capture::start(move |value| {
                    sender.send(Message::Captured(value));
                }) {
                    Ok(c) => self.capture = Some(c),
                    Err(e) => {
                        self.capture_key = None;
                        self.error = e;
                    }
                }
            }
            Message::Captured(value) => {
                self.capture = None;
                if let Some(key) = self.capture_key.take() {
                    if let Some(value) = value {
                        let mut candidate = self.settings.clone();
                        candidate.set(key, value);
                        self.commit(candidate);
                    }
                }
            }
            Message::Pane(open) => self.pane_open = open,
            Message::Caption => super::settings_process::update_caption(&self.settings),
            Message::Size(size) => {
                let narrow = size.width < 1000.;
                if self.narrow != narrow {
                    self.narrow = narrow;
                    self.pane_open = !narrow;
                }
                super::settings_process::update_caption(&self.settings);
            }
        }
    }
    fn view(&self, _: &Settings, context: &mut ViewContext<Self>) -> View {
        context.window_title("Settings · Virtual Desktop Helper");
        context.window_visuals(
            WindowVisuals::new()
                .client_size(
                    self.settings
                        .get("ui-width")
                        .parse::<f64>()
                        .unwrap_or(1080.)
                        .clamp(680., 1600.),
                    780.,
                )
                .constraints(WindowConstraints {
                    min_width: Some(680.),
                    min_height: Some(540.),
                    ..Default::default()
                })
                .backdrop(WindowBackdrop::Mica)
                .theme(match self.settings.get("general.theme") {
                    "light" => WindowTheme::Light,
                    "dark" => WindowTheme::Dark,
                    _ => WindowTheme::System,
                }),
        );
        context.on_window_size(context.callback(Message::Size));
        context.on_color_scheme(context.callback(|_: ColorScheme| Message::Caption));
        let caption_settings = self.settings.clone();
        context.use_effect(
            "caption-theme",
            self.settings.get("general.theme").to_string(),
            move || {
                super::settings_process::update_caption(&caption_settings);
                None
            },
        );
        let nav = PAGES.iter().map(|(key, title, icon)| {
            (
                *key,
                NavigationViewItem::new()
                    .tag(*key)
                    .is_selected(self.page == *key)
                    .slots([
                        SlotView::new(
                            NavigationViewItemSlot::Content,
                            TextBlock::new().text(*title),
                        ),
                        SlotView::new(NavigationViewItemSlot::Icon, FontIcon::new().glyph(*icon)),
                    ]),
            )
        });
        let title = PAGES
            .iter()
            .find(|p| p.0 == self.page)
            .map(|p| p.1)
            .unwrap_or("General");
        let description = match self.page.as_str() {
            "tray" => "Know where you are, without opening Task View.",
            "overlays" => "Choose when and where desktop names appear.",
            "hotkeys" => "Move between desktops without leaving the keyboard.",
            "advanced" => "Fine-tune your setup and manage configuration.",
            _ => "Make your virtual desktops feel like home.",
        };
        let body = match self.page.as_str() {
            "tray" => self.tray(context),
            "overlays" => self.overlays(context),
            "hotkeys" => self.hotkeys(context),
            "advanced" => self.advanced(context),
            _ => self.general(context),
        };
        let content=ScrollViewer::new().horizontal_scroll_bar_visibility(ScrollBarVisibility::Disabled).content(
            StackPanel::new().spacing(20.).margin(Thickness::new(28.,20.,32.,32.)).max_width(920.).horizontal_alignment(HorizontalAlignment::Stretch).children((
                StackPanel::new().spacing(8.).children((TextBlock::new().text(title).font_size(28.).font_weight(FontWeight::SEMI_BOLD),description_text(description))),
                InfoBar::new().is_open(!self.error.is_empty()).severity(InfoBarSeverity::Error).title("Change wasn't saved").message(&self.error).on_closed(context.callback(|()|Message::Dismiss)),
                body,
                TextBlock::new().text("Toggles and choices save automatically. Text fields use Save. Closing Settings keeps the helper running.").font_size(12.).opacity(0.65).text_wrapping(TextWrapping::Wrap)
            )));
        NavigationView::new()
            .open_pane_length(240.)
            .pane_display_mode(if self.narrow {
                NavigationViewPaneDisplayMode::LeftCompact
            } else {
                NavigationViewPaneDisplayMode::Left
            })
            .is_pane_open(self.pane_open)
            .on_is_pane_open_changed(context.callback(Message::Pane))
            .is_settings_visible(false)
            .is_back_button_visible(NavigationViewBackButtonVisible::Collapsed)
            .always_show_header(false)
            .on_selected_tag_changed(context.callback(Message::Page))
            .slots([
                SlotView::collection(NavigationViewSlot::MenuItems, nav),
                SlotView::new(NavigationViewSlot::Content, content),
                SlotView::new(
                    NavigationViewSlot::PaneFooter,
                    TextBlock::new()
                        .text(if self.pane_open {
                            "Virtual Desktop Helper 3.0"
                        } else {
                            "3.0"
                        })
                        .font_size(12.)
                        .opacity(0.6)
                        .margin(16.),
                ),
            ])
    }
}
impl SettingsUi {
    fn commit(&mut self, candidate: Settings) -> bool {
        match super::settings_process::publish(&candidate, self.parent as HWND) {
            Ok(()) => {
                self.settings = candidate;
                self.tray_preview.render(&self.settings);
                super::settings_process::update_caption(&self.settings);
                self.advanced = config_text(&self.settings);
                self.error.clear();
                true
            }
            Err(e) => {
                self.error = e;
                false
            }
        }
    }
    fn enabled(&self, key: &str) -> bool {
        !self.settings.arguments.contains_key(key)
    }
    fn row(&self, icon: &str, title: &str, description: &str, key: &str, control: View) -> View {
        let locked = !key.is_empty() && !self.enabled(key);
        let description = if locked {
            format!("{description} Set by a command-line override.")
        } else {
            description.into()
        };
        let controls = Border::new()
            .grid_column(if self.narrow { 1 } else { 2 })
            .grid_row(i32::from(self.narrow))
            .margin(if self.narrow {
                Thickness::new(0., 12., 0., 0.)
            } else {
                Thickness::new(16., 0., 0., 0.)
            })
            .vertical_alignment(VerticalAlignment::Center)
            .horizontal_alignment(if self.narrow {
                HorizontalAlignment::Left
            } else {
                HorizontalAlignment::Right
            })
            .content(control);
        card(
            Grid::new()
                .columns([GridLength::Pixel(32.), GridLength::STAR, GridLength::Auto])
                .rows([GridLength::Auto, GridLength::Auto])
                .column_spacing(16.)
                .children((
                    FontIcon::new()
                        .glyph(icon)
                        .grid_column(0)
                        .vertical_alignment(VerticalAlignment::Center),
                    StackPanel::new()
                        .grid_column(1)
                        .spacing(4.)
                        .vertical_alignment(VerticalAlignment::Center)
                        .children((
                            TextBlock::new()
                                .text(title)
                                .font_size(14.)
                                .text_wrapping(TextWrapping::Wrap),
                            description_text(&description),
                        )),
                    controls,
                )),
        )
    }
    fn toggle(
        &self,
        c: &ViewContext<Self>,
        key: &'static str,
        icon: &str,
        title: &str,
        description: &str,
    ) -> View {
        self.row(
            icon,
            title,
            description,
            key,
            ToggleSwitch::new()
                .is_on(self.settings.bool(key))
                .is_enabled(self.enabled(key))
                .on_toggled(c.callback(move |v: bool| Message::Set(key, v.to_string())))
                .automation_name(title)
                .into(),
        )
    }
    fn choice(
        &self,
        c: &ViewContext<Self>,
        key: &'static str,
        icon: &str,
        title: &str,
        description: &str,
        choices: &'static [(&'static str, &'static str)],
    ) -> View {
        // Preserve custom modifier combinations and legacy values in the picker.
        let mut options: Vec<(String, String)> = choices
            .iter()
            .map(|(v, label)| (v.to_string(), label.to_string()))
            .collect();
        let selected = options
            .iter()
            .position(|(v, _)| v == self.settings.get(key))
            .unwrap_or_else(|| {
                options.push((self.settings.get(key).into(), self.settings.get(key).into()));
                options.len() - 1
            });
        self.row(
            icon,
            title,
            description,
            key,
            ComboBox::new()
                .width(210.)
                .items_source(options.iter().map(|(_, label)| label.clone()))
                .selected_index(selected)
                .is_enabled(self.enabled(key))
                .automation_name(title)
                .on_selection_changed(c.callback(move |index: Option<usize>| {
                    index
                        .and_then(|i| options.get(i))
                        .map(|(v, _)| Message::Set(key, v.clone()))
                        .unwrap_or(Message::Page(None))
                }))
                .into(),
        )
    }
    fn number(
        &self,
        c: &ViewContext<Self>,
        key: &'static str,
        title: &str,
        description: &str,
        min: f64,
        max: f64,
    ) -> View {
        self.row(
            "\u{E8A4}",
            title,
            description,
            key,
            NumberBox::new()
                .width(150.)
                .minimum(min.min(self.settings.number(key)))
                .maximum(max.max(self.settings.number(key)))
                .value(self.settings.number(key))
                .is_enabled(self.enabled(key))
                .automation_name(title)
                .on_value_changed(c.callback(move |v: Option<f64>| {
                    Message::Set(key, v.unwrap_or(min).to_string())
                }))
                .into(),
        )
    }
    fn text_setting(
        &self,
        c: &ViewContext<Self>,
        key: &'static str,
        title: &str,
        description: &str,
    ) -> View {
        let edit_key = key.to_string();
        let save_key = edit_key.clone();
        let value = self
            .edits
            .get(key)
            .map(String::as_str)
            .unwrap_or_else(|| self.settings.get(key));
        self.row(
            "\u{E8D2}",
            title,
            description,
            key,
            StackPanel::new()
                .orientation(Orientation::Horizontal)
                .spacing(8.)
                .children((
                    TextBox::new()
                        .width(160.)
                        .text(value)
                        .is_enabled(self.enabled(key))
                        .automation_name(title)
                        .on_text_changed(c.callback(move |v| Message::Edit(edit_key.clone(), v))),
                    Button::new()
                        .is_enabled(self.enabled(key) && self.edits.contains_key(key))
                        .on_click(c.callback(move |()| Message::Save(save_key.clone())))
                        .content("Save"),
                )),
        )
    }
    fn general(&self, c: &ViewContext<Self>) -> View {
        let startup = if registry::packaged() {
            self.row(
                "\u{E7E8}",
                "Start with Windows",
                "Manage this app in Windows Startup Apps.",
                "",
                Button::new()
                    .on_click(c.callback(|()| Message::Startup))
                    .content("Open Windows Settings"),
            )
        } else {
            self.toggle(
                c,
                "general.startupWithWindows",
                "\u{E7E8}",
                "Start with Windows",
                "Have your desktop tools ready when you sign in.",
            )
        };
        section(vec![
            self.hero(),
            group_title("Personalization"),
            self.choice(
                c,
                "general.theme",
                "\u{E790}",
                "App theme",
                "Choose a look, or follow Windows.",
                &THEME,
            ),
            group_title("Startup and behavior"),
            startup,
            self.toggle(
                c,
                "feature.showSplashScreen",
                "\u{E734}",
                "Welcome message",
                "Show a brief overlay when the helper starts.",
            ),
            self.toggle(
                c,
                "feature.restorePreviousWindowFocus",
                "\u{E8A7}",
                "Restore window focus",
                "Return to the last active window when you switch desktops.",
            ),
        ])
    }
    fn tray(&self, c: &ViewContext<Self>) -> View {
        section(vec![
            self.hero(),
            group_title("Indicators"),
            self.toggle(
                c,
                "feature.showDesktopNumberInIconTray",
                "\u{E8EF}",
                "Desktop number",
                "See the current desktop at a glance.",
            ),
            self.toggle(
                c,
                "feature.showDesktopNameInIconTray",
                "\u{E8D2}",
                "Desktop initial",
                "Use the first character of the desktop name.",
            ),
            self.toggle(
                c,
                "feature.showDesktopNumberInIconTray.clickToOpenTaskView",
                "\u{E7F4}",
                "Click to open Task View",
                "Click the desktop indicator to see all your desktops.",
            ),
            group_title("Navigation"),
            self.toggle(
                c,
                "feature.showPrevNextIcons",
                "\u{E8AB}",
                "Previous and next buttons",
                "Switch desktops directly from the tray.",
            ),
            self.toggle(
                c,
                "feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds",
                "\u{E890}",
                "Hide unavailable arrows",
                "Hide Previous on the first desktop and Next on the last.",
            ),
            group_title("Appearance"),
            self.toggle(
                c,
                "theme.icons.transparentBackground",
                "\u{E790}",
                "Transparent background",
                "Blend into the taskbar without a black or white square.",
            ),
            self.text_setting(
                c,
                "feature.showPrevNextIcons.prevChar",
                "Previous symbol",
                "Choose the character used for the previous arrow.",
            ),
            self.text_setting(
                c,
                "feature.showPrevNextIcons.nextChar",
                "Next symbol",
                "Choose the character used for the next arrow.",
            ),
        ])
    }
    fn overlays(&self, c: &ViewContext<Self>) -> View {
        section(vec![
            self.hero(),
            group_title("When switching desktops"),
            self.toggle(
                c,
                "feature.showDesktopSwitchOverlay",
                "\u{E8BD}",
                "Desktop switch notification",
                "Briefly show the name of the desktop you switch to.",
            ),
            self.choice(
                c,
                "feature.showDesktopSwitchOverlay.position",
                "\u{E8A9}",
                "Position",
                "Place the notification where it works best for you.",
                &POS,
            ),
            self.number(
                c,
                "feature.showDesktopSwitchOverlay.duration",
                "Display time",
                "Milliseconds. Set to 0 to keep it visible until the next switch.",
                0.,
                60000.,
            ),
            self.toggle(
                c,
                "feature.showDesktopSwitchOverlay.showOnAllMonitors",
                "\u{E7F4}",
                "Show on every monitor",
                "Display the notification across your screens.",
            ),
            self.toggle(
                c,
                "feature.showDesktopSwitchOverlay.animate",
                "\u{E768}",
                "Animation",
                "Fade the notification in smoothly.",
            ),
            self.toggle(
                c,
                "feature.showDesktopSwitchOverlay.translucent",
                "\u{E790}",
                "Translucent notification",
                "Let the desktop show through the notification.",
            ),
            self.row(
                "\u{E768}",
                "Try the notification",
                "Preview your current appearance and placement.",
                "",
                Button::new()
                    .on_click(c.callback(|()| Message::Preview(false)))
                    .content("Preview"),
            ),
            group_title("Always-visible desktop name"),
            self.toggle(
                c,
                "feature.showDesktopStatusOverlay",
                "\u{E7B3}",
                "Desktop status label",
                "Keep a small desktop name on screen.",
            ),
            self.choice(
                c,
                "feature.showDesktopStatusOverlay.position",
                "\u{E8A9}",
                "Label position",
                "Choose a screen edge or corner.",
                &POS,
            ),
            self.toggle(
                c,
                "feature.showDesktopStatusOverlay.showOnAllMonitors",
                "\u{E7F4}",
                "Label on every monitor",
                "Show the permanent label on all screens.",
            ),
            self.row(
                "\u{E768}",
                "Try the status label",
                "Show a temporary preview of the permanent label.",
                "",
                Button::new()
                    .on_click(c.callback(|()| Message::Preview(true)))
                    .content("Preview"),
            ),
            group_title("Size and text"),
            self.number(
                c,
                "theme.overlay.width",
                "Notification width",
                "Device-independent pixels; scales with each monitor.",
                100.,
                1600.,
            ),
            self.number(
                c,
                "theme.overlay.height",
                "Notification height",
                "Device-independent pixels.",
                40.,
                800.,
            ),
            self.number(
                c,
                "theme.overlay.fontSize",
                "Notification text size",
                "Points.",
                8.,
                96.,
            ),
            self.number(
                c,
                "theme.status.width",
                "Status label width",
                "Device-independent pixels.",
                80.,
                1000.,
            ),
            self.number(
                c,
                "theme.status.height",
                "Status label height",
                "Device-independent pixels.",
                20.,
                200.,
            ),
            self.number(
                c,
                "theme.status.fontSize",
                "Status text size",
                "Points.",
                8.,
                48.,
            ),
            self.number(
                c,
                "theme.status.offset",
                "Distance from screen edge",
                "Device-independent pixels.",
                0.,
                200.,
            ),
        ])
    }
    fn shortcut(
        &self,
        c: &ViewContext<Self>,
        enabled: &'static str,
        key: &'static str,
        title: &str,
        description: &str,
    ) -> View {
        let recording = self.capture_key == Some(key);
        let content = StackPanel::new()
            .orientation(Orientation::Horizontal)
            .spacing(16.)
            .children((
                Button::new()
                    .min_width(160.)
                    .is_enabled(self.enabled(key))
                    .style(if recording {
                        ButtonStyle::Accent
                    } else {
                        ButtonStyle::Default
                    })
                    .automation_name(format!("Change {title} shortcut"))
                    .on_click(c.callback(move |()| Message::Capture(key)))
                    .content(if recording {
                        "Press shortcut…"
                    } else {
                        self.settings.get(key)
                    }),
                ToggleSwitch::new()
                    .is_on(self.settings.bool(enabled))
                    .is_enabled(self.enabled(enabled))
                    .automation_name(format!("Enable {title}"))
                    .on_toggled(c.callback(move |v: bool| Message::Set(enabled, v.to_string()))),
            ));
        self.row("\u{E765}", title, description, key, content)
    }
    fn hotkeys(&self, c: &ViewContext<Self>) -> View {
        section(vec![
        description_text("Click a shortcut to record a new key combination. Use Ctrl, Alt, Shift or Win with another key. Press Esc to cancel."),
        group_title("Switch desktops"),self.shortcut(c,"feature.useHotKeyToJumpToPreviousDesktop","feature.useHotKeyToJumpToPreviousDesktop.hotkey","Most recent desktop","Toggle between the two desktops you used most recently."),self.shortcut(c,"feature.useHotKeyToSwitchDesktopForward","feature.useHotKeyToSwitchDesktopForward.hotkey","Next desktop","Move one desktop to the right."),self.shortcut(c,"feature.useHotKeyToSwitchDesktopBackward","feature.useHotKeyToSwitchDesktopBackward.hotkey","Previous desktop","Move one desktop to the left."),
        group_title("Jump to a desktop number"),self.toggle(c,"feature.useHotKeyToJumpToDesktopNumber","\u{E8EF}","Number shortcuts","Jump directly to desktops 1–9."),self.choice(c,"feature.useHotKeyToJumpToDesktopNumber.hotkey","\u{E765}","Modifier keys","Hold these keys while pressing a desktop number.",&[("Alt","Alt"),("Ctrl + Alt","Ctrl + Alt"),("Ctrl + Shift","Ctrl + Shift"),("Win + Alt","Win + Alt"),("Ctrl + Alt + Shift","Ctrl + Alt + Shift")]),
        self.toggle(c,"feature.useHotKeyToJumpToDesktopNumber.useDigits","\u{E765}","Number row","Use the 1–9 keys above the letters."),self.toggle(c,"feature.useHotKeyToJumpToDesktopNumber.useNumpadDigits","\u{E765}","Number pad","Use the 1–9 keys on the number pad."),self.toggle(c,"feature.useHotKeyToJumpToDesktopNumber.useFunctionKeys","\u{E765}","Function keys","Use F1–F9 to select the corresponding desktop."),
        description_text("Custom hotkeys and actions are preserved. Edit them in Advanced → Configuration editor.")
    ])
    }
    fn advanced(&self, c: &ViewContext<Self>) -> View {
        section(vec![
            self.row(
                "\u{E8B7}",
                "Configuration and logs",
                "Open the folder containing your settings and app.log.",
                "",
                Button::new()
                    .on_click(c.callback(|()| Message::Folder))
                    .content("Open folder"),
            ),
            self.text_setting(
                c,
                "theme.icons.font",
                "Tray font",
                "The font used for desktop numbers.",
            ),
            self.text_setting(
                c,
                "theme.overlay.font",
                "Notification font",
                "The font used for switch notifications.",
            ),
            self.text_setting(
                c,
                "theme.status.font",
                "Status font",
                "The font used for the permanent label.",
            ),
            Expander::new()
                .horizontal_alignment(HorizontalAlignment::Stretch)
                .slots([
                    SlotView::new(
                        ExpanderSlot::Header,
                        StackPanel::new().spacing(4.).children((
                            TextBlock::new().text("Configuration editor"),
                            description_text(
                                "All settings, colors and custom hotkeys. Changes here need Save.",
                            ),
                        )),
                    ),
                    SlotView::new(
                        ExpanderSlot::Content,
                        StackPanel::new().spacing(12.).children((
                            TextBox::new()
                                .text(&self.advanced)
                                .accepts_return(true)
                                .text_wrapping(TextWrapping::NoWrap)
                                .height(350.)
                                .automation_name("Configuration editor")
                                .on_text_changed(c.callback(Message::Advanced)),
                            Button::new()
                                .style(ButtonStyle::Accent)
                                .horizontal_alignment(HorizontalAlignment::Left)
                                .on_click(c.callback(|()| Message::SaveAdvanced))
                                .content("Save configuration"),
                        )),
                    ),
                ]),
        ])
    }
    fn hero(&self) -> View {
        card(self.tray_preview.view())
    }
}
fn card(content: View) -> View {
    Border::new()
        .background(ThemeBrush::CardBackground)
        .border_brush(ThemeBrush::CardStroke)
        .border_thickness(1.)
        .corner_radius(8.)
        .padding(20.)
        .content(content)
}
fn description_text(text: &str) -> View {
    TextBlock::new()
        .text(text)
        .font_size(12.)
        .opacity(0.72)
        .text_wrapping(TextWrapping::Wrap)
        .into()
}
fn group_title(text: &str) -> View {
    TextBlock::new()
        .text(text)
        .font_size(14.)
        .font_weight(FontWeight::SEMI_BOLD)
        .margin(Thickness::new(0., 12., 0., 4.))
        .into()
}
fn section(views: Vec<View>) -> View {
    StackPanel::new()
        .spacing(4.)
        .keyed_children(views.into_iter().enumerate().map(|(i, v)| (i as u64, v)))
}
fn config_text(settings: &Settings) -> String {
    let mut values: BTreeMap<String, String> = DEFAULTS
        .iter()
        .map(|(k, v)| (k.to_string(), v.to_string()))
        .collect();
    values.extend(settings.config.clone());
    values
        .iter()
        .map(|(k, v)| format!("{k}: {}\r\n", crate::settings::quote(v)))
        .collect()
}
pub fn run() -> Result<(), String> {
    let settings = Settings::load()?;
    App::run_component::<SettingsUi>(settings).map_err(|e|format!("WinUI Settings could not start: {e}. Install Microsoft Windows App Runtime 2.4 or later."))
}
