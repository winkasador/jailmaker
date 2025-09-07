using System.Collections.Generic;
using Jailbreak.Content;
using Jailbreak.Data;
using Jailbreak.Utility;
using Jailbreak.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Render;

public class MapRenderer {

    protected const int SHADOW_NONE = 0;
    protected const int SHADOW_FULL = 1;
    protected const int SHADOW_TOP_LEFT = 2;
    protected const int SHADOW_BOTTOM_RIGHT = 3;

    protected GraphicsDevice _graphicsDevice;

    protected Texture2D _pixelTexture;

    protected Texture2D _tilesetTexture;
    protected List<Texture2D> _tileTextures;

    protected Texture2D _groundTexture;
    protected Texture2D _undergroundTexture;

    protected List<Texture2D> _shadowTextures = new();
    protected Color _shadowColor = new(0, 0, 0, 100);

    public MapRenderer(GraphicsDevice graphicsDevice, DynamicContentManager _content) {
        _graphicsDevice = graphicsDevice;

        _shadowTextures.Add(_content.GetContent<Texture2D>("escapists:shadow_full"));
        _shadowTextures.Add(_content.GetContent<Texture2D>("escapists:shadow_corner_top"));
        _shadowTextures.Add(_content.GetContent<Texture2D>("escapists:shadow_corner_bottom"));
        //_shadowTextures.Add(_content.LoadTexture("escapists/Textures/Shadow/ShadowPipeHorizontal", "shadow_pipe_horizontal"));
        //_shadowTextures.Add(_content.LoadTexture("escapists/Textures/Shadow/ShadowPipeVertical", "shadow_pipe_vertical"));

        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);
    }

    public List<Texture2D> TileTextures {
        get { return _tileTextures; }
    }

    public Texture2D TilesetTexture {
        get { return _tilesetTexture; }
        set {
            _tilesetTexture = value;
            TilesetUtil.ReplaceColors(_tilesetTexture, Color.White, Color.Transparent);
            _tileTextures = TilesetUtil.AtlasToTextureList(TilesetTexture, _graphicsDevice, TilesetData.DefaultTileSize, TilesetData.DefaultTileSize);
        }
    }

    public Texture2D GroundTexture {
        get { return _groundTexture; }
        set { _groundTexture = value; }
    }

    public Texture2D UndergroundTexture {
        get { return _undergroundTexture; }
        set { _undergroundTexture = value; }
    }

    public virtual void RenderMap(SpriteBatch batch, Map map, int activeFloor) {
        RenderLayer(batch, map, activeFloor);
    }

    public virtual void RenderLayer(SpriteBatch batch, Map map, int layer) {
        Rectangle drawArea = new Rectangle(0, 0, map.Width * TilesetData.DefaultTileSize, map.Height * TilesetData.DefaultTileSize);
        if(layer == 0)
            batch.Draw(_undergroundTexture, drawArea, drawArea, new Color(255, 255, 255, 150));
        else if(layer == 1)
            batch.Draw(_groundTexture, drawArea, drawArea, Color.White);
        else {
            batch.Draw(_pixelTexture, drawArea, drawArea, new Color(0, 0, 0, 150));
        }

        var tiles = map.GetTilesOfFloor(layer);
        for(int y = 0; y < map.Height; y++) {
            for(int x = 0; x < map.Width; x++) {
                int tile = tiles[y,x];
                if(tile != TilesetData.EmptyTile) {
                    if(tile <= 0 || tile > _tileTextures.Count) {
                        batch.Draw(_pixelTexture, new Rectangle(x * TilesetData.DefaultTileSize, y * TilesetData.DefaultTileSize, TilesetData.DefaultTileSize, TilesetData.DefaultTileSize), new Color(255, 255, 255, tile * 2));
                    }
                    else {
                        batch.Draw(_tileTextures[tile - 1], new Rectangle(x * TilesetData.DefaultTileSize, y * TilesetData.DefaultTileSize, TilesetData.DefaultTileSize, TilesetData.DefaultTileSize), Color.White);
                    }
                }

                if(map.ShadowMap.ContainsKey(layer)) {
                    var shadowType = map.ShadowMap[layer][y][x];
                    if(shadowType != SHADOW_NONE) {
                        batch.Draw(_shadowTextures[shadowType - 1], new Rectangle(x * TilesetData.DefaultTileSize, y * TilesetData.DefaultTileSize, TilesetData.DefaultTileSize, TilesetData.DefaultTileSize), _shadowColor);
                    }
                }                
            }
        }
    }

    public Texture2D GetTileTextureAt(Map map, int x, int y, int floor) {
        int tileId = map.GetTileAt(x, y, floor);

        if(tileId != 0) {
            return _tileTextures[tileId - 1];
        }

        return _tileTextures[33];
    }

}