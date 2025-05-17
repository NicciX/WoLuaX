using System;

using Dalamud.Game.ClientState.Objects.Types;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace WoLuaX;

public class Hooks: IDisposable {
	private bool disposed;

	public unsafe IGameObject? UITarget {
		get {
			PronounModule* pronouns = PronounModule.Instance();
			if (pronouns is null)
				return null;
			GameObject* actor = pronouns->UiMouseOverTarget;
			return actor is null ? null : Service.Objects.CreateObjectReference((nint)actor);
		}
	}

	public Hooks() {
		// nop
	}

	protected virtual void Dispose(bool disposing) {
		if (disposed)
			return;
        disposed = true;

		if (disposing) {
			// nop
		}

		// nop
	}

	public void Dispose() {
        Dispose(true);
		GC.SuppressFinalize(this);
	}
}
