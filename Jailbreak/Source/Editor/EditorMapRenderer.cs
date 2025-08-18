using System;
using Jailbreak.Content;
using Jailbreak.Render;
using Jailbreak.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Editor;

public class EditorMapRenderer : MapRenderer {

    private Color _cursorPaintBright = Color.Yellow;
    private Color _cursorPaintDark = Color.Orange;

    private Color _cursorEraseBright = new Color(200, 0, 0, 100);
    private Color _cursorEraseDark = new Color(120, 0, 0, 100);

    private Color _debugShadowCaster = new Color(0, 0, 200, 200);
    private Color _debugShadowHalf = new Color(0, 140, 0, 200);
    private Color _debugShadowPresent = new Color(0, 200, 0, 200);
    private Color _debugShadowAbsent = new Color(200, 0, 0, 200);

    public EditorMapRenderer(GraphicsDevice graphicsDevice, DynamicContentManager content) : base(graphicsDevice, content) {}

    public override void RenderMap(SpriteBatch batch, Map map, int activeFloor)
    {
        DrawMapOutline(batch, map, 3);
        RenderLayer(batch, map, 1);
        if(activeFloor != 1) {
            RenderLayer(batch, map, activeFloor);
        }
    }

    private void DrawMapOutline(SpriteBatch batch, Map map, int size) {
        for(int i = size; i > 0; i--) {
            float alpha = i / size;
            batch.Draw(_pixelTexture, new Rectangle(-i, -i, map.Width * 16 + i * 2, map.Height * 16 + i * 2), new Color(0f, 0f, 0f, alpha));
        }
    }

    public void DrawSelection(SpriteBatch batch, Rectangle selection) {
        batch.Draw(_pixelTexture, new Rectangle(selection.X * 16, selection.Y * 16, selection.Width * 16, selection.Height * 16), new Color(48, 87, 225, 99));
    }

    public void DrawGrid(SpriteBatch batch, Map map, int cellWidth, int cellHeight) {
        for (int x = 0; x <= (map.Width - 1) * TILE_SIZE; x += cellWidth) {
            batch.Draw(_pixelTexture, new Rectangle(x, 0, 1, map.Height * TILE_SIZE), new Color(80, 80, 80, 100));
        }
        for (int y = 0; y <= (map.Height - 1) * TILE_SIZE; y += cellHeight) {
            batch.Draw(_pixelTexture, new Rectangle(0, y, map.Width * TILE_SIZE, 1), new Color(80, 80, 80, 100));
        }
    }

    public void DrawCursor(SpriteBatch batch, EditMode mode, int selectedTile, Point tilePosition, float stateTime) {
        switch(mode) {
            case EditMode.Paint:
                if(selectedTile == 0) break;
                if(selectedTile <= 0 || selectedTile > _tileTextures.Count) {
                    batch.Draw(_pixelTexture, new Rectangle(tilePosition.X * 16, tilePosition.Y * 16, 16, 16), new Color(213, 8, 224));
                }
                else {
                    batch.Draw(_tileTextures[selectedTile - 1], new Rectangle(tilePosition.X * 16, tilePosition.Y * 16, 16, 16), new Color(252, 234, 146, 210));
                }
                break;
            case EditMode.Erase:
                var color = (int)(stateTime * 7) % 2 == 0 ? _cursorEraseBright : _cursorEraseDark;
                batch.Draw(_pixelTexture, new Rectangle(tilePosition.X * 16, tilePosition.Y * 16, 16, 16), color);
                break;
        }
    }

    public void DebugDrawShadows(SpriteBatch batch, Map map, int activeFloor) {
        if(activeFloor == 0 || activeFloor == 2) return;

        var tiles = map.GetTilesOfFloor(activeFloor);

        for(int y = 0; y < map.Height; y++) {
            for(int x = 0; x < map.Width; x++) {
                int tile = tiles[y,x];
                if(tile != EMPTY_TILE && map.TilesetData.Tiles[tile - 1].CastsShadow) {
                    batch.Draw(_pixelTexture, new Rectangle(x * 16, y * 16, 16, 16), _debugShadowCaster);
                }
                else {
                    if(map.ShadowMap.Count > activeFloor && map.ShadowMap[0].Length >= map.Height &&map.ShadowMap[0][0].Length >= map.Width) {
                        int shadowType = map.ShadowMap[activeFloor][y][x];
                        switch(shadowType) {
                            case SHADOW_NONE:
                                batch.Draw(_pixelTexture, new Rectangle(x * 16, y * 16, 16, 16), _debugShadowAbsent);
                                break;
                            case SHADOW_TOP_LEFT:
                            case SHADOW_BOTTOM_RIGHT:
                                batch.Draw(_pixelTexture, new Rectangle(x * 16, y * 16, 16, 16), _debugShadowHalf);
                                break;
                            case SHADOW_FULL:
                                batch.Draw(_pixelTexture, new Rectangle(x * 16, y * 16, 16, 16), _debugShadowPresent);
                                break;
                        }
                    }
                }
            }
        }
    }

}