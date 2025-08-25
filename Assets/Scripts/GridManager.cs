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

    private GridInitializer gridInitializer;
    private GridMover gridMover;
    private MoveHistoryManager moveHistoryManager;
    private WinConditionChecker winConditionChecker;

    public Vector2Int PlayerPosition => playerPosition;
    public TileType[,] Grid => grid;
    public GameObject[,] GridObjects { get => gridObjects; set => gridObjects = value; }
    public GameObject[,] BackgroundTiles { get => backgroundTiles; set => backgroundTiles = value; }
    public List<Vector2Int> TargetPositions => targetPositions;
    public GameObject WallPrefab => wallPrefab;
    public GameObject PlayerPrefab => playerPrefab;
    public GameObject BoxPrefab => boxPrefab;
    public GameObject TargetPrefab => targetPrefab;
    public GameObject EmptyTilePrefab => emptyTilePrefab;
    public float TileSize => tileSize;

    public event Action OnPlayerMoved;
    public event Action OnWinConditionMet;
    public event Action<string> OnLevelLoadError;

    private void Awake()
    {
        gridInitializer = new GridInitializer(this, wallPrefab, playerPrefab, boxPrefab, targetPrefab, emptyTilePrefab, tileSize);
        gridMover = new GridMover(this, tileSize);
        moveHistoryManager = new MoveHistoryManager(this, gridInitializer);
        winConditionChecker = new WinConditionChecker(this);
    }

    public void InitializeLevel(SokobanLevel level)
    {
        if (!gridInitializer.IsValidLevel(level))
        {
            OnLevelLoadError?.Invoke("Invalid level data or null level");
            return;
        }

        moveHistoryManager.ClearHistory();
        gridInitializer.ClearGridObjectsAndTiles();
        gridInitializer.InitializeGrid(level, ref grid, ref gridObjects, ref backgroundTiles, ref targetPositions, ref playerPosition);
        moveHistoryManager.SaveInitialState(grid, playerPosition);
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return GridUtils.IsValidPosition(position, grid);
    }

    public void UpdateGrid(Vector2Int oldPos, Vector2Int newPos, TileType type)
    {
        // Pass moveHistoryManager instance yang sama ke GridMover
        gridMover.UpdateGrid(oldPos, newPos, type, grid, gridObjects, targetPositions, ref playerPosition, OnPlayerMoved, winConditionChecker, moveHistoryManager);
    }

    public void UpdatePlayerPosition(Vector2Int newPosition)
    {
        playerPosition = newPosition;
    }

    public void UndoMove()
    {
        moveHistoryManager.UndoMove(ref grid, ref gridObjects, ref backgroundTiles, ref playerPosition, targetPositions);
    }

    public void RestartLevel(SokobanLevel level)
    {
        moveHistoryManager.ClearHistory();
        gridInitializer.ClearGridObjectsAndTiles();
        InitializeLevel(level);
    }

    public bool CheckWinCondition()
    {
        bool isWin = winConditionChecker.CheckWinCondition(grid, targetPositions);
        if (isWin) OnWinConditionMet?.Invoke();
        return isWin;
    }

    public void NotifyWinConditionMet()
    {
        OnWinConditionMet?.Invoke();
    }

    public void ClearLevel()
    {
        gridInitializer.ClearGridObjectsAndTiles();
    }
}