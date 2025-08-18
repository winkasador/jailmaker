using System;
using System.Collections.Generic;

namespace Jailbreak.World.Generation;

public class ComplexGenerator : IGenerator {

    private Random _random;
    private int _iterations = 100;

    public ComplexGenerator() {
        _random = new Random();
    }

    public ComplexGenerator(int seed) {
        _random = new Random(seed);
    }

    public void Apply(Map map) {
        GenerateChaoticRegions(map);
    }

    private void GenerateChaoticRegions(Map map) {
        int[,] cells = new int[map.Height, map.Width];

        for (int y = 0; y < map.Height; y++) {
            for (int x = 0; x < map.Width; x++) {
                cells[y, x] = _random.Next(0, 2);
            }
        }

        int[,] nextCells = new int[map.Height, map.Width];

        for(int i = 0; i < _iterations; i++) {
            for (int y = 0; y < map.Height; y++) {
                for (int x = 0; x < map.Width; x++) {
                    int wallCount = CountNeighboringWalls(cells, x, y, map.Width, map.Height);
                    if (wallCount >= 2 && wallCount <= 4)
                        nextCells[y, x] = 1;
                    else if(wallCount >= 7) 
                        nextCells[y, x] = _random.NextDouble() > 0.6 ? 1 : 0;
                    else
                        nextCells[y, x] = 0;
                }
            }

            Array.Copy(nextCells, cells, cells.Length);
        }

        FixDiagonalConnections(cells, map.Width, map.Height);

        int[,] tiles = map.GetTilesOfFloor(1);

        for(int y = 0; y < map.Height; y++) {
            for(int x = 0; x < map.Width; x++) {
                tiles[y,x] = cells[y,x] == 0 ? 1 : 27;
            }
        }
    }

    private int CountNeighboringWalls(int[,] cells, int x, int y, int width, int height) {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height) {
                    if(cells[ny, nx] == 1) {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    private void FixDiagonalConnections(int[,] map, int width, int height) {
        Random rand = new Random();

        for (int y = 1; y < height - 1; y++) {
            for (int x = 1; x < width - 1; x++) {
                if (map[y, x] == 0) {
                    if (map[y - 1, x - 1] == 0 && AreMutualNeighborsWalls(map, x, y, x - 1, y - 1)) // Top-left
                        ResolveDiagonalConflict(map, x, y, x - 1, y - 1, rand);
                    if (map[y - 1, x + 1] == 0 && AreMutualNeighborsWalls(map, x, y, x + 1, y - 1)) // Top-right
                        ResolveDiagonalConflict(map, x, y, x + 1, y - 1, rand);
                    if (map[y + 1, x - 1] == 0 && AreMutualNeighborsWalls(map, x, y, x - 1, y + 1)) // Bottom-left
                        ResolveDiagonalConflict(map, x, y, x - 1, y + 1, rand);
                    if (map[y + 1, x + 1] == 0 && AreMutualNeighborsWalls(map, x, y, x + 1, y + 1)) // Bottom-right
                        ResolveDiagonalConflict(map, x, y, x + 1, y + 1, rand);
                }
            }
        }
    }

    private bool AreMutualNeighborsWalls(int[,] map, int x1, int y1, int x2, int y2)
    {
       var neighbors1 = new List<int[]> 
        {
            new int[] { x1 - 1, y1 },
            new int[] { x1 + 1, y1 },
            new int[] { x1, y1 - 1 },
            new int[] { x1, y1 + 1 }
        };

        var neighbors2 = new List<int[]> 
        {
            new int[] { x2 - 1, y2 },
            new int[] { x2 + 1, y2 },
            new int[] { x2, y2 - 1 },
            new int[] { x2, y2 + 1 }
        };

        foreach (var n1 in neighbors1)
        {
            foreach (var n2 in neighbors2)
            {
                if (n1[0] == n2[0] && n1[1] == n2[1] && map[n1[1], n1[0]] == 0)
                    return false;
            }
        }

        return true;
    }

    void ResolveDiagonalConflict(int[,] map, int x1, int y1, int x2, int y2, Random rand) {
        if (rand.NextDouble() < 0.5) {
            // Collapse one of the floors into a wall
            if (rand.NextDouble() < 0.5)
                map[y1, x1] = 1; // Turn first tile into a wall
            else
                map[y2, x2] = 1; // Turn second tile into a wall
        }
        else {
            // Collapse a mutual neighbor into a floor
            if (x1 == x2) {
                if (y1 < y2)
                    map[y1 + 1, x1] = 0; // Make bottom neighbor a floor
                else
                    map[y2 + 1, x2] = 0; // Make top neighbor a floor
            }
            else if (y1 == y2) {
                if (x1 < x2)
                    map[y1, x1 + 1] = 0; // Make right neighbor a floor
                else
                    map[y2, x2 + 1] = 0; // Make left neighbor a floor
            }
        }
    }
}