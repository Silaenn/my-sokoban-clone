using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private GridManager gridManager;
    public event Action OnMove;

    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in scene");
        }
    }

    private void Update()
    {
        Vector2Int direction = GetInputDirection();
        if (direction != Vector2Int.zero)
        {
            Move(direction);
        }
        HandleUndo();
        HandleRestart();
    }

    private Vector2Int GetInputDirection()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) return new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) return new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) return new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.RightArrow)) return new Vector2Int(1, 0);
        return Vector2Int.zero;
    }

    private void Move(Vector2Int direction)
    {
        if (gridManager == null) return;

        Vector2Int playerPos = gridManager.PlayerPosition;
        Vector2Int newPos = playerPos + direction;

        if (!gridManager.IsValidPosition(newPos)) return;

        if (gridManager.Grid[newPos.x, newPos.y] is TileType.Empty or TileType.Target)
        {
            gridManager.UpdateGrid(playerPos, newPos, TileType.Player);
            OnMove?.Invoke();
        }
        else if (gridManager.Grid[newPos.x, newPos.y] == TileType.Box)
        {
            Vector2Int boxNewPos = newPos + direction;
            if (gridManager.IsValidPosition(boxNewPos) && 
                gridManager.Grid[boxNewPos.x, boxNewPos.y] is TileType.Empty or TileType.Target)
            {
                gridManager.UpdateGrid(newPos, boxNewPos, TileType.Box);
                gridManager.UpdateGrid(playerPos, newPos, TileType.Player);
                OnMove?.Invoke();
            }
        }
    }

    private void HandleUndo()
    {
        if (Input.GetKeyDown(KeyCode.U)) // Tombol 'U' untuk Undo
        {
            gridManager.UndoMove();
        }
    }

    private void HandleRestart()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Tombol 'R' untuk Restart
        {
            if (FindObjectOfType<GameManager>() is GameManager gameManager)
            {
                gameManager.RestartCurrentLevel();
            }
        }
    }
}