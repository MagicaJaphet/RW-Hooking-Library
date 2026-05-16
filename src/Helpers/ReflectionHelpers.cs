using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;
using MagicaHookingLibrary.Helpers;
using MoreSlugcats;
using Watcher;
using System.CodeDom;
using System.Security.Cryptography;

namespace MagicaHookingLibrary.Helpers;
public static class ReflectionHelpers
{
	public static List<Type> ScanTypes()
	{
		List<Type> AllTypes = [];
		foreach (Assembly assembly in GetScanAssemblies())
		{
			Type[] types = null;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = e.Types;
			}
			catch { }

			if (types != null)
			{
				AllTypes.AddRange(from t in types where t != null select t);
			}
		}
		return AllTypes;
	}
	public static BindingFlags anyFlag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	/// <summary>
	/// Taken from DevConsole, a blacklist of Assemblies we should avoid referencing when iterating through all loaded Assemblies.
	/// </summary>
	private static readonly HashSet<string> dllBlacklist =
	[
		"0Harmony",
		"0Harmony20",
		"Accessibility",
		"Assembly-CSharp-firstpass",
		"BepInEx.Harmony",
		"BepInEx.MonoMod.Loader",
		"BepInEx.MultiFolderLoader",
		"BepInEx.Preloader",
		"BepInEx",
		"com.Epic.OnlineServices",
		"com.playeveryware.eos.core",
		"Dragons.PublicDragon",
		"GalaxyCSharp",
		"GoKit",
		"HOOKS-Assembly-CSharp",
		"HarmonyXInterop",
		"Microsoft.Win32.Registry",
		"Mono.Cecil.Mdb",
		"Mono.Cecil.Pdb",
		"Mono.Cecil.Rocks",
		"Mono.Cecil",
		"Mono.Data.Sqlite",
		"Mono.Posix",
		"Mono.Security",
		"Mono.WebBrowser",
		"MonoMod.Common",
		"MonoMod.RuntimeDetour",
		"MonoMod.Utils",
		"MonoMod",
		"Newtonsoft.Json",
		"Novell.Directory.Ldap",
		"Purchasing.Common",
		"Rewired.Runtime",
		"Rewired_Core",
		"Rewired_Windows",
		"SonyNP",
		"SonyPS4CommonDialog",
		"SonyPS4SaveData",
		"SonyPS4SavedGames",
		"StovePCSDK.NET",
		"System.ComponentModel.Composition",
		"System.ComponentModel.DataAnnotations",
		"System.Configuration",
		"System.Core",
		"System.Data",
		"System.Design",
		"System.Diagnostics.StackTrace",
		"System.DirectoryServices",
		"System.Drawing.Design",
		"System.Drawing",
		"System.EnterpriseServices",
		"System.Globalization.Extensions",
		"System.IO.Compression.FileSystem",
		"System.IO.Compression",
		"System.Net.Http",
		"System.Numerics",
		"System.Runtime.Serialization.Formatters.Soap",
		"System.Runtime.Serialization.Xml",
		"System.Runtime.Serialization",
		"System.Runtime",
		"System.Security.AccessControl",
		"System.Security.Principal.Windows",
		"System.Security",
		"System.ServiceModel.Internals",
		"System.Transactions",
		"System.Web.ApplicationServices",
		"System.Web.Services",
		"System.Web",
		"System.Windows.Forms",
		"System.Xml.Linq",
		"System.Xml.XPath.XDocument",
		"System.Xml",
		"System",
		"Unity.Addressables",
		"Unity.Analytics.DataPrivacy",
		"Unity.Burst.Unsafe",
		"Unity.Burst",
		"Unity.Mathematics",
		"Unity.MemoryProfiler",
		"Unity.Microsoft.GDK",
		"Unity.Microsoft.GDK.Tools",
		"Unity.ResourceManager",
		"Unity.ScriptableBuildPipeline",
		"Unity.Services.Analytics",
		"Unity.Services.Core.Analytics",
		"Unity.Services.Core.Configuration",
		"Unity.Services.Core.Device",
		"Unity.Services.Core.Environments.Internal",
		"Unity.Services.Core.Environments",
		"Unity.Services.Core.Internal",
		"Unity.Services.Core.Networking",
		"Unity.Services.Core.Registration",
		"Unity.Services.Core.Scheduler",
		"Unity.Services.Core.Telemetry",
		"Unity.Services.Core.Threading",
		"Unity.Services.Core",
		"Unity.TextMeshPro",
		"Unity.Timeline",
		"UnityEngine.AIModule",
		"UnityEngine.ARModule",
		"UnityEngine.AccessibilityModule",
		"UnityEngine.Advertisements",
		"UnityEngine.AndroidJNIModule",
		"UnityEngine.AnimationModule",
		"UnityEngine.AssetBundleModule",
		"UnityEngine.AudioModule",
		"UnityEngine.ClothModule",
		"UnityEngine.ClusterInputModule",
		"UnityEngine.ClusterRendererModule",
		"UnityEngine.CoreModule",
		"UnityEngine.CrashReportingModule",
		"UnityEngine.DSPGraphModule",
		"UnityEngine.DirectorModule",
		"UnityEngine.GIModule",
		"UnityEngine.GameCenterModule",
		"UnityEngine.GridModule",
		"UnityEngine.HotReloadModule",
		"UnityEngine.IMGUIModule",
		"UnityEngine.ImageConversionModule",
		"UnityEngine.InputLegacyModule",
		"UnityEngine.InputModule",
		"UnityEngine.JSONSerializeModule",
		"UnityEngine.LocalizationModule",
		"UnityEngine.Monetization",
		"UnityEngine.ParticleSystemModule",
		"UnityEngine.PerformanceReportingModule",
		"UnityEngine.Physics2DModule",
		"UnityEngine.PhysicsModule",
		"UnityEngine.ProfilerModule",
		"UnityEngine.Purchasing.AppleCore",
		"UnityEngine.Purchasing.AppleMacosStub",
		"UnityEngine.Purchasing.AppleStub",
		"UnityEngine.Purchasing.Codeless",
		"UnityEngine.Purchasing.SecurityCore",
		"UnityEngine.Purchasing.SecurityStub",
		"UnityEngine.Purchasing.Stores",
		"UnityEngine.Purchasing.WinRTCore",
		"UnityEngine.Purchasing.WinRTStub",
		"UnityEngine.Purchasing",
		"UnityEngine.RuntimeInitializeOnLoadManagerInitializerModule",
		"UnityEngine.ScreenCaptureModule",
		"UnityEngine.SharedInternalsModule",
		"UnityEngine.SpatialTracking",
		"UnityEngine.SpriteMaskModule",
		"UnityEngine.SpriteShapeModule",
		"UnityEngine.StreamingModule",
		"UnityEngine.SubstanceModule",
		"UnityEngine.SubsystemsModule",
		"UnityEngine.TLSModule",
		"UnityEngine.TerrainModule",
		"UnityEngine.TerrainPhysicsModule",
		"UnityEngine.TextCoreModule",
		"UnityEngine.TextRenderingModule",
		"UnityEngine.TilemapModule",
		"UnityEngine.UI",
		"UnityEngine.UIElementsModule",
		"UnityEngine.UIElementsNativeModule",
		"UnityEngine.UIModule",
		"UnityEngine.UNETModule",
		"UnityEngine.UmbraModule",
		"UnityEngine.UnityAnalyticsCommonModule",
		"UnityEngine.UnityAnalyticsModule",
		"UnityEngine.UnityConnectModule",
		"UnityEngine.UnityCurlModule",
		"UnityEngine.UnityTestProtocolModule",
		"UnityEngine.UnityWebRequestAssetBundleModule",
		"UnityEngine.UnityWebRequestAudioModule",
		"UnityEngine.UnityWebRequestModule",
		"UnityEngine.UnityWebRequestTextureModule",
		"UnityEngine.UnityWebRequestWWWModule",
		"UnityEngine.VFXModule",
		"UnityEngine.VRModule",
		"UnityEngine.VehiclesModule",
		"UnityEngine.VideoModule",
		"UnityEngine.VirtualTexturingModule",
		"UnityEngine.WindModule",
		"UnityEngine.XR.LegacyInputHelpers",
		"UnityEngine.XRModule",
		"UnityEngine",
		"UnityPlayer",
		"com.rlabrecque.steamworks.net",
		"mscorlib",
		"netstandard",
		"XblPCSandbox",
	];

	/// <summary>
	/// Taken from DevConsole, scans all of the avaliable Assembllies and excludes our blacklisted ones.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<Assembly> GetScanAssemblies()
	{
		return AppDomain.CurrentDomain.GetAssemblies().Where(asm => !dllBlacklist.Contains(asm.GetName().Name));
	}

	public static Assembly GetTraceAssembly(Assembly origin)
	{
		var scanAssemblies = GetScanAssemblies();
		var trace = new StackTrace(true);
		var firstTrace = false;
		for (int i = 0; i < trace.FrameCount; i++)
		{
			MethodBase method = trace.GetFrame(i).GetMethod();
			Assembly asm = method?.ReflectedType?.Assembly;

			if (!firstTrace && asm.GetName().Name != Assembly.GetExecutingAssembly().GetName().Name)
			{
				continue;
			}
			if (!scanAssemblies.Contains(asm)) break;
			if (asm.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name)
			{
				firstTrace = true;
				continue;
			}
			if (asm != typeof(RainWorld).Assembly && asm != Assembly.GetExecutingAssembly())
			{
				origin = asm;
				break;
			}
		}
		return origin;
	}

	public static bool GetModInfoFromAssembly(string assemblyPath, out string modInfoPath, out Dictionary<string, object> json)
	{
		json = null;
		modInfoPath = null;
		while (Directory.GetParent(assemblyPath) is DirectoryInfo parent && !File.Exists(modInfoPath))
		{
			assemblyPath = parent.FullName;
			modInfoPath = Path.Combine(assemblyPath, "modinfo.json");
		}

		if (!string.IsNullOrEmpty(modInfoPath) && File.Exists(modInfoPath))
		{
			var jsonDict = File.ReadAllText(modInfoPath).dictionaryFromJson();
			if (jsonDict != null)
			{
				json = jsonDict;
				return true;
			}
		}
		return false;
	}

	public static bool JsonDictValue<T>(this Dictionary<string, object> json, string key, out T value) where T : class
	{
		if (json != null && json.TryGetValue(key, out var obj) && obj is T data)
		{
			value = data;
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Returns all valid <see cref="FieldInfo"/> that can be accessed without a separate assembly which may not be loaded.
	/// </summary>
	public static IEnumerable<FieldInfo> GetValidFieldsFromType(Type t) => from field in t.GetFields(BindingFlags.Public | BindingFlags.Static) where TryCatchFieldType(field) select field;

	/// <summary>
	/// Shorthand to retrieve all valid loaded fields at the plugin's runtime.
	/// </summary>
	public static IEnumerable<FieldInfo> GetAllValidLoadedFields() => ScanTypes().SelectMany(GetValidFieldsFromType);

	/// <summary>
	/// Wrapper to catch soft dependencies.
	/// </summary>
	public static bool TryCatchFieldType(FieldInfo field)
	{
		try
		{
			return field != null && field.FieldType != null;
		}
		catch (FileNotFoundException ex)
		{
			Plugin.Logger.LogInfo($"Attempted to access field {field.Name ?? "???"} from {ex.FileName}, caught potential error.");
		}
		catch { }
		return false;
	}

	/// <summary>
	/// Returns reflected ExtEnum fields in classes that only contain ExtEnum values.
	/// </summary>
	public static IEnumerable<FieldInfo> GetReflectedExtEnumFields<T>() where T : ExtEnum<T>
	{
		return from t in ScanTypes()
			   let fields = GetValidFieldsFromType(t) 
			   from f in fields where typeof(T).IsAssignableFrom(f.FieldType) && !fields.Any(IsExtEnumNotPartOfAExtEnumOnlyClass<T>) 
			   select f;
	}

	public static bool IsExtEnumNotPartOfAExtEnumOnlyClass<T>(FieldInfo test) where T : ExtEnum<T>
	{
		return test?.FieldType != null && !typeof(T).IsAssignableFrom(test.FieldType);
	}

	/// <summary>
	/// Implemented quick access to the field info that contains the <see cref="SlugcatStats.Name"/>.
	/// </summary>
	public static FieldInfo GetSlugcatFieldInfo(this string name)
	{
		SlugcatNameFieldInfo ??= (from t in GetReflectedExtEnumFields<SlugcatStats.Name>() select t).ToDictionary(t => t.Name);
		
		if (SlugcatNameFieldInfo.TryGetValue(name, out var field))
		{
			return field;
		}
		return null;
	}

	internal static Dictionary<string, FieldInfo> SlugcatNameFieldInfo;
}
