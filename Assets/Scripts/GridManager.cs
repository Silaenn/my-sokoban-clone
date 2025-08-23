using System;
using System.Collections;
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
    [SerializeField] private int initialPoolSize = 10;

    private TileType[,] grid;
    private GameObject[,] gridObjects;
    private GameObject[,] targetObjects; // Menyimpan target terpisah
    private List<Vector2Int> targetPositions;
    private Vector2Int playerPosition;
    private Dictionary<TileType, Queue<GameObject>> objectPool;

    public Vector2Int PlayerPosition => playerPosition;
    public TileType[,] Grid => grid;

    public event Action OnPlayerMoved;
    public event Action OnWinConditionMet;
    public event Action<string> OnLevelLoadError;

    private const float OffsetScale = 2.5f;
    private const float TargetZOffset = 0.1f; // Untuk memastikan target di belakang box/player

    private void Awake()
    {
        InitializeObjectPool();
    }

    /// <summary>
    /// Initializes the object pool for each tile type.
    /// </summary>
    private void InitializeObjectPool()
    {
        objectPool = new Dictionary<TileType, Queue<GameObject>>
        {
            { TileType.Wall, new Queue<GameObject>() },
            { TileType.Player, new Queue<GameObject>() },
            { TileType.Box, new Queue<GameObject>() },
            { TileType.Empty, new Queue<GameObject>() }
        };

        // Pool untuk target disimpan terpisah karena dikelola secara berbeda
        objectPool.Add(TileType.Target, new Queue<GameObject>());

        foreach (TileType type in Enum.GetValues(typeof(TileType)))
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(GetPrefabForType(type));
                obj.SetActive(false);
                objectPool[type].Enqueue(obj);
            }
        }
    }

    /// <summary>
    /// Initializes the game grid based on the provided Sokoban level data.
    /// </summary>
    public void InitializeLevel(SokobanLevel level)
    {
        if (level == null || level.gridData == null || level.width <= 0 || level.height <= 0 || level.gridData.Length != level.height)
        {
            string errorMessage = "Invalid level data or level is null";
            Debug.LogError(errorMessage);
            OnLevelLoadError?.Invoke(errorMessage);
            return;
        }

        ClearGrid(); // Clear previous grid if any

        grid = new TileType[level.width, level.height];
        gridObjects = new GameObject[level.width, level.height];
        targetObjects = new GameObject[level.width, level.height]; // Array untuk target
        targetPositions = new List<Vector2Int>();
        playerPosition = level.playerStartPosition;

        Vector2 offset = CalculateOffset(level.width, level.height);

        for (int y = 0; y < level.height; y++)
        {
            if (y >= level.gridData.Length || level.gridData[y].Length != level.width)
            {
                Debug.LogError($"Invalid grid data at row {y}");
                continue;
            }

            for (int x = 0; x < level.width; x++)
            {
                Vector3 position = new Vector3(x * tileSize + offset.x, y * tileSize + offset.y, 0);
                char tile = level.gridData[y][x];
                TileType type = TileType.Empty;

                switch (tile)
                {
                    case 'W':
                        type = TileType.Wall;
                        break;
                    case 'P':
                        type = TileType.Player;
                        break;
                    case 'B':
                        type = TileType.Box;
                        break;
                    case 'T':
                        // Target tidak disimpan di grid, hanya di targetObjects
                        targetPositions.Add(new Vector2Int(x, y));
                        targetObjects[x, y] = GetPooledObject(TileType.Target, new Vector3(position.x, position.y, TargetZOffset), Quaternion.identity);
                        break;
                }

                grid[x, y] = type;
                gridObjects[x, y] = GetPooledObject(type, position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Checks if the given position is valid within the grid and not a wall.
    /// </summary>
    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1) && grid[pos.x, pos.y] != TileType.Wall;
    }

    /// <summary>
    /// Updates the grid by moving an entity from old position to new position.
    /// </summary>
    public void UpdateGrid(Vector2Int oldPos, Vector2Int newPos, TileType type)
    {
        if (!IsValidPosition(newPos)) return;

        GameObject obj = gridObjects[oldPos.x, oldPos.y];
        if (obj == null) return;

        grid[oldPos.x, oldPos.y] = TileType.Empty;
        grid[newPos.x, newPos.y] = type;

        Vector2 offset = CalculateOffset(grid.GetLength(0), grid.GetLength(1));
        SetEmptyTile(oldPos, offset);

        // Hanya kembalikan objek ke pool jika bukan target
        GameObject targetObj = gridObjects[newPos.x, newPos.y];
        if (targetObj != null && grid[newPos.x, newPos.y] != TileType.Empty)
        {
            ReturnToPool(targetObj, grid[newPos.x, newPos.y]);
        }

        gridObjects[newPos.x, newPos.y] = obj;

        Vector3 targetPos = new Vector3(newPos.x * tileSize + offset.x, newPos.y * tileSize + offset.y, 0);
        StartCoroutine(MoveObject(obj.transform, targetPos, 0.2f));

        if (type == TileType.Player)
        {
            UpdatePlayerPosition(newPos);
            OnPlayerMoved?.Invoke();
        }
    }

    /// <summary>
    /// Updates the player's position.
    /// </summary>
    public void UpdatePlayerPosition(Vector2Int newPos)
    {
        playerPosition = newPos;
    }

    /// <summary>
    /// Checks if all target positions have boxes on them.
    /// </summary>
    public bool CheckWinCondition()
    {
        bool isWin = targetPositions.All(target => grid[target.x, target.y] == TileType.Box);
        if (isWin) OnWinConditionMet?.Invoke();
        return isWin;
    }

    /// <summary>
    /// Clears the grid and returns all objects to the pool.
    /// </summary>
    public void ClearGrid()
    {
        if (gridObjects == null) return;

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (gridObjects[x, y] != null)
                {
                    ReturnToPool(gridObjects[x, y], grid[x, y]);
                    gridObjects[x, y] = null;
                }
                if (targetObjects[x, y] != null)
                {
                    ReturnToPool(targetObjects[x, y], TileType.Target);
                    targetObjects[x, y] = null;
                }
            }
        }

        grid = null;
        gridObjects = null;
        targetObjects = null;
        targetPositions?.Clear();
    }

    private Vector2 CalculateOffset(int width, int height)
    {
        return new Vector2(-width * tileSize / OffsetScale, -height * tileSize / OffsetScale);
    }

    private void SetEmptyTile(Vector2Int pos, Vector2 offset)
    {
        if (gridObjects[pos.x, pos.y] != null && grid[pos.x, pos.y] != TileType.Empty)
        {
            ReturnToPool(gridObjects[pos.x, pos.y], grid[pos.x, pos.y]);
        }

        gridObjects[pos.x, pos.y] = GetPooledObject(TileType.Empty,
            new Vector3(pos.x * tileSize + offset.x, pos.y * tileSize + offset.y, 0),
            Quaternion.identity);
    }

    private GameObject GetPooledObject(TileType type, Vector3 position, Quaternion rotation)
    {
        if (!objectPool.ContainsKey(type)) objectPool[type] = new Queue<GameObject>();

        GameObject obj;
        if (objectPool[type].Count > 0)
        {
            obj = objectPool[type].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.transform.SetParent(transform);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(GetPrefabForType(type), position, rotation, transform);
        }

        return obj;
    }

    private void ReturnToPool(GameObject obj, TileType type)
    {
        if (obj == null) return;
        obj.SetActive(false);
        objectPool[type].Enqueue(obj);
    }

    private GameObject GetPrefabForType(TileType type)
    {
        switch (type)
        {
            case TileType.Wall: return wallPrefab;
            case TileType.Player: return playerPrefab;
            case TileType.Box: return boxPrefab;
            case TileType.Target: return targetPrefab;
            case TileType.Empty: return emptyTilePrefab;
            default: return emptyTilePrefab;
        }
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