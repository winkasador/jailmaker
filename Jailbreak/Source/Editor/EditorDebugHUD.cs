using System;
using System.Collections.Generic;
using System.Linq;
using Jailbreak.Content;
using Jailbreak.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Editor;

public class EditorDebugHUD {

    private Jailbreak _game;
    private Performance _performance;
    private DynamicContentManager _content;

    private SpriteFont _font;

    private int _yPosition;

    public EditorDebugHUD(IServiceProvider services, SpriteFont font) {
        _game = services.GetService(typeof(Jailbreak)) as Jailbreak;
        _performance = services.GetService(typeof(Performance)) as Performance;
        _content = services.GetService(typeof(DynamicContentManager)) as DynamicContentManager;

        _font = font;
    }

    public void Draw(SpriteBatch batch, Rectangle bounds) {
        _yPosition = bounds.Y + 4;

        DrawLine(batch, bounds, $"Jailbreak Editor 0.1");

        string mod = $"Mod: \"{_game.Mod.Id}\"";
        mod += FormatCreditedUserList(" by ", _game.Mod.Authors);
        mod += FormatCreditedUserList(" with credits to", _game.Mod.Credits) + ".";

        DrawLine(batch, bounds, mod);

        string fpsText = $"FPS: {_performance.CurrentFPS.Value}{(_performance.TargetFPS.Value == -1 ? "" : "/" + _performance.TargetFPS.Value)}{(_performance.IsVSync.Value ? " (VSync)" : "")}";
        DrawLine(batch, bounds, fpsText);

        string frameTimeText = $"Frame Time: {_performance.FrameTime.Value:F1}ms (Update: {_performance.UpdateTime.Value:F1}ms, Draw: {_performance.DrawTime.Value:F1}ms)";
        DrawLine(batch, bounds, frameTimeText);

        double memoryUsage = _performance.CurrentMemoryUsage.Value / (1024 * 1024);
        double memoryLimit = _performance.MaximumAvailableMemory.Value / (1024 * 1024);
        string memoryUsageText = $"Memory: {memoryUsage:F1}MB / {memoryLimit:F1}MB";
        DrawLine(batch, bounds, memoryUsageText);

        IncrementLine();

        DrawLine(batch, bounds, "Content...");
        foreach (Type contentType in _content.GetSupportedContentTypes()) {
            string contentEntryText = $"- {_content.GetContentName(contentType)}: {_content.GetAmountOfContentType(contentType)}";
            DrawLine(batch, bounds, contentEntryText);
        }

        //IncrementLine();

        //batch.DrawString(_font, "Editor", new Vector2(5, verticalOffset += 25), Color.White);
        //batch.DrawString(_font, $"Selected Tile: {_state.selectedTile} (Index: {_state.selectedTile - 1})", new Vector2(5, verticalOffset += 25), Color.White);
    }

    private void IncrementLine() {
        _yPosition += (int)(_font.MeasureString("A").Y + 4);
    }

    private void DrawLine(SpriteBatch batch, Rectangle bounds, string line) {
        DrawLine(batch, bounds, line, Color.White);
    }

    private void DrawLine(SpriteBatch batch, Rectangle bounds, string line, Color color) {
        batch.DrawString(_font, line, new Vector2(bounds.X + 5, _yPosition), color);
        IncrementLine();
    }

    private string FormatCreditedUserList(string label, IList<CreditedUser> users) {
        if (users == null || users.Count == 0) return string.Empty;

        string names = string.Join(", ", users.Take(users.Count - 1).Select(p => p.Name));
        if (users.Count > 1)
            names += " and " + users.Last().Name;
        else
            names = users[0].Name;

        return $"{label} {names}";
    }

}