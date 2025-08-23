using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SokobanLevel currentLevel;
    private GridManager gridManager;
    private PlayerController playerController;

    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        playerController = FindObjectOfType<PlayerController>();

        if (gridManager == null || playerController == null)
        {
            Debug.LogError("Required components missing in scene");
            return;
        }

        playerController.OnLevelComplete += HandleLevelComplete;
    }

    private void Start()
    {
        if (currentLevel != null)
        {
            gridManager.InitializeLevel(currentLevel);
        }
        else
        {
            Debug.LogError("No level assigned to GameManager");
        }
    }

    private void HandleLevelComplete()
    {
        Debug.Log("Level Completed!");
        // Tambahkan logika untuk load level berikutnya atau menampilkan UI kemenangan
    }

    private void OnDestroy()
    {
        playerController.OnLevelComplete -= HandleLevelComplete;
    }
}