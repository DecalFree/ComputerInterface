using ComputerInterface.Enumerations;
using UnityEngine;

namespace ComputerInterface.Interfaces;

public interface IMonitor {
    EMonitorType  MonitorType { get; }
    
    string AssetName { get; }
    
    int ScreenWidth { get; }
    
    int ScreenHeight { get; }
    
    Vector3 LocalPosition { get; }
    
    Vector3 LocalEulerAngles { get; }
}