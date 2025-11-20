using System.Linq;
using IPA;
using IPA.Loader;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using IpaLogger = IPA.Logging.Logger;

namespace EyeTrackingPlug;

[Plugin(RuntimeOptions.DynamicInit)]
internal class Plugin
{
    internal static IpaLogger Log { get; private set; } = null!;
    
    [Init]
    public Plugin(IpaLogger ipaLogger, PluginMetadata pluginMetadata)
    {
        Log = ipaLogger;
        Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized.");
        OpenXRRestarter.Instance.onAfterShutdown += EyeGazeEnabler;
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

        if(OpenXRSettings.Instance.features.First((f => f is EyeGazeInteraction)).enabled || OpenXRSettings.Instance.isRunning )
            // Lucky. If other mods or something did/doing the OpenXR restart, we don't need do it.
        else
            OpenXRRestarter.Instance.PauseAndShutdownAndRestart();
    }

    [OnExit]
    public void OnApplicationQuit()
    {
        Log.Debug("OnApplicationQuit");
        // OpenXRRestarter.Instance.onAfterShutdown -= EyeGazeEnabler;
    }
}
