using FFXIVClientStructs.FFXIV.Client.Game.UI;

using MoonSharp.Interpreter;

using WoLua.Constants;
using WoLua.Lua;

using WoLua.Api;
using WoLua.Lua.Api;

namespace WoLua.Lua.Api.Game;

[MoonSharpUserData]
public class ChatApi: ApiBase { // TODO luadoc all of this
	[MoonSharpHidden]
	internal ChatApi(ScriptContainer source) : base(source) { }
	public string? Msg => WoLuaApi.Msg;
	public string? Sender => WoLuaApi.Sender;
	public string? Chn => WoLuaApi.Chn;
	public string? Match => WoLuaApi.Match;
	public uint? Stamp => WoLuaApi.Stamp;

}
