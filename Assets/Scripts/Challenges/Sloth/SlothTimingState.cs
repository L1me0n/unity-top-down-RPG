using UnityEngine;

[System.Serializable]
public class SlothTimingState
{
    public enum SlothRunState
    {
        None,
        Setup,
        AwaitingStart,
        Running,
        Stopped,
        Resolved,
        Cancelled
    }

    [Header("Timing")]
    public float markerNormalized = 0f;      // 0..1
    public float markerDirection = 1f;       // +1 or -1
    public float markerSpeed = 0.75f;        // normalized units per second

    [Header("Success Zone")]
    public float successZoneCenter = 0.5f;   // 0..1
    public float successZoneWidth = 0.16f;   // normalized width

    [Header("Flow")]
    public bool started = false;
    public bool stopped = false;
    public bool succeeded = false;
    public bool failed = false;

    public SlothRunState runtimeState = SlothRunState.None;

    public float SuccessZoneMin => Mathf.Clamp01(successZoneCenter - successZoneWidth * 0.5f);
    public float SuccessZoneMax => Mathf.Clamp01(successZoneCenter + successZoneWidth * 0.5f);

    public void ResetForNewRun(float startMarkerNormalized = 0f, float startDirection = 1f)
    {
        markerNormalized = Mathf.Clamp01(startMarkerNormalized);
        markerDirection = Mathf.Sign(Mathf.Approximately(startDirection, 0f) ? 1f : startDirection);
        markerSpeed = Mathf.Max(0.01f, markerSpeed);

        started = false;
        stopped = false;
        succeeded = false;
        failed = false;

        runtimeState = SlothRunState.Setup;
    }

    public void ConfigureSuccessZone(float center, float width)
    {
        successZoneCenter = Mathf.Clamp01(center);
        successZoneWidth = Mathf.Clamp(width, 0.01f, 1f);
    }

    public void ConfigureSpeed(float speed)
    {
        markerSpeed = Mathf.Max(0.01f, speed);
    }

    public void EnterAwaitingStart()
    {
        runtimeState = SlothRunState.AwaitingStart;
    }

    public void StartRun()
    {
        if (runtimeState == SlothRunState.Resolved || runtimeState == SlothRunState.Cancelled)
            return;

        started = true;
        stopped = false;
        succeeded = false;
        failed = false;

        runtimeState = SlothRunState.Running;
    }

    public void Tick(float deltaTime)
    {
        if (runtimeState != SlothRunState.Running)
            return;

        float next = markerNormalized + markerDirection * markerSpeed * deltaTime;

        if (next >= 1f)
        {
            next = 1f - (next - 1f);
            markerDirection = -1f;
        }
        else if (next <= 0f)
        {
            next = -next;
            markerDirection = 1f;
        }

        markerNormalized = Mathf.Clamp01(next);
    }

    public bool StopAndEvaluate()
    {
        if (runtimeState != SlothRunState.Running)
            return false;

        stopped = true;
        runtimeState = SlothRunState.Stopped;

        bool inside =
            markerNormalized >= SuccessZoneMin &&
            markerNormalized <= SuccessZoneMax;

        succeeded = inside;
        failed = !inside;
        runtimeState = SlothRunState.Resolved;

        return inside;
    }

    public void Cancel()
    {
        runtimeState = SlothRunState.Cancelled;
    }

    public string BuildSummary()
    {
        return
            $"SlothTimingState | state={runtimeState} | started={started} | stopped={stopped} | " +
            $"marker={markerNormalized:0.000} | zone=[{SuccessZoneMin:0.000}..{SuccessZoneMax:0.000}] | " +
            $"success={succeeded} | fail={failed}";
    }
}