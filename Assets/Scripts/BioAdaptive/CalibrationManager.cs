using System.Collections;
using BioAdaptive;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Controls the Calibration scene UI.
    ///   Waiting panel  → shown until Python WebSocket connects
    ///   Calibration panel → shows progress bar and timer during the 60-second rest phase
    ///   Results panel  → shows baseline values for 5 s, then reveals the Start button
    /// </summary>
    public class CalibrationManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject waitingPanel;
        public GameObject calibrationPanel;
        public GameObject resultsPanel;

        [Header("Calibration Panel")]
        public Image           progressBarFill;
        public TextMeshProUGUI timerText;

        [Header("Results Panel")]
        public TextMeshProUGUI hrText;
        public TextMeshProUGUI hrvText;
        public TextMeshProUGUI respText;
        public Button          startButton;

        [Header("Settings")]
        public string gameSceneName          = "SampleScene";
        public float  resultsDisplayDuration = 5f;

        // ── Unity lifecycle ──────────────────────────────────────────────────

        private void Start()
        {
            ShowPanel(waitingPanel);
            if (startButton != null) startButton.gameObject.SetActive(false);
            if (startButton != null) startButton.onClick.AddListener(LoadGameScene);

            if (BioBridge.Instance == null)
            {
                Debug.LogError("[CalibrationManager] BioBridge not found. Make sure the scene is launched from the project.");
                return;
            }

            BioBridge.Instance.OnConnected           += HandleConnected;
            BioBridge.Instance.OnDisconnected        += HandleDisconnected;
            BioBridge.Instance.OnCalibrationProgress += HandleProgress;
            BioBridge.Instance.OnCalibrationComplete += HandleComplete;

            if (BioBridge.Instance.IsConnected)
                HandleConnected();
        }

        private void OnDestroy()
        {
            if (BioBridge.Instance == null) return;
            BioBridge.Instance.OnConnected           -= HandleConnected;
            BioBridge.Instance.OnDisconnected        -= HandleDisconnected;
            BioBridge.Instance.OnCalibrationProgress -= HandleProgress;
            BioBridge.Instance.OnCalibrationComplete -= HandleComplete;
        }

        // ── BioBridge event handlers ─────────────────────────────────────────

        private void HandleConnected()
        {
            ShowPanel(calibrationPanel);
            if (progressBarFill != null) progressBarFill.fillAmount = 0f;
            if (timerText       != null) timerText.text = "0 / 60 s";
        }

        private void HandleDisconnected()
        {
            if (calibrationPanel != null && calibrationPanel.activeSelf)
                ShowPanel(waitingPanel);
        }

        private void HandleProgress(CalibrationProgressMessage msg)
        {
            if (progressBarFill != null)
                progressBarFill.fillAmount = msg.progress;
            if (timerText != null)
                timerText.text = $"{Mathf.RoundToInt(msg.elapsed_sec)} / {Mathf.RoundToInt(msg.total_sec)} s";
        }

        private void HandleComplete(CalibrationCompleteMessage msg)
        {
            ShowPanel(resultsPanel);
            if (hrText   != null) hrText.text   = $"FC au repos : {msg.hr_rest:F0} bpm ({msg.hr_label})";
            if (hrvText  != null) hrvText.text  = $"HRV au repos : {msg.hrv_rest:F0} ms";
            if (respText != null) respText.text = $"Respiration : {msg.resp_rest:F1} bpm ({msg.resp_label})";
            StartCoroutine(RevealStartButton(resultsDisplayDuration));
        }

        private IEnumerator RevealStartButton(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (startButton != null) startButton.gameObject.SetActive(true);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void ShowPanel(GameObject panel)
        {
            if (waitingPanel     != null) waitingPanel.SetActive(false);
            if (calibrationPanel != null) calibrationPanel.SetActive(false);
            if (resultsPanel     != null) resultsPanel.SetActive(false);
            if (panel            != null) panel.SetActive(true);
        }

        private void LoadGameScene() => SceneManager.LoadScene(gameSceneName);
    }
}
