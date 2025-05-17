using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Dalamud.Hooking;
using WoLuaX.Constants;

namespace WoLuaX.Game;

public abstract class GameFunctionBase<T> where T : Delegate {
	private readonly nint addr = nint.Zero;
	public nint Address => addr;
	private T? function;
	public bool Valid => function is not null || Address != nint.Zero;
	public T? Delegate {
		get {
			if (function is not null)
				return function;
			if (Address != nint.Zero) {
                function = Marshal.GetDelegateForFunctionPointer<T>(Address);
				return function;
			}
			Service.Log.Error($"[{LogTag.PluginCore}] {GetType().Name} invocation FAILED: no pointer available");
			return null;
		}
	}
	internal GameFunctionBase(string sig, int offset = 0) {
		if (Service.Scanner.TryScanText(sig, out addr)) {
            addr += offset;
			ulong totalOffset = (ulong)Address.ToInt64() - (ulong)Service.Scanner.Module.BaseAddress.ToInt64();
			Service.Log.Information($"[{LogTag.PluginCore}] {GetType().Name} loaded; address = 0x{Address.ToInt64():X16}, base memory offset = 0x{totalOffset:X16}");
		}
		else {
			Service.Log.Warning($"[{LogTag.PluginCore}] {GetType().Name} FAILED, could not find address from signature: ${sig.ToUpper()}");
		}
	}
	[SuppressMessage("Reliability", "CA2020:Prevent from behavioral change", Justification = "If this explodes, we SHOULD be throwing")]
	internal GameFunctionBase(nint address, int offset = 0) {
        addr = address + offset; // this will throw on overflow
		ulong totalOffset = (ulong)Address.ToInt64() - (ulong)Service.Scanner.Module.BaseAddress.ToInt64();
		Service.Log.Information($"[{LogTag.PluginCore}] {GetType().Name} loaded; address = 0x{Address.ToInt64():X16}, base memory offset = 0x{totalOffset:X16}");
	}

	public dynamic? Invoke(params dynamic[] parameters)
		=> Delegate?.DynamicInvoke(parameters);

	public Hook<T> Hook(T handler) => Service.Interop.HookFromAddress(Address, handler);
}
