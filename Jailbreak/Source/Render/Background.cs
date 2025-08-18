using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Render;

public abstract class Background {

    protected GraphicsDevice _device;

    public Background(GraphicsDevice device) {
        _device = device;
    }

    public abstract void Draw(SpriteBatch batch, Matrix matrix, Rectangle destination);

}