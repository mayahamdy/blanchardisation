using QuantumMiniGolf;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalGameManager : MonoBehaviour
{
    public LocalGolfController playerController;
    
    [Header("UI Elements")]
    public GameObject startMessageUI;
    public GameObject winMessageUI; // Reference for the win text

    private bool gameStarted = false;

    void Start()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (startMessageUI != null)
        {
            startMessageUI.SetActive(true);
        }

        // Make sure the win message is hidden at the start
        if (winMessageUI != null)
        {
            winMessageUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!gameStarted && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        gameStarted = true;

        if (playerController != null)
            playerController.enabled = true;

        if (startMessageUI != null)
            startMessageUI.SetActive(false);

        // Start the adaptive countdown for this hole.
        if (HoleTimer.Instance != null)
            HoleTimer.Instance.StartHole();
    }

    public void ShowWinScreen()
    {
        gameStarted = false;

        if (HoleTimer.Instance != null)
            HoleTimer.Instance.StopTimer();

        if (playerController != null)
            playerController.enabled = false;

        if (winMessageUI != null)
            winMessageUI.SetActive(true);
    }
}