using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ComputerInterface
{
    internal class AssetsLoader : IDisposable
    {
        public bool IsLoaded { get; private set; }

        private AssetBundle _loadedBundle;
        private Task _loadingTask;

        private readonly Dictionary<string, Object> _assetCache = new();

        public async Task<T> GetAsset<T>(string name) where T : Object
        {
            if (_assetCache.TryGetValue(name, out Object cachedObject)) return (T)cachedObject;

            if (!IsLoaded)
            {
                _loadingTask ??= LoadBundleAsyncInternal();

                await _loadingTask;
            }

            TaskCompletionSource<T> completionSource = new();

            AssetBundleRequest assetBundleRequest = _loadedBundle.LoadAssetAsync<T>(name);
            assetBundleRequest.completed += _ =>
            {
                if (assetBundleRequest.asset == null)
                {
                    Debug.LogError($"Asset {name} not found");
                    completionSource.SetResult(null);
                    return;
                }

                completionSource.SetResult((T)assetBundleRequest.asset);
            };

            T completedTask = await completionSource.Task;
            _assetCache.Add(name, completedTask);
            return completedTask;
        }

        private async Task LoadBundleAsyncInternal()
        {
            TaskCompletionSource<AssetBundle> completionSource = new();

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ComputerInterface.Content.CIBundle");
            if (stream == null)
            {
                Debug.LogError("Couldn't load embedded assets");
                return;
            }

            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(stream);
            assetBundleCreateRequest.completed += _ =>
            {
                completionSource.SetResult(assetBundleCreateRequest.assetBundle);
            };

            _loadedBundle = await completionSource.Task;

            IsLoaded = true;
        }

        public void Unload()
        {
            if (!IsLoaded) return;
            Debug.LogError("Unloading bundle");
            IsLoaded = false;
            _loadedBundle.Unload(true);
            _loadedBundle = null;
        }

        public void Dispose()
        {
            Unload();
        }
    }
}