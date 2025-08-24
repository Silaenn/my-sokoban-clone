using UnityEngine;

public static class GridUtils
{
    public static Vector2 CalculateOffset(int width, int height, float tileSize)
    {
        const float OffsetScale = 2.5f; return new Vector2(-width * tileSize / OffsetScale, -height * tileSize / OffsetScale);
    }
    public static GameObject GetPrefabForType(TileType type, GameObject wallPrefab, GameObject playerPrefab,
    GameObject boxPrefab, GameObject targetPrefab, GameObject emptyTilePrefab)
    {
        return type switch
        {
            TileType.Wall => wallPrefab,
            TileType.Player => playerPrefab,
            TileType.Box => boxPrefab,
            TileType.Target => targetPrefab,
            _ => emptyTilePrefab
        };
    }

    public static TileType GetTileType(char tile)
    {
        return tile switch
        {
            'W' => TileType.Wall,
            'P' => TileType.Player,
            'B' => TileType.Box,
            'T' => TileType.Target,
            _ => TileType.Empty
        };
    }

    public static bool IsValidPosition(Vector2Int position, TileType[,] grid)
    {
        return position.x >= 0 && position.x < grid.GetLength(0) &&
               position.y >= 0 && position.y < grid.GetLength(1) &&
               grid[position.x, position.y] != TileType.Wall;
    }
    public static string[] ConvertGridToStringArray(TileType[,] grid)
    {
        string[] gridData = new string[grid.GetLength(1)];
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            char[] row = new char[grid.GetLength(0)];
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                row[x] = GetCharForTileType(grid[x, grid.GetLength(1) - 1 - y]);
            }
            gridData[y] = new string(row);
        }
        return gridData;
    }
    private static char GetCharForTileType(TileType type)
    {
        return type switch
        {
            TileType.Wall => 'W',
            TileType.Player => 'P',
            TileType.Box => 'B',
            TileType.Target => 'T',
            _ => 'O'
        };
    }
}