using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Dalamud.Game.ClientState.Objects.Types;

using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using MoonSharp.Interpreter;

using WoLuaX.Api;
using WoLuaX.Lua;
using WoLuaX.Lua.Docs;
using WoLuaX.Lua.Api.Game;
using WoLuaX.Constants;
using WoLuaX.Ui.Chat;
using WoLuaX.Game;

namespace WoLuaX.Lua.Api;

// This API is for everything pertaining to the actual game, including holding more specific APIs.
[MoonSharpUserData]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Documentation generation only reflects instance members")]
public class GameApi: ApiBase {

	#region Initialisation

	[MoonSharpHidden]
	public GameApi(ScriptContainer source) : base(source) { }

	#endregion

	#region Sub-APIs

	public PlayerApi Player { get; private set; } = null!;
	public ChocoboApi Chocobo { get; private set; } = null!;
	public ToastApi Toast { get; private set; } = null!;
	public DalamudApi Dalamud { get; private set; } = null!;
	public ChatApi Chat { get; set; } = null!;

	#endregion

	#region Chat

	[LuaDoc($"Prints a message into the user's local chat log using {Plugin.Name}'s default colour.",
		$"The message will be automatically prefixed with `[{Plugin.Name}]` and the script's name for clarity.")]
	public void PrintMessage([AsLuaType(LuaType.Any), LuaDoc("Multiple values will be concatenated with a single space")] params DynValue[] messages) {
		if (Disposed)
			return;
		if (messages.Length == 0)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
        Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, null, Owner.PrettyName);
	}

	[LuaDoc("Prints a message into the user's local chat log in red.",
		$"The message will be automatically prefixed with `[{Plugin.Name}]` and the script's name for clarity.")]
	public void PrintError([AsLuaType(LuaType.Any), LuaDoc("Multiple values will be concatenated with a single space")] params DynValue[] messages) {
		if (Disposed)
			return;

		string message = string.Join(
			" ",
			messages.Select(dv => ToUsefulString(dv))
		);
        Log(message, LogTag.LocalChat);
		Service.Plugin.Print(message, Foreground.Error, Owner.PrettyName);
	}

	[LuaDoc("Sends text to the game as if the user had typed it into their chat box themselves.",
		"***THIS IS DANGEROUS.***",
		"If the text to be sent does NOT start with a '/' then it will be treated as a **plain chat message**. USE **EXTREME** CAUTION.")]
	public void SendChat(string chatline) {
		if (Disposed)
			return;

		string cleaned = Service.ServerChat.SanitiseText(chatline);
		if (!string.IsNullOrWhiteSpace(cleaned)) {
            Log(cleaned, LogTag.ServerChat);
			Service.ServerChat.SendMessage(cleaned);
		}
	}
	#endregion

	#region Object table

	[LuaDoc("Iterates over all real and not-dead entities near the player.",
		"Only objects close enough to be loaded by the client will be seen.",
		"Order is neither specified nor guaranteed.")]
	public IEnumerable<EntityWrapper> NearbyEntities => Service.Objects.Where(o => o is not null && o.IsValid() && o.IsTargetable && !o.IsDead).Select(o => new EntityWrapper(o)).ToList();

	[LuaDoc("Finds the nearest (three-dimensional distance) real and not-dead entity with the given (case-sensitive) name.",
		"If no such entity could be found, the returned EntityWrapper will point to nothing.",
		"This is an intensive method, since it must examine EVERY nearby game object to find matches, then identify the nearest one. Please cache the return value during execution frames.")]
	public EntityWrapper FindNearestEntity(string name) {
        Log($"Searching object table for name: {name}", LogTag.ObjectTable);
		return NearbyEntities
            .Where(o => o.Name == name)
			.OrderBy(e => e.Distance)
			.FirstOrDefault(EntityWrapper.Empty);
	}

	#endregion

	#region FATE table

	public IEnumerable<FateWrapper> Fates => Service.FateTable.Select(f => new FateWrapper(f)).Where(f => f.Exists).ToList();

	public FateWrapper FindFate(string name) {
        Log($"Searching FATE table for name: {name}", LogTag.FateTable);
		return Fates
            .Where(f => f.Exists && f.Name == name)
			.OrderBy(f => f.DistanceToCentre)
			.FirstOrDefault(FateWrapper.Empty);
	}

	#endregion

	#region Map flag

	[LuaDoc("Indicates whether or not the player's custom map flag marker is currently set.",
		"If this is false, the `<flag>` chat placeholder will display `(No location set for map link)` instead.")]
	public unsafe bool HasMapFlag {
		get {
			AgentMap* map = AgentMap.Instance();
			return map is not null && map->IsFlagMarkerSet;
		}
	}

	[LuaDoc("Clears the player's custom map flag marker, hiding it from display.")]
	public unsafe void ClearMapFlag() {
		AgentMap* map = AgentMap.Instance();
		if (map is not null)
			map->IsFlagMarkerSet = false;
	}

	[LuaDoc("Sets the player's custom map flag marker to the given x/y coordinates in the current zone.",
		"The coordinates must be MAP coordinates, not WORLD coordinates.",
		"If you want to set the flag on an entity or FATE, you can pass the appropriate wrapper instead.")]
	public unsafe void SetMapFlag(float x, float y) {
		AgentMap* map = AgentMap.Instance();
		if (map is null)
			return;
		map->IsFlagMarkerSet = false;
		map->SetFlagMapMarker(map->CurrentTerritoryId, map->CurrentMapId, x, y);
	}

	[LuaDoc("Sets the player's custom map flag marker to the location of the provided game world object or position.")]
	public void SetMapFlag([AsLuaType("EntityWrapper|FateWrapper|PlayerApi|WorldPosition")] IWorldObjectWrapper pos) {
		if (pos.MapX is float x && pos.MapY is float y)
            SetMapFlag(x, y);
	}

	#endregion

	[LuaDoc("Plays one of the sixteen `<se.##>` sound effects without printing anything to the user's chat.",
		"Using an ID that isn't in the range of 1-16 (inclusive) will silently fail. With an emphasis on silently.")]
	[return: LuaDoc("`true` if the provided sound effect ID was a valid sound, `false` if it wasn't, or `nil` if there was an internal error")]
	public bool? PlaySoundEffect(int id) {
		if (Disposed)
			return null;

		if (!Service.Sounds.Valid)
			return null;
		Sound sound = SoundsExtensions.FromGameIndex(id);
		if (sound.IsSound())
			Service.Sounds.Play(sound);
		return sound.IsSound();
	}

	[LuaDoc("Returns an object holding the current Eorzean time as separate hours and minutes.")]
	public EorzeanTime EorzeanTime => new();

	[LuaDoc("Returns a wrapper for the current weather in the current zone.",
		"This wrapper provides the raw (internal) numeric (unsigned integer) ID of the weather, the short name, and a small description of what the weather \"looks\" like.")]
	public unsafe WeatherWrapper Weather {
		get {
			EnvManager* env = EnvManager.Instance();
			return env is null
				? new(0)
				: new(env->ActiveWeather);
		}
	}

	// TODO allow accessing job gauge data via Service.JobGauges

}
