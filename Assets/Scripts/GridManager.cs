using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject boxPrefab;
    [SerializeField] GameObject targetPrefab;
    [SerializeField] GameObject emptytPrefab;
    [SerializeField] float tileSize = 1f;

    TileType[,] grid;
    GameObject[,] gridObjects;
    bool[,] targetGrid;
    List<Vector2Int> targetPositions;
    Vector2Int playerPosition;

    public Vector2Int PlayerPosition => playerPosition;
    public TileType[,] Grid => grid;

    public void InitializeLevel(SokobanLevel level)
    {
        if (level == null || level.gridData == null || level.width <= 0 || level.height <= 0 || level.gridData.Length != level.height)
        {
            Debug.LogError("Invalid level data or level is null");
            return;
        }

        grid = new TileType[level.width, level.height];
        gridObjects = new GameObject[level.width, level.height];
        targetGrid = new bool[level.width, level.height];
        targetPositions = new List<Vector2Int>();
        playerPosition = level.playerStartPosition;

        float offsetX = -level.width * tileSize / 2.5f;
        float offsetY = -level.height * tileSize / 2.5f;

        for (int y = 0; y < level.height; y++)
        {
            if (y >= level.gridData.Length || level.gridData[y].Length != level.width)
            {
                Debug.LogError($"Invalid grid data at row {y}");
                continue;
            }

            for (int x = 0; x < level.width; x++)
            {
                Vector3 position = new Vector3(x * tileSize + offsetX, y * tileSize + offsetY, 0);
                char tile = level.gridData[y][x];

                switch (tile)
                {
                    case 'W':
                        grid[x, y] = TileType.Wall;
                        gridObjects[x, y] = Instantiate(wallPrefab, position, Quaternion.identity, transform);
                        break;
                    case 'P':
                        grid[x, y] = TileType.Player;
                        gridObjects[x, y] = Instantiate(playerPrefab, position, Quaternion.identity, transform);
                        break;
                    case 'B':
                        grid[x, y] = TileType.Box;
                        gridObjects[x, y] = Instantiate(boxPrefab, position, Quaternion.identity, transform);
                        break;
                    case 'T':
                        grid[x, y] = TileType.Empty;
                        gridObjects[x, y] = Instantiate(targetPrefab, position, Quaternion.identity, transform);
                        targetGrid[x, y] = true;
                        targetPositions.Add(new Vector2Int(x, y));
                        break;
                    default:
                        grid[x, y] = TileType.Empty;
                        gridObjects[x, y] = Instantiate(emptytPrefab, position, Quaternion.identity, transform);
                        break;
                }
            }
        }
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1) && grid[pos.x, pos.y] != TileType.Wall;
    }

    public void UpdateGrid(Vector2Int oldPos, Vector2Int newPos, TileType type)
    {
        GameObject obj = gridObjects[oldPos.x, oldPos.y];
        if (obj == null) return;

        grid[oldPos.x, oldPos.y] = TileType.Empty;
        grid[newPos.x, newPos.y] = type;

        if (gridObjects[newPos.x, newPos.y] != null && gridObjects[newPos.x, newPos.y] != emptytPrefab)
        {
            Destroy(gridObjects[newPos.x, newPos.y]);
        }

        // Gunakan offset yang sama seperti di InitializeLevel
        float offsetX = -grid.GetLength(0) * tileSize / 2.5f;
        float offsetY = -grid.GetLength(1) * tileSize / 2.5f;
        gridObjects[oldPos.x, oldPos.y] = Instantiate(emptytPrefab, new Vector3(oldPos.x * tileSize + offsetX, oldPos.y * tileSize + offsetY, 0), Quaternion.identity, transform);
        gridObjects[newPos.x, newPos.y] = obj;

        Vector3 targetPos = new Vector3(newPos.x * tileSize + offsetX, newPos.y * tileSize + offsetY, 0);
        StartCoroutine(MoveObject(obj.transform, targetPos, 0.2f));
    }

    public void UpdatePlayerPosition(Vector2Int newPos)
    {
        playerPosition = newPos;
    }

    public bool CheckWinCondition()
    {
        foreach (Vector2Int target in targetPositions)
        {
            if (grid[target.x, target.y] != TileType.Box || !targetGrid[target.x, target.y])
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator MoveObject(Transform obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        obj.position = targetPos;
    }
}