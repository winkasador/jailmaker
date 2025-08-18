using System;
using System.Collections.Generic;
using System.Linq;

namespace Jailbreak.Data;

public class Tileset {

    private string _id;
    private bool _isCustom;
    private string _texturePath;
    private Dictionary<string, List<int>> _editorGroups;
    private List<Tile> _tiles;

    public Tileset(string id, bool custom, string texturePath, Dictionary<string, List<int>> editorGroups, List<Tile> tiles) {
        _id = id;
        _isCustom = custom;
        _texturePath = texturePath;
        _editorGroups = editorGroups;
        _tiles = tiles;
    }
    
    public string Id {
        get { return _id; }
    }

    public bool IsCustom {
        get { return _isCustom; }
    }

    public string TexturePath {
        get { return _texturePath; }
    }

    public List<Tile> Tiles {
        get { return _tiles; }
    }

    public int TileCount {
        get { return _tiles.Count; }
    }

    public int GetNextTileInGroup(int tileId) {
        tileId = tileId - 1;
        if(tileId == -1) return 0;

        var result = _editorGroups
            .Where(kv => kv.Value.Contains(tileId))
            .Select(kv => new { Group = kv.Key, Position = kv.Value.IndexOf(tileId) })
            .FirstOrDefault();
        
        if(result == null) return tileId + 1;

        List<int> list = _editorGroups[result.Group];
        int currentIndex = result.Position;
        return list[(currentIndex + 1) % list.Count] + 1;
    }

    public int GetPreviousTileInGroup(int tileId) {
        tileId = tileId - 1;
        if(tileId == -1) return 0;

        var result = _editorGroups
            .Where(kv => kv.Value.Contains(tileId))
            .Select(kv => new { Group = kv.Key, Position = kv.Value.IndexOf(tileId) })
            .FirstOrDefault();
        
        if(result == null) return tileId + 1;

        List<int> list = _editorGroups[result.Group];
        int currentIndex = result.Position;
        return list[(currentIndex - 1 + list.Count) % list.Count] + 1;
    }

    public class Tile {

        /// <summary> This tile casts and blocks shadows. </summary>
        private bool _hasShadow;
        private string _tileType;

        public Tile(bool hasShadow, string tileType) {
            _hasShadow = hasShadow;
            _tileType = tileType;
        }

        public bool CastsShadow {
            get { return _hasShadow; }
        }

        public string TileType {
            get { return _tileType; }
        }

    }

}