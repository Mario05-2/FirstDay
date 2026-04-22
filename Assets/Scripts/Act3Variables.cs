using UnityEngine;
using Yarn.Unity;

public class Act3Variables : MonoBehaviour
{
    [YarnCommand("winScreen")]
    public static void WinScreen()
    {
        Debug.Log("Player has won the game! Triggering win screen...");
        UIMananger uiManager = FindFirstObjectByType<UIMananger>();
        if (uiManager != null)
        {
            uiManager.WinMenu();
        }
    }

}
