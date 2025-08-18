using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.World;

public class Camera {
    
    private float _zoom;
    private float _smoothRate;
    private Vector2 _position;
    private Vector2 _targetPosition;
    private Viewport _viewport;
    private Rectangle _bounds;

    public Camera(Viewport viewport) {
        _zoom = 1.0f;
        _smoothRate = 6f;
        _position = Vector2.Zero;
        _targetPosition = Vector2.Zero;
        _viewport = viewport;
    }

    public float Zoom {
        get => _zoom;
        set => _zoom = MathHelper.Clamp(value, 0.5f, 5f);
    }

    public float SmoothRate {
        get => _smoothRate;
        set => _smoothRate = value;
    }

    public Vector2 Position {
        get => _position;
        set => _position = value;
    }

    public Vector2 TargetPosition {
        get => _targetPosition;
        set {
            var x = MathHelper.Clamp(value.X, _bounds.Left, _bounds.Right);
            var y = MathHelper.Clamp(value.Y, _bounds.Top, _bounds.Bottom);

            _targetPosition = new Vector2(x, y);
        }
    }

    public Viewport Viewport {
        get => _viewport;
        set => _viewport = value;
    }

    public Rectangle Bounds {
        get => _bounds;
        set => _bounds = value;
    }

    public void MoveToTarget(float deltaTime) {
        _position = Vector2.Lerp(_position, _targetPosition, _smoothRate * deltaTime);
    }

    public void SnapTo(Vector2 position) {
        TargetPosition = position;
        Position = TargetPosition;
    }

    public Vector2 Unproject(Vector2 screenPosition) {
        Matrix inverseTransform = Matrix.Invert(GetMatrix());
        Vector3 worldPosition = Vector3.Transform(new Vector3(screenPosition, 0), inverseTransform);
        return new Vector2(worldPosition.X, worldPosition.Y);
    }

    public Matrix GetMatrix() {
        return
            Matrix.CreateTranslation(new Vector3(-_position, 0)) *
            Matrix.CreateScale(new Vector3(_zoom, _zoom, 1)) *
            Matrix.CreateTranslation(new Vector3(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0));
    }
}

