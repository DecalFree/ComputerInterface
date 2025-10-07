using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ComputerInterface.Tools;

internal static class AssetLoader {
    private static bool _assetBundleInitialized;
    private static AssetBundle _storedAssetBundle;
    private static Dictionary<string, Object> _loadedAssetsCache = [];

    private static Task<AssetBundle> LoadAssetBundle() {
        if (_assetBundleInitialized)
            return Task.FromResult(_storedAssetBundle);

        var stream = typeof(Plugin).Assembly.GetManifestResourceStream("ComputerInterface.Content.CIBundle");
        var newAssetBundle = AssetBundle.LoadFromStream(stream);
        stream?.Close();

        _storedAssetBundle = newAssetBundle;
        _assetBundleInitialized = true;
        return Task.FromResult(newAssetBundle);
    }

    public static async Task<T> LoadAsset<T>(string assetName) where T : Object {
        if (!_assetBundleInitialized)
            await LoadAssetBundle();
        
        if (_loadedAssetsCache != null && _loadedAssetsCache.TryGetValue(assetName, out var loadedAsset))
            return (T)loadedAsset;
        
        Logging.Info($"Loading asset: {assetName}");
        _loadedAssetsCache ??= [];
        
        var newlyLoadedAsset = _storedAssetBundle.LoadAsset<T>(assetName);

        _loadedAssetsCache.Add(assetName, newlyLoadedAsset);
        return newlyLoadedAsset;
    }
}