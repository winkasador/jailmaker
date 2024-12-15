use std::collections::HashMap;

use notan::{math::Vec2, prelude::*};

use crate::AppContext;

pub struct Keybinding {
    pub primary_key: Option<KeyCode>,
    pub secondary_key: Option<KeyCode>
}

impl Keybinding {
    pub fn new(primary_key: Option<KeyCode>, secondary_key: Option<KeyCode>) -> Self {
        Self {
            primary_key,
            secondary_key
        }
    }
}

pub fn get_input_vector(app: &mut App, ctx: &mut AppContext) -> Vec2 {
    let mut input_vec = Vec2::ZERO;

    if is_keybinding_pressed(app, &ctx.keybindings.get("move_up")) {
        input_vec.y -= 1.0;
    }
    if is_keybinding_pressed(app, &ctx.keybindings.get("move_left")) {
        input_vec.x -= 1.0;
    }
    if is_keybinding_pressed(app, &ctx.keybindings.get("move_down")) {
        input_vec.y += 1.0;
    }
    if is_keybinding_pressed(app, &ctx.keybindings.get("move_right")) {
        input_vec.x += 1.0;
    }

    input_vec
}

pub fn is_keybinding_pressed(app: &mut App, keybinding_opt: &Option<&Keybinding>) -> bool {
    if let Some(keybinding) = keybinding_opt {
        if let Some(primary) = keybinding.primary_key {
            if app.keyboard.is_down(primary) {
                return true;
            }
        }
        if let Some(secondary) = keybinding.secondary_key {
            if app.keyboard.is_down(secondary) {
                return true;
            }
        }
    }
    false   
}

pub fn was_keybinding_pressed(app: &mut App, keybinding_opt: &Option<&Keybinding>) -> bool {
    if let Some(keybinding) = keybinding_opt {
        if let Some(primary) = keybinding.primary_key {
            if app.keyboard.was_released(primary) {
                return true;
            }
        }
        if let Some(secondary) = keybinding.secondary_key {
            if app.keyboard.was_released(secondary) {
                return true;
            }
        }
    }
    false   
}

pub fn default_keybindings() -> HashMap<String, Keybinding> {
    let mut keybindings = HashMap::new();
    keybindings.insert("move_up".to_owned(), Keybinding::new(Some(KeyCode::W), Some(KeyCode::Up)));
    keybindings.insert("move_down".to_owned(), Keybinding::new(Some(KeyCode::S), Some(KeyCode::Down)));
    keybindings.insert("move_left".to_owned(), Keybinding::new(Some(KeyCode::A), Some(KeyCode::Left)));
    keybindings.insert("move_right".to_owned(), Keybinding::new(Some(KeyCode::D), Some(KeyCode::Right)));
    keybindings.insert("zoom_in".to_owned(), Keybinding::new(Some(KeyCode::Q), None));
    keybindings.insert("zoom_out".to_owned(), Keybinding::new(Some(KeyCode::E), None));
    keybindings.insert("debug".to_owned(), Keybinding::new(Some(KeyCode::F3), None));
    keybindings.insert("fullscreen".to_owned(), Keybinding::new(Some(KeyCode::F11), None));

    keybindings
}