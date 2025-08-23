using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button exitButton;

    void Awake()
    {
        startButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainGame");
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
#endif

    });
    }

}
