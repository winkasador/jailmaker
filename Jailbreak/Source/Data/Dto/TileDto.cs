using static Jailbreak.Data.TilesetData;

namespace Jailbreak.Data.Dto;

public class TileDto {

    public int Index { get; set; }
    public bool HasShadow { get; set; }
    public string TileType { get; set; }

    public Tile ToTile() {
        return new Tile(HasShadow, TileType);
    }

}