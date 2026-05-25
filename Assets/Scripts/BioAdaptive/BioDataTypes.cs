using System;

namespace BioAdaptive
{
    [Serializable]
    public class BioMessageBase
    {
        public string type;
    }

    // Received once per second during the 60-second rest calibration phase.
    [Serializable]
    public class CalibrationProgressMessage
    {
        public string type;
        public float progress;       // 0.0 → 1.0
        public float elapsed_sec;
        public float total_sec;
    }

    // Received once when calibration finishes. Stores the player's physiological baseline.
    [Serializable]
    public class CalibrationCompleteMessage
    {
        public string type;
        public float hr_rest;        // resting heart rate (bpm)
        public string hr_label;      // e.g. "normale"
        public float hrv_rest;       // resting HRV (ms)
        public float resp_rest;      // resting breath rate (bpm)
        public string resp_label;
        public float eda_baseline;   // resting skin conductance
    }

    // Received 10 times per second during gameplay.
    [Serializable]
    public class BioDataMessage
    {
        public string type;
        public bool  shot_start;             // true on the frame an EMG contraction begins
        public bool  shot_end;               // true on the frame the EMG contraction ends
        public float stress;                 // 0.0 (calm) → 1.0 (high stress)
        public float heart_rate;
        public float heart_rate_variability;
        public float breath_rate;
        public float eda_level;
    }
}
