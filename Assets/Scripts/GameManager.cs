using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public SokobanLevel[] levels; // Public agar bisa diakses dari LevelComplate
    private GridManager gridManager;
    private PlayerController playerController;
    public int currentLevelIndex = 0; // Public agar bisa diakses

    private void Awake()
    {
        InitializeComponents();
        SubscribeToEvents();
    }

    private void Start()
    {
        if (levels.Length > 0 && levels[currentLevelIndex] != null)
        {
            gridManager.InitializeLevel(levels[currentLevelIndex]);
        }
        else
        {
            Debug.LogError("No levels assigned or current level is null");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeComponents()
    {
        gridManager = FindObjectOfType<GridManager>();
        playerController = FindObjectOfType<PlayerController>();

        if (gridManager == null || playerController == null)
        {
            Debug.LogError("Required components missing in scene");
        }
    }

    private void SubscribeToEvents()
    {
        if (gridManager != null)
        {
            gridManager.OnWinConditionMet += HandleLevelComplete;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (gridManager != null)
        {
            gridManager.OnWinConditionMet -= HandleLevelComplete;
        }
    }

    private void HandleLevelComplete()
    {
        Debug.Log("Level Completed!");
        currentLevelIndex++;
        if (currentLevelIndex < levels.Length)
        {
            gridManager.ClearGridObjectsAndTiles();
            gridManager.InitializeLevel(levels[currentLevelIndex]);
        }
        else
        {
            Debug.Log("All levels completed!");
            // Panggil UI LevelComplate jika semua level selesai akan ditangani di LevelComplate
        }
    }

    public void RestartCurrentLevel()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            gridManager.ClearGridObjectsAndTiles();
            gridManager.InitializeLevel(levels[currentLevelIndex]);
        }
    }
}