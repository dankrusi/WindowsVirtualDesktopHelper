#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
mod app;
mod hotkeys;
mod platform;
mod settings;
mod ui;

fn main() {
    if let Err(error) = app::run() {
        platform::report(&error);
        std::process::exit(1);
    }
}
