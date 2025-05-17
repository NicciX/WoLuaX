using System.Numerics;

using Dalamud.Game.ClientState.Fates;

using MoonSharp.Interpreter;

using WoLua.Lua.Docs;

using Fate = Dalamud.Game.ClientState.Fates.IFate;
using FateContext = FFXIVClientStructs.FFXIV.Client.Game.Fate.FateContext;
using WoLuaX.Constants;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember(nameof(WorldFate))]
[MoonSharpHideMember(nameof(Equals))]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Deconstruct))]
public sealed record class FateWrapper(Fate? WorldFate): IWorldObjectWrapper {
	public static readonly FateWrapper Empty = new((Fate?)null);

	internal unsafe FateContext* Struct => Valid ? (FateContext*)WorldFate!.Address : null;

	[LuaDoc("Whether this FATE is a valid object in the game's memory. If this is false, this wrapper is meaningless.")]
	public bool Valid => WorldFate is Fate fate && Service.ClientState.LocalPlayer?.IsValid() is true;

	[LuaDoc("Whether this FATE still exists in the world. This will be true if the FATE is valid and has not yet ended, even if it is WAITING to end.")]
	public bool Exists => State is not (FateState.Ended or FateState.Failed);

	#region Display

	[LuaDoc("The name of this FATE.")]
	public string? Name => Valid ? WorldFate!.Name.TextValue : null;

	[LuaDoc("The current progress of this FATE towards its completion, as a percentage.")]
	public byte? Progress => Exists ? WorldFate!.Progress! : null;

	#endregion

	#region Levels

	[LuaDoc("The (minimum) level of this FATE. You are recommended to be at least this level to participate.",
		"If you are below this level, the game weighs your contribution less, and combat is expected to be much harder.")]
	public byte? MinLevel => Valid ? WorldFate!.Level! : null;

	[LuaDoc("The maximum level of this FATE, above which you need to manually level sync down in order to participate.")]
	public unsafe byte? MaxLevel => Valid ? Struct->MaxLevel : null;

	#endregion

	#region Time

	[LuaDoc("The total duration of this FATE, in seconds.",
		"If this FATE has already ended, this will be `nil`.")]
	public int? Duration => Valid ? WorldFate!.Duration : null;

	[LuaDoc("The remaining time to complete this FATE, in seconds.",
		"If this FATE has not yet started, this will be equal to `.Duration`.",
		"If this FATE has already ended, this will be `nil`.")]
	public int? TimeLeft => Valid
        ? State switch {
			FateState.Preparation => Duration,
			FateState.Running or FateState.WaitingForEnd => (int)WorldFate!.TimeRemaining,
			FateState.Ended or FateState.Failed => 0,
			_ => null,
		}
		: null;

	[LuaDoc("The amount of time this FATE has so far been going for, in seconds.",
		"This is functionally equivalent to `.Duration - .TimeLeft`.",
		"If this FATE has not yet started, this will be `0`.",
		"If this FATE has already ended, this will be equal to `.Duration`.")]
	public int? TimeElapsed => Duration is int total && TimeLeft is int left ? total - left : null;

	#endregion

	#region States

	[MoonSharpHidden]
	public FateState State => Valid ? WorldFate!.State : FateState.Ended;

	[LuaDoc("Whether this FATE is in the preparation phase, before it is actually started.")]
	public bool Waiting => State is FateState.Preparation;

	[LuaDoc("Whether this FATE is currently running.")]
	public bool Running => State is FateState.Running;

	[LuaDoc("Whether this FATE is currently in its ending phase, as seen in item turnin FATEs.")]
	public bool Ending => State is FateState.WaitingForEnd;

	[LuaDoc("Whether this FATE is considered \"active\", either waiting to be started or currently in progress.")]
	public bool Active => State is FateState.Preparation or FateState.Running;

	#endregion

	#region Position
	// X and Z are the horizontal coordinates, Y is the vertical one
	// But that's not how the game displays things to the player, because fuck you I guess, so we swap those two around for consistency

	[LuaPlayerDoc("The raw (internal) X coordinate of this FATE.",
		"This represents the east/west position.")]
	public float? PosX => Exists ? WorldFate!.Position.X : null;

	[LuaPlayerDoc("The raw (internal) Y coordinate of this FATE.",
		"This represents the north/south position.",
		"Please note that this is _actually_ the internal _Z_ coordinate, since the game engine uses X/Z for horizontal position and Y for vertical.",
		"For the sake of consistency with the map coordinates displayed to the player, " + Plugin.Name + " swaps them.")]
	public float? PosY => Exists ? WorldFate!.Position.Z : null;

	[LuaPlayerDoc("The raw (internal) Z coordinate of this FATE.",
		"This represents the vertical position.",
		"Please note that this is _actually_ the internal _Y_ coordinate, since the game engine uses X/Z for horizontal position and Y for vertical.",
		"For the sake of consistency with the map coordinates displayed to the player, " + Plugin.Name + " swaps them.")]
	public float? PosZ => Exists ? WorldFate!.Position.Y : null;

	public WorldPosition Position => new(PosX, PosY, PosZ);

	[LuaDoc("The player-friendly map-style X (east/west) coordinate of this FATE.")]
	public float? MapX => Position.MapX;
	[LuaDoc("The player-friendly map-style Y (north/south) coordinate of this FATE.")]
	public float? MapY => Position.MapY;
	[LuaDoc("The player-friendly map-style Z (height) coordinate of this FATE.")]
	public float? MapZ => Position.MapZ;

	[LuaDoc("Provides three values consisting of this FATE's X (east/west), Y (north/south), and Z (vertical) coordinates.",
		"If the FATE doesn't exist in the world (`.Exists == false`), all three values will be nil.")]
	public DynValue MapCoords {
		get {
			Vector3? coords = Position.UiCoords;
			return coords is not null
				? DynValue.NewTuple(DynValue.NewNumber(coords.Value.X), DynValue.NewNumber(coords.Value.Y), DynValue.NewNumber(coords.Value.Z))
				: DynValue.NewTuple(null, null, null);
		}
	}

	[LuaDoc("The radius of this FATE.",
		"FATEs are defined as cylinders, with their position being the central point at the bottom.",
		"This is the maximum (horizontal) distance you can be from the middle of it.")]
	public unsafe float? Radius => Valid ? Struct->Radius : null;

	#endregion

	#region Distance

	[LuaDoc("The flat (horizontal only) distance between the player and the \"position\" of this FATE.",
		"FATEs are defined as cylinders, with their position being the central point at the bottom.")]
	public float? FlatDistanceToCenter => Exists
        ? Position.FlatDistance
		: null;
	[SkipDoc("alternative spelling only")]
	public float? FlatDistanceToCentre => FlatDistanceToCenter;

	[LuaDoc("The three-dimensional distance between the player and the \"position\" of this FATE.",
		"FATEs are defined as cylinders, with their position being the central point at the bottom.",
		"Note that this means that if you're above a FATE, the distance to actually \"enter\" it is less than this would suggest.",
		"However, you probably still need to go about that far in order to reach the ground where the objectives will be.")]
	public float? DistanceToCenter => Exists
        ? Position.Distance
		: null;
	[SkipDoc("alternative spelling only")]
	public float? DistanceToCentre => DistanceToCenter;

	[LuaDoc("The flat (horizontal only) distance between the player and the edge of this FATE.",
		"This is calculated as the distance to the central point minus the radius.",
		"If the player is currently within the FATE's (horizontal) bounds, this will be negative.",
		"Note that FATEs are defined as cylinders, with their position being the central point at the bottom.",
		"As a result, even if this is negative, the player may be above the FATE's bounds.")]
	public float? FlatDistanceToEdge => FlatDistanceToCenter is float dist && Radius is float size ? dist - size : null;

	// There is no method for 3d edge distance, due to FATEs being cylinders and not being able to get their height.
	// A calculation based on the distance to the centre minus the radius would be invalid,
	// since that would only calculate distance to a SPHERICAL border around the FATE's position.

	#endregion

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	[LuaDoc("Creates a string describing the FATE, including its name, state, minimum level, and maximum level.")]
	public override string ToString() => Valid ? $"FATE[{Name ?? "<ERROR: UNKNOWN FATE>"} - {State}, {MinLevel}/{MaxLevel}]" : "FATE[invalid]";

	#region Conversions

	public static implicit operator bool(FateWrapper? fate) => fate?.Exists ?? false;

	#endregion

}
