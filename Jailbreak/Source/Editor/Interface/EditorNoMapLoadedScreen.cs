using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Editor.Interface;

public class EditorNoMapLoadedScreen {

    private Jailbreak _game;
    private SpriteFont _font;
    private Texture2D _pixel;

    public EditorNoMapLoadedScreen(IServiceProvider services, SpriteFont font) {
        _game = services.GetRequiredService(typeof(Jailbreak)) as Jailbreak;
        _font = font;
        _pixel = new Texture2D(_game.GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Draw(SpriteBatch batch) {
        Vector2 middle = new Vector2(
            _game.GraphicsDevice.Viewport.Width / 2f,
            _game.GraphicsDevice.Viewport.Height / 2f
        );

        string line1 = "No map loaded!";
        string line2 = "Create a New Project or Open an Existing One";
        int width = (int)Math.Max(_font.MeasureString(line1).X, _font.MeasureString(line2).X);
        int height = (int)(_font.MeasureString(line1).Y + _font.MeasureString(line2).Y);

        Rectangle border = new Rectangle((int)(middle.X - width / 2), (int)(middle.Y - height / 2), width, height);
        Rectangle innerBorder = border;
        innerBorder.Inflate(8, 8);
        Rectangle outerBorder = border;
        outerBorder.Inflate(10, 10);

        batch.Draw(_pixel, outerBorder, Color.White);
        batch.Draw(_pixel, innerBorder, Color.Black);

        DrawCenteredText(batch, _font, line1, middle + new Vector2(0, -12), Color.White);
        DrawCenteredText(batch, _font, line2, middle + new Vector2(0, 12), Color.White);

        int horizontalPadding = 8;
        int topPadding = 48;
        int itemMargin = 8;

        DrawCenteredText(batch, _font, "JAILMAKER", new Vector2(middle.X, topPadding), Color.LightGray);

        // OFFICIAL PRISONS

        string[] prisons = ["Tutorial", "Centre Perks", "Stalag Flucht", "Shankton State Pen", "Jungle Compound", "San Pancho", "HMP Irongate"];

        int yOffset = topPadding;
        string officialPrisons = "Official Prisons";
        DrawRightAlignedText(batch, _font, officialPrisons, new Vector2(_game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.LightGray);
        yOffset += (int)_font.MeasureString(officialPrisons).Y + itemMargin;

        foreach (string prison in prisons) {
            DrawRightAlignedText(batch, _font, prison, new Vector2(_game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.Gray);
            yOffset += (int)_font.MeasureString(prison).Y + itemMargin;
        }

        yOffset += (int)_font.MeasureString("C").Y + itemMargin;

        // BONUS PRISONS

        string[] bonusPrisons = ["Alcatraz", "Banned Camp", "Camp Epsilon", "Duck Tapes Are Forever", "Escape Team", "Fhurst Peak", "Fort Bamford", "Jingle Cells", "Paris Central Pen", "Santa's Sweatshop", "Tower of London"];

        string bonusPrisonsTitle = "Bonus Prisons";
        DrawRightAlignedText(batch, _font, bonusPrisonsTitle, new Vector2(_game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.LightGray);
        yOffset += (int)_font.MeasureString(bonusPrisonsTitle).Y + itemMargin;

        foreach (string prison in bonusPrisons) {
            DrawRightAlignedText(batch, _font, prison, new Vector2(_game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.Gray);
            yOffset += (int)_font.MeasureString(prison).Y + itemMargin;
        }

        yOffset += (int)_font.MeasureString("C").Y + itemMargin;

        // CUSTOM PRISONS

        string customPrisonsTitle = "Custom Prisons";
        DrawRightAlignedText(batch, _font, customPrisonsTitle, new Vector2(_game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.LightGray);
        yOffset += (int)_font.MeasureString(customPrisonsTitle).Y + itemMargin;

        DrawRightAlignedText(batch, _font, "(No custom maps installed.)", new Vector2(_game.GraphicsDevice.Viewport.Width - horizontalPadding, yOffset), Color.Gray);

        yOffset = (int)(middle.Y * 2 - _font.MeasureString("C").Y / 2 - 6);

        string attribution = "The Escapists and Included Assets (c) Mouldy Toof Studios and Team17 Digital Ltd.";
        DrawCenteredText(batch, _font, attribution, new Vector2(middle.X, yOffset), Color.Gray);

        yOffset -= (int)_font.MeasureString(attribution).Y + 6;

        string authorText = "~ A Winkassador! Product ~";
        DrawCenteredText(batch, _font, authorText, new Vector2(middle.X, yOffset), Color.Gray);
    }

    public void DrawCenteredText(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color) {
        Vector2 textSize = font.MeasureString(text);
        batch.DrawString(font, text, position, color, 0f, textSize / 2f, 1f, SpriteEffects.None, 0f);
    }

    // TODO: Move into a TextRenderer class.
    public void DrawRightAlignedText(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color) {
        Vector2 textSize = font.MeasureString(text);
        batch.DrawString(font, text, position, color, 0f, textSize, 1f, SpriteEffects.None, 0f);
    }

}