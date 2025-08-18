using System.IO;
using Jailbreak.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jailbreak.Render;

public class MinimapRenderer {

    public Texture2D DrawMinimap(GraphicsDevice device, Map map, MapRenderer mapRenderer) {
        Texture2D texture = new Texture2D(device, map.Width, map.Height);
        Color[] pixelData = new Color[map.Width * map.Height];

        for(int y = 0; y < map.Height; y++) {
            for(int x = 0; x < map.Width; x++) {
                pixelData[y * map.Width + x] = GetAverageColor(mapRenderer.GetTileTextureAt(map, x, y, 1));
            }
        }

        texture.SetData(pixelData);

        using (FileStream stream = new FileStream("./minimap.png", FileMode.Create))
        {
            texture.SaveAsPng(stream, map.Width, map.Height);
        }

        return texture;
    }

    public Color GetAverageColor(Texture2D texture) {
        Color[] colors = new Color[texture.Width * texture.Height];
        texture.GetData(colors);

        int r = 0, g = 0, b = 0, count = colors.Length;
        foreach (Color color in colors) {
            r += color.R;
            g += color.G;
            b += color.B;
        }

        return new Color(r / count, g / count, b / count);
    }

}