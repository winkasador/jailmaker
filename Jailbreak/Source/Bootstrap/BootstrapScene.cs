using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak;

public class BootstrapScene : Scene.Scene {

    private SpriteFont _font;
    private SpriteBatch _batch;

    private Texture2D _errorBackdropImage;

    public BootstrapScene(Jailbreak game) : base(game, null) {
        _batch = new SpriteBatch(Game.GraphicsDevice);
        _font = Game.Content.Load<SpriteFont>("escapists/Fonts/Escapists");

        _errorBackdropImage = game.ContentManager.GetContent<Texture2D>("_global/image.error");
    }

    public override void Draw(float deltaTime) {
        int lineHeight = 22;

        int yPos = 5;

        _batch.Begin(samplerState: SamplerState.PointClamp);
        _batch.Draw(_errorBackdropImage, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.Gray);

        _batch.DrawString(_font, "Something Went Wrong...", new Vector2(5, yPos), Color.White);
        _batch.DrawString(_font, "Jailmaker could not find a valid installation of the Escapists.", new Vector2(5, yPos += lineHeight * 2), Color.White);
        _batch.DrawString(_font, "Jailmaker requires the Steam version of the Escapists in order to work.", new Vector2(5, yPos += lineHeight * 2), Color.White);
        _batch.DrawString(_font, "The Console, Mobile, Unreal, and Escapists 2 versions are not supported.", new Vector2(5, yPos += lineHeight), Color.White);
        _batch.End();
    }

    public override void Ready() {}

    public override void Update(float deltaTime) {
        
    }
}