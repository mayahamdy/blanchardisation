using QuantumMiniGolf;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalGameManager : MonoBehaviour
{
    public LocalGolfController playerController;

    [Header("UI Elements")]
    public GameObject      startMessageUI;
    public ResultsScreenUI resultsScreen;

    private bool gameStarted = false;

    void Start()
    {
        if (playerController != null)
            playerController.enabled = false;

        if (startMessageUI != null)
            startMessageUI.SetActive(true);
    }

    void Update()
    {
        if (!gameStarted && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            StartGame();
    }

    private void StartGame()
    {
        gameStarted = true;

        if (playerController != null)
            playerController.enabled = true;

        if (startMessageUI != null)
            startMessageUI.SetActive(false);

        if (HoleTimer.Instance != null)
            HoleTimer.Instance.StartHole();

        if (SessionStats.Instance != null)
            SessionStats.Instance.StartSession();
    }

    public void ShowWinScreen()
    {
        gameStarted = false;

        if (HoleTimer.Instance != null)
            HoleTimer.Instance.StopTimer();

        if (SessionStats.Instance != null)
            SessionStats.Instance.StopSession();

        if (playerController != null)
            playerController.enabled = false;

        if (resultsScreen != null)
            resultsScreen.Show();
    }
}
