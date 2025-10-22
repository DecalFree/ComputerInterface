using System.Collections.Generic;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using ComputerInterface.Models.Monitor;
using UnityEngine;

namespace ComputerInterface.Behaviours;

internal class MonitorController {
    private readonly CIConfig _config = Plugin.CIConfig;

    private readonly List<IMonitor> _monitors = [
        new ModernMonitor(),
        new ClassicMonitor()
    ];

    public IMonitor GetCurrentMonitor() =>
        _monitors[(int)_config.CurrentMonitorType.Value];
    
    public Vector2 GetComputerScreenDimensions(IMonitor monitor) =>
        new(monitor.ScreenWidth, monitor.ScreenHeight);

    public void SetMonitor(EMonitorType monitorType) {
        _config.CurrentMonitorType.Value = monitorType;
        // CustomComputer.Singleton.PrepareMonitor();
        CustomComputer.Singleton.GetField<ComputerViewController>("_computerViewController").SetMonitor(GetCurrentMonitor());
    }
}