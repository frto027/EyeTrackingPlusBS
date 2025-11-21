using System;
using System.IO;
using UnityEngine;
using Zenject;

namespace EyeTrackingPlug.DataProvider;


struct ReplayData
{
    public float SongTime;
    public EyeTrackingData EyeTrackingData;
}

public class ReplayDataParseException : Exception
{
    public ReplayDataParseException(string message) : base(message)
    {
    }
}

public class ReplayDataProvider:IEyeDataProvider, IInitializable, IDisposable
{
    [Inject]
    private AudioTimeSyncController _audioTimeSyncController = null!;

    [Inject]
    private RecordOrUnityDataProvider _recordOrUnityDataProvider = null!;
    
    
    private ReplayData[]? _datas;
    
    
    public void LoadData()
    {
        try
        {
            if(_recordOrUnityDataProvider.replayDataBytes != null)
                ParseData(_recordOrUnityDataProvider.replayDataBytes);
        }
        catch (Exception e)
        {
            Plugin.Log.Debug(e);
            _datas = null;
        }
    }

    public void DropData()
    {
        _datas = null;
    }

    public bool HasData()
    {
        return _datas != null;
    }
    
    private void ParseData(byte[] data)
    {
        if(data == null || data.Length < 8)
        {
            throw new ReplayDataParseException("Invalid replay data");
        }
        var reader = new BinaryReader(new MemoryStream(data));
        var i32 = ()=>reader.ReadInt32();
        var f32 = ()=>reader.ReadSingle();
        var v3 = () => new Vector3(f32(), f32(), f32());
        var q4 = () => new Quaternion(f32(), f32(), f32(), f32());

        var version = i32();
        if(version != 1)
            throw new ReplayDataParseException("Invalid replay version");
        var count = i32();
        _datas = new ReplayData[count];
        for (int i = 0; i < count; i++)
        {
            var time = f32();
            var lpos = v3();
            var lrot = q4();
            var rpos = v3();
            var rrot = q4();
            _datas[i] = new ReplayData()
            {
                SongTime = time,
                EyeTrackingData = new EyeTrackingData()
                {
                    LeftPosition = lpos,
                    RightPosition = rpos,
                    LeftRotation = lrot,
                    RightRotation = rrot
                }
            };
        }
    }

    private int currIndex = 0;
    public bool GetData(out EyeTrackingData data)
    {
        data = new EyeTrackingData();
        if (_datas == null)
            return false;

        if (_datas.Length == 0)
            return false;
        
        var now = _audioTimeSyncController.songTime;
        
        if (_datas[0].SongTime > now)
            return false;
        var isInSongTime = (int index) =>
        {
            if (index >= _datas.Length) return false;
            if (index < 0) return false;
            if(_datas[index].SongTime > now) return false;
            if (index + 1 < _datas.Length)
            {
                if(_datas[index+1].SongTime <= now) 
                    return false;
                return true;
            }
            else
                return true;
        };

        if (isInSongTime(currIndex))
            return false;
        if (currIndex + 1 < _datas.Length && isInSongTime(currIndex + 1))
        {
            currIndex++;
            data = _datas[currIndex].EyeTrackingData;
            return true;
        }

        for (int i = 0; i < _datas.Length; i++)
        {
            if (isInSongTime(i))
            {
                currIndex = i;
                data = _datas[currIndex].EyeTrackingData;
                return true;
            }
        }

        return false;
    }

    public void Initialize()
    {
        Plugin.Log.Debug("Replay Data Provider Initialized");
        LoadData();
        _recordOrUnityDataProvider.replayProvider = this;
    }

    public void Dispose()
    {
        Plugin.Log.Debug("Replay Data Provider Disposed");
        _recordOrUnityDataProvider.replayProvider = null;
    }
}
