use camera::Camera;
use notan::{draw::*, math::Vec2, prelude::*};

mod camera;

const CAMERA_MOVE_SPEED: f32 = 200.0;

#[derive(AppState)]
struct EditorState {
    camera: camera::Camera,
    show_debug_info: bool
}

#[notan_main]
fn main() -> Result<(), String> {
    // Check the documentation for more options
    let window_config = WindowConfig::new()
        .set_title("Jailmaker")
        .set_size(1026, 600) // window's size
        .set_vsync(true) // enable vsync
        .set_resizable(true) // window can be resized
        .set_min_size(600, 400) // Set a minimum window size
        .set_window_icon_data(Some(include_bytes!("../res/icon.png")))
        .set_taskbar_icon_data(Some(include_bytes!("../res/icon.png")));

    notan::init_with(setup)
        .add_config(DrawConfig)
        .add_config(window_config)
        .update(update)
        .draw(draw)
        .build()
}

fn setup(gfx: &mut Graphics) -> EditorState {
    EditorState {
        camera: Camera::new(),
        show_debug_info: true
    }
}

fn update(app: &mut App, state: &mut EditorState) {
    let delta = app.timer.delta_f32();

    let mut camera_move_amount = Vec2::ZERO;

    if app.keyboard.is_down(KeyCode::W) {
        camera_move_amount.y = -CAMERA_MOVE_SPEED * app.timer.delta_f32();
    }

    if app.keyboard.is_down(KeyCode::A) {
        camera_move_amount.x = -CAMERA_MOVE_SPEED * app.timer.delta_f32();
    }

    if app.keyboard.is_down(KeyCode::S) {
        camera_move_amount.y = CAMERA_MOVE_SPEED * app.timer.delta_f32();
    }

    if app.keyboard.is_down(KeyCode::D) {
        camera_move_amount.x = CAMERA_MOVE_SPEED * app.timer.delta_f32();
    }

    // No Vec2.add()? :(
    state.camera.target_position.x += camera_move_amount.x;
    state.camera.target_position.y += camera_move_amount.y;

    state.camera.move_camera_towards_target(delta);
}

fn draw(app: &mut App, gfx: &mut Graphics, state: &mut EditorState) {
    let mut draw = gfx.create_draw();
    draw.clear(Color::BLACK);

    if state.show_debug_info {
        debug_draw(app, gfx, &mut draw, state);
    }

    gfx.render(&draw);
}

fn debug_draw(app: &mut App, gfx: &mut Graphics, draw: &mut Draw, state: &mut EditorState) {
    draw.circle(5.0)
        .position(state.camera.position.x, state.camera.position.y)
        .color(Color::PINK);

    draw.circle(5.0)
        .position(state.camera.target_position.x, state.camera.target_position.y)
        .color(Color::GREEN);
}