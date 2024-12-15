use notan::math::Vec2;

pub struct Camera {
    pub position: Vec2,
    pub zoom: f32,
    pub viewport_width: f32,
    pub viewport_height: f32
}

impl Camera {
    pub fn new(viewport_width: u32, viewport_height: u32) -> Self {
        Camera {
            position: Vec2::ZERO,
            zoom: 1.0,
            viewport_width: viewport_width as f32,
            viewport_height: viewport_height as f32
        }
    }

}

/// Converts a point in world space to a point in screen space.
pub fn project(camera: &Camera, world_pos: Vec2) -> Vec2 {
    let mut screen_pos = Vec2::ZERO;

    screen_pos.x = world_pos.x - camera.position.x;
    screen_pos.x = screen_pos.x * camera.zoom;
    screen_pos.x = screen_pos.x + camera.viewport_width / 2.0;

    screen_pos.y = world_pos.y - camera.position.y;
    screen_pos.y = screen_pos.y * camera.zoom;
    screen_pos.y = screen_pos.y + camera.viewport_height / 2.0;

    screen_pos
}

/// Converts a point in screen space to a point in world space.
pub fn unproject(camera: &Camera, screen_pos: Vec2) -> Vec2 {
    let mut world_pos = Vec2::ZERO;

    world_pos.x = screen_pos.x - camera.viewport_width / 2.0;
    world_pos.x = world_pos.x / camera.zoom;
    world_pos.x = world_pos.x + camera.position.x;

    world_pos.y = world_pos.y - camera.viewport_height / 2.0;
    world_pos.y = world_pos.y / camera.zoom;
    world_pos.y = world_pos.y + camera.position.y;

    world_pos
}