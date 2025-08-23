using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Sokoban/Level")]
public class SokobanLevel : ScriptableObject
{
    [Tooltip("Width of the level grid")]
    public int width;

    [Tooltip("Height of the level grid")]
    public int height;

    [Tooltip("Grid data as strings (0=Empty, W=Wall, P=Player, B=Box, T=Target)")]
    public string[] gridData;

    [Tooltip("Starting position of the player")]
    public Vector2Int playerStartPosition;
}
