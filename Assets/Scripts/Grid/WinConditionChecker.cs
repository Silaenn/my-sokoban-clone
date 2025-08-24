using System; 
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WinConditionChecker
{
    private readonly GridManager gridManager;

    public WinConditionChecker(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    public bool CheckWinCondition(TileType[,] grid, List<Vector2Int> targetPositions)
    {
        return targetPositions.All(target => grid[target.x, target.y] == TileType.Box);
    }

    public IEnumerator CheckWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (CheckWinCondition(gridManager.Grid, gridManager.TargetPositions))
        {
            gridManager.NotifyWinConditionMet();
        }
    }
}

