use std::{collections::HashMap, process::exit};

use notan_egui::{self, *};
use notan::prelude::*;

use crate::AppConfig;

pub fn init(config: AppConfig) -> Result<(), String> {
    let icon_data = include_bytes!("../assets/icons/jailmaker.png");

    let window_cfg = WindowConfig::new()
        .set_title("Jailmaker")
        .set_size(config.resolution.0, config.resolution.1)
        .set_maximized(config.maximized)
        .set_fullscreen(config.fullscreen)
        .set_window_icon_data(Some(icon_data))
        .set_taskbar_icon_data(Some(icon_data))
        .set_vsync(config.vsync)
        .set_high_dpi(config.high_dpi);
    
    notan::init_with(State::new)
        .add_config(window_cfg)
        .add_config(EguiConfig)
        .update(update)
        .draw(draw)
        .build()
}

fn update(app: &mut App, state: &mut State) {
    handle_input(app);
}

fn draw(gfx: &mut Graphics, plugins: &mut Plugins, state: &mut State) {
    // Viewport

    // UI
    let mut output = plugins.egui(|ctx| {
        TopBottomPanel::top("menu_bar").show(ctx, |ui| {
            menu::bar(ui, |ui| {
                ui.menu_button("File", |ui| {
                    if ui.button("Quit").clicked() {
                        exit(0);
                    }
                });
                ui.menu_button("Edit", |ui| {
                    ui.label("Empty.");
                });
                ui.menu_button("View", |ui| {
                    ui.label("Empty.");
                });
                ui.menu_button("Tools", |ui| {
                    ui.label("Empty.");
                })
            });
        });
        TopBottomPanel::bottom("status_line").show(ctx, |ui| {
            ui.horizontal(|ui| {
                ui.columns(2, |columns| {
                    columns[0].label("No Map Opened");
                    let right_widgets = &mut columns[1];
                    right_widgets.with_layout(notan_egui::Layout::right_to_left(notan_egui::Align::Center), |ui| {
                        if ui.button("Jailmaker 0.1.0").on_hover_text("Hi!").clicked() {
                            state.window_states.insert("about".to_string(), true);
                        }
                    });
                });

            });
        });
        SidePanel::left("side_bar").resizable(false).show(ctx, |ui| {
            ui.heading("Center Perks");
            ui.separator();
            ui.heading("Objects");
            ui.collapsing("Doors", |ui| {});
            ui.collapsing("Cells", |ui| {});
            ui.collapsing("Gym", |ui| {});
            ui.collapsing("Jobs", |ui| {});
            ui.collapsing("Waypoints", |ui| {});
            ui.collapsing("Security", |ui| {});
            ui.collapsing("Misc", |ui| {});
            ui.collapsing("Jeep", |ui| {});
            ui.collapsing("Extra", |ui| {});
            ui.separator();
            ui.heading("Tileset");
            ui.separator();
        });
        TopBottomPanel::top("control_bar").show(ctx, |ui| {
            ui.horizontal(|ui| {
                ui.button("New");
                ui.button("Open");
                ui.button("Save");
                ui.separator();
                ui.button("Validate");
                ui.button("Export");
                ui.separator();
                ui.button("Play");
                ui.button("Stop");
                ui.separator();
                ui.button("Settings");
                ui.button("Help");
                ui.separator();
                ui.button("Zoom In");
                ui.button("Reset Zoom");
                ui.button("Zoom Out");
            });
        });
        SidePanel::right("tool_bar").resizable(false).show(ctx, |ui| {
            ui.button("Draw");
        });

        if state.window_states.get("about").is_some() {
            let about_window_state = *state.window_states.get("about").unwrap();
            if about_window_state {
                Window::new("About Jailmaker").resizable(false).show(ctx, |ui| {
                    ui.label(":)");
                });
            }
        }
    });

    output.clear_color(Color::new(0.05, 0.05, 0.05, 1.0));
    gfx.render(&output);
}

fn handle_input(app: &mut App) {
    if app.keyboard.was_pressed(KeyCode::F11) {
        let is_fullscreen = app.window().is_fullscreen();
        app.window().set_fullscreen(!is_fullscreen);
    }
}

#[derive(AppState)]
struct State {
    window_states: HashMap<String, bool>,
}

impl State {
    pub fn new(gfx: &mut Graphics) -> Self {
        let instance = Self {
            window_states: HashMap::new()
        };

        instance
    }
}