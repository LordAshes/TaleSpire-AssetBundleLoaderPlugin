# Asset Bundle Loader Plugin

This unofficial TaleSpire plugin is for providing easy AssetBundle loading at runtime
functionality. Supports both loading various AssetBundles on parent plugin startup and  
loading AssetBundles on demand during runtime.

## Change Log

1.0.0: Initial release

## Install

Install using R2ModMan or similar. Reference the plugin DLL in the parent plugin projects and add

```C#
[BepInDependency(LordAshes.AssetBundleLoaderPlugin.Guid)]
````

to the plugin main (BaseUnityPlugin) class. This will allow the parent plugin to use the various
methods provided by this plugin which allow easy access to AssetBundle loading.

## Typical Usage

```C#
[BepInPlugin(Guid, Name, Version)]
[BepInDependency(LordAshes.AssetBundleLoaderPlugin.Guid)]
public class AssetBundleExamplePlugin : BaseUnityPlugin
{
    // Plugin info
    public const string Name = "Asset Bundle Example Plug-In";
    public const string Guid = "org.lordashes.plugins.assetbundleexample";
    public const string Version = "1.0.0.0";
	
    // Content directory
    private string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

    void Awake()
    {
		// Load any Asset Bundles in the Manifest file that are marked as preload
        LordAshes.AssetBundleLoaderPlugin.LoadManifest(dir+"Config/"+AssetBundleExamplePlugin.Guid+"/AssetBundles.json);
    }
	
	void Update()
	{
	     ...
		 
		 // Load an Asset Bundle specified in the Asset Bundle JSO Manifest file
		 LordAshes.AssetBundleLoaderPlugin.LoadAssetBundle("Pokemon");
		 
		 ...
		 
		 // Load an Asset Bundle not specified in the Asset Bundle JSO Manifest file
		 LordAshes.AssetBundleLoaderPlugin.LoadAssetBundleNotInManifest("Dragon", "D:/Steam/steamapps/common/TaleSpire/TaleSpire_CustomData/Minis/dragon/char_dragon_0000000001");
		 
		 ...
		 
		 // Create anm object from the prefab Dragon01 in the Assset Bundle associated with the unique name Dragon
		 GameObject go = GetPrefabInstance("Dragon", "Dragon01");
	}
}
```

## The Ways To Load

The plugin allow the following three different methods for loading AssetBundles at runtime...

### AssetBundle Manifest

Code similar to the following can be used 

```C#
LordAshes.AssetBundleLoaderPlugin.LoadManifest(*manifestDirectory*);
```

to load AssetBundle JSON Manifest file which associates AssetBundles with a unique name and
indicates if they should be preloaded. This method of loading AssetBundles is ideal when the
parent plugin knows which assetBundles are needed at startup. However, even if some AssetBundles
are known in advance but not necessarily loaded on startup, it can be advantageous to include
them in the manifest. The main advantage of using the manifest file is that the location of the
AssetBundle is defined in the manifest file as opposed to being hard coded in the plugin. This
means that the location can easily be modified as needed.

The name of the AssetBundle JSON file is always *assetBundles.json* but the LoadManifest()
method allows the specification of the manifest location. Typically this would be placed in
*TaleSpire_CustomData/Config/{plugin Guid}* where the plugin guid would be the guid of the
parent plugin so that each plugin can have its own AssetBundle manifest.

The contents of the *assetBundles.json* look...

```JSON
{
	"Android": { "source": "D:/Steam/steamapps/common/TaleSpire/TaleSpire_CustomData/Minis/AndroidV5/char_avdroidv5_0000000001", "preload": false},
	"Kiki": { "source": "D:/Steam/steamapps/common/TaleSpire/TaleSpire_CustomData/Minis/lordashes/lordashes", "preload": true}
}
```

The key for each line is the unique name associated with the AssetBundle and is the name that will be used to access the AssetBundle in code.
The source property of the value indicates the full path and file name of the AssetBundle.
The preload property of the value indicates if the AssetBundle should be loaded when the LoadManifest method is called.

If the AssetBundle is not preloaded then it information is still stored and thus the AssetBundle can be loaded later using the unique name. See below.

In most cases the LoadManifest() is used in the parent plugin's Awake() function to preload any AssetBundles marked for preload at plugin startup.

### Loading From The Manifest

If a AssetBundle is defined in the manifest file but has preload set to false (see above), the AssetBundle can be loaded, at a later time, using:

```C#
LordAshes.AssetBundleLoaderPlugin.LoadAssetBundle(*manifestUniqueName*);
```

Where *manifestUniqueName* is one of the keys in the manifest file. Using this method allows the AssetBundle to be loaded only when needed thus
preventing the unnecessary loading of AssetBundles which may not be needed but it still gives the advanatge of being able to refer to the AssetBundle
by the unique name instead of specifying the exact path and filename of the AssetBundle. As discussed above, this allows the location of the AssetBundle
to be changed without affecting the parent plugin.

### Loading Non-Manifest AssetBundles

In some cases, the name of the AssetBundles is not known in advance or it is not practical to update the manifest with all the possible AssetBundles.
In such cases the LoadAssetBundle() method cannot be used since this method uses the manifest unique name. Instead the following method is used:

```C#
LordAshes.AssetBundleLoaderPlugin.LoadAssetBundleNotInManifest(*manifestUniqueName*, *source*);
```

Where *manifestUniqueName* creates a unique name for the AssetBundle (which is how it is referred to in code).
Where *source* specifies the full path and file name of the AssetBundle.

While this method allows access to AssetBundles not listed in the manifest, the disadvantage of using this method is that the location of the AssetBundle
becomes hard coded and cannot be changed without needing to re-compile the parent plugin.

## Using Runtime Load AssetBundles

Two methods have been provided for using assets in a runtime loaded AssetBundle...

```C#
GetAsset(*assetBundleName*, *objectName*)
```

This method returns an GameObject associated with the name *objectName* in the AssetBundle associated with the unique name *assetBundleName*.
In most cases this will return the corresponding prefab object which normally needs to be Instanced. As such the second method is more common
for creating GameObject instances. However, if one is making a large number of instances of a single prefab then using this method is more
efficient since the prefab only needs to be looked up once. 

```C#
GetPrefabInstance(*assetBundleName*, *objectName*)
```

This method returns an GameObject instance associated with the prefab name *objectName* in the AssetBundle associated with the unique name
*assetBundleName*. Typically this method is the method used to create instances of AssetBundle objects unless one is making a large amount
of instances of the same prefab (in which case it is more efficient to get a reference to the prefab and the make instances of it).

## Helper Methods

```C#
GetAssetBundleInfo(*assetBundleName*)
```

Gets information about the indicated AssetBundle. This provides the same information as the AssetBundle Manifest file but it also includes
a "loaded" property which indicates the state of the AssetBundle. 0=NotLoaded, 1=Loading, 2=Loaded, -1=FailedToLoad

```C#
GetAssetBundlesInfo(*assetBundleName*)
```

Same as above but returns the information for all AssetBundles.

```C#
GetAssetNamesInAssetBundle(*assetBundleName*)
```

Retrieves the names of all assets in the AssetBundle.

## Unloading AssetBundles

```C#
LordAshes.AssetBundleLoaderPlugin.UnloadAssetBundle(*manifestUniqueName*);
```

## To Reload Or Not To Reload

Both the LoadAssetBundle() and the LoadAssetBundleNotInManifest() can specify an optional parameter after the required set of parameters. This is the

reload setting. This is a boolean value indicating if attempts to load an AssetBundle that is currently loaded or load an AssetBundle which previously
failed to load should be reloaded. In the case of a AssetBundle already loaded, if reload is true then the AsetBundle will be unloaded and then loaded
again. In the case of a previous failed load, the load will be repeated. If reload is false then a warning will be issues and no action taken in these
two cases.




 
