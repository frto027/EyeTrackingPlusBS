using System;
using JetBrains.Annotations;
using Zenject;

namespace EyeTrackingPlug.DataProvider;

public class ReplayOrRawEyeDataProvider: IEyeDataProvider
{
    [Inject]
    private IEyeDataProvider _eyeDataProvider = null!;

    internal BeatLeaderReplayDataProvider? blReplayProvider = null;
    
    [PublicAPI]
    public bool IsReplayData => blReplayProvider != null && blReplayProvider.HasData();
    public bool GetData(out EyeTrackingData data)
    {
        if(blReplayProvider != null && blReplayProvider.HasData())
            return blReplayProvider.GetData(out data);
        return _eyeDataProvider.GetData(out data);
    }
}
