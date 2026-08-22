use std::fs;
use std::io::Write;
use std::path::{Path, PathBuf};
use std::process::Command;
use zip::write::FileOptions;
use zip::{CompressionMethod, ZipWriter};

fn main() {
    let root = Path::new(env!("CARGO_MANIFEST_DIR"))
        .parent()
        .unwrap()
        .to_path_buf();

    for loader in ["BepInEx", "MelonLoader"] {
        for project in &["ComputerInterface", "ComputerInterface.Commands"] {
            if let Err(error) = build_release(&root, project, loader) {
                eprintln!("{}", error);
                std::process::exit(1);
            }
        }

        match zip_release(&root, loader) {
            Ok(path) => eprintln!("{}", path.display()),
            Err(error) => {
                eprintln!("{}", error);
                std::process::exit(1);
            }
        }
    }
}

fn build_release(root: &Path, project: &str, loader: &str) -> Result<(), String> {
    let csproj = root.join(project).join(format!("{project}.csproj"));

    let status = Command::new("dotnet")
        .args(["build", csproj.to_str().unwrap(), "-c", loader])
        .status()
        .map_err(|error| format!("{}", error))?;

    if status.success() {
        Ok(())
    } else {
        Err(format!("Build failed for {} with {}", status, loader))
    }
}

fn zip_release(root: &Path, loader: &str) -> Result<PathBuf, String> {
    let release_dir = root.join("release");
    fs::create_dir_all(&release_dir).map_err(|error| format!("{}", error))?;

    let zip_path = release_dir.join(format!("ComputerInterface-{}.zip", loader));
    let file = fs::File::create(&zip_path).map_err(|error| format!("{}", error))?;

    let mut zip = ZipWriter::new(file);
    let options = FileOptions::<()>::default().compression_method(CompressionMethod::Deflated);

    let subfolder = if loader == "MelonLoader" {
        "Mods"
    } else {
        "ComputerInterface"
    };

    for project in &["ComputerInterface", "ComputerInterface.Commands"] {
        let dll_path = root
            .join(project)
            .join("bin")
            .join(loader)
            .join("netstandard2.1")
            .join(format!("{}.dll", project));
        let bytes = fs::read(&dll_path).map_err(|error| format!("{}", error))?;

        let entry_name = format!("{}/{}.{}.dll", subfolder, project, loader);
        zip.start_file(&entry_name, options)
            .map_err(|error| format!("{}", error))?;
        zip.write_all(&bytes)
            .map_err(|error| format!("{}", error))?;
    }

    zip.finish().map_err(|error| format!("{}", error))?;
    Ok(zip_path)
}
