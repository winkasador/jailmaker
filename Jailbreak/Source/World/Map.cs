using System;
using System.Collections.Generic;
using System.Linq;
using Jailbreak.Content;
using Jailbreak.Data;
using Microsoft.Xna.Framework;

namespace Jailbreak.World;

public class Map {

    private const int DEFAULT_CUSTOM_MAP_WIDTH = 109;
    private const int DEFAULT_CUSTOM_MAP_HEIGHT = 109;
    private const int DEFAULT_FLOOR_COUNT = 4;
    private const int TILE_SIZE = 16;

    private const int SHADOW_NONE = 0;
    private const int SHADOW_FULL = 1;
    private const int SHADOW_TOP_LEFT = 2;
    private const int SHADOW_BOTTOM_RIGHT = 3;
    private const int SHADOW_PIPE_HORIZONTAL = 4;
    private const int SHADOW_PIPE_VERTICAL = 5;

    private string _mapName;

    private int _width, _height;
    private int _floorCount;
    private List<int[,]> _tileLayers;
    private string _tilesetId;
    private TilesetData _tilesetData;
    private bool _isCustom;


    private Dictionary<int, int[][]> _shadowMap = new();

    public Map(DynamicContentManager content) {
        _width = DEFAULT_CUSTOM_MAP_WIDTH;
        _height = DEFAULT_CUSTOM_MAP_HEIGHT;
        _floorCount = DEFAULT_FLOOR_COUNT;
        _tileLayers = new List<int[,]>(_floorCount);
        _tilesetData = content.GetContent<TilesetData>("escapists:perks"); // TODO: Specify default tileset in Mod Manifest.

        for (int floor = 0; floor < _floorCount; floor++) {
            _tileLayers.Add(new int[_height, _width]);
        }

        ComputeShadows();
    }

    public Map(DynamicContentManager content, PropertyContainer propertyContainer, string tilesetAndGroundOverride = "") {
        if (propertyContainer.ContainsProperty("Custom", "Info") && propertyContainer.GetProperty("Custom", "Info") == "-1") {
            _width = DEFAULT_CUSTOM_MAP_WIDTH;
            _height = DEFAULT_CUSTOM_MAP_HEIGHT;

            _tilesetId = propertyContainer.GetProperty("Tileset", "Info") + "_custom";
            _isCustom = true;
        }
        else {
            _height = propertyContainer.GetSectionOrEmpty("Tiles").Count;
            if (_height != 0) {
                _width = propertyContainer.GetProperty("0", "Tiles").Split("_").Length;
            }
            else {
                _height = 96;
                _width = 93;
            }

            _tilesetId = tilesetAndGroundOverride;
            _isCustom = false;
        }

        _floorCount = DEFAULT_FLOOR_COUNT;
        _tileLayers = new List<int[,]>(_floorCount);

        _tilesetData = content.GetContent<TilesetData>("escapists:" + _tilesetId); // TODO: Remove hard dependency on 'escapists/'.

        for (int floor = 0; floor < _floorCount; floor++) {
            _tileLayers.Add(new int[_height, _width]);
        }

        _mapName = propertyContainer.GetPropertyOrDefault("MapName", "Info", "Unnamed Prison");

        _tileLayers[0] = LoadFloorFromPropertySection(propertyContainer.GetSectionOrEmpty("Underground"));
        _tileLayers[1] = LoadFloorFromPropertySection(propertyContainer.GetSectionOrEmpty("Tiles"));
        _tileLayers[2] = LoadFloorFromPropertySection(propertyContainer.GetSectionOrEmpty("Vents"));
        _tileLayers[3] = LoadFloorFromPropertySection(propertyContainer.GetSectionOrEmpty("Roof"));

        var objectSection = propertyContainer.GetSectionOrEmpty("Objects");
        foreach (var entry in objectSection) {
            var objectInfo = entry.Value.Split("x");
            int id = int.Parse(objectInfo[0]);
            int x = int.Parse(objectInfo[1]);
            int y = int.Parse(objectInfo[2]);
            int floor = int.Parse(objectInfo[3]);
        }

        ComputeShadows();
    }

    private int[,] LoadFloorFromPropertySection(Dictionary<string, string> section) {
        int[,] tiles = new int[_height, _width];

        foreach (var row in section) {
            var rowString = row.Value;
            if (rowString.EndsWith('_')) {
                rowString = rowString.Substring(0, rowString.Length - 1);
            }

            var columns = rowString.Split('_');
            var rowTiles = columns.Select(s => int.TryParse(s, out int result) ? result : (int?)null)
                                  .Where(i => i.HasValue)
                                  .Where(i => i >= 0 && i <= 100)
                                  .Select(i => i.Value)
                                  .ToArray();

            var rowIndex = int.Parse(row.Key);
            if (rowIndex > _height) continue;

            for (int x = 0; x < rowTiles.Length; x++) {
                tiles[rowIndex, x] = rowTiles[x];
            }
        }

        return tiles;
    }

