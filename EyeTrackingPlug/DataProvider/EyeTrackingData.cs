using UnityEngine;

namespace EyeTrackingPlug.DataProvider;

public struct EyeTrackingData
{
    public Vector3 LeftPosition;
    public Quaternion LeftRotation;
    public Vector3 RightPosition;
    public Quaternion RightRotation;
    
    // The following things is not available in replay.
    public Vector3? FixationPoint;
    public float? LeftEyeOpenAmount;
    public float? RightEyeOpenAmount;
}