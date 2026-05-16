using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicaHookingLibrary.Interfaces;
public interface IOwnHooks
{
	/// <summary>
	/// Hooks that would apply before other hooks normally would.
	/// </summary>
	public void PreApply();
	public void OnApply();
	/// <summary>
	/// Hooks that would apply after other hooks normally would.
	/// </summary>
	public void PostApply();
}