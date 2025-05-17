using System;
using System.Numerics;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

using ECommons.DalamudServices;

using FFXIVClientStructs.FFXIV.Client.Game.Character;

using Lumina.Excel;
using Lumina.Excel.Sheets;

using MoonSharp.Interpreter;

using WoLua.Lua.Docs;

using CharacterData = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterData;
using NativeCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using NativeGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using WoLuaX.Constants;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember(nameof(Entity))]
[MoonSharpHideMember(nameof(Equals))]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Deconstruct))]
public sealed record class EntityWrapper(IGameObject? Entity): IWorldObjectWrapper, IEquatable<EntityWrapper> { // TODO luadoc all of this
	public static readonly EntityWrapper Empty = new((IGameObject?)null);

	#region Conversions
	private unsafe NativeGameObject* go => this ? (NativeGameObject*)Entity!.Address : null;
	private unsafe NativeCharacter* cs => IsPlayer ? (NativeCharacter*)Entity!.Address : null;

	public static implicit operator bool(EntityWrapper? entity) => entity?.Exists ?? false;
	#endregion

	public bool Exists => Entity is not null && Entity.IsValid() && Entity.ObjectKind is not ObjectKind.None;

	private readonly ExcelSheet<Race> race = Service.DataManager.GetExcelSheet<Race>()!;
	public string? Type => this ? Entity!.ObjectKind.ToString() : null;

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => this ? $"{Type}[{Entity!.Name ?? string.Empty}]" : string.Empty;

	public bool? Alive => this ? !Entity?.IsDead : null;

	public unsafe MountWrapper Mount {
		get {
			NativeCharacter* player = cs;
			if (player is null)
				return new(0);
			MountContainer? mount = player->IsMounted() ? player->Mount : null;
			return new(mount?.MountId ?? 0);
		}
	}

	#region Player display

	//public EquipApi Equipped { get; set; } = null!;

	public string? Name => this
		? Entity?.Name?.TextValue ?? string.Empty
		: null;

	public string? Firstname => IsPlayer
        ? Name!.Split(' ')[0]
		: Name;

	public string? Lastname => IsPlayer
        ? Name!.Split(' ')[1]
		: Name;

	private unsafe Title? playerTitle {
		get {
			if (!IsPlayer)
				return null;
			NativeCharacter* player = cs;
			CharacterData cdata = player->CharacterData;
			ushort titleId = cdata.TitleId;
			return titleId == 0
				? null
				: ExcelContainer.Titles.GetRow(titleId);
		}
	}
	public bool? HasTitle => IsPlayer ? playerTitle is not null : null;
	public string? TitleText {
		get {
			if (!IsPlayer)
				return null;
			Title? title = playerTitle;
			return title.HasValue
				? MF(title.Value.Masculine.ToString(), title.Value.Feminine.ToString())
				: string.Empty;
		}
	}
	public bool? TitleIsPrefix => IsPlayer ? playerTitle?.IsPrefix : null;

	public string? CompanyTag => this && Entity is ICharacter self ? self.CompanyTag.TextValue : null;

	#endregion

	#region Gender

	public unsafe bool? IsMale => this ? go->Sex == 0 : null;
	public unsafe bool? IsFemale => this ? go->Sex == 1 : null;
	public unsafe bool? IsGendered => this ? (IsMale ?? false) || (IsFemale ?? false) : null;

	public string? MF(string male, string female) => MFN(male, female, null!);
	public string? MFN(string male, string female, string neither) => this ? IsGendered ?? false ? IsMale ?? false ? male : female : neither : null;

	public DynValue MF(DynValue male, DynValue female) => MFN(male, female, DynValue.Nil);
	public DynValue MFN(DynValue male, DynValue female, DynValue neither) => this ? IsGendered ?? false ? IsMale ?? false ? male : female : neither : DynValue.Nil;

	#endregion

