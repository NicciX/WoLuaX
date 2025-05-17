using WoLua.Lua;

namespace WoLuaX.Lua.Actions;

public abstract class ScriptAction {
	public void Run(ScriptContainer script) {
		if (script is null || script.Disposed)
			return;

        Process(script);
	}
	protected abstract void Process(ScriptContainer script);

	public override string ToString()
		=> GetType().Name;
}
