using BepInEx;
using BepInEx.Logging;
using MagicaHookingLibrary.Common_Strings;
using MagicaHookingLibrary.Helpers;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static MagicaHookingLibrary.Helpers.HookHelpers;
#pragma warning restore CS0618

namespace MagicaHookingLibrary;

/// <summary>
/// A plugin template class to inherit from which helps with applying hooks and loading resources.
/// </summary>
public abstract class PluginTemplate : BaseUnityPlugin
{

	/// <summary>
	/// A held instance of <see cref="HookHelpers"/> for the purposes on singular init variables instead of sharing from the same pool.
	/// </summary>
	public WrapperBools Wrappers { get; } = new();

	/// <summary>
	/// An automatically set instance of the plugin's ID, if it exists.
	/// </summary>
	public static string MOD_ID { get; private set; }
	/// <summary>
	/// An automatically set instance of the plugin's name, if it exists.
	/// </summary>
	public static string MOD_NAME { get; private set; }
	/// <summary>
	/// An automatically set instance of the plugin's <see cref="Version"/>, if it exists.
	/// </summary>
	public Version MOD_VERSION { get; private set; }

	public PluginTemplate()
	{
		var originAssembly = ReflectionHelpers.GetTraceAssembly(typeof(PluginTemplate).Assembly);
		var plugin = (from c in originAssembly.DefinedTypes select c.GetCustomAttribute(typeof(BepInPlugin))).OfType<BepInPlugin>().FirstOrDefault();
		if (plugin != null)
		{
			MOD_ID = plugin.GUID;
			MOD_NAME = plugin.Name;
			MOD_VERSION = plugin.Version;

			#if DEBUG
			// Automatically handle JSON configuration from here, to quickly update everything
			if (ReflectionHelpers.GetModInfoFromAssembly(originAssembly.Location, out string modInfoPath, out var jsonDict))
			{
				bool shouldWrite = false;
				if (jsonDict.JsonDictValue(ModInfoKeys.ID, out string id) && id != MOD_ID)
				{
					shouldWrite = true;
					jsonDict[ModInfoKeys.ID] = MOD_ID;
				}

				if (jsonDict.JsonDictValue(ModInfoKeys.Name, out string name) && name != MOD_NAME)
				{
					shouldWrite = true;
					jsonDict[ModInfoKeys.Name] = MOD_NAME;
				}

				if (jsonDict.JsonDictValue(ModInfoKeys.Version, out string version) && new Version(version) != MOD_VERSION)
				{
					shouldWrite = true;
					jsonDict[ModInfoKeys.Version] = MOD_VERSION.ToString();
				}

				if (!jsonDict.JsonDictValue(ModInfoKeys.Requirements, out List<string> requirements) || requirements == null)
				{
					IEnumerable<(Assembly, BepInPlugin)> modPlugins = from a in ReflectionHelpers.GetScanAssemblies() from c in a.DefinedTypes let plug = c.GetCustomAttribute(typeof(BepInPlugin)) where plug != null select (a, plug as BepInPlugin);
					List<string> dependencies = requirements ?? [];
					List<string> dependencyNames = jsonDict.JsonDictValue(ModInfoKeys.Requirements, out List<string> requirementsNames) ? requirementsNames : [];
					
					foreach (var assemblyDep in (from c in Assembly.GetExecutingAssembly().DefinedTypes select c.GetCustomAttribute(typeof(BepInDependency))).OfType<BepInDependency>())
					{
						foreach ((Assembly assembly, BepInPlugin modPlugin) in modPlugins)
						{
							if (modPlugin.GUID == assemblyDep.DependencyGUID)
							{
								if (ReflectionHelpers.GetModInfoFromAssembly(assembly.Location, out var _, out var modJson) && modJson.JsonDictValue(ModInfoKeys.ID, out string modID) && modJson.JsonDictValue(ModInfoKeys.Name, out string modName))
								{
									if (requirements == null || (requirements != null && !requirements.Contains(modPlugin.GUID)))
									{
										shouldWrite = true;
										dependencies.Add(modID);
										dependencyNames.Add(modName);
									}
								}
								break;
							}
						}
					}
					jsonDict[ModInfoKeys.Requirements] = dependencies;
					jsonDict[ModInfoKeys.RequirementNames] = dependencyNames;
				}

				if (shouldWrite)
				{
					Logger?.LogInfo($"JSON and Assembly mismatch for {MOD_ID}!");
					File.WriteAllText(modInfoPath, Newtonsoft.Json.JsonConvert.SerializeObject(jsonDict, Newtonsoft.Json.Formatting.Indented));
				}
			}
			#endif
		}
		ReflectionHelpers.ScanTypes();

		// Find some way to automatically unsubscribe from all event subscriptions at some point :P
	}

