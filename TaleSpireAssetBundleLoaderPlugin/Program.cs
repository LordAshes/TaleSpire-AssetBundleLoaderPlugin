using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;

using UnityEngine;

using System;
using System.Collections.Generic;

namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    public class AssetBundleLoaderPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Asset Bundle Loader Plug-In";
        public const string Guid = "org.lordashes.plugins.assetbundleloader";
        public const string Version = "1.0.0.0";

        // Dictionaries for holding asset bundles references and asset bundles information
        private static Dictionary<string, AssetBundleInfo> assetBundlesInfo = null;
        private static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// Function for initializing plugin
        /// (This function is called once by TaleSpire)
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Lord Ashes AssetBundle Loader Plugin Active.");
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// (This function is called periodically by TaleSpire)
        /// </summary>
        void Update()
        {
        }

        /// <summary>
        /// Method to preload asset bundles from the manifest
        /// </summary>
        /// <param name="source">PString path to the JSON manifest file</param>
        public static void LoadManifest(string source)
        {
            Debug.Log("Reading AssetBundle manifest from '" + source + "/assetBundles.json'");

            assetBundlesInfo = JsonConvert.DeserializeObject<Dictionary<string, AssetBundleInfo>>(System.IO.File.ReadAllText(source + "/assetBundles.json"));

            foreach (KeyValuePair<string, AssetBundleInfo> assetBundle in assetBundlesInfo)
            {
                Debug.Log("AssetBundle Manifset Contains '" + assetBundle.Key + "' (source: '" + assetBundle.Value.source + "') with preload set to '" + assetBundle.Value.preload + "'");
                if (assetBundle.Value.preload)
                {
                    LoadAssetBundle(assetBundle.Key);
                }
            }
        }

        /// <summary>
        /// Method to obtain AssetBundle information
        /// </summary>
        /// <param name="name">Asset bundle unique name</param>
        /// <returns>AssetBundleInfo object about the asset bundle</returns>
        public static AssetBundleInfo GetAssetBundleInfo(string name)
        {
            return (assetBundlesInfo.ContainsKey(name)) ? assetBundlesInfo[name] : null;
        }

        /// <summary>
        /// Method to obtain AssetBundle information on all AssetBundles loaded through this plugin
        /// </summary>
        /// <returns>Dictionary of AsetBundleInfo index by asset bundle unique name</returns>
        public static Dictionary<string,AssetBundleInfo> GetAssetBundlesInfo()
        {
            return assetBundlesInfo;
        }

        /// <summary>
        /// Method for listing asset names in asset bundgle
        /// </summary>
        /// <param name="assetBundleName">Unique id for asset bundle</param>
        /// <returns>Array of string names</returns>
        public static string[] GetAssetNamesInAssetBundle(string assetBundleName)
        {
            return assetBundles[assetBundleName].GetAllAssetNames();
        }

        /// <summary>
        /// Method to load a manifest specified asset bundle
        /// </summary>
        /// <param name="name">Unique name assigned to the asset bundle</param>
        /// <param name="reload">Boolean indicating if failed loads should try to reload</param>
        public static void LoadAssetBundle(string name, bool reload = false)
        {
            if (assetBundlesInfo.ContainsKey(name))
            {
                Debug.Log("AssetBundle '" + name + "' has a source of '"+ assetBundlesInfo[name].source+"' according to the AssetBundle Manifest");
                LoadAssetBundleNotInManifest(name, assetBundlesInfo[name].source, reload);
            }
            else
            {
                Debug.LogWarning("AssetBundle '"+name+ "' is not defined in the AssetBundle Manifest file. Use LoadAssetBundleNotInManifest() to load non-manifest assetBundle.");
            }
        }

        /// <summary>
        /// Method to load asset bundle from the specified location
        /// </summary>
        /// <param name="name">Unique name assigned to the asset bundle</param>
        /// <param name="source">String representing the path and file name of the asset bundle</param>
        /// <param name="reload">Boolean indicating if failed loads should try to reload</param>
        public static void LoadAssetBundleNotInManifest(string name, string source, bool reload = false)
        {
            if (!assetBundlesInfo.ContainsKey(name))
            {
                // Make new info entry for any asset bundlesnot in the manifest
                Debug.Log("Adding AssetBundle '" + name + "' with source '" + source + "' to the AssetBundleInfo dictionary");
                assetBundlesInfo.Add(name, new AssetBundleInfo() { source = source, preload = false, loaded = LoadState.notLoaded });
            }
            switch (assetBundlesInfo[name].loaded)
            {
                // Regular Load Since Aset Bundle Has Not Been Loaded
                case LoadState.notLoaded:
                    assetBundlesInfo[name].loaded = LoadState.loading;
                    try
                    {
                        assetBundles[name] = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assetBundlesInfo[name].source), System.IO.Path.GetFileName(assetBundlesInfo[name].source)));
                        Debug.Log("Asset Bundle '" + name + " (" + assetBundlesInfo[name].source + ") has been loaded");
                        assetBundlesInfo[name].loaded = LoadState.loaded;
                    }
                    catch (Exception)
                    {
                        Debug.Log("Asset Bundle '" + name + " (" + assetBundlesInfo[name].source + ") has failed to load");
                        assetBundlesInfo[name].loaded = LoadState.failedToLoad;
                    }
                    break;
                // Ignore Request Because Asset Bundle Is Being Loaded
                case LoadState.loading:
                    Debug.Log("Asset Bundle '" + name + " (" + assetBundlesInfo[name].source + ") load in progress");
                    break;
                // Previous Failed Load, Try Again Only If Reload Is True
                case LoadState.failedToLoad:
                    if (reload == true)
                    {
                        assetBundlesInfo[name].loaded = LoadState.notLoaded;
                        LoadAssetBundleNotInManifest(name, source, reload);
                    }
                    else
                    {
                        Debug.LogWarning("Asset Bundle '" + name + " (" + assetBundlesInfo[name].source + ") failed to load previously. Reload set to false");
                    }
                    break;
                // Asset Bundle Is Currently Loaded, Unload And Reload Only If Reload Is True
                case LoadState.loaded:
                    if (reload == true)
                    {
                        assetBundles[name].Unload(true);
                        Debug.Log("Asset Bundle '" + name + " (" + assetBundlesInfo[name].source + ") unloaded");
                        assetBundles.Remove(name);
                        assetBundlesInfo[name].loaded = LoadState.notLoaded;
                        LoadAssetBundleNotInManifest(name, source, reload);
                    }
                    else
                    {
                        Debug.LogWarning("Asset Bundle '" + name + " (" + assetBundlesInfo[name].source + ") is already loaded. Reload set to false");
                    }
                    break;
            }
        }

        /// <summary>
        /// Unload a Asset Bundle
        /// </summary>
        /// <param name="name">Unique identified for the asset bundle</param>
        public static void UnloadAssetBundle(string name)
        {
            assetBundles[name].Unload(true);
            Debug.Log("Asset Bundle '" + name + " (" + assetBundlesInfo[name].source + ") unloaded");
            assetBundles.Remove(name);
        }

        /// <summary>
        /// Method to get an asset from the corresponding asset bundle. Usually a prefab.
        /// Normally you can use the GetPrefabInstance() instead but getting a reference to the prefab using this method can be more efficient if you are making many prefab instances.
        /// </summary>
        /// <param name="assetBundleName">Unique identifier for the asset bundle</param>
        /// <param name="objectName">Name of the object (prefab) to be instanced</param>
        /// <returns>GameObject (prefab) of the specified name</returns>
        public static GameObject GetAsset(string assetBundleName, string objectName)
        {
            Debug.Log("Getting '"+objectName+"' from Asset Bundle '" + assetBundleName + " (" + assetBundlesInfo[assetBundleName].source + ")");
            return assetBundles[assetBundleName].LoadAsset<GameObject>(objectName);
        }

        /// <summary>
        /// Method used to get an instance of a prefab asset from the asset bundle
        /// </summary>
        /// <param name="assetBundleName">Unique identifier for the asset bundle</param>
        /// <param name="objectName">Name of the object (prefab) to be instanced</param>
        /// <returns>GameObject instance of the specified prefab</returns>
        public static GameObject GetPrefabInstance(string assetBundleName, string objectName)
        {
            Debug.Log("Getting '" + objectName + "' Instance from Asset Bundle '" + assetBundleName + " (" + assetBundlesInfo[assetBundleName].source + ")");
            return Instantiate(assetBundles[assetBundleName].LoadAsset<GameObject>(objectName));
        }

        /// <summary>
        /// Enumeration for asset bundle states
        /// </summary>
        public enum LoadState
        {
            failedToLoad = -1,
            notLoaded = 0,
            loading = 1,
            loaded = 2
        }

        /// <summary>
        /// Class for holding AssetBundles
        /// </summary>
        public class AssetBundleInfo
        {
            public string source { get; set; }
            public bool preload { get; set; } = false;
            public LoadState loaded { get; set; } = LoadState.notLoaded;
        }
    }
}