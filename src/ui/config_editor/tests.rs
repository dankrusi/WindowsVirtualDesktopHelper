//! Exercise the real Reactor reconciliation engine without opening desktop UI.
use super::*;
use std::{
    collections::BTreeMap,
    fs,
    time::{SystemTime, UNIX_EPOCH},
};
use windows_reactor::test::*;

struct HeadlessEditor(Editor);
impl Component for HeadlessEditor {
    type Input = Input;
    type Message = Message;
    fn create(input: &Input, _: &ComponentContext<Self>) -> Self {
        let mut editor = Editor::new(input);
        // The native window hook and two-phase multiline mount are not needed
        // by RecordingRuntime. Draft/edit/save logic and TextBox are production code.
        editor.ready = true;
        Self(editor)
    }
    fn update(&mut self, message: Message, _: &ComponentContext<Self>) {
        match message {
            Message::Edit(text) => self.0.edit(text),
            Message::Save => {
                assert!(self.0.save(), "{}", self.0.error);
            }
            _ => {}
        }
    }
    fn view(&self, _: &Input, context: &mut ViewContext<Self>) -> View {
        StackPanel::new().children((
            draft_textbox(&self.0.document, self.0.ready)
                .on_text_changed(context.callback(Message::Edit)),
            Button::new()
                .is_enabled(self.0.document.dirty())
                .on_click(context.callback(|()| Message::Save))
                .content("Save"),
        ))
    }
}

#[test]
fn typing_does_not_reset_native_text_or_save_until_the_save_button() {
    let path = std::env::temp_dir().join(format!(
        "wvdh-editor-{}-{}.config",
        std::process::id(),
        SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap()
            .as_nanos()
    ));
    let settings = Settings {
        config: BTreeMap::new(),
        arguments: BTreeMap::new(),
        path: path.clone(),
    };
    settings.save().unwrap();
    let original_file = fs::read(&path).unwrap();
    let saves = Rc::new(Cell::new(0));
    let captured_saves = saves.clone();
    let input = Input {
        session: Session::new(&settings),
        parent: 0,
        saved: Callback::new(move |_: Settings| captured_saves.set(captured_saves.get() + 1)),
    };
    let mut pump = Pump::new(RecordingRuntime::default());
    pump.mount_view(View::component::<HeadlessEditor>(input))
        .unwrap();
    let textbox = property_node(&pump, PropertyId::TextBoxText);
    let save = property_node(&pump, PropertyId::ButtonIsEnabled);
    let original_text = match pump
        .runtime()
        .node(textbox)
        .unwrap()
        .property(PropertyId::TextBoxText)
        .unwrap()
    {
        PropertyValue::Str(text) => text.clone(),
        _ => panic!("Expected text"),
    };
    for text in [
        "theme.overlay.width: 6".into(),
        "theme.overlay.width: 60".into(),
        "theme.overlay.width: 600\r\ncustom.note: \"Work: \u{1f980}\"\r\n".into(),
        "theme.overlay.width: 600\rcustom.note: \"Work\"\r".into(),
        original_text, // Native undo back to the original document.
        "theme.overlay.width: 600\ncustom.note: \"Work\"\n".into(),
    ] {
        let batch = pump.runtime().commands().len();
        pump.runtime_mut()
            .record_property_observation(
                textbox,
                PropertyId::TextBoxText,
                PropertyValue::Str(text.clone()),
            )
            .unwrap();
        queue(
            &mut pump,
            textbox,
            EventId::TextBoxTextChanged,
            EventPayload::Str(text.clone()),
        );
        assert_eq!(
            pump.runtime()
                .node(textbox)
                .unwrap()
                .property(PropertyId::TextBoxText),
            Some(&PropertyValue::Str(text))
        );
        assert!(
            pump.runtime().commands()[batch..]
                .iter()
                .flatten()
                .all(|command| !matches!(
                    command,
                    Command::SetProperty {
                        property: PropertyId::TextBoxText,
                        ..
                    } | Command::ClearProperty {
                        property: PropertyId::TextBoxText,
                        ..
                    }
                )),
            "Editing must not rewrite the native Text property"
        );
        assert_eq!(
            fs::read(&path).unwrap(),
            original_file,
            "Typing must not save"
        );
        assert_eq!(saves.get(), 0);
    }
    queue(&mut pump, save, EventId::ButtonClick, EventPayload::Unit);
    assert_eq!(saves.get(), 1);
    let saved = crate::settings::parse(&fs::read_to_string(&path).unwrap()).unwrap();
    assert_eq!(saved["theme.overlay.width"], "600");
    assert_eq!(saved["custom.note"], "Work");
    fs::remove_file(path).unwrap();
}
fn property_node(pump: &Pump<RecordingRuntime>, property: PropertyId) -> NodeId {
    pump.runtime()
        .commands()
        .iter()
        .flatten()
        .find_map(|command| match command {
            Command::SetProperty {
                node,
                property: found,
                ..
            } if *found == property => Some(*node),
            _ => None,
        })
        .expect("Expected mounted control")
}
fn queue(pump: &mut Pump<RecordingRuntime>, node: NodeId, event: EventId, payload: EventPayload) {
    let revision = pump.event_revision(node, event).unwrap();
    pump.queue_event(QueuedEvent::new(node, event, revision, payload));
    pump.dispatch_events().unwrap();
    pump.dispatch_components(32).unwrap();
}
