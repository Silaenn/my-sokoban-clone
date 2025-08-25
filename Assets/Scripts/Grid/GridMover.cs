using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMover
{
    private readonly GridManager gridManager;
    private readonly float tileSize;

    public GridMover(GridManager gridManager, float tileSize)
    {
        this.gridManager = gridManager;
        this.tileSize = tileSize;
    }

    public void UpdateGrid(Vector2Int oldPos, Vector2Int newPos, TileType type, TileType[,] grid,
        GameObject[,] gridObjects, List<Vector2Int> targetPositions, ref Vector2Int playerPosition,
        Action onPlayerMoved, WinConditionChecker winConditionChecker, MoveHistoryManager moveHistoryManager)
    {
        if (!GridUtils.IsValidPosition(newPos, grid) || !IsValidMove(newPos, grid)) return;

        GameObject movingObject = gridObjects[oldPos.x, oldPos.y];
        if (movingObject == null) return;

        // Simpan state sebelum pergerakan (state saat ini sebelum move)

        UpdateGridState(oldPos, newPos, type, grid, gridObjects, targetPositions);
        AnimateMovement(movingObject, newPos);

        if (type == TileType.Player)
        {
            playerPosition = newPos;
            onPlayerMoved?.Invoke();
            moveHistoryManager.SaveMoveState(grid, playerPosition);
            gridManager.StartCoroutine(winConditionChecker.CheckWinAfterDelay(1.5f));
        }
    }

    private bool IsValidMove(Vector2Int newPos, TileType[,] grid)
    {
        return grid[newPos.x, newPos.y] == TileType.Empty || grid[newPos.x, newPos.y] == TileType.Target;
    }

    private void UpdateGridState(Vector2Int oldPos, Vector2Int newPos, TileType type,
        TileType[,] grid, GameObject[,] gridObjects, List<Vector2Int> targetPositions)
    {
        if (gridObjects[newPos.x, newPos.y] != null && type == TileType.Box)
        {
            UnityEngine.Object.Destroy(gridObjects[newPos.x, newPos.y]);
        }

        bool oldPosWasTarget = targetPositions.Contains(oldPos);
        grid[oldPos.x, oldPos.y] = oldPosWasTarget ? TileType.Target : TileType.Empty;
        grid[newPos.x, newPos.y] = type;
        gridObjects[newPos.x, newPos.y] = gridObjects[oldPos.x, oldPos.y];
        gridObjects[oldPos.x, oldPos.y] = null;
    }

    private void AnimateMovement(GameObject obj, Vector2Int newPos)
    {
        Vector2 offset = GridUtils.CalculateOffset(gridManager.Grid.GetLength(0), gridManager.Grid.GetLength(1), tileSize);
        Vector3 targetPos = new Vector3(newPos.x * tileSize + offset.x, newPos.y * tileSize + offset.y, 0);
        gridManager.StartCoroutine(MoveObject(obj.transform, targetPos, 0.2f));
    }

    private IEnumerator MoveObject(Transform obj, Vector3 targetPos, float duration)
    {
        if (obj == null) yield break;

        Vector3 startPos = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            obj.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        if (obj != null) obj.position = targetPos;
    }
}