	#region Titles
	public unsafe string? Title => IsPlayer ? TitleText : null;
	public string? TitlePrefix => IsPlayer && playerTitle?.IsPrefix == true ? TitleText : null;
	public string? TitleSuffix => IsPlayer && playerTitle?.IsPrefix == false ? TitleText : null;
	public string? TitleFull => IsPlayer ? TitleText : null;
	public string? TitleFullPrefix => IsPlayer && playerTitle?.IsPrefix == true ? TitleText : null;
	public string? TitleFullSuffix => IsPlayer && playerTitle?.IsPrefix == false ? TitleText : null;
	public string? TitleFullName => IsPlayer ? $"{TitleText} {Name}" : null;
	public string? TitleFullPrefixName => IsPlayer && playerTitle?.IsPrefix == true ? $"{TitleText} {Name}" : null;
	public string? TitleFullSuffixName => IsPlayer && playerTitle?.IsPrefix == false ? $"{TitleText} {Name}" : null;
	public string? TitleFullNameNoPrefix => IsPlayer ? $"{Name} {TitleText}" : null;
	
	#endregion

	#region Stats

	#region Custom
	public uint Height => this && Entity is ICharacter self ? (uint)self.Customize[(int)CustomizeIndex.Height] : 0;
	//public string NameDay => this && this.Entity is ICharacter self ? (uint)self. : 0;
	public uint BustSize => this && Entity is ICharacter self ? (uint)self.Customize[(int)CustomizeIndex.BustSize] : 0;
	public uint HairStyle => this && Entity is ICharacter self ? (uint)self.Customize[(int)CustomizeIndex.HairStyle] : 0;
	public uint RaceId => this && Entity is ICharacter self ? (uint)self.Customize[(int)CustomizeIndex.Race] : 0;

	public uint TribeId => this && Entity is ICharacter self ? (uint)self.Customize[(int)CustomizeIndex.Tribe] : 0;

	//public string? Race => this && this.Entity is ICharacter self ? this.Service.DataManager.GetExcelSheet<Race>()!.GetRow(this.GlamId).Name.ExtractText();
	public unsafe string? Race => Service.DataManager.GetExcelSheet<Race>()!.GetRow(RaceId).Feminine.ToString();

	public unsafe string? Tribe => Service.DataManager.GetExcelSheet<Tribe>()!.GetRow(TribeId).Feminine.ToString();

	
	#endregion

	#endregion
	#region Worlds

	public ushort? HomeWorldId => IsPlayer && Entity is IPlayerCharacter p ? (ushort)p.HomeWorld.Value.RowId : null;
	public string? HomeWorld => IsPlayer && Entity is IPlayerCharacter p ? p.HomeWorld.Value.Name!.ToString() : null;

	public ushort? CurrentWorldId => IsPlayer && Entity is IPlayerCharacter p ? (ushort)p.CurrentWorld.Value.RowId : null;
	public string? CurrentWorld => IsPlayer && Entity is IPlayerCharacter p ? p.CurrentWorld.Value.Name!.ToString() : null;

	#endregion

	#region Entity type

	public bool IsPlayer => this && Entity?.ObjectKind is ObjectKind.Player;
	public bool IsCombatNpc => this && Entity?.ObjectKind is ObjectKind.BattleNpc;
	public bool IsTalkNpc => this && Entity?.ObjectKind is ObjectKind.EventNpc;
	public bool IsNpc => IsCombatNpc || IsTalkNpc;
	public bool IsTreasure => this && Entity?.ObjectKind is ObjectKind.Treasure;
	public bool IsAetheryte => this && Entity?.ObjectKind is ObjectKind.Aetheryte;
	public bool IsGatheringNode => this && Entity?.ObjectKind is ObjectKind.GatheringPoint;
	public bool IsEventObject => this && Entity?.ObjectKind is ObjectKind.EventObj;
	public bool IsMount => this && Entity?.ObjectKind is ObjectKind.MountType;
	public bool IsMinion => this && Entity?.ObjectKind is ObjectKind.Companion;
	public bool IsRetainer => this && Entity?.ObjectKind is ObjectKind.Retainer;
	public bool IsArea => this && Entity?.ObjectKind is ObjectKind.Area;
	public bool IsHousingObject => this && Entity?.ObjectKind is ObjectKind.Housing;
	public bool IsCutsceneObject => this && Entity?.ObjectKind is ObjectKind.Cutscene;
	public bool IsCardStand => this && Entity?.ObjectKind is ObjectKind.CardStand;
	public bool IsOrnament => this && Entity?.ObjectKind is ObjectKind.Ornament;

	#endregion

	#region Stats

	public byte? Level => this && Entity is ICharacter self ? self.Level : null;

