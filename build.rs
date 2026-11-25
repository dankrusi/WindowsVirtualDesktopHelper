#[path = "scripts/localization_build.rs"]
mod localization;
fn main() {
    localization::generate();
    println!("cargo:rerun-if-changed=assets/app.manifest");
    if std::env::var("CARGO_CFG_TARGET_ENV").as_deref() == Ok("msvc") {
        println!("cargo:rerun-if-changed=assets/app.rc");
        println!("cargo:rerun-if-changed=assets/app.ico");
        embed_resource::compile_for(
            "assets/app.rc",
            ["WindowsVirtualDesktopHelper"],
            embed_resource::NONE,
        )
        .manifest_required()
        .expect("compile the Windows application icon");
        let manifest = std::path::Path::new(&std::env::var("CARGO_MANIFEST_DIR").unwrap())
            .join("assets/app.manifest");
        println!("cargo:rustc-link-arg-bin=WindowsVirtualDesktopHelper=/MANIFEST:EMBED");
        println!(
            "cargo:rustc-link-arg-bin=WindowsVirtualDesktopHelper=/MANIFESTINPUT:{}",
            manifest.display()
        );
    }
}
