using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveHistoryManager
{
    private readonly GridManager gridManager;
    private readonly GridInitializer gridInitializer;
    private readonly Stack<(TileType[,] gridState, Vector2Int playerPos, Vector2Int[] boxPos)> moveHistory = new();

    public MoveHistoryManager(GridManager gridManager, GridInitializer gridInitializer)
    {
        this.gridManager = gridManager;
        this.gridInitializer = gridInitializer;
    }

    public void SaveInitialState(TileType[,] grid, Vector2Int playerPosition)
    {
        TileType[,] initialGrid = (TileType[,])grid.Clone();
        Vector2Int[] initialBoxPos = GetBoxPositions(grid);
        moveHistory.Push((initialGrid, playerPosition, initialBoxPos));
    }

    public void SaveMoveState(TileType[,] grid, Vector2Int playerPosition)
    {
        TileType[,] currentGrid = (TileType[,])grid.Clone();
        Vector2Int[] boxPos = GetBoxPositions(grid);
        moveHistory.Push((currentGrid, playerPosition, boxPos));
    }

    public void UndoMove(ref TileType[,] grid, ref GameObject[,] gridObjects, ref GameObject[,] backgroundTiles,
        ref Vector2Int playerPosition, List<Vector2Int> targetPositions)
    {
        if (moveHistory.Count <= 1)
        {
            Debug.Log("No more moves to undo. Already at initial state.");
            return;
        }

        // Pop state terakhir (langkah sebelumnya)
        moveHistory.Pop(); // Buang state saat ini
        var lastState = moveHistory.Peek(); // Ambil state sebelumnya
        TileType[,] prevGridState = lastState.gridState;
        Vector2Int prevPlayerPos = lastState.playerPos;
        Vector2Int[] prevBoxPos = lastState.boxPos;

        if (prevGridState.GetLength(0) != grid.GetLength(0) || prevGridState.GetLength(1) != grid.GetLength(1))
        {
            Debug.LogWarning("Invalid grid state in move history. Skipping undo.");
            return;
        }

        // Bersihkan grid dan objek saat ini
        gridInitializer.ClearGridObjectsAndTiles();
        grid = (TileType[,])prevGridState.Clone();
        playerPosition = prevPlayerPos;

        // Buat ulang background tiles
        gridInitializer.CreateBackgroundTiles(new SokobanLevel
        {
            width = grid.GetLength(0),
            height = grid.GetLength(1),
            gridData = GridUtils.ConvertGridToStringArray(grid),
            playerStartPosition = prevPlayerPos
        }, backgroundTiles);

        // Bangun ulang objek grid
        RebuildGridObjects(prevPlayerPos, prevBoxPos, grid, gridObjects, targetPositions);
    }

    public void ClearHistory()
    {
        moveHistory.Clear();
    }

    private Vector2Int[] GetBoxPositions(TileType[,] grid)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y] == TileType.Box)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }
        return positions.ToArray();
    }

    private void RebuildGridObjects(Vector2Int playerPos, Vector2Int[] boxPos, TileType[,] grid,
        GameObject[,] gridObjects, List<Vector2Int> targetPositions)
    {
        Vector2 offset = GridUtils.CalculateOffset(grid.GetLength(0), grid.GetLength(1), gridManager.TileSize);
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector3 basePos = new Vector3(x * gridManager.TileSize + offset.x, y * gridManager.TileSize + offset.y, 0);

                if (targetPositions.Contains(pos))
                {
                    Vector3 targetPos = new Vector3(basePos.x, basePos.y, GridInitializer.TargetZOffset);
                    UnityEngine.Object.Instantiate(gridManager.TargetPrefab, targetPos, Quaternion.identity, gridManager.transform);
                }

                if (grid[x, y] == TileType.Wall)
                {
                    gridObjects[x, y] = UnityEngine.Object.Instantiate(gridManager.WallPrefab, basePos, Quaternion.identity, gridManager.transform);
                }

                if (Array.Exists(boxPos, bPos => bPos == pos))
                {
                    grid[x, y] = TileType.Box;
                    gridObjects[x, y] = UnityEngine.Object.Instantiate(gridManager.BoxPrefab, basePos, Quaternion.identity, gridManager.transform);
                }

                if (pos == playerPos)
                {
                    grid[x, y] = TileType.Player;
                    gridObjects[x, y] = UnityEngine.Object.Instantiate(gridManager.PlayerPrefab, basePos, Quaternion.identity, gridManager.transform);
                }
            }
        }
    }
}
