using System;
using BeatLeader;
namespace EyeTrackingPlug;

public class BeatLeaderProxy
{
    public static event Action OnFinalizeReplay = () => { };
    
    public BeatLeaderProxy(ReplayRecorder replayRecorder)
    {
        replayRecorder.OnFinalizeReplay += () => OnFinalizeReplay?.Invoke();
    }
    
    public static bool TryWriteCustomDataStatic(string key, byte[] data)
    {
        return ReplayRecorder.TryWriteCustomDataStatic(key, data);
    }
}