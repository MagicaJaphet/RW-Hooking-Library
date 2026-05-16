using BepInEx.Logging;
using IL.Watcher;
using MoreSlugcats;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicaHookingLibrary.Helpers;

public static class ExtEnumHelpers
{
	/// <summary>
	/// Returns all registered <see cref="ExtEnum{T}"/> of it's type as an instance of itself.
	/// </summary>
	public static IEnumerable<T> GetAllValues<T>() where T : ExtEnum<T> => from entry in ExtEnum<T>.values.entries select ExtEnumBase.Parse(typeof(T), entry, true) as T;


	/// <summary>
	/// Returns the registered <see cref="ExtEnum{T}"/> of the string value if it exists. Otherwise returns false.
	/// </summary>
	public static bool TryGetExtEnum<T>(string name, out T extEnum) where T : ExtEnum<T>
	{
		extEnum = null;
		ConstructorInfo extEnumConstructor = typeof(T).GetConstructor([typeof(string), typeof(bool)]);
		if (extEnumConstructor != null && extEnumConstructor.Invoke([name, false]) is T e)
		{
			extEnum = e;
		}
		return extEnum != null;
	}
}

/// <summary>
/// An extension class made to alleviate some common issues that a normal <see cref="ExtEnum{T}"/> class may have.
/// </summary>
/// <typeparam name="T">The type of <see cref="ExtEnum{T}"/> to extend.</typeparam>
/// <remarks>
/// Used to register enums.
/// </remarks>
public abstract class ExtExtEnum<T>(string name, bool register = false) : ExtEnum<T>(string.Concat(PreFix, name), register) where T : class
{
	/// <summary>
	/// A string to override with new, used to make <see cref="ExtEnum{T}"/> value names more unique to avoid conflictions.
	/// </summary>
	public static string PreFix { get; } = string.Empty;

	/// <summary>
	/// A subscription which will tell the helper to register our <see cref="ExtEnum{T}"/> at the right time.
	/// </summary>
	internal static void RegisterEnums(ManualLogSource logger)
	{
		FieldInfo[] enumFields = [.. from field in typeof(T).GetFields(ReflectionHelpers.anyFlag) where field != null select field];
		// Gets all of the fields declared under this type
		// With all of that field info, lets run our generic constructor to register the values for ease of use later 
		// This avoids us writing ExtEnum<T> enum = new(nameof(enum), true) for each and every value
		if (enumFields.Length > 0)
		{
			logger?.LogInfo($"Found {enumFields.Length} ExtEnums to register under {nameof(T)}!");

			ConstructorInfo extEnumRegister = typeof(T).GetConstructor([typeof(string), typeof(bool)]);
			if (extEnumRegister != null)
			{
				foreach (FieldInfo field in enumFields)
				{
					field.SetValue(null, extEnumRegister.Invoke([field.Name, true])); // Runs the ExtEnum enum = new(nameof(enum), true) for each enum field;
					logger?.LogInfo($"Registered {nameof(T)} {field.Name}");
				}
			}
		}
	}
}