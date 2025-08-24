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

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found in scene. Please assign it in the Inspector.");
                enabled = false;
                return;
            }
        }

        instruction.SetActive(true);
        levelComplate.SetActive(false);
        mainMenu.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        UpdateLevelText();
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnLevelCompleted += UpdateLevelText;
            gameManager.OnAllLevelsCompleted += ShowLevelCompleteUI;
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnLevelCompleted -= UpdateLevelText;
            gameManager.OnAllLevelsCompleted -= ShowLevelCompleteUI;
        }
    }

    private void ShowLevelCompleteUI()
    {
        instruction.SetActive(false);
        levelComplate.SetActive(true);
        
    }

    private void UpdateLevelText()
    {
        if (levelText == null || gameManager == null || gameManager.Levels == null)
        {
            Debug.LogWarning("Cannot update level text: Missing levelText, gameManager, or levels.");
            return;
        }

        int currentLevel = gameManager.CurrentLevelIndex + 1;
        levelText.text = $"Level: {currentLevel}/{gameManager.Levels.Length}";
    }
}

