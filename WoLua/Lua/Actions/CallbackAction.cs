using System;
using System.Collections.ObjectModel;

using MoonSharp.Interpreter;

using WoLua.Lua;

using WoLua.Constants;
using WoLua.Lua.Api;

namespace WoLua.Lua.Actions;

public class CallbackAction: ScriptAction {
	public DynValue Function { get; }
	private readonly DynValue[] arguments;
	public ReadOnlyCollection<DynValue> Arguments => Array.AsReadOnly(this.arguments);

	public CallbackAction(DynValue callback, params DynValue[] arguments) {
		this.Function = callback;
		this.arguments = arguments;
	}

	protected override void Process(ScriptContainer script) {
		script.Log(ApiBase.ToUsefulString(this.Function), LogTag.ActionCallback);
		try {
			script.Engine.Call(this.Function, this.arguments);
		}
		catch (ArgumentException e) {
			Service.Plugin.Error("Error in queued callback function", e, script.PrettyName);
		}
	}

	public override string ToString()
		=> $"Invoke: {ApiBase.ToUsefulString(this.Function, true)}";
}
