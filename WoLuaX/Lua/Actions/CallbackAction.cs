using System;
using System.Collections.ObjectModel;

using MoonSharp.Interpreter;
using WoLuaX.Lua;
using WoLuaX.Constants;
using WoLuaX.Lua.Api;

namespace WoLuaX.Lua.Actions;

public class CallbackAction: ScriptAction {
	public DynValue Function { get; }
	private readonly DynValue[] arguments;
	public ReadOnlyCollection<DynValue> Arguments => Array.AsReadOnly(arguments);

	public CallbackAction(DynValue callback, params DynValue[] arguments) {
        Function = callback;
		this.arguments = arguments;
	}

	protected override void Process(ScriptContainer script) {
		script.Log(ApiBase.ToUsefulString(Function), LogTag.ActionCallback);
		try {
			script.Engine.Call(Function, arguments);
		}
		catch (ArgumentException e) {
			Service.Plugin.Error("Error in queued callback function", e, script.PrettyName);
		}
	}

	public override string ToString()
		=> $"Invoke: {ApiBase.ToUsefulString(Function, true)}";
}
