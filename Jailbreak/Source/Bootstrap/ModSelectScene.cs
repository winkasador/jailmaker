using Jailbreak.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Bootstrap;

public class ModSelectScene : Scene.Scene {

    private SpriteFont _font;
    private SpriteBatch _batch;

    private Texture2D _errorBackdropImage;

    public ModSelectScene(Jailbreak game) : base(game) {
        _batch = new SpriteBatch(Game.GraphicsDevice);
        _font = Game.Content.Load<SpriteFont>("escapists/Fonts/Escapists");

        _errorBackdropImage = game.ContentManager.GetContent<Texture2D>("_global/image.error");
    }

    public override void Draw(float deltaTime) {
        int lineHeight = 22;

        int yPos = 5;

        _batch.Begin(samplerState: SamplerState.PointClamp);
        _batch.Draw(_errorBackdropImage, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.Gray);

        _batch.DrawString(_font, "~ Mod Select ~", new Vector2(5, yPos), Color.White);
        foreach (ModDefinition mod in Game.ModManager.InstalledMods.Values) {
            _batch.DrawString(_font, $"{mod.Id} by {mod.Authors[0].Name}", new Vector2(5, yPos += lineHeight), Color.White);
        }

        _batch.End();
    }

    public override void Ready() {}

    public override void Update(float deltaTime) {
        
    }
}