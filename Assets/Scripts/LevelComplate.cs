using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelComplate : MonoBehaviour
{
    [SerializeField] private GameObject instruction;
    [SerializeField] private GameObject levelComplate;
    [SerializeField] private Button mainMenu;
    [SerializeField] private TextMeshProUGUI levelText; // Tambahkan field untuk teks level

    private GridManager gridManager;
    private GameManager gameManager;

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in scene");
            return;
        }

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene");
            return;
        }

        instruction.SetActive(true);
        levelComplate.SetActive(false); // Awalnya nonaktif sampai level selesai
        mainMenu.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });

        UpdateLevelText();
    }

    void OnEnable()
    {
        if (gridManager != null)
        {
            gridManager.OnWinConditionMet += OnLevelComplete;
        }
    }

    void OnDisable()
    {
        if (gridManager != null)
        {
            gridManager.OnWinConditionMet -= OnLevelComplete;
        }
    }

    private void OnLevelComplete()
    {
        if (gridManager != null && gameManager.currentLevelIndex >= gameManager.levels.Length)
        {
            gridManager.ClearGridObjectsAndTiles();
            instruction.SetActive(false);
            levelComplate.SetActive(true); // Tampilkan hanya saat semua level selesai
        }   
        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        if (levelText != null && gridManager != null)
        {
            int currentLevel = gameManager.currentLevelIndex + 1; // +1 karena index mulai dari 0
            levelText.text = $"Level: {currentLevel}/{gameManager.levels.Length}";
        }
    }
}