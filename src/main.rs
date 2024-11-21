use notan::{log::info, prelude::*};

mod jailmaker;

pub struct AppConfig {
    pub vsync: bool,
    pub maximized: bool,
    pub resolution: (u32, u32),
    pub fullscreen: bool,
    pub high_dpi: bool
}

#[notan_main]
fn main() -> Result<(), String> {
    let config = load_config();
    let result = jailmaker::init(config);

    info!("Shutting Down Jailmaker.");

    result
}

fn load_config() -> AppConfig {
    AppConfig { 
        vsync: true, 
        maximized: false,
        resolution: ((1920.0 / 1.3) as u32, (1080.0 / 1.3) as u32),
        fullscreen: false,
        high_dpi: true
    }
}