#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
mod app;
mod hotkeys;
mod platform;
mod settings;
mod ui;

fn main() {
    let result = if std::env::args().any(|a| a == "--settings-ui") {
        ui::settings::run()
    } else {
        app::run()
    };
    if let Err(error) = result {
        platform::report(&error);
        std::process::exit(1);
    }
}