    public void ComputeShadows() {
        _shadowMap.Clear();

        if (TilesetData == null) return;

        for (int floor = 0; floor < FloorCount; floor++) {
            if (floor == 0 || floor == 2) continue; // Underground and vents don't have shadows.

            var shadows = new int[Height][];
            for (int row = 0; row < Height; row++) {
                shadows[row] = new int[Width];
            }

            var tiles = GetTilesOfFloor(floor);
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    int tile = tiles[y, x];
                    if (floor > 1 && tile == 0) continue; // 0 means air on the roof, so it can't have shadows on it.

                    if (tile > 0 && tile < TilesetData.TileCount && TilesetData.Tiles[tile - 1].CastsShadow) {
                        shadows[y][x] = 0;
                        continue;
                    }

                    int horizontalTile = GetTileAt(x - 1, y, floor);
                    int verticalTile = GetTileAt(x, y - 1, floor);
                    int diagonalTile = GetTileAt(x - 1, y - 1, floor);

                    bool horizontalShadow = horizontalTile <= 0 ? false : TilesetData.Tiles[horizontalTile - 1].CastsShadow;
                    bool verticalShadow = verticalTile <= 0 ? false : TilesetData.Tiles[verticalTile - 1].CastsShadow;
                    bool diagonalShadow = diagonalTile <= 0 ? false : TilesetData.Tiles[diagonalTile - 1].CastsShadow;

                    int shadowType;

                    if (diagonalShadow) shadowType = SHADOW_FULL;
                    else if (horizontalShadow && verticalShadow) shadowType = SHADOW_FULL;
                    else if (horizontalShadow) shadowType = SHADOW_TOP_LEFT;
                    else if (verticalShadow) shadowType = SHADOW_BOTTOM_RIGHT;
                    else shadowType = SHADOW_NONE;

                    /*if(floor == 1) {
                        int pipeTile = GetTileAt(x - 1, y - 1, 3);
                        if(pipeTile == 46) {
                            shadowType = SHADOW_PIPE_HORIZONTAL;
                        }
                        else if(pipeTile == 47) {
                            shadowType = SHADOW_PIPE_VERTICAL;
                        }
                    }*/

                    shadows[y][x] = shadowType;
                }
            }

            _shadowMap.Add(floor, shadows);
        }
    }

    public string MapName {
        get { return _mapName; }
        set { _mapName = value; }
    }

    public int Width {
        get { return _width; }
        set { _width = value; }
    }

    public int Height {
        get { return _height; }
        set { _height = value; }
    }

    public string TilesetId {
        get { return _tilesetId; }
    }

    public TilesetData TilesetData {
        get { return _tilesetData; }
        private set { _tilesetData = value; }
    }

    public int FloorCount {
        get { return _floorCount; }
        set { _floorCount = value; }
    }

    public bool IsCustom {
        get { return _isCustom; }
    }

    public Dictionary<int, int[][]> ShadowMap {
        get { return _shadowMap; }
    }

    public int[,] GetTilesOfFloor(int floor) {
        return _tileLayers[floor];
    }

    public int GetTileAt(Point point, int floor) {
        return GetTileAt(point.X, point.Y, floor);
    }

    public int GetTileAt(int x, int y, int floor) {
        if (x < 0 || x > _width - 1 || y < 0 || y > _height - 1) return -1;
        if (floor < 0 || floor > _floorCount - 1) return -1;

        return _tileLayers[floor][y, x];
    }

    public bool SetTileAt(Point point, int floor, int tile) {
        return SetTileAt(point.X, point.Y, floor, tile);
    }

    public bool SetTileAt(int x, int y, int floor, int tile) {
        if (x < 0 || x > _width - 1 || y < 0 || y > _height - 1) return false;
        if (floor < 0 || floor > _floorCount - 1) return false;

        _tileLayers[floor][y, x] = tile;

        ComputeShadows();

        return true;
    }

    public int GetTilesetTileWidth() {
        return TILE_SIZE;
    }

    public int GetTilesetTileHeight() {
        return TILE_SIZE;
    }

    public bool ContainsWorldPos(Point point) {
        return point.X >= 0 && point.X < _width * TILE_SIZE && point.Y >= 0 && point.Y < _height * TILE_SIZE;
    }

    public bool ContainsTilePos(Point point) {
        return point.X >= 0 && point.X < _width && point.Y >= 0 && point.Y < _height;
    }

    public void ChangeTileset(TilesetData tileset) {
        TilesetData = tileset;
        ComputeShadows();
    }

}