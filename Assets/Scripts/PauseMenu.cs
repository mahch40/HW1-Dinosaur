using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private GameInput input;
    private bool isPaused;

    private void Awake()
    {
        input = new GameInput();
    }

    private void OnEnable()
    {
        input.Gameplay.Enable();
        input.Gameplay.Pause.performed += OnPause;
    }

    private void OnDisable()
    {
        input.Gameplay.Pause.performed -= OnPause;
        input.Gameplay.Disable();
    }

    private void Start()
    {
        panel.SetActive(false);
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.IsGameOver) return;

        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetPaused(true);
    }

    public void Resume()
    {
        isPaused = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetPaused(false);
    }
}