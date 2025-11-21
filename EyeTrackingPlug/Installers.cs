using System;
using EyeTrackingPlug.DataProvider;
using IPA.Loader;
using Zenject;

namespace EyeTrackingPlug;

public class AppInstaller : Installer<AppInstaller>
{
    public override void InstallBindings()
    {
        // There is two data sources
        
        // the first is UnityEyeDataProvider, which reads eye tracking data from unity directly.
        // use this if you want to interact with game UI.
        Container.Bind(typeof(UnityEyeDataProvider), typeof(IInitializable), typeof(IDisposable))
            .To(typeof(UnityEyeDataProvider)).AsSingle();
        
        // the seconed is RecordOrUnityDataProvider, which reads eye tracking data not only fron unity, but also the replay file of beatleader.
        if (PluginManager.IsEnabled(PluginManager.GetPluginFromId("BeatLeader")))
        {
            Container.Bind(typeof(RecordOrUnityDataProvider), typeof(IInitializable), typeof(IDisposable))
                .To<RecordOrUnityDataProvider>().AsSingle();
            Plugin.Log.Info("Beatleader detected, install RecordOrUnityDataProvider.");
        }
        else
        {
            Plugin.Log.Info("Beatleader not detected, RecordOrUnityDataProvider is not installed.");
        }
    }
}

public class SinglePlayerInstaller : Installer<SinglePlayerInstaller>
{
    public override void InstallBindings()
    {
        // ReplayDataProvider
        if (PluginManager.IsEnabled(PluginManager.GetPluginFromId("BeatLeader")))
        {
            Container.Bind(typeof(ReplayDataProvider), typeof(IInitializable), typeof(IDisposable))
                .To<ReplayDataProvider>().AsSingle();
        }
        
    }
}