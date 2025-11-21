using System;
using BeatLeader.Models;
using BeatLeader.Replayer;
using IPA.Loader;
using Zenject;

namespace EyeTrackingPlug.DataProvider;

public class RecordOrUnityDataProvider:IEyeDataProvider, IInitializable, IDisposable
{
    public static RecordOrUnityDataProvider Instance { get; private set; } = null!;
    [Inject]
    UnityEyeDataProvider _eyeDataProvider = null!;

    public ReplayDataProvider? replayProvider = null;

    public byte[]? replayDataBytes = null;
    public bool GetData(out EyeTrackingData data)
    {
        if(replayProvider != null && replayProvider.HasData())
            return replayProvider.GetData(out data);
        return _eyeDataProvider.GetData(out data);
    }

    public void Initialize()
    {
        Instance = this;
        Plugin.Log.Info("RecordOrUnityDataProvider initializing...");
        if (PluginManager.IsEnabled(PluginManager.GetPluginFromId("BeatLeader")))
            InitBeatLeader();
    }

    public void Dispose()
    {
        Instance = null!;
    }

    private void InitBeatLeader()
    {
        ReplayerLauncher.ReplayWasStartedEvent += (ReplayLaunchData replayData) =>
        {
            if (replayData.MainReplay.CustomData.TryGetValue("EyeTrackingP", out var data))
            {
                if (data != null)
                    replayDataBytes = data;
                replayProvider?.LoadData();
            }
        };

        ReplayerLauncher.ReplayWasFinishedEvent += data =>
        {
            replayDataBytes = null;
            replayProvider?.DropData();
        };

    }
}