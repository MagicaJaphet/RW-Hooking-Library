using BepInEx;
using BepInEx.Logging;
using MagicaHookingLibrary.Helpers;
using MonoMod.Utils;
using System.Linq;
using System.Security.Permissions;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MagicaHookingLibrary;

[BepInPlugin("magica.hookinglibrary", "Hooking Library", "1.0.0")]
public class Plugin : PluginTemplate
{
	public static new ManualLogSource Logger;

    public Plugin() : base()
	{
		Logger = base.Logger;
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}
}

// FEATURE: Auto handle loading resources for atlases/sounds/etc
// FEATURE: Helpers for tediously repetative tasks (will have to think of some later)