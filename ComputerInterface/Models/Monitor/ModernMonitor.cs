using ComputerInterface.Enumerations;
using ComputerInterface.Interfaces;
using UnityEngine;

namespace ComputerInterface.Models.Monitor;

public class ModernMonitor : IMonitor {
    public EMonitorType MonitorType => EMonitorType.Modern;
    
    public string AssetName => "Modern Monitor";
    
    public int ScreenWidth => 41;
    
    public int ScreenHeight => 12;

    public Vector3 LocalPosition => new(2.1861f, -0.5944f, -0.0001f);

    public Vector3 LocalEulerAngles => new(0f, 270f, 270.02f);
}