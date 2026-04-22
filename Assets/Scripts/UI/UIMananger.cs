using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMananger : MonoBehaviour
{
    public static UIMananger Instance { get; private set; }

    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject winMenu;
    [SerializeField] GameObject caughtMenu;
    [SerializeField] GameObject stealthMeterOverlay;

    public static bool isGamePaused;

    bool prevCursorVisible;
    CursorLockMode prevLockState;

    public void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }
    void Start()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (winMenu != null) winMenu.SetActive(false);
        if (caughtMenu != null) caughtMenu.SetActive(false);
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
        if (stealthMeterOverlay != null)
            stealthMeterOverlay.SetActive(false);
        Time.timeScale = 0f;
        isGamePaused = true;


        prevCursorVisible = Cursor.visible;
        prevLockState = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

     
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void WinMenu()
    {
        winMenu.SetActive(true);
        if (stealthMeterOverlay != null)
            stealthMeterOverlay.SetActive(false);
        Time.timeScale = 0f;
        isGamePaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void CaughtMenu()
    {
        if (caughtMenu != null)
            caughtMenu.SetActive(true);
        else
            Debug.LogWarning("UIMananger: caughtMenu is not assigned in the Inspector.");

        if (stealthMeterOverlay != null)
            stealthMeterOverlay.SetActive(false);

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
        if (stealthMeterOverlay != null)
            stealthMeterOverlay.SetActive(true);
        Time.timeScale = 1f;
        isGamePaused = false;

        Cursor.visible = prevCursorVisible;
        Cursor.lockState = prevLockState;
    }
}