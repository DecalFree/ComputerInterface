using ComputerInterface.Enumerations;
using ComputerInterface.Interfaces;
using UnityEngine;

namespace ComputerInterface.Models.Monitor;

public class ClassicMonitor : IMonitor {
    public EMonitorType MonitorType => EMonitorType.Classic;
    
    public string AssetName => "Classic Monitor";
    
    public int ScreenWidth => 52;
    
    public int ScreenHeight => 12;

    public Vector3 LocalPosition => new(-0.0787f, -0.12f, 0.5344f);
    
    public Vector3 LocalEulerAngles => Vector3.right * 90f;
}