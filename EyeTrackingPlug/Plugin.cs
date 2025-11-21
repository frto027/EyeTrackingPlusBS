using System.Linq;
using EyeTrackingPlug.BeatLeaderRecorder;
using EyeTrackingPlug.DataProvider;
using IPA;
using IPA.Loader;
using IPA.Utilities;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using IpaLogger = IPA.Logging.Logger;

namespace EyeTrackingPlug;

[Plugin(RuntimeOptions.SingleStartInit)]
internal class Plugin
{
    internal static IpaLogger Log { get; private set; } = null!;

    public static string EtAgent = "";
    
    
    [Init]
    public Plugin(Zenjector zenjector, IpaLogger ipaLogger, PluginMetadata pluginMetadata)
    {
        Log = ipaLogger;
        
        OpenXRRestarter.Instance.onAfterShutdown += EyeGazeEnabler;
        // Do not restart OpenXR immediately. I don't want to be too aggressive, even with the default 5-second delay before restarting.
        // If other mods also require a restart, then why not do them together later?
        
        zenjector.UseLogger(ipaLogger);
        
        zenjector.Install<AppInstaller>(Location.App);
        zenjector.Install<SinglePlayerInstaller>(Location.Singleplayer);
        
        BeatLeaderRecorderInstaller.PluginInit(zenjector);
        
        EtAgent = $"{pluginMetadata.Name}/{pluginMetadata.HVersion} ({OpenXRRuntime.LibraryName},{OpenXRRuntime.name}/{OpenXRRuntime.version}/{OpenXRRuntime.apiVersion}/{OpenXRRuntime.pluginVersion})";
        
        Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized, etAgent: {EtAgent}");
    }

    private static void EyeGazeEnabler()
    {
        var profile = OpenXRSettings.Instance.features.First((f => f is EyeGazeInteraction));
        profile.enabled = true;
    }
    
    [OnStart]
    public void OnApplicationStart()
    {
        Log.Debug("OnApplicationStart");

        if (OpenXRSettings.Instance.features.First((f => f is EyeGazeInteraction)).enabled ||
            OpenXRRestarter.Instance.isRunning)
        {
            // Lucky. If other mods or something did/doing the OpenXR restart, we don't need do it.
        }
        else
        {
            OpenXRRestarter.Instance.PauseAndShutdownAndRestart();
        }
    }
    [OnExit]
    public void OnApplicationExit()
    {
        
    }
}
