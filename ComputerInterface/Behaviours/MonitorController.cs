using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using ComputerInterface.Models.Monitor;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ComputerInterface.Behaviours;

internal class MonitorController {
    private readonly CIConfig _config = Plugin.CIConfig;

    public readonly List<IMonitor> Monitors = [
        new ClassicMonitor(),
        new ModernMonitor()
    ];

    public IMonitor GetCurrentMonitor() =>
        Monitors[(int)_config.CurrentMonitorType.Value];
    
    public Vector2 GetComputerScreenDimensions(IMonitor monitor) =>
        new(monitor.ScreenWidth, monitor.ScreenHeight);

    public async void SetMonitor(EMonitorType monitorType) {
        _config.CurrentMonitorType.Value = monitorType;
        CustomComputer.Singleton.PrepareMonitor(GetCurrentScene(), GetComputerLocation(), false);
        await Task.Delay(1); // Wait 0.001 seconds to fully end preparing the monitor, this stops it from duplicating itself. -DecalFree
        CustomComputer.Singleton.GetField<ComputerViewController>("_computerViewController").SetMonitor(GetCurrentMonitor());
    }

    private Scene GetCurrentScene() {
        switch (ZoneManagement.instance.activeZones.First()) {
            case GTZone.monkeBlocks:
            case GTZone.monkeBlocksShared:
                return SceneManager.GetSceneByName("GorillaTag");
            
            default:
                return SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        }
    }

    private string GetComputerLocation() {
        switch (ZoneManagement.instance.activeZones.First()) {
            case GTZone.monkeBlocks:
                return "Environment Objects/MonkeBlocksRoomPersistent/MonkeBlocksComputer/GorillaComputerObject/ComputerUI";
            case GTZone.monkeBlocksShared:
                return "Environment Objects/LocalObjects_Prefab/SharedBlocksMapSelectLobby/GorillaComputerObject/ComputerUI";
            default:
                return GetCurrentScene().GetComponentInHierarchy<GorillaComputerTerminal>().gameObject.transform.GetChild(0).GetPath();
        }
    }
}