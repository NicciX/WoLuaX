using System;

using MoonSharp.Interpreter;
using WoLuaX.Constants;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember(nameof(Equals))]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Deconstruct))]
public sealed record class JobData(uint Id, string? Name, string? Abbreviation): IEquatable<JobData> { // TODO luadoc all of this
	public const string
		InvalidJobName = "adventurer",
		InvalidJobAbbr = "ADV";
	public bool Equals(JobData? other)
		=> Id == other?.Id;
	public override int GetHashCode()
		=> Id.GetHashCode();

	public string? Abbr
		=> Abbreviation;
	public string? ShortName
		=> Abbreviation;

	public bool Valid
		=> Id > 0 && Name is not null and not InvalidJobName && Abbreviation is not null and not InvalidJobAbbr;

	public bool IsCrafter
		=> Valid && Id is >= 8 and <= 15;
	public bool IsGatherer
		=> Valid && Id is >= 16 and <= 18;
	public bool IsMeleeDPS
		=> Valid && Id is 2 or 4 or 20 or 22 or 29 or 30 or 34 or 39;
	public bool IsRangedDPS
		=> Valid && Id is 5 or 23 or 31 or 38;
	public bool IsMagicDPS
		=> Valid && Id is 7 or 25 or 26 or 27 or 35;
	public bool IsHealer
		=> Valid && Id is 6 or 24 or 28 or 33 or 40;
	public bool IsTank
		=> Valid && Id is 3 or 19 or 21 or 32 or 37;

	public bool IsDPS
		=> IsMeleeDPS || IsRangedDPS || IsMagicDPS;

	public bool IsDiscipleOfWar
		=> IsMeleeDPS || IsRangedDPS || IsTank;
	public bool IsDiscipleOfMagic
		=> IsMagicDPS || IsHealer;

	public bool IsBlu
		=> Valid && Id is 36;
	public bool IsLimited
		=> IsBlu;

	#region Metamathods

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString()
		=> Name ?? "Adventurer";

	#endregion
}
