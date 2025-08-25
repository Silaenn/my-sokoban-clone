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
        Vector2Int[] initialBoxPos = GetBoxPositions(grid);
        TileType[,] initialGrid = (TileType[,])grid.Clone();
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

        // Pop state saat ini dan ambil state sebelumnya
        moveHistory.Pop(); // Buang state saat ini
        var previousState = moveHistory.Peek(); // Ambil state sebelumnya tanpa menghapusnya
        
        TileType[,] prevGridState = previousState.gridState;
        Vector2Int prevPlayerPos = previousState.playerPos;
        Vector2Int[] prevBoxPos = previousState.boxPos;

        if (prevGridState.GetLength(0) != grid.GetLength(0) || prevGridState.GetLength(1) != grid.GetLength(1))
        {
            Debug.LogWarning("Invalid grid state in move history. Skipping undo.");
            return;
        }

        // Update grid state
        grid = (TileType[,])prevGridState.Clone();
        playerPosition = prevPlayerPos;

        // Optimized: Hanya update posisi objects yang berubah, tidak recreate semua
        UpdateObjectPositions(prevPlayerPos, prevBoxPos, gridObjects, targetPositions);
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

    private void UpdateObjectPositions(Vector2Int playerPos, Vector2Int[] boxPos, GameObject[,] gridObjects, List<Vector2Int> targetPositions)
    {
        Vector2 offset = GridUtils.CalculateOffset(gridObjects.GetLength(0), gridObjects.GetLength(1), gridManager.TileSize);

        // Clear current movable objects (player and boxes)
        for (int x = 0; x < gridObjects.GetLength(0); x++)
        {
            for (int y = 0; y < gridObjects.GetLength(1); y++)
            {
                GameObject obj = gridObjects[x, y];
                if (obj != null)
                {
                    // Check if it's a player or box object (not wall)
                    if (obj.name.Contains("Player") || obj.name.Contains("Box"))
                    {
                        UnityEngine.Object.Destroy(obj);
                        gridObjects[x, y] = null;
                    }
                }
            }
        }

        // Place player at previous position
        Vector3 playerWorldPos = new Vector3(
            playerPos.x * gridManager.TileSize + offset.x,
            playerPos.y * gridManager.TileSize + offset.y,
            0
        );
        gridObjects[playerPos.x, playerPos.y] = UnityEngine.Object.Instantiate(
            gridManager.PlayerPrefab,
            playerWorldPos,
            Quaternion.identity,
            gridManager.transform
        );

        // Place boxes at previous positions
        foreach (Vector2Int box in boxPos)
        {
            Vector3 boxWorldPos = new Vector3(
                box.x * gridManager.TileSize + offset.x,
                box.y * gridManager.TileSize + offset.y,
                0
            );
            gridObjects[box.x, box.y] = UnityEngine.Object.Instantiate(
                gridManager.BoxPrefab,
                boxWorldPos,
                Quaternion.identity,
                gridManager.transform
            );
        }
    }
}