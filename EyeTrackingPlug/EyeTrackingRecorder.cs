using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;

namespace EyeTrackingPlug;

public struct RecordItem
{
    public Vector3 positionL, positionR;
    public Quaternion rotationL, rotationR;

    void Encode(BinaryWriter binaryWriter)
    {
        var v3 = (Vector3 v) =>
        {
            binaryWriter.Write(v.x);
            binaryWriter.Write(v.y);
            binaryWriter.Write(v.z);
        };
        var q4 = (Quaternion q) =>
        {
            binaryWriter.Write(q.x);
            binaryWriter.Write(q.y);
            binaryWriter.Write(q.z);
            binaryWriter.Write(q.w);
        };
        v3(positionL);
        q4(rotationL);
        v3(positionR);
        q4(rotationR);
    }
}

public class EyeTrackingRecorder : MonoBehaviour
{
    public bool recording = false;
    
    private List<InputDevice> _devices = new List<InputDevice>();

    public static List<RecordItem> records = new List<RecordItem>();
    void Start()
    {
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, _devices);
        records.Clear();
        
    }
    
    void Update()
    {
        if(!recording)
            return;
        if (_devices.Count == 0)
            return;
        
        InputDevice device = _devices[0];
        if(!device.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyes))
            return;
        if(!eyes.TryGetLeftEyePosition(out Vector3 leftEyePosition))
            return;
        if (!eyes.TryGetLeftEyeRotation(out Quaternion leftEyeRotation))
            return;
        if (!eyes.TryGetRightEyePosition(out Vector3 rightEyePosition)) 
            return;
        if (!eyes.TryGetRightEyeRotation(out Quaternion rightEyeRotation))
            return;
        
        records.Add(new RecordItem()
        {
            positionL = leftEyePosition,
            positionR = rightEyePosition,
            rotationL = leftEyeRotation,
            rotationR = rightEyeRotation
        });
    }
}