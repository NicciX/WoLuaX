using System;

using WoLuaX.Lua;
using WoLuaX.Constants;

namespace WoLuaX.Lua.Actions;

public class PauseAction: ScriptAction {
	public uint Delay { get; }

	internal PauseAction(uint ms) => Delay = ms;

	protected override void Process(ScriptContainer script) {
		script.Log($"{Delay}ms", LogTag.ActionPause);
		script.ActionQueue.ActionThreshold = DateTime.Now.AddMilliseconds(Delay);
	}

	public override string ToString()
		=> $"Delay({Delay}ms)";

}
