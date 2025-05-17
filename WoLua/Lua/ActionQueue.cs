using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Dalamud.Plugin.Services;

using MoonSharp.Interpreter;

using WoLua.Lua;
using WoLuaX.Constants;
using WoLuaX.Lua.Actions;

namespace WoLuaX.Lua;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "It's a queue for script actions")]
public class ActionQueue: IDisposable {

	private readonly ConcurrentQueue<ScriptAction> queue = new();
	public DateTime ActionThreshold { get; internal set; } = DateTime.MinValue;
	public ScriptContainer Script { get; private set; }

	public ActionQueue(ScriptContainer source) {
        Script = source;
		Service.Framework.Update += tick;
	}

	public int Count => queue.Count;

	internal void Clear()
		=> queue.Clear();
	internal void Add(ScriptAction action)
		=> queue.Enqueue(action);

	public bool? PullEvent() {
		if (!Service.ClientState.IsLoggedIn || Service.GameLifecycle.LogoutToken.IsCancellationRequested || Service.GameLifecycle.DalamudUnloadingToken.IsCancellationRequested || Service.GameLifecycle.GameShuttingDownToken.IsCancellationRequested) {
            Clear();
			return null;
		}
		if (DateTime.Now < ActionThreshold)
			return null;
		if (!queue.TryDequeue(out ScriptAction? action))
			return false;
        Script.Log(action.ToString(), LogTag.ActionQueue);
		action.Run(Script);
		return true;
	}

	private void tick(IFramework framework) => PullEvent();

	#region IDisposable
	private bool disposed = false;

	protected virtual void Dispose(bool disposing) {
		if (disposed)
			return;
        disposed = true;

		if (disposing) {
			Service.Framework.Update -= tick;
            Clear();
		}

        Script.Log(GetType().Name, LogTag.Dispose, true);

        Script = null!;
	}

	~ActionQueue() {
        Dispose(false);
	}

	[MoonSharpHidden]
	public void Dispose() {
        Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
