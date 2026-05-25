using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BioAdaptive
{
    /// <summary>
    /// Persistent singleton that maintains the WebSocket connection to the Python server.
    /// Runs the receive loop on a background thread and dispatches events on the Unity main thread.
    /// If Python is not running, it retries every 2 seconds — the game never crashes.
    /// </summary>
    public class BioBridge : MonoBehaviour
    {
        public static BioBridge Instance { get; private set; }

        // Auto-creates BioBridge at app start so it exists in every scene.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance != null) return;
            var go = new GameObject("[BioBridge]");
            DontDestroyOnLoad(go);
            go.AddComponent<BioBridge>();
        }

        // ── Events (always fired on the Unity main thread) ──────────────────
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<CalibrationProgressMessage> OnCalibrationProgress;
        public event Action<CalibrationCompleteMessage> OnCalibrationComplete;
        public event Action<BioDataMessage> OnBioData;
        public event Action OnShotStart;  // fires once when EMG contraction begins
        public event Action OnShotEnd;    // fires once when EMG contraction ends

        // ── Readable state ───────────────────────────────────────────────────
        public bool IsConnected { get; private set; }
        public BioDataMessage LastData { get; private set; }
        public CalibrationCompleteMessage CalibrationResult { get; private set; }

        // ── Internals ────────────────────────────────────────────────────────
        private CancellationTokenSource _cts;
        private readonly ConcurrentQueue<string> _jsonQueue   = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();
        private bool _prevShotStart;
        private bool _prevShotEnd;

        private const string WS_URL           = "ws://localhost:8765";
        private const int    RECONNECT_DELAY  = 2000; // ms

        // ── Unity lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LastData = new BioDataMessage
            {
                stress                 = 0f,
                heart_rate             = 70f,
                heart_rate_variability = 40f,
                breath_rate            = 14f,
                eda_level              = 0f,
            };
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => ConnectionLoop(_cts.Token));
        }

        private void Update()
        {
            // Drain connection-state actions queued from the background thread.
            while (_actionQueue.TryDequeue(out var action))
                action?.Invoke();

            // Drain JSON messages queued from the WebSocket receive loop.
            while (_jsonQueue.TryDequeue(out var json))
                ProcessMessage(json);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
        }

        // ── Background thread: connection + receive ──────────────────────────

        private async Task ConnectionLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                ClientWebSocket ws = null;
                try
                {
                    ws = new ClientWebSocket();
                    await ws.ConnectAsync(new Uri(WS_URL), ct);

                    // Signal main thread: connected
                    _actionQueue.Enqueue(() => { IsConnected = true; OnConnected?.Invoke(); });

                    await ReceiveLoop(ws, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BioBridge] Connection error: {ex.Message}");
                }
                finally
                {
                    ws?.Dispose();
                }

                // Signal main thread: disconnected
                _actionQueue.Enqueue(() => { IsConnected = false; OnDisconnected?.Invoke(); });

                try { await Task.Delay(RECONNECT_DELAY, ct); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task ReceiveLoop(ClientWebSocket ws, CancellationToken ct)
        {
            var buffer = new byte[65536];
            var sb     = new StringBuilder();

            while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                }
                catch
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close) break;

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    if (result.EndOfMessage)
                    {
                        _jsonQueue.Enqueue(sb.ToString());
                        sb.Clear();
                    }
                }
            }
        }

        // ── Main thread: JSON parsing ────────────────────────────────────────

        private void ProcessMessage(string json)
        {
            try
            {
                var baseMsg = JsonUtility.FromJson<BioMessageBase>(json);
                if (baseMsg == null) return;

                switch (baseMsg.type)
                {
                    case "calibration_progress":
                        var prog = JsonUtility.FromJson<CalibrationProgressMessage>(json);
                        OnCalibrationProgress?.Invoke(prog);
                        break;

                    case "calibration_complete":
                        CalibrationResult = JsonUtility.FromJson<CalibrationCompleteMessage>(json);
                        OnCalibrationComplete?.Invoke(CalibrationResult);
                        break;

                    case "data":
                        var data = JsonUtility.FromJson<BioDataMessage>(json);
                        LastData = data;
                        OnBioData?.Invoke(data);

                        // Rising-edge detection for both shot events.
                        if (data.shot_start && !_prevShotStart) OnShotStart?.Invoke();
                        if (data.shot_end   && !_prevShotEnd)   OnShotEnd?.Invoke();
                        _prevShotStart = data.shot_start;
                        _prevShotEnd   = data.shot_end;
                        break;

                    default:
                        Debug.LogWarning($"[BioBridge] Unknown message type: {baseMsg.type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BioBridge] Parse error: {ex.Message}\nJSON: {json}");
            }
        }
    }
}
