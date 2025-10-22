using UnityEngine;

namespace ComputerInterface.Interfaces;

public interface IMonitor {
    string AssetName { get; }
    
    int ScreenWidth { get; }
    
    int ScreenHeight { get; }
    
    Vector3 LocalPosition { get; }
    
    Vector3 LocalEulerAngles { get; }
}