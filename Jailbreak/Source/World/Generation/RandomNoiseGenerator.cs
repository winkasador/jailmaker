using System;

namespace Jailbreak.World.Generation;

public class RandomNoiseGenerator : IGenerator
{

    private Random _random;

    public RandomNoiseGenerator() {
        _random = new Random();
    }

    public RandomNoiseGenerator(int seed) {
        _random = new Random(seed);
    }

    public void Apply(Map map) {
        int tilesetSize = map.TilesetData.TileCount;

        for(int floor = 0; floor < map.FloorCount; floor++) {
            int[,] tiles = map.GetTilesOfFloor(floor);
            for(int y = 0; y < map.Height; y++) {
                for(int x = 0; x < map.Width; x++) {
                    tiles[y,x] = _random.Next(1, tilesetSize);
                }
            }
        }
    }

}