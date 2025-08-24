using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public SokobanLevel[] levels; // Public agar bisa diakses dari LevelComplate
    private GridManager gridManager;
    private PlayerController playerController;
    private int currentLevelIndex = 0;

    public SokobanLevel[] Levels => levels;
    public int CurrentLevelIndex => currentLevelIndex;

    public event Action OnLevelCompleted; // Event untuk memberi tahu level selesai
    public event Action OnAllLevelsCompleted; // Event untuk semua level selesai

    private void Awake()
    {
        InitializeComponents();
        SubscribeToEvents();
    }


    private void Start()
    {
        if (levels == null || levels.Length == 0 || levels[currentLevelIndex] == null)
        {
            Debug.LogError("No levels assigned or current level is null");
            return;
        }
        gridManager.InitializeLevel(levels[currentLevelIndex]);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeComponents()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found in scene. Please assign it in the Inspector.");
                enabled = false; // Nonaktifkan skrip jika komponen tidak ditemukan
                return;
            }
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found in scene. Please assign it in the Inspector.");
                enabled = false;
                return;
            }
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
        OnLevelCompleted?.Invoke(); // Beri tahu bahwa level selesai
        currentLevelIndex++;
        if (currentLevelIndex < levels.Length)
        {
            gridManager.InitializeLevel(levels[currentLevelIndex]);
        }
        else
        {
            Debug.Log("All levels completed!");
            OnAllLevelsCompleted?.Invoke(); // Beri tahu bahwa semua level selesai
            gridManager.ClearLevel();
        }
    }


    public void RestartCurrentLevel()
    {
        if (gridManager == null)
        {
            Debug.LogError("Cannot restart level: GridManager is null.");
            return;
        }

        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length && levels[currentLevelIndex] != null)
        {
            gridManager.InitializeLevel(levels[currentLevelIndex]);
        }
        else
        {
            Debug.LogError("Cannot restart level: Invalid level index or level data.");
        }
    }
}
