using System;
using System.Collections.Generic;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

namespace EyeTrackingPlug.DataProvider;


public class UnityEyeDataProvider: IEyeDataProvider, IInitializable, IDisposable
{
    public static UnityEyeDataProvider Instance { get; private set; } = null!;
    
    private List<InputDevice> _devices = new List<InputDevice>();
    
    private void FlushDev()
    {
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, _devices);
    }

    private void FlushDev(InputDevice _)
    {
        FlushDev();
    }
    
    public void Initialize()
    {
        Instance = this;
        FlushDev();
        InputDevices.deviceConfigChanged +=FlushDev;
        InputDevices.deviceConnected += FlushDev;
        InputDevices.deviceDisconnected += FlushDev;
    }

    public void Dispose()
    {
        Instance = null!;
        InputDevices.deviceConfigChanged -= FlushDev;
        InputDevices.deviceConnected -= FlushDev;
        InputDevices.deviceDisconnected -= FlushDev;
    }
    
    public bool GetData(out EyeTrackingData data)
    {
        data = new EyeTrackingData();
        Plugin.Log.Info(_devices.Count + " eye tracking data found.");
        if (_devices.Count == 0)
            return false;
        var device = _devices[0];
        
        Plugin.Log.Info(device.name);
        if(!device.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyes))
            return false;
        
        if (!eyes.TryGetLeftEyePosition(out data.LeftPosition))
            return false;
        if (!eyes.TryGetLeftEyeRotation(out data.LeftRotation))
            return false;
        if (!eyes.TryGetRightEyePosition(out data.RightPosition))
            return false;
        if (!eyes.TryGetRightEyeRotation(out data.RightRotation))
            return false;
        
        if(eyes.TryGetFixationPoint(out Vector3 FixationPoint))
            data.FixationPoint = FixationPoint;
        else
            data.FixationPoint = null;
        
        if (eyes.TryGetLeftEyeOpenAmount(out float leftOpenAmount))
            data.LeftEyeOpenAmount = leftOpenAmount;
        else
            data.LeftEyeOpenAmount = null;

        if (eyes.TryGetRightEyeOpenAmount(out float rightOpenAmount))
            data.RightEyeOpenAmount = rightOpenAmount;
        else
            data.RightEyeOpenAmount = null;
        
        return true;
    }

}