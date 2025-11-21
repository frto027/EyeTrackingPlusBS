using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BeatLeader;
using EyeTrackingPlug.DataProvider;
using IPA.Loader;
using SiraUtil.Zenject;
using Zenject;

namespace EyeTrackingPlug;

struct RecordedEyeTrackingData
{
    public float SongTime;
    public EyeTrackingData EyeTrackingData;
}

public class BeatLeaderRecorder : ITickable, IInitializable, IDisposable
{
    [InjectOptional]
    private readonly ReplayRecorder? _recorder = null!;

    [Inject]
    private readonly UnityEyeDataProvider _eyeDataProvider = null!;
    
    [Inject]
    private readonly AudioTimeSyncController _audioTimeSyncController = null!;
    
    private bool _recordEnabled = false;
    
    private readonly List<RecordedEyeTrackingData> _records = new List<RecordedEyeTrackingData>();

    public void Initialize()
    {
        // The beatleader will not install the recorder if beatleader or scoresaber is in replay mode.
        _recordEnabled = _recorder != null;
        
        
        _recorder?.OnFinalizeReplay += OnFinalizeReplay;
        
        Plugin.Log.Info($"Initializing BeatLeaderRecorder, _recordEnabled: {_recordEnabled}");

    }
    public void Dispose()
    {
        Plugin.Log.Debug("Disposing Eye DataProvider");
        _recorder?.OnFinalizeReplay -= OnFinalizeReplay;
    }

    private float _lastRecordedTime = 0;
    public void Tick()
    {
        if(!_recordEnabled)
            return;
        if(_audioTimeSyncController.state != AudioTimeSyncController.State.Playing)
            return;
        // 30fps is enough. 24fps is good. 6fps is debug friendly.
        if(_audioTimeSyncController.songTime - _lastRecordedTime < 1.0 / 6)
            return;
        if (_eyeDataProvider!.GetData(out EyeTrackingData eyeTrackingData))
        {
            _lastRecordedTime = _audioTimeSyncController.songTime;
            _records.Add(new  RecordedEyeTrackingData()
            {
                SongTime = _audioTimeSyncController.songTime,
                EyeTrackingData = eyeTrackingData
            });
        }
    }

    private void OnFinalizeReplay()
    {
        if(!_recordEnabled)
            return;
        if (_records.Count == 0)
        {
            Plugin.Log.Info("Zero data recorded, don't save the eye tracking data.");
            return;
        }
        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream, Encoding.ASCII, true);

        var i32 = (Int32 i) => binaryWriter.Write(i);
        var f32 = (float f) => binaryWriter.Write(f);
        var v3 = (UnityEngine.Vector3 v) => {f32(v.x);f32(v.y);f32(v.z);};
        var q4 = (UnityEngine.Quaternion q) => {f32(q.x);f32(q.y);f32(q.z);f32(q.w);};

        i32(1);//version
        i32(_records.Count);//count
        
        foreach (var record in _records)
        {
            f32(record.SongTime);
            v3(record.EyeTrackingData.LeftPosition);
            q4(record.EyeTrackingData.LeftRotation);
            v3(record.EyeTrackingData.RightPosition);
            q4(record.EyeTrackingData.RightRotation);
        }
        
        //etAgent
        var bytes = Encoding.UTF8.GetBytes(Plugin.EtAgent);
        i32(bytes.Length);
        binaryWriter.Write(bytes);
        
        var customDataBytes = memoryStream.ToArray();
        
        Plugin.Log.Notice($"Recorded {customDataBytes.Length} bytes. {_records.Count} data recorded. (not recorded for debug purpose)");
        // recorder.TryWriteCustomData("EyeTrackingR", memoryStream.ToArray());
    }
}