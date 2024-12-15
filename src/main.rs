use std::collections::HashMap;

use camera::Camera;
use input::Keybinding;
use notan::{draw::*, math::Vec2, prelude::*};

mod camera;
mod input;

const CAMERA_MOVE_SPEED: f32 = 200.0;
const CAMERA_ZOOM_SPEED: f32 = 0.1;
const CAMERA_SMOOTHING_SPEED: f32 = 9.0;

const FADE_THRESHOLD: f32 = 0.3; // The time at which the window goes from fully opqaue to fading in. (This covers up flickering caused by toggling fullscreen.)
const FADE_DURATION: f32 = 0.35; // Time until the window becomes fully visible again.

#[derive(AppState)]
struct AppContext {
    font: Font,
    camera_target: Vec2,
    camera: camera::Camera,
    show_debug_info: bool,
    keybindings: HashMap<String, Keybinding>,
    window_fade_timer: f32,
}

#[notan_main]
fn main() -> Result<(), String> {
    
    // Check the documentation for more options
    let window_config = WindowConfig::new()
        .set_title("Jailmaker")
        .set_size(1026, 600)
        .set_vsync(false)
        .set_resizable(true)
        .set_min_size(600, 400)
        .set_window_icon_data(Some(include_bytes!("../res/icon.png")))
        .set_taskbar_icon_data(Some(include_bytes!("../res/icon.png")));

    notan::init_with(setup)
        .add_config(DrawConfig)
        .add_config(window_config)
        .update(update)
        .draw(draw)
        .build()
}

fn setup(app: &mut App, gfx: &mut Graphics) -> AppContext {
    AppContext {
        font: gfx.create_font(include_bytes!("../res/Escapists.ttf")).unwrap(),
        show_debug_info: true,
        camera: Camera::new(app.window().width(), app.window().height()),
        camera_target: Vec2::ZERO,
        keybindings: input::default_keybindings(),
        window_fade_timer: FADE_DURATION
    }
}

fn update(app: &mut App, ctx: &mut AppContext) {
    let delta = app.timer.delta_f32();

    // Camera Update
    input(app, ctx, delta);

    if ctx.window_fade_timer > 0.0 {
        ctx.window_fade_timer -= delta;
    }
}

fn input(app: &mut App, ctx: &mut AppContext, delta: f32) {
    if input::was_keybinding_pressed(app, &ctx.keybindings.get("fullscreen")) {
        let is_fullscreen = app.window().is_fullscreen();
        app.window().set_fullscreen(!is_fullscreen);
        ctx.window_fade_timer = FADE_DURATION;
    }
    if input::was_keybinding_pressed(app, &ctx.keybindings.get("debug")) {
        ctx.show_debug_info = !ctx.show_debug_info;
    }

    ctx.camera.viewport_width = app.window().width() as f32;
    ctx.camera.viewport_height = app.window().height() as f32;

    // Movement
    let mut input_vec = input::get_input_vector(app, ctx);

    input_vec = input_vec.normalize_or_zero();
    ctx.camera_target.x += input_vec.x * CAMERA_MOVE_SPEED * delta;
    ctx.camera_target.y += input_vec.y * CAMERA_MOVE_SPEED * delta;

    ctx.camera.position = ctx.camera.position.lerp(ctx.camera_target, CAMERA_SMOOTHING_SPEED * delta);

    // Zoom
    if app.keyboard.is_down(KeyCode::Q) {
        ctx.camera.zoom += CAMERA_ZOOM_SPEED
    }
    else if app.keyboard.is_down(KeyCode::E) {
        ctx.camera.zoom -= CAMERA_ZOOM_SPEED
    }
}

fn draw(app: &mut App, gfx: &mut Graphics, ctx: &mut AppContext) {
    if ctx.window_fade_timer > FADE_THRESHOLD {
        return;
    }

    let mut draw = gfx.create_draw();
    draw.clear(Color::new(0.1, 0.1, 0.1, 1.0));

    if ctx.show_debug_info {
        debug_draw(app, gfx, &mut draw, ctx);
    }

    if ctx.window_fade_timer > 0.0 {
        draw.rect((0.0, 0.0), (app.window().width() as f32, app.window().height() as f32))
            .color(Color::new(0.1, 0.1, 0.1, ctx.window_fade_timer / FADE_THRESHOLD));
    }

    gfx.render(&draw);
}

fn debug_draw(app: &mut App, gfx: &mut Graphics, draw: &mut Draw, ctx: &mut AppContext) {
    let projected_p = camera::project(&ctx.camera, Vec2::ZERO);
    let projected_camera_position = camera::project(&ctx.camera, ctx.camera.position);
    let projected_camera_target = camera::project(&ctx.camera, ctx.camera_target);

    draw.circle(5.0)
        .position(projected_p.x, projected_p.y)
        .color(Color::BLUE);

    draw.circle(5.0)
        .position(projected_camera_position.x, projected_camera_position.y)
        .color(Color::PINK);

    draw.circle(5.0)
        .position(projected_camera_target.x, projected_camera_target.y)
        .color(Color::GREEN);

    draw.text(&ctx.font, &format!("FPS: {}", app.timer.fps() as i32))
        .position(3.0, 1.0)
        .size(16.0)
        .color(Color::WHITE);
}