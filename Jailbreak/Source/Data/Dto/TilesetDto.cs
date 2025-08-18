using System.Collections.Generic;
using Jailbreak.Content;
using static Jailbreak.Data.Tileset;

namespace Jailbreak.Data.Dto;

public class TilesetDto {

    public string Id { get; set; }
    public bool Custom { get; set; }
    public ContentPath TexturePath { get; set; } = new("");
    public int TileCount { get; set; }
    public List<TileDto> Tiles { get; set; } = new();
    public Dictionary<string, List<int>> EditorGroups = new();

    public Tileset ToTileset(DynamicContentManager contentManager) {
        Tile defaultTile = new TileDto() {
            HasShadow = false,
        }.ToTile();
        
        List<Tile> compiledTiles = new();
        
        Dictionary<int, TileDto> tileMap = new Dictionary<int, TileDto>();

        foreach (TileDto tile in Tiles) {
            tileMap.Add(tile.Index, tile);
        }

        for(int i = 0; i < TileCount; i++) {
            if(tileMap.ContainsKey(i)) {
                compiledTiles.Add(tileMap[i].ToTile());
            }
            else {
                compiledTiles.Add(defaultTile);
            }
        }

        if(contentManager == null) {
            return new Tileset(Id, Custom, TexturePath.Path, EditorGroups, compiledTiles);
        }
        return new Tileset(Id, Custom, contentManager.ResolveFilePath(TexturePath.Path), EditorGroups, compiledTiles);
    }

}