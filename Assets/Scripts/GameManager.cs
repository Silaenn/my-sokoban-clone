using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SokobanLevel[] levels;
    private GridManager gridManager;
    private PlayerController playerController;
    private int currentLevelIndex = 0;

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
            // Tambahkan logika untuk menampilkan akhir game jika diinginkan
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