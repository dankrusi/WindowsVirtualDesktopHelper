//! Actual WinUI 3 / Fluent controls, hosted only while Settings is open.
use crate::{
    platform::registry,
    settings::{Settings, DEFAULTS},
};
use std::collections::BTreeMap;
use windows_reactor::*;
use windows_sys::Win32::Foundation::HWND;

const PAGES: [(&str, &str, &str); 6] = [
    ("general", "General", "\u{E713}"),
    ("tray", "Tray icons", "\u{E9D9}"),
    ("overlays", "Desktop overlays", "\u{E7F4}"),
    ("hotkeys", "Keyboard shortcuts", "\u{E765}"),
    ("advanced", "Advanced", "\u{E90F}"),
    ("about", "About", "\u{E946}"),
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
    Flush(u64),
    SetMany(Vec<(&'static str, String)>),
    Reset(Vec<String>),
    Link(&'static str),
    Log,
    Preview(bool),
    SchedulePreview(u64),
    AutoPreview(u64),
    OpenEditor,
    EditorSaved(Settings),
    Folder,
    Startup,
    Dismiss,
    Capture(&'static str),
    Captured(Option<String>),
    Size(WindowSize),
    Pane(bool),
    Caption,
    RefreshTray(super::tray_preview::Refresh),
}
struct SettingsUi {
    sender: LocalSender<Message>,
    settings: Settings,
    parent: usize,
    page: String,
    error: String,
    edits: BTreeMap<String, String>,
    editor: super::config_editor::Session,
    pending: Option<ComponentTask>,
    generation: u64,
    pending_preview: Option<ComponentTask>,
    preview_generation: u64,
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
            sender: context.sender(),
            tray_preview: super::tray_preview::Preview::new(input, move |reconnect| {
                sender.send(match reconnect {
                    Some(reconnect) => Message::RefreshTray(reconnect),
                    None => Message::Page(Some("about".into())),
                });
            }),
            settings: input.clone(),
            parent: input.get("ui-parent").parse().unwrap_or(0),
            page: if input.get("ui-page") == "about" {
                "about"
            } else {
                "general"
            }
            .into(),
            error: String::new(),
            edits: BTreeMap::new(),
            editor: super::config_editor::Session::new(input),
            pending: None,
            generation: 0,
            pending_preview: None,
            preview_generation: 0,
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
                self.flush();
                self.tray_preview
                    .refresh(&self.settings, super::tray_preview::Refresh::Change);
                self.page = page;
                if self.narrow {
                    self.pane_open = false;
                }
                self.capture = None;
                self.capture_key = None;
            }
            Message::Page(None) => {}
            Message::Set(key, value) => {
                self.flush();
                if self.settings.get(key) == value {
                    return;
                }
                let mut candidate = self.settings.clone();
                candidate.set(key, value);
                self.commit(candidate);
            }
            Message::Reset(keys) => {
                self.flush();
                if !self.edits.is_empty() {
                    return;
                }
                let mut candidate = self.settings.clone();
                let result = candidate
                    .reset(keys.iter().map(String::as_str))
                    .and_then(|()| {
                        super::settings_process::reset(&candidate, self.parent as HWND, &keys)
                    });
                match result {
                    Ok(()) => self.accept(candidate),
                    Err(error) => self.error = error,
                }
            }
            Message::SetMany(values) => {
                if values
                    .iter()
                    .all(|(key, value)| self.settings.get(key) == value)
                {
                    return;
                }
                self.flush();
                let mut candidate = self.settings.clone();
                for (key, value) in values {
                    candidate.set(key, value);
                }
                self.commit(candidate);
            }
            Message::Edit(key, value) => {
                if value == self.settings.get(&key) && !self.edits.contains_key(&key) {
                    return;
                }
                self.edits.insert(key, value);
                self.schedule(context);
            }
            Message::Flush(generation) => {
                if generation == self.generation {
                    self.flush();
                }
            }
            Message::Preview(status) => {
                self.cancel_auto_preview();
                self.preview.show(
                    &self.settings,
                    self.tray_preview.desktop_name(),
                    status,
                    Some(2200),
                );
            }
            Message::SchedulePreview(generation) => {
                if generation == self.preview_generation
                    && self.settings.bool("feature.showDesktopSwitchOverlay")
                {
                    self.pending_preview = Some(context.spawn_background(move |_| {
                        std::thread::sleep(std::time::Duration::from_millis(500));
                        Message::AutoPreview(generation)
                    }));
                }
            }
            Message::AutoPreview(generation) => {
                if generation == self.preview_generation
                    && self.settings.bool("feature.showDesktopSwitchOverlay")
                {
                    self.pending_preview = None;
                    let duration = self
                        .settings
                        .int("feature.showDesktopSwitchOverlay.duration");
                    self.preview.show(
                        &self.settings,
                        self.tray_preview.desktop_name(),
                        false,
                        Some(if duration == 0 { 2200 } else { duration as u32 }),
                    );
                }
            }
            Message::OpenEditor => {
                self.flush();
                if !self.edits.is_empty() || !self.editor.begin_open() {
                    return;
                }
                let input = super::config_editor::Input {
                    session: self.editor.clone(),
                    parent: self.parent,
                    saved: context.sender().callback(Message::EditorSaved),
                };
                if !context.open_window(View::component::<super::config_editor::Editor>(input)) {
                    self.editor.closed();
                    self.error = "Couldn't open the configuration editor. Try again.".into();
                }
            }
            Message::EditorSaved(settings) => self.accept(settings),
            Message::Link(url) => unsafe {
                super::open_url(std::ptr::null_mut(), url);
            },
            Message::Log => {
                let path = self.settings.path.with_file_name("app.log");
                if path.exists() {
                    unsafe {
                        super::open_url(std::ptr::null_mut(), &path.to_string_lossy());
                    }
                } else {
                    self.error = "No log has been created yet.".into();
                }
            }
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
            "about" => "A little more clarity for your Windows desktops.",
            _ => "Make your virtual desktops feel like home.",
        };
        let body = match self.page.as_str() {
            "tray" => self.tray(context),
            "overlays" => self.overlays(context),
            "hotkeys" => self.hotkeys(context),
            "advanced" => self.advanced(context),
            "about" => self.about(context),
            _ => self.general(context),
        };
        let content = ScrollViewer::new()
            .horizontal_scroll_bar_visibility(ScrollBarVisibility::Disabled)
            .content(
                StackPanel::new()
                    .spacing(20.)
                    .margin(Thickness::new(28., 20., 32., 32.))
                    .max_width(920.)
                    .horizontal_alignment(HorizontalAlignment::Stretch)
                    .children((
                        StackPanel::new().spacing(8.).children((
                            TextBlock::new()
                                .text(title)
                                .font_size(28.)
                                .font_weight(FontWeight::SEMI_BOLD),
                            description_text(description),
                        )),
                        InfoBar::new()
                            .is_open(!self.error.is_empty())
                            .severity(InfoBarSeverity::Error)
                            .title("Change wasn't saved")
                            .message(&self.error)
                            .on_closed(context.callback(|()| Message::Dismiss)),
                        body,
                    )),
            );
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
                self.accept(candidate);
                true
            }
            Err(e) => {
                self.error = e;
                false
            }
        }
    }
    fn accept(&mut self, candidate: Settings) {
        if switch_preview_changed(&self.settings, &candidate) {
            self.cancel_auto_preview();
            self.preview.clear();
            self.sender
                .send(Message::SchedulePreview(self.preview_generation));
        }
        self.settings = candidate;
        self.tray_preview.render(&self.settings);
        super::settings_process::update_caption(&self.settings);
        self.editor.update(&self.settings);
        self.error.clear();
    }
    fn cancel_auto_preview(&mut self) {
        self.preview_generation += 1;
        if let Some(task) = self.pending_preview.take() {
            task.cancel();
        }
    }
    fn schedule(&mut self, context: &ComponentContext<Self>) {
        if let Some(task) = self.pending.take() {
            task.cancel();
        }
        self.generation += 1;
        let generation = self.generation;
        self.pending = Some(context.spawn_background(move |_| {
            std::thread::sleep(std::time::Duration::from_millis(600));
            Message::Flush(generation)
        }));
    }
    fn flush(&mut self) {
        if let Some(task) = self.pending.take() {
            task.cancel();
        }
        if self.edits.is_empty() {
            return;
        }
        let mut candidate = self.settings.clone();
        for (key, value) in &self.edits {
            candidate.set(key, value.clone());
        }
        if self.commit(candidate) {
            self.edits.clear();
        }
    }
    fn enabled(&self, key: &str) -> bool {
        !self.settings.arguments.contains_key(key) && self.active(key)
    }
    fn active(&self, key: &str) -> bool {
        if key == "feature.showDesktopNumberInIconTray.clickToOpenTaskView" {
            return self.settings.bool("feature.showDesktopNumberInIconTray")
                || self.settings.bool("feature.showDesktopNameInIconTray");
        }
        let parent = if key.starts_with("theme.overlay.") {
            Some("feature.showDesktopSwitchOverlay")
        } else if key.starts_with("theme.status.") {
            Some("feature.showDesktopStatusOverlay")
        } else {
            [
                "feature.showPrevNextIcons",
                "feature.showDesktopSwitchOverlay",
                "feature.showDesktopStatusOverlay",
                "feature.useHotKeyToJumpToDesktopNumber",
                "feature.useHotKeyToJumpToPreviousDesktop",
                "feature.useHotKeyToSwitchDesktopForward",
                "feature.useHotKeyToSwitchDesktopBackward",
            ]
            .into_iter()
            .find(|p| key.strip_prefix(p).is_some_and(|s| s.starts_with('.')))
        };
        parent.is_none_or(|key| self.settings.bool(key))
    }
    fn managed(&self, keys: &[&str], supported: bool, control: View) -> View {
        let overridden = keys
            .iter()
            .any(|key| self.settings.arguments.contains_key(*key));
        if !overridden && supported {
            return control;
        }
        let values = keys
            .iter()
            .map(|key| format!("{key}: {}", self.settings.get(key)))
            .collect::<Vec<_>>()
            .join("\n");
        let reset_keys = keys
            .iter()
            .map(|key| (*key).to_string())
            .collect::<Vec<_>>();
        StackPanel::new().spacing(4.).children((
            control,
            Border::new().corner_radius(8.).background(ThemeBrush::CardBackground).content(
                Grid::new().columns([GridLength::STAR, GridLength::Auto]).children((
                    InfoBar::new().grid_column(0).is_open(true).is_closable(false).severity(InfoBarSeverity::Informational)
                        .title(if overridden { "Set by command-line options" } else { "Custom configuration" })
                        .message(format!("{}\n{values}", if overridden { "Reset restores the default for this session. Launch options apply again after restarting." } else { "Edit this value in Advanced, or reset it to use the options here." })),
                    Button::new().grid_column(1).margin(16.).vertical_alignment(VerticalAlignment::Center)
                        .automation_name("Reset to default").on_click(self.sender.callback(move |()| Message::Reset(reset_keys.clone())))
                        .content("Reset")
                ))
            )
        ))
    }
    fn row(&self, icon: &str, title: &str, description: &str, key: &str, control: View) -> View {
        let is_toggle = DEFAULTS
            .iter()
            .any(|(name, value)| *name == key && (*value == "true" || *value == "false"));
        let stacked = self.narrow && !is_toggle;
        let controls = Border::new()
            .grid_column(if stacked { 1 } else { 2 })
            .grid_row(i32::from(stacked))
            .margin(if stacked {
                Thickness::new(0., 12., 0., 0.)
            } else {
                Thickness::new(16., 0., 0., 0.)
            })
            .vertical_alignment(VerticalAlignment::Center)
            .horizontal_alignment(if stacked {
                HorizontalAlignment::Left
            } else {
                HorizontalAlignment::Right
            })
            .content(control);
        self.managed(
            &[key],
            true,
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
                                description_text(description),
                            )),
                        controls,
                    )),
            ),
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
            switch()
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
        let selected = choices
            .iter()
            .position(|(value, _)| *value == self.settings.get(key));
        self.managed(
            &[key],
            selected.is_some(),
            self.row(
                icon,
                title,
                description,
                "",
                ComboBox::new()
                    .width(210.)
                    .items_source(choices.iter().map(|(_, label)| *label))
                    .selected_index(selected)
                    .placeholder_text("Custom")
                    .is_enabled(self.enabled(key) && selected.is_some())
                    .automation_name(title)
                    .on_selection_changed(c.callback(move |index: Option<usize>| {
                        index
                            .and_then(|i| choices.get(i))
                            .map(|(value, _)| Message::Set(key, (*value).into()))
                            .unwrap_or(Message::Page(None))
                    }))
                    .into(),
            ),
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
            TextBox::new()
                .width(210.)
                .text(value)
                .is_enabled(self.enabled(key))
                .automation_name(title)
                .on_text_changed(c.callback(move |value| Message::Edit(key.into(), value)))
                .into(),
        )
    }
    fn presets(&self, c: &ViewContext<Self>, keys: &'static [&'static str], title: &str) -> View {
        let selected = crate::settings::preset_index(&self.settings, keys);
        self.managed(
            keys,
            selected.is_some(),
            self.row(
                "\u{E8A4}",
                title,
                "",
                "",
                ComboBox::new()
                    .width(210.)
                    .items_source(["Small", "Medium", "Large"])
                    .selected_index(selected)
                    .placeholder_text("Custom")
                    .is_enabled(selected.is_some() && keys.iter().all(|key| self.enabled(key)))
                    .automation_name(title)
                    .on_selection_changed(c.callback(move |index: Option<usize>| {
                        index
                            .and_then(|i| crate::settings::SIZES.get(i))
                            .map(|value| {
                                Message::SetMany(
                                    keys.iter().map(|key| (*key, (*value).into())).collect(),
                                )
                            })
                            .unwrap_or(Message::Page(None))
                    }))
                    .into(),
            ),
        )
    }
    fn overlay_size(
        &self,
        c: &ViewContext<Self>,
        dimensions: &'static [&'static str],
        custom_key: &'static str,
    ) -> View {
        let keys = [dimensions[0], dimensions[1], custom_key];
        let selected = if self.settings.bool(custom_key) {
            3
        } else {
            crate::settings::preset_index(&self.settings, dimensions).unwrap_or(3)
        };
        let numeric = dimensions
            .iter()
            .map(|key| (*key, format!("{:.0}", self.settings.number(key))))
            .collect::<Vec<_>>();
        let picker = ComboBox::new()
            .width(if selected == 3 { 140. } else { 210. })
            .items_source(["Small", "Medium", "Large", "Custom"])
            .selected_index(Some(selected))
            .is_enabled(keys.iter().all(|key| self.enabled(key)))
            .automation_name("Size")
            .on_selection_changed(c.callback(move |index: Option<usize>| {
                let Some(index) = index.filter(|i| *i != selected && *i <= 3) else {
                    return Message::Page(None);
                };
                let mut values = if index == 3 {
                    numeric.clone()
                } else {
                    dimensions
                        .iter()
                        .map(|key| (*key, crate::settings::SIZES[index].into()))
                        .collect()
                };
                values.push((custom_key, (index == 3).to_string()));
                Message::SetMany(values)
            }));
        let mut controls = vec![picker.into()];
        if selected == 3 {
            controls.push(
                Button::new()
                    .width(62.)
                    .is_enabled(keys.iter().all(|key| self.enabled(key)))
                    .automation_name("Reset size")
                    .on_click(c.callback(move |()| {
                        Message::Reset(keys.iter().map(|key| (*key).into()).collect())
                    }))
                    .content("Reset"),
            );
        }
        let mut rows = vec![self.managed(
            &keys,
            true,
            self.row(
                "\u{E8A4}",
                "Size",
                "",
                "",
                StackPanel::new()
                    .orientation(Orientation::Horizontal)
                    .spacing(8.)
                    .keyed_children(controls.into_iter().enumerate().map(|(i, v)| (i as u64, v))),
            ),
        )];
        if selected == 3 {
            for (key, title, maximum) in [
                (dimensions[0], "Width", 1920_f64),
                (dimensions[1], "Height", 1080_f64),
            ] {
                let value = self.settings.number(key);
                let enabled = self.enabled(key) && self.enabled(custom_key);
                rows.push(
                    self.row(
                        "\u{E8A4}",
                        title,
                        "",
                        key,
                        StackPanel::new().width(210.).spacing(4.).children((
                            TextBlock::new()
                                .text(format!("{value:.0} px"))
                                .opacity(if enabled { 1. } else { 0.4 })
                                .horizontal_alignment(HorizontalAlignment::Right),
                            Slider::new()
                                .minimum(1.)
                                .maximum(maximum.max(value))
                                .step_frequency(1.)
                                .value(value)
                                .is_enabled(enabled)
                                .automation_name(title)
                                .on_value_changed(c.callback(move |value| {
                                    Message::SetMany(vec![
                                        (key, format!("{value:.0}")),
                                        (custom_key, "true".into()),
                                    ])
                                })),
                        )),
                    ),
                );
            }
        }
        StackPanel::new()
            .spacing(4.)
            .keyed_children(rows.into_iter().enumerate().map(|(i, v)| (i as u64, v)))
    }
    fn distance(&self, c: &ViewContext<Self>, key: &'static str, position: &str) -> View {
        let value = self.settings.number(key);
        let supported = (0. ..=200.).contains(&value);
        let relevant = self.settings.get(position) != "middlecenter";
        let enabled = self.enabled(key) && supported && relevant;
        self.managed(
            &[key],
            supported,
            self.row(
                "\u{E8A9}",
                "Distance from screen edge",
                if relevant {
                    ""
                } else {
                    "Not used with the Center position."
                },
                "",
                StackPanel::new().width(210.).spacing(4.).children((
                    TextBlock::new()
                        .text(format!("{value:.0} px"))
                        .opacity(if enabled { 1. } else { 0.4 })
                        .horizontal_alignment(HorizontalAlignment::Right),
                    Slider::new()
                        .minimum(0.)
                        .maximum(200.)
                        .step_frequency(1.)
                        .value(value.clamp(0., 200.))
                        .is_enabled(enabled)
                        .automation_name("Distance from screen edge")
                        .on_value_changed(
                            c.callback(move |value| Message::Set(key, format!("{value:.0}"))),
                        ),
                )),
            ),
        )
    }
    fn symbols(&self, c: &ViewContext<Self>) -> View {
        const KEYS: [&str; 2] = [
            "feature.showPrevNextIcons.prevChar",
            "feature.showPrevNextIcons.nextChar",
        ];
        const PAIRS: [(&str, &str, &str); 5] = [
            ("\u{2039}", "\u{203a}", "\u{2039}  \u{203a}    Chevrons"),
            ("\u{2190}", "\u{2192}", "\u{2190}  \u{2192}    Arrows"),
            ("\u{25c0}", "\u{25b6}", "\u{25c0}  \u{25b6}    Triangles"),
            (
                "\u{00ab}",
                "\u{00bb}",
                "\u{00ab}  \u{00bb}    Double chevrons",
            ),
            ("<", ">", "<  >    Angles"),
        ];
        let selected = PAIRS.iter().position(|(prev, next, _)| {
            self.settings.get(KEYS[0]) == *prev && self.settings.get(KEYS[1]) == *next
        });
        self.managed(
            &KEYS,
            selected.is_some(),
            self.row(
                "\u{E8AB}",
                "Previous/next symbol",
                "Choose a matching pair of navigation symbols.",
                "",
                ComboBox::new()
                    .width(210.)
                    .items_source(PAIRS.iter().map(|p| p.2))
                    .selected_index(selected)
                    .placeholder_text("Custom")
                    .is_enabled(selected.is_some() && KEYS.iter().all(|key| self.enabled(key)))
                    .automation_name("Previous/next symbol")
                    .on_selection_changed(c.callback(|index: Option<usize>| {
                        index
                            .and_then(|i| PAIRS.get(i))
                            .map(|(prev, next, _)| {
                                Message::SetMany(vec![
                                    (KEYS[0], (*prev).into()),
                                    (KEYS[1], (*next).into()),
                                ])
                            })
                            .unwrap_or(Message::Page(None))
                    }))
                    .into(),
            ),
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
            card(self.tray_preview.desktop_view(&self.settings)),
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
            self.symbols(c),
        ])
    }
    fn overlays(&self, c: &ViewContext<Self>) -> View {
        let mut rows = vec![card(self.tray_preview.desktop_view(&self.settings))];
        for status in [false, true] {
            let (
                title,
                feature,
                position,
                all_monitors,
                animate,
                translucent,
                border,
                corners,
                dimensions,
                text,
                offset,
            ): (
                _,
                _,
                _,
                _,
                _,
                _,
                _,
                _,
                &'static [&'static str],
                &'static [&'static str],
                _,
            ) = if status {
                (
                    "Always-visible desktop name",
                    "feature.showDesktopStatusOverlay",
                    "feature.showDesktopStatusOverlay.position",
                    "feature.showDesktopStatusOverlay.showOnAllMonitors",
                    "feature.showDesktopStatusOverlay.animate",
                    "feature.showDesktopStatusOverlay.translucent",
                    "theme.status.border",
                    "theme.status.roundedCorners",
                    &["theme.status.width", "theme.status.height"],
                    &["theme.status.fontSize"],
                    "theme.status.offset",
                )
            } else {
                (
                    "When switching desktops",
                    "feature.showDesktopSwitchOverlay",
                    "feature.showDesktopSwitchOverlay.position",
                    "feature.showDesktopSwitchOverlay.showOnAllMonitors",
                    "feature.showDesktopSwitchOverlay.animate",
                    "feature.showDesktopSwitchOverlay.translucent",
                    "theme.overlay.border",
                    "theme.overlay.roundedCorners",
                    &["theme.overlay.width", "theme.overlay.height"],
                    &["theme.overlay.fontSize"],
                    "theme.overlay.offset",
                )
            };
            rows.extend([
                group_title(title),
                self.toggle(
                    c,
                    feature,
                    "\u{E7F4}",
                    if status {
                        "Show desktop name"
                    } else {
                        "Show desktop notification"
                    },
                    if status {
                        "Keep the current desktop name visible."
                    } else {
                        "Briefly show the desktop name after switching."
                    },
                ),
                self.overlay_size(
                    c,
                    dimensions,
                    if status {
                        "theme.status.customSize"
                    } else {
                        "theme.overlay.customSize"
                    },
                ),
                self.presets(c, text, "Text size"),
                self.choice(c, position, "\u{E8A9}", "Position", "", &POS),
                self.distance(c, offset, position),
                self.toggle(
                    c,
                    border,
                    "\u{E739}",
                    "Border",
                    "Outline the desktop label.",
                ),
                self.toggle(
                    c,
                    corners,
                    "\u{E7C1}",
                    "Rounded corners",
                    "Soften the edges of the label.",
                ),
                self.toggle(
                    c,
                    translucent,
                    "\u{E790}",
                    "Translucent",
                    "Let the desktop show through.",
                ),
                self.toggle(
                    c,
                    animate,
                    "\u{E768}",
                    "Animation",
                    "Fade the label in smoothly.",
                ),
                self.toggle(
                    c,
                    all_monitors,
                    "\u{E7F4}",
                    "Show on every monitor",
                    "Use the same appearance across your screens.",
                ),
            ]);
            if !status {
                rows.push(self.number(
                    c,
                    "feature.showDesktopSwitchOverlay.duration",
                    "Display time",
                    "Milliseconds. Use 0 to keep visible until the next switch.",
                    0.,
                    60000.,
                ));
            }
            rows.push(
                self.row(
                    "\u{E768}",
                    "Preview on desktop",
                    "Try the current appearance and placement.",
                    "",
                    Button::new()
                        .is_enabled(self.settings.bool(feature))
                        .on_click(c.callback(move |()| Message::Preview(status)))
                        .content("Preview"),
                ),
            );
        }
        section(rows)
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
                switch()
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
            self.row(
                "\u{E943}",
                "Configuration editor",
                "Edit all settings, colors and custom hotkeys in a separate window.",
                "",
                Button::new()
                    .on_click(c.callback(|()| Message::OpenEditor))
                    .content("Open editor"),
            ),
        ])
    }
    fn about(&self, c: &ViewContext<Self>) -> View {
        let link = |title: &str, url: &'static str| {
            Button::new()
                .on_click(c.callback(move |()| Message::Link(url)))
                .content(title)
        };
        section(vec![
            card(StackPanel::new().spacing(16.).children((
                Border::new().width(80.).height(80.).corner_radius(16.).background(ThemeBrush::Accent).horizontal_alignment(HorizontalAlignment::Left)
                    .content(Canvas::new().width(48.).height(40.).children((
                        Border::new().canvas_left(0.).canvas_top(0.).width(36.).height(28.).corner_radius(3.).border_brush(Color::rgb(255,255,255)).border_thickness(2.),
                        Border::new().canvas_left(12.).canvas_top(12.).width(36.).height(28.).corner_radius(3.).background(ThemeBrush::Accent).border_brush(Color::rgb(255,255,255)).border_thickness(2.)
                    ))),
                TextBlock::new().text("Windows Virtual Desktop Helper").font_size(24.).font_weight(FontWeight::SEMI_BOLD).text_wrapping(TextWrapping::Wrap),
                description_text(&format!("Version {}  \u{00b7}  by Dan Krusi and contributors", env!("CARGO_PKG_VERSION"))),
                TextBlock::new().text("Know which desktop you're on. Move between workspaces with tray icons, desktop labels and keyboard shortcuts.").text_wrapping(TextWrapping::Wrap),
                StackPanel::new().orientation(Orientation::Horizontal).spacing(8.).children((link("View on GitHub", "https://github.com/dankrusi/WindowsVirtualDesktopHelper"), link("Donate", "https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6")))
            ))),
            group_title("Support and diagnostics"),
            self.row("\u{E119}", "Contact", "dan@dankrusi.com", "", link("Email Dan", "mailto:dan@dankrusi.com")),
            self.row("\u{E8A5}", "Report an issue", "Share a bug report or suggest an improvement.", "", link("GitHub issues", "https://github.com/dankrusi/WindowsVirtualDesktopHelper/issues")),
            self.row("\u{E9D9}", "Application log", "View recent activity and connection errors.", "", Button::new().on_click(c.callback(|()| Message::Log)).content("Show log")),
            self.row("\u{E713}", "Configuration", "Inspect settings, colors and custom shortcuts.", "", Button::new().on_click(c.callback(|()| Message::Page(Some("advanced".into())))).content("Show configuration")),
            group_title("Open source"),
            card(StackPanel::new().spacing(8.).children((
                TextBlock::new().text("Made with Rust and Microsoft WinUI 3").font_weight(FontWeight::SEMI_BOLD),
                description_text("GPL-3.0. VirtualDesktopAPI: MIT. Windows bindings and windows-reactor: MIT / Apache-2.0. rustc-hash: Apache-2.0 / MIT. uuid: MIT. sha1_smol: BSD-3-Clause."),
                link("License and source", "https://github.com/dankrusi/WindowsVirtualDesktopHelper")
            )))
        ])
    }
    fn hero(&self) -> View {
        card(self.tray_preview.view())
    }
}
impl Drop for SettingsUi {
    fn drop(&mut self) {
        self.cancel_auto_preview();
        self.flush();
    }
}
fn switch_preview_changed(before: &Settings, after: &Settings) -> bool {
    DEFAULTS.iter().any(|(key, _)| {
        (*key == "feature.showDesktopSwitchOverlay"
            || key.starts_with("feature.showDesktopSwitchOverlay.")
            || (key.starts_with("theme.overlay.") && *key != "theme.overlay.customSize"))
            && before.get(key) != after.get(key)
    })
}
fn switch() -> ToggleSwitch {
    // Constrain WinUI's default On/Off content area to the actual switch track.
    ToggleSwitch::new()
        .min_width(40.)
        .width(40.)
        .horizontal_alignment(HorizontalAlignment::Right)
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
pub fn run() -> Result<(), String> {
    let settings = Settings::load()?;
    App::run_component::<SettingsUi>(settings).map_err(|e|format!("WinUI Settings could not start: {e}. Install Microsoft Windows App Runtime 2.4 or later."))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn automatic_preview_tracks_effective_switch_settings_only() {
        let before = Settings {
            config: BTreeMap::new(),
            arguments: BTreeMap::new(),
            path: Default::default(),
        };
        for (key, value, expected) in [
            ("theme.overlay.width", "600", true),
            ("theme.overlay.roundedCorners", "false", true),
            (
                "feature.showDesktopSwitchOverlay.position",
                "topcenter",
                true,
            ),
            ("feature.showDesktopSwitchOverlay.duration", "4000", true),
            ("feature.showDesktopSwitchOverlay", "false", true),
            ("theme.overlay.customSize", "true", false),
            ("theme.status.width", "600", false),
            ("feature.showDesktopStatusOverlay", "true", false),
            ("theme.overlay.width", "480", false),
        ] {
            let mut after = before.clone();
            after.set(key, value);
            assert_eq!(switch_preview_changed(&before, &after), expected, "{key}");
        }
        let mut overridden = before.clone();
        overridden
            .arguments
            .insert("theme.overlay.width".into(), "800".into());
        let mut after = overridden.clone();
        after.set("theme.overlay.width", "600");
        assert!(!switch_preview_changed(&overridden, &after));
    }
}
