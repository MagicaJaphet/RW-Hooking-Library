using BepInEx.Logging;
using MagicaHookingLibrary.Interfaces;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using System.Collections;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Data;

namespace MagicaHookingLibrary.Helpers;

public static class HookHelpers
{
	internal static IEnumerable<TypeInfo> ScanHookClasses(Assembly ass, ManualLogSource logger)
	{
		return from c in ass.GetTypes() where typeof(IOwnHooks).IsAssignableFrom(c) && !c.IsAbstract select c.GetTypeInfo();
	}

	/// <summary>
	/// An init wrapper that invokes <paramref name="orig"/> before running our code.
	/// </summary>
	internal static void InitWrapper<T>(ref bool isInit, T orig, RainWorld self, Action<RainWorld> action, HookType type = HookType.On) where T : Delegate
	{
		orig.DynamicInvoke([self]);

		try
		{
			if (!isInit)
			{
				isInit = true;
				action(self);
			}
		}
		catch (Exception ex)
		{
			Plugin.Logger.LogError(ex);
		}
	}


	public static VariableDefinition ImplementLocalVariable<T>(this ILContext il)
	{
		VariableDefinition localVar = new(il.Module.ImportReference(typeof(T)));
		il.Body.Variables.Add(localVar);
		il.Body.InitLocals = true;

		return localVar;
	}

	public static ILCursor CloneAndGoToNext(this ILCursor c, MoveType moveType = MoveType.Before, params Func<Instruction, bool>[] moves)
	{
		ILCursor clone = c.Clone();
		return clone.GotoNext(moveType, moves);
	}

	public static ILCursor CloneAndGoToNext(this ILCursor c, params Func<Instruction, bool>[] moves)
	{
		ILCursor clone = c.Clone();
		return clone.GotoNext(MoveType.Before, moves);
	}

	public static ILCursor CloneAndGoToPrev(this ILCursor c, MoveType moveType = MoveType.Before, params Func<Instruction, bool>[] moves)
	{
		ILCursor clone = c.Clone();
		return clone.GotoPrev(moveType, moves);
	}

	public static ILCursor CloneAndGoToPrev(this ILCursor c, params Func<Instruction, bool>[] moves)
	{
		ILCursor clone = c.Clone();
		return clone.GotoPrev(MoveType.Before, moves);
	}

	/// <summary>
	/// Attempts to move the <see cref="ILCursor"/> after the next instance of <see cref="SlugcatStats.Name"/>. Returns true if successful.
	/// </summary>
	public static bool TryMoveToNextSlugcatBool(this ILCursor cursor, FieldInfo name)
	{
		if (cursor.TryGotoNext(MoveType.After,
				move => move.MatchLdsfld(name),
				move => move.MatchCallOrCallvirt(out _)))
			{
				cursor.MoveAfterLabels();
				return true;
			}
		return false;
	}

	public static void EmitLdarg0Delegate<T>(this ILCursor cursor, T dele) where T : Delegate
	{
		cursor.Emit(OpCodes.Ldarg_0);
		cursor.EmitDelegate<T>(dele);
	}

	public enum HookType
	{
		Pre,
		On,
		Post
	}

	/// <summary>
	/// Automatically apply hooks without needing to actually go out and manually call each class. There are going to be a lot of them.
	/// </summary>
	public static void ApplyHooks(HookType type, ManualLogSource logger)
	{
		foreach (Type hookClass in ScanHookClasses(Assembly.GetCallingAssembly(), logger))
		{
			object hookClassInstance = Activator.CreateInstance(hookClass);
			string hookType = Enum.GetName(typeof(HookType), type);
			if (hookClass.GetMethod($"{hookType}Apply") is MethodInfo applyMethod && applyMethod.GetMethodBody().GetILAsByteArray().Length > 2)
			{
				applyMethod.Invoke(hookClassInstance, null);
				logger.LogInfo($"{hookClass.Name} for {Enum.GetName(typeof(HookType), type)}ModsInit applied!");
			}
		}
	}
}