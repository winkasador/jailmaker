use notan::math::Vec2;

pub struct Camera {
    pub target_position: Vec2,
    pub position: Vec2,
    pub zoom: f32,
    pub smoothing_amount: f32
}

impl Camera {
    pub fn new() -> Self {
        Camera {
            target_position: Vec2::ZERO,
            position: Vec2::ZERO,
            zoom: 1.0,
            smoothing_amount: 30.0
        }
    }

    pub fn move_camera_towards_target(&mut self, delta: f32) {
        self.position = self.position.lerp(self.target_position, self.smoothing_amount * delta);
    }
}