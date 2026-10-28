fn main() {
    println!("cargo:rerun-if-changed=assets/app.manifest");
    if std::env::var("CARGO_CFG_TARGET_ENV").as_deref() == Ok("msvc") {
        let manifest = std::path::Path::new(&std::env::var("CARGO_MANIFEST_DIR").unwrap())
            .join("assets/app.manifest");
        println!("cargo:rustc-link-arg-bin=WindowsVirtualDesktopHelper=/MANIFEST:EMBED");
        println!(
            "cargo:rustc-link-arg-bin=WindowsVirtualDesktopHelper=/MANIFESTINPUT:{}",
            manifest.display()
        );
    }
}
