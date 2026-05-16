using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicaHookingLibrary.Helpers;
public static class MiscHelpers
{
	#region Player Extensions
	/// <summary>
	/// Checks if the <see cref="Player"/> is holding something, if so return the <see cref="PhysicalObject"/>s in a list.
	/// </summary>
	public static bool IsHoldingSomething(this Player player, out PhysicalObject[] grasps)
	{
		grasps = [.. (from grasp in player.grasps where grasp != null select grasp.grabbed).OfType<PhysicalObject>()];
		return grasps.Length > 0;
	}
	#endregion

	#region RoomSpecificScript Helpers
	public static bool IsPlayerInRoom(this UpdatableAndDeletable script, Player player)
	{
		return !script.slatedForDeletetion && script.room == player.room;
	}

	public static IEnumerable<Player> RealizedPlayers(this UpdatableAndDeletable script)
	{
		return from p in script.room.game.Players let realized = p.realizedCreature as Player where realized != null orderby realized.playerState.playerNumber select realized;
	}
	#endregion

	#region Misc Helpers / Extensions
	/// <summary>
	/// A quick way to access the current <see cref="RainWorldGame"/>, if it exists. Only avaliable in game, and not menus.
	/// </summary>
	/// <returns></returns>
	public static bool TryGetCurrentGame(out RainWorldGame game)
	{
		game = null;
		if (Custom.rainWorld.processManager?.currentMainLoop is RainWorldGame actualGame)
		{ 
			game = actualGame;
		}
		return game != null;
	}

	public static IEnumerable<T> GetInstancesOf<T>(this Room room) where T : Type
	{
		return room.updateList.OfType<T>();
	}

	public static string BlankConditional(this bool b, string text)
	{
		return b ? text : "";
	}

	/// <summary>
	/// Meant for quick general testing of IL hooks.
	/// </summary>
	public static void LogIL(this ManualLogSource logger, ILContext il, params ILCursor[] cursors)
	{
		logger?.LogDebug(il.ToString());
		foreach (var c in cursors)
		{
			logger?.LogDebug(c.ToString());
		}
	}

	public static bool IsModActive(string id)
	{
		return ModManager.ActiveMods.Find(mod => mod.id == id) != null;
	}
	#endregion
}