	/// <summary>
	/// The second method invoked by <see cref="BepInEx.Bootstrap.Chainloader"/>. Virtually similar to Awake. Includes pre-wrapped hooks for the ModsInit methods.
	/// </summary>
	public virtual void OnEnable()
	{
		// Load resources here
		On.RainWorld.PreModsInit += Wrappers.PreWrapper(PreModsInit);
		On.RainWorld.OnModsInit += Wrappers.OnWrapper(OnModsInit);
		On.RainWorld.PostModsInit += Wrappers.PostWrapper(PostModsInit);
	}

	/// <summary>
	/// The first method invoked by <see cref="Menu.InitializationScreen.Update"/> or <see cref="RainWorld.Update"/>. Where you'd want to apply hooks that wrap around orig before other code mods modify them, or at one of the earliest possible convieniences.
	/// </summary>
	public virtual void PreModsInit(RainWorld self) {}

	/// <summary>
	/// The second method invoked by <see cref="Menu.InitializationScreen.Update"/> or <see cref="RainWorld.Update"/>. Where <see cref="ExtEnum{T}"/> are initalized, and most code mods apply their hooks.
	/// </summary>
	public virtual void OnModsInit(RainWorld self) {}

	public void RegisterAllExtExtEnums()
	{
		Logger?.LogInfo(Assembly.GetCallingAssembly().FullName);
		MethodInfo[] registerMethods = [.. from c in Assembly.GetCallingAssembly().DefinedTypes where typeof(ExtEnumBase).IsAssignableFrom(c) && !c.IsAbstract let m = c.GetMethod("RegisterEnums", BindingFlags.Static | BindingFlags.NonPublic) where m != null && !m.ContainsGenericParameters select m];
		foreach (MethodInfo register in registerMethods)
		{
			try
			{
				register.Invoke(null, []);
			}
			catch (Exception ex)
			{
				Logger?.LogError($"Failed to register ExtEnums: {ex}");
			}
		}
	}

	/// <summary>
	/// The last method invoked by <see cref="Menu.InitializationScreen.Update"/> or <see cref="RainWorld.Update"/>. Where you'd want to apply hooks that wrap around orig after other code mods modify them, or at the lastest possible opportunity.
	/// </summary>
	public virtual void PostModsInit(RainWorld self) {}

	/// <summary>
	/// Used to load in resources at the typical point they're loaded in game.
	/// </summary>
	public virtual void LoadResources() { }
}

public class WrapperBools
{
	private bool isPreInit;
	private bool isOnInit;
	private bool isPostInit;

	internal On.RainWorld.hook_PreModsInit PreWrapper(Action<RainWorld> modsInitAction) => (orig, self) => InitWrapper(ref isPreInit, orig, self, modsInitAction);
	internal On.RainWorld.hook_OnModsInit OnWrapper(Action<RainWorld> modsInitAction) => (orig, self) => InitWrapper(ref isOnInit, orig, self, modsInitAction);
	internal On.RainWorld.hook_PostModsInit PostWrapper(Action<RainWorld> modsInitAction) => (orig, self) => InitWrapper(ref isPostInit, orig, self, modsInitAction);
}