	public JobData Job {
		get {
			return this && Entity is ICharacter self
				? new(self.ClassJob.RowId, self.ClassJob!.Value.Name!.ToString().ToLower(), self.ClassJob!.Value.Abbreviation!.ToString().ToUpper())
				: new(0, JobData.InvalidJobName, JobData.InvalidJobAbbr);
		}
	}

	public uint? Hp => this && Entity is ICharacter self && self.MaxHp > 0 ? self.CurrentHp : null;
	public uint? MaxHp => this && Entity is ICharacter self ? self.MaxHp : null;

	public uint? Mp => this && Entity is ICharacter self && self.MaxMp > 0 ? self.CurrentMp : null;
	public uint? MaxMp => this && Entity is ICharacter self ? self.MaxMp : null;

	public uint? Cp => this && Entity is ICharacter self && self.MaxCp > 0 ? self.CurrentCp : null;
	public uint? MaxCp => this && Entity is ICharacter self ? self.MaxCp : null;

	public uint? Gp => this && Entity is ICharacter self && self.MaxGp > 0 ? self.CurrentGp : null;
	public uint? MaxGp => this && Entity is ICharacter self ? self.MaxGp : null;

	#endregion

	#region Flags

	public bool IsHostile => this && Entity is ICharacter self && self.StatusFlags.HasFlag(StatusFlags.Hostile);
	public bool InCombat => this && Entity is ICharacter self && self.StatusFlags.HasFlag(StatusFlags.InCombat);
	public bool WeaponDrawn => this && Entity is ICharacter self && self.StatusFlags.HasFlag(StatusFlags.WeaponOut);
	public bool IsPartyMember => this && Entity is ICharacter self && self.StatusFlags.HasFlag(StatusFlags.PartyMember);
	public bool IsAllianceMember => this && Entity is ICharacter self && self.StatusFlags.HasFlag(StatusFlags.AllianceMember);
	public bool IsFriend => this && Entity is ICharacter self && self.StatusFlags.HasFlag(StatusFlags.Friend);
	public bool IsCasting => this && Entity is IBattleChara self && self.IsCasting;
	public bool CanInterrupt => this && Entity is IBattleChara self && self.IsCasting && self.IsCastInterruptible;

	#endregion

	#region Position
	// X and Z are the horizontal coordinates, Y is the vertical one
	// But that's not how the game displays things to the player, because fuck you I guess, so we swap those two around for consistency

	public float? PosX => this ? Entity!.Position.X : null;
	public float? PosY => this ? Entity!.Position.Z : null;
	public float? PosZ => this ? Entity!.Position.Y : null;

	public WorldPosition Position => new(PosX, PosY, PosZ);

	[LuaDoc("The player-friendly map-style X (east/west) coordinate of this entity.")]
	public float? MapX => Position.MapX;
	[LuaDoc("The player-friendly map-style Y (north/south) coordinate of this entity.")]
	public float? MapY => Position.MapY;
	[LuaDoc("The player-friendly map-style Z (height) coordinate of this entity.")]
	public float? MapZ => Position.MapZ;

	public DynValue MapCoords {
		get {
			Vector3? coords = Position.UiCoords;
			return coords is not null
				? DynValue.NewTuple(DynValue.NewNumber(coords.Value.X), DynValue.NewNumber(coords.Value.Y), DynValue.NewNumber(coords.Value.Z))
				: DynValue.NewTuple(null, null, null);
		}
	}

	public double? RotationRadians => Entity?.Rotation is float rad ? rad : null;
	public double? RotationDegrees => RotationRadians is double rad ? rad * 180 / Math.PI : null;

	#endregion

	#region Distance

	public float? FlatDistanceFrom(EntityWrapper? other) => Position.FlatDistanceFrom(other);
	public float? FlatDistanceFrom(PlayerApi player) => FlatDistanceFrom(player.Entity);

	public float? DistanceFrom(EntityWrapper? other) => Position.DistanceFrom(other);
	public float? DistanceFrom(PlayerApi player) => DistanceFrom(player.Entity);

	public float? FlatDistance => Position.FlatDistance;
	public float? Distance => Position.Distance;

	#endregion

	#region Target

	public EntityWrapper Target => new(this ? Entity!.TargetObject : null);
	public bool? HasTarget => Target;

	#endregion

	#region IEquatable
	public bool Equals(EntityWrapper? other) => Entity == other?.Entity;
	public override int GetHashCode() => Entity?.GetHashCode() ?? 0;
	#endregion

}
