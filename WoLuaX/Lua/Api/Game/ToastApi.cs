using System.Diagnostics.CodeAnalysis;

using Dalamud.Game.Gui.Toast;

using MoonSharp.Interpreter;
using WoLuaX.Lua;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "It doesn't matter")]
public class ToastApi: ApiBase { // TODO luadoc all of this
	internal ToastApi(ScriptContainer source) : base(source) { }

	public void Short(string text) {
		if (Disposed)
			return;

		Service.Toast.ShowNormal(text, new ToastOptions() { Speed = ToastSpeed.Fast });
	}
	public void Long(string text) {
		if (Disposed)
			return;

		Service.Toast.ShowNormal(text, new ToastOptions() { Speed = ToastSpeed.Slow });
	}

	public void Error(string text) {
		if (Disposed)
			return;

		Service.Toast.ShowError(text);
	}

	public void TaskComplete(string text, bool silent) {
		if (Disposed)
			return;

		Service.Toast.ShowQuest(text, new QuestToastOptions() { DisplayCheckmark = true, PlaySound = !silent });
	}
	public void TaskComplete(string text, uint icon, bool silent) {
		if (Disposed)
			return;

		Service.Toast.ShowQuest(text, new QuestToastOptions() { IconId = icon, PlaySound = !silent });
	}
	public void TaskComplete(string text)
		=> TaskComplete(text, false);
	public void TaskComplete(string text, uint icon)
		=> TaskComplete(text, icon, false);

}
