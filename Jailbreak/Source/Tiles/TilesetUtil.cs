using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Utility;

public static class TilesetUtil {

    public static void ReplaceColors(Texture2D texture, Color color) {
        ReplaceColors(texture, color, Color.Transparent);
    }

    public static void ReplaceColors(Texture2D texture, Color colorToReplace, Color replaceWith) {
        if(texture == null) return;

        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);

        for (int i = 0; i < pixels.Length; i++) {
            if (pixels[i].R == colorToReplace.R && pixels[i].G == colorToReplace.G && pixels[i].B == colorToReplace.B && pixels[i].A == colorToReplace.A) {
                pixels[i] = replaceWith;
            }
        }

        texture.SetData(pixels);
    }

    /// <summary>
    /// Converts a tileset texture into a list of individual tiles, from top to bottom, left to right.
    /// </summary>
    public static List<Texture2D> AtlasToTextureList(Texture2D tilesetTexture, GraphicsDevice graphicsDevice, int tileWidth = 16, int tileHeight = 16) {
        if(tilesetTexture == null) return new();
        
        List<Texture2D> tiles = new List<Texture2D>();

        int tilesPerRow = tilesetTexture.Width / tileWidth;
        int tilesPerColumn = tilesetTexture.Height / tileHeight;

        for(int x = 0; x < tilesPerRow; x++) {
            for(int y = 0; y < tilesPerColumn; y++) {
                Rectangle tileRect = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                Texture2D tileTexture = new Texture2D(graphicsDevice, tileWidth, tileHeight);
                Color[] tileData = new Color[tileWidth * tileHeight];
                tilesetTexture.GetData(0, tileRect, tileData, 0, tileData.Length);
                tileTexture.SetData(tileData);

                tiles.Add(tileTexture);
            }
        }

        return tiles;
    }

}