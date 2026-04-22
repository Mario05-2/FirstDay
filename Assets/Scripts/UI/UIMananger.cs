using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMananger : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject deathMenu;
    [SerializeField] GameObject winMenu;

    public static bool isGamePaused;

    bool prevCursorVisible;
    CursorLockMode prevLockState;

    public void Awake()
    {
        deathMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    void Start()
    {
        pauseMenu.SetActive(false);
        isGamePaused = false;

        prevCursorVisible = Cursor.visible;
        prevLockState = Cursor.lockState;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!isGamePaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;


        prevCursorVisible = Cursor.visible;
        prevLockState = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

     
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void DeathMenu()
    {
        deathMenu.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void WinMenu()
    {
        winMenu.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;

        Cursor.visible = prevCursorVisible;
        Cursor.lockState = prevLockState;
    }
}