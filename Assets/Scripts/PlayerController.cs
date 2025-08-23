using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private GridManager gridManager;
    public event Action OnMove;
    public event Action OnLevelComplete;

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
        Vector2Int direction = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = new Vector2Int(0, 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) direction = new Vector2Int(0, -1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = new Vector2Int(-1, 0);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) direction = new Vector2Int(1, 0);

        if (direction != Vector2Int.zero)
        {
            Move(direction);
        }
    }

    private void Move(Vector2Int direction)
    {
        if (gridManager == null) return;

        Vector2Int playerPos = gridManager.PlayerPosition;
        Vector2Int newPos = playerPos + direction;

        if (gridManager.IsValidPosition(newPos))
        {
            if (gridManager.Grid[newPos.x, newPos.y] == TileType.Empty || gridManager.Grid[newPos.x, newPos.y] == TileType.Target)
            {
                gridManager.UpdateGrid(playerPos, newPos, TileType.Player);
                gridManager.UpdatePlayerPosition(newPos);
                OnMove?.Invoke();
            }
            else if (gridManager.Grid[newPos.x, newPos.y] == TileType.Box)
            {
                Vector2Int boxNewPos = newPos + direction;
                if (gridManager.IsValidPosition(boxNewPos) && (gridManager.Grid[boxNewPos.x, boxNewPos.y] == TileType.Empty || gridManager.Grid[boxNewPos.x, boxNewPos.y] == TileType.Target))
                {
                    gridManager.UpdateGrid(newPos, boxNewPos, TileType.Box);
                    gridManager.UpdateGrid(playerPos, newPos, TileType.Player);
                    gridManager.UpdatePlayerPosition(newPos);
                    OnMove?.Invoke();
                }
            }

            if (gridManager.CheckWinCondition())
            {
                OnLevelComplete?.Invoke();
            }
        }
    }
}