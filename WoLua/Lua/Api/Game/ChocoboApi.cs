using FFXIVClientStructs.FFXIV.Client.Game.UI;

using MoonSharp.Interpreter;
using WoLua.Lua;
using WoLuaX.Constants;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
public class ChocoboApi: ApiBase { // TODO luadoc all of this
	[MoonSharpHidden]
	internal ChocoboApi(ScriptContainer source) : base(source) { }
	private unsafe CompanionInfo? obj {
		get {
			UIState* ui = UIState.Instance();
			return ui is null ? null : ui->Buddy.CompanionInfo;
		}
	}
	public static implicit operator bool(ChocoboApi? bird) => bird?.obj is not null;

	public float? TimeLeft => obj?.TimeLeft;
	public bool? Summoned => this ? (TimeLeft ?? 0) > 0 : null;
	public uint? CurrentXP => obj?.CurrentXP;
	public byte? Rank => obj?.Rank;
	public bool? Unlocked => this ? (Rank ?? 0) > 0 : null;
	public byte? Stars => obj?.Stars;
	public byte? SkillPoints => obj?.SkillPoints;
	public byte? DefenderLevel => obj?.DefenderLevel;
	public byte? AttackerLevel => obj?.AttackerLevel;
	public byte? HealerLevel => obj?.HealerLevel;
	public unsafe string? Name => obj?.NameString;

	public unsafe uint? CurrentHp => Summoned ?? false ? obj!.Value.Companion->CurrentHealth : null;
	public unsafe uint? MaxHp => Summoned ?? false ? obj!.Value.Companion->MaxHealth : null;

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => Name ?? string.Empty;
}
