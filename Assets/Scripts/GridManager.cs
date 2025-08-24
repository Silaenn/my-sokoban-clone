using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private GameObject emptyTilePrefab;
    [SerializeField] private float tileSize = 1f;

    private TileType[,] grid;
    private GameObject[,] gridObjects;
    private GameObject[,] backgroundTiles;
    private List<Vector2Int> targetPositions = new();
    private Vector2Int playerPosition;

    private Stack<(TileType[,] gridState, Vector2Int playerPos, Vector2Int[] boxPos)> moveHistory = new();

    public Vector2Int PlayerPosition => playerPosition;
    public TileType[,] Grid => grid;

    public event Action OnPlayerMoved;
    public event Action OnWinConditionMet;
    public event Action<string> OnLevelLoadError;

    private const float OffsetScale = 2.5f;
    private const float BackgroundZOffset = 0.1f;
    private const float TargetZOffset = 0.05f; // Z untuk target agar di belakang player/box

    public void InitializeLevel(SokobanLevel level)
    {
        if (!IsValidLevel(level))
        {
            OnLevelLoadError?.Invoke("Invalid level data or null level");
            return;
        }

        moveHistory.Clear();

        ClearGridObjectsAndTiles();
        InitializeGrid(level);
        CreateBackgroundTiles(level);
        PopulateGrid(level);
        SaveInitialState();
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < grid.GetLength(0) &&
               position.y >= 0 && position.y < grid.GetLength(1) &&
               grid[position.x, position.y] != TileType.Wall;
    }

    public void UpdateGrid(Vector2Int oldPos, Vector2Int newPos, TileType type)
    {
        if (!IsValidPosition(newPos) || !IsValidMove(newPos)) return;

        GameObject movingObject = gridObjects[oldPos.x, oldPos.y];
        if (movingObject == null) return;

        SaveMoveState();
        UpdateGridState(oldPos, newPos, type);
        AnimateMovement(movingObject, newPos);

        if (type == TileType.Player)
        {
            UpdatePlayerPosition(newPos);
            OnPlayerMoved?.Invoke();
            StartCoroutine(CheckWinAfterDelay(1.5f));
        }
    }

    public void UpdatePlayerPosition(Vector2Int newPosition)
    {
        playerPosition = newPosition;
    }

    public void UndoMove()
    {
        if (moveHistory.Count == 0) return;

        var lastState = moveHistory.Pop();
        TileType[,] prevGridState = lastState.gridState;
        Vector2Int prevPlayerPos = lastState.playerPos;
        Vector2Int[] prevBoxPos = lastState.boxPos;

        if (prevGridState.GetLength(0) != grid.GetLength(0) || prevGridState.GetLength(1) != grid.GetLength(1))
        {
            Debug.LogWarning("Invalid grid state in move history. Skipping undo.");
            return;
        }


        // Clear existing objects and tiles
        ClearGridObjectsAndTiles();

        // Restore grid state
        grid = (TileType[,])prevGridState.Clone();
        playerPosition = prevPlayerPos;

        // Recreate background tiles
        CreateBackgroundTiles(new SokobanLevel
        {
            width = grid.GetLength(0),
            height = grid.GetLength(1),
            gridData = ConvertGridToStringArray(),
            playerStartPosition = prevPlayerPos
        });

        // Rebuild grid objects
        RebuildGridObjects(prevPlayerPos, prevBoxPos);
    }

    public void RestartLevel(SokobanLevel level)
    {
        moveHistory.Clear();
        ClearGridObjectsAndTiles();
        InitializeLevel(level);
    }

    public bool CheckWinCondition()
    {
        bool isWin = targetPositions.All(target => grid[target.x, target.y] == TileType.Box);
        if (isWin) OnWinConditionMet?.Invoke();
        return isWin;
    }

    private bool IsValidLevel(SokobanLevel level)
    {
        return level != null &&
               level.gridData != null &&
               level.width > 0 &&
               level.height > 0 &&
               level.gridData.Length == level.height &&
               level.gridData.All(row => row.Length == level.width);
    }

    private void InitializeGrid(SokobanLevel level)
    {
        grid = new TileType[level.width, level.height];
        gridObjects = new GameObject[level.width, level.height];
        backgroundTiles = new GameObject[level.width, level.height];
        targetPositions.Clear();
        playerPosition = level.playerStartPosition;
    }

    private void CreateBackgroundTiles(SokobanLevel level)
    {
        Vector2 offset = CalculateOffset(level.width, level.height);
        for (int x = 0; x < level.width; x++)
        {
            for (int y = 0; y < level.height; y++)
            {
                Vector3 position = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, BackgroundZOffset);
                backgroundTiles[x, y] = Instantiate(emptyTilePrefab, position, Quaternion.identity, transform);
            }
        }
    }

    private void PopulateGrid(SokobanLevel level)
    {
        Vector2 offset = CalculateOffset(level.width, level.height);
        for (int y = 0; y < level.height; y++)
        {
            int flippedY = level.height - 1 - y;
            for (int x = 0; x < level.width; x++)
            {
                Vector3 position = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, 0);
                TileType type = GetTileType(level.gridData[flippedY][x]);

                grid[x, y] = type;

                if (type == TileType.Target)
                {
                    targetPositions.Add(new Vector2Int(x, y));
                }

                // Instantiate target always if it's a target position
                if (targetPositions.Contains(new Vector2Int(x, y)))
                {
                    Vector3 targetPos = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, TargetZOffset);
                    Instantiate(targetPrefab, targetPos, Quaternion.identity, transform);
                }

                // Then instantiate movable or wall
                if (type != TileType.Empty && type != TileType.Target)
                {
                    gridObjects[x, y] = Instantiate(GetPrefabForType(type), position, Quaternion.identity, transform);
                }
            }
        }
    }

    private bool IsValidMove(Vector2Int newPos)
    {
        return grid[newPos.x, newPos.y] == TileType.Empty || grid[newPos.x, newPos.y] == TileType.Target;
    }

    private void UpdateGridState(Vector2Int oldPos, Vector2Int newPos, TileType type)
    {
        // If newPos has an object (e.g., box if pushing), destroy it if necessary (but for target, it's statis, so no destroy)
        if (gridObjects[newPos.x, newPos.y] != null && type == TileType.Box) // Only for pushing box
        {
            Destroy(gridObjects[newPos.x, newPos.y]);
        }

        bool oldPosWasTarget = targetPositions.Contains(oldPos);
        grid[oldPos.x, oldPos.y] = oldPosWasTarget ? TileType.Target : TileType.Empty;
        grid[newPos.x, newPos.y] = type;
        gridObjects[newPos.x, newPos.y] = gridObjects[oldPos.x, oldPos.y];
        gridObjects[oldPos.x, oldPos.y] = null;
    }

    private void AnimateMovement(GameObject obj, Vector2Int newPos)
    {
        Vector2 offset = CalculateOffset(grid.GetLength(0), grid.GetLength(1));
        Vector3 targetPos = new Vector3(newPos.x * tileSize + offset.x, newPos.y * tileSize + offset.y, 0);
        StartCoroutine(MoveObject(obj.transform, targetPos, 0.2f));
    }

    public void ClearGridObjectsAndTiles()
    {
        DestroyGridObjects();
        DestroyBackgroundTiles();

        foreach (Transform child in transform)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void DestroyGridObjects()
    {
        if (gridObjects == null || grid == null) return;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (gridObjects[x, y] != null)
                {
                    Destroy(gridObjects[x, y]);
                }
            }
        }
        gridObjects = new GameObject[grid.GetLength(0), grid.GetLength(1)];
    }

    private void DestroyBackgroundTiles()
    {
        if (backgroundTiles == null || grid == null) return;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (backgroundTiles[x, y] != null)
                {
                    Destroy(backgroundTiles[x, y]);
                }
            }
        }
        backgroundTiles = new GameObject[grid.GetLength(0), grid.GetLength(1)];
    }

    private void ResetGridData()
    {
        grid = null;
        gridObjects = null;
        backgroundTiles = null;
        targetPositions.Clear();
    }

    private Vector2 CalculateOffset(int width, int height)
    {
        return new Vector2(-width * tileSize / OffsetScale, -height * tileSize / OffsetScale);
    }

    private GameObject GetPrefabForType(TileType type)
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

    private TileType GetTileType(char tile)
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

    private System.Collections.IEnumerator CheckWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CheckWinCondition();
    }

    private System.Collections.IEnumerator MoveObject(Transform obj, Vector3 targetPos, float duration)
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

    private void SaveInitialState()
    {
        TileType[,] initialGrid = (TileType[,])grid.Clone();
        Vector2Int[] initialBoxPos = GetBoxPositions();
        moveHistory.Push((initialGrid, playerPosition, initialBoxPos));
    }

    private void SaveMoveState()
    {
        TileType[,] currentGrid = (TileType[,])grid.Clone();
        Vector2Int[] boxPos = GetBoxPositions();
        moveHistory.Push((currentGrid, playerPosition, boxPos));
    }

    private Vector2Int[] GetBoxPositions()
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

    private void RebuildGridObjects(Vector2Int playerPos, Vector2Int[] boxPos)
    {
        Vector2 offset = CalculateOffset(grid.GetLength(0), grid.GetLength(1));
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector3 basePos = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, 0);

                // Always instantiate target if it's a target position
                if (targetPositions.Contains(pos))
                {
                    Vector3 targetPos = new Vector3(basePos.x, basePos.y, TargetZOffset);
                    Instantiate(targetPrefab, targetPos, Quaternion.identity, transform);
                    // Do not set grid to Target if there's something on it; keep the state
                }

                // Instantiate wall if wall
                if (grid[x, y] == TileType.Wall)
                {
                    gridObjects[x, y] = Instantiate(wallPrefab, basePos, Quaternion.identity, transform);
                }

                // Instantiate box if box
                if (Array.Exists(boxPos, bPos => bPos == pos))
                {
                    grid[x, y] = TileType.Box; // Restore if needed
                    gridObjects[x, y] = Instantiate(boxPrefab, basePos, Quaternion.identity, transform);
                }

                // Instantiate player if player
                if (pos == playerPos)
                {
                    grid[x, y] = TileType.Player;
                    gridObjects[x, y] = Instantiate(playerPrefab, basePos, Quaternion.identity, transform);
                }
            }
        }
    }

    private string[] ConvertGridToStringArray()
    {
        string[] gridData = new string[grid.GetLength(1)];
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            char[] row = new char[grid.GetLength(0)];
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                row[x] = GetCharForTileType(grid[x, grid.GetLength(1) - 1 - y]); // Flip Y for consistency
            }
            gridData[y] = new string(row);
        }
        return gridData;
    }

    private char GetCharForTileType(TileType type)
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