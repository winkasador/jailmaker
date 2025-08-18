using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Render;

public class SolidColorBackground : Background {

    private Texture2D _pixel;
    public Color Color { get; set; }

    public SolidColorBackground(GraphicsDevice device, Color color) : base(device) {
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData([Color.White]);

        Color = color;
    }

    public override void Draw(SpriteBatch batch, Matrix matrix, Rectangle destination) {
        batch.Draw(_pixel, destination, Color);
    }

}