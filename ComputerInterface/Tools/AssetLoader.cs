using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ComputerInterface.Tools;

internal static class AssetLoader {
    private static bool _assetBundleInitialized;
    private static AssetBundle _storedAssetBundle;
    private static Dictionary<string, Object> _loadedAssetsCache = [];

    private static async Task LoadAssetBundle() {
        if (_assetBundleInitialized)
            return;

        Stream stream = typeof(PluginCore).Assembly.GetManifestResourceStream("ComputerInterface.Content.CIBundle");
        AssetBundleCreateRequest bundleCreateRequest = AssetBundle.LoadFromStreamAsync(stream);

        TaskCompletionSource<AssetBundle> completionSource = new();
        bundleCreateRequest.completed += _ => {
            stream?.Close();
            completionSource.SetResult(bundleCreateRequest.assetBundle);
        };
        AssetBundle newAssetBundle = await completionSource.Task;

        _storedAssetBundle = newAssetBundle;
        _assetBundleInitialized = true;
    }

    public static async Task<T> LoadAsset<T>(string assetName) where T : Object {
        if (!_assetBundleInitialized)
            await LoadAssetBundle();

        if (_loadedAssetsCache != null && _loadedAssetsCache.TryGetValue(assetName, out Object loadedAsset))
            return (T)loadedAsset;

        Logging.Info($"Loading asset: {assetName}");
        _loadedAssetsCache ??= [];

        T newlyLoadedAsset = _storedAssetBundle.LoadAsset<T>(assetName);

        _loadedAssetsCache.Add(assetName, newlyLoadedAsset);
        return newlyLoadedAsset;
    }
}