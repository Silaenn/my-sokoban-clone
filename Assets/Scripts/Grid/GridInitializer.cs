using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridInitializer
{
    readonly GridManager gridManager;
    readonly GameObject wallPrefab;
    readonly GameObject playerPrefab;
    readonly GameObject boxPrefab;
    readonly GameObject targetPrefab;
    readonly GameObject emptyTilePrefab;
    readonly float tileSize;
    const float OffsetScale = 2.5f;
    const float BackgroundZOffset = 0.1f;
    public const float TargetZOffset = 0.05f;

    public GridInitializer(GridManager gridManager, GameObject wallPrefab, GameObject playerPrefab,
    GameObject boxPrefab, GameObject targetPrefab, GameObject emptyTilePrefab, float tileSize)
    {
        this.gridManager = gridManager;
        this.wallPrefab = wallPrefab;
        this.playerPrefab = playerPrefab;
        this.boxPrefab = boxPrefab;
        this.targetPrefab = targetPrefab;
        this.emptyTilePrefab = emptyTilePrefab;
        this.tileSize = tileSize;
    }

    public bool IsValidLevel(SokobanLevel level)
    {
        return level != null &&
               level.gridData != null &&
               level.width > 0 &&
               level.height > 0 &&
               level.gridData.Length == level.height &&
               level.gridData.All(row => row.Length == level.width);
    }

    public void InitializeGrid(SokobanLevel level, ref TileType[,] grid, ref GameObject[,] gridObjects,
        ref GameObject[,] backgroundTiles, ref List<Vector2Int> targetPositions, ref Vector2Int playerPosition)
    {
        grid = new TileType[level.width, level.height];
        gridObjects = new GameObject[level.width, level.height];
        backgroundTiles = new GameObject[level.width, level.height];
        targetPositions.Clear();
        playerPosition = level.playerStartPosition;

        CreateBackgroundTiles(level, backgroundTiles);
        PopulateGrid(level, grid, gridObjects, targetPositions);
    }

    public void ClearGridObjectsAndTiles()
    {
        DestroyGridObjects();
        DestroyBackgroundTiles();

        foreach (Transform child in gridManager.transform)
        {
            if (child != null)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }

    public void CreateBackgroundTiles(SokobanLevel level, GameObject[,] backgroundTiles)
    {
        Vector2 offset = GridUtils.CalculateOffset(level.width, level.height, tileSize);
        for (int x = 0; x < level.width; x++)
        {
            for (int y = 0; y < level.height; y++)
            {
                Vector3 position = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, BackgroundZOffset);
                backgroundTiles[x, y] = UnityEngine.Object.Instantiate(emptyTilePrefab, position, Quaternion.identity, gridManager.transform);
            }
        }
    }

    private void PopulateGrid(SokobanLevel level, TileType[,] grid, GameObject[,] gridObjects, List<Vector2Int> targetPositions)
    {
        Vector2 offset = GridUtils.CalculateOffset(level.width, level.height, tileSize);
        for (int y = 0; y < level.height; y++)
        {
            int flippedY = level.height - 1 - y;
            for (int x = 0; x < level.width; x++)
            {
                Vector3 position = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, 0);

                TileType type = GridUtils.GetTileType(level.gridData[flippedY][x]);

                grid[x, y] = type;

                if (type == TileType.Target)
                {
                    targetPositions.Add(new Vector2Int(x, y));
                }

                if (targetPositions.Contains(new Vector2Int(x, y)))
                {
                    Vector3 targetPos = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, TargetZOffset);

                    UnityEngine.Object.Instantiate(targetPrefab, targetPos, Quaternion.identity, gridManager.transform);
                }

                if (type != TileType.Empty && type != TileType.Target)
                {
                    gridObjects[x, y] = UnityEngine.Object.Instantiate(GridUtils.GetPrefabForType(type, wallPrefab, playerPrefab, boxPrefab, targetPrefab, emptyTilePrefab),
                        position, Quaternion.identity, gridManager.transform);
                }
                
            }
        }
    }

    private void DestroyGridObjects()
    {
        if (gridManager.Grid == null || gridManager.Grid.Length == 0) return;
        for (int x = 0; x < gridManager.Grid.GetLength(0); x++)
        {
            for (int y = 0; y < gridManager.Grid.GetLength(1); y++)
            {
                if (gridManager.GridObjects[x, y] != null)
                {
                    UnityEngine.Object.Destroy(gridManager.GridObjects[x, y]);
                }
            }
        }
        gridManager.GridObjects = new GameObject[gridManager.Grid.GetLength(0), gridManager.Grid.GetLength(1)];
    }

    private void DestroyBackgroundTiles()
    {
        if (gridManager.Grid == null || gridManager.Grid.Length == 0) return;
        for (int x = 0; x < gridManager.Grid.GetLength(0); x++)
        {
            for (int y = 0; y < gridManager.Grid.GetLength(1); y++)
            {
                if (gridManager.BackgroundTiles[x, y] != null)
                {
                    UnityEngine.Object.Destroy(gridManager.BackgroundTiles[x, y]);
                }
            }
        }
        gridManager.BackgroundTiles = new GameObject[gridManager.Grid.GetLength(0), gridManager.Grid.GetLength(1)];
    }
}
