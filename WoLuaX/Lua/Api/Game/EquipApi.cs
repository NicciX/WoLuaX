using System;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;

using MoonSharp.Interpreter;
using ECommons.Logging;
using ECommons.Automation;
using System.Globalization;

using Lumina.Excel.Sheets;
using System.Linq;
//using static FFXIVClientStructs.FFXIV.Client.Graphics.Render.ModelRenderer;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using System.ComponentModel;
using WoLuaX.Lua;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
public class EquipApi: ApiBase {
	[MoonSharpHidden]
	internal EquipApi(ScriptContainer source) : base(source) { }

	//private readonly ExcelSheet<Stain> stain = Service.DataManager.GetExcelSheet<Stain>()!;

	//private unsafe void OnSetupContentsFinder(AddonEvent type, AddonArgs args) => Callback.Fire((AtkUnitBase*)args.Addon, true, 12, 0);

	public bool Loaded => !Disposed
        && Service.ClientState.LocalPlayer is not null
		&& Service.ClientState.LocalContentId is not 0;
	public static implicit operator bool(EquipApi? player) => player?.Loaded ?? false;

	private object GetRow<T>(uint itemID) => throw new NotImplementedException();

	//public SlotItemApi head { get; set; } = null!;
	//private unsafe SlotItem Head.ItemId => this.head;

	//public unsafe uint? Head => this.Loaded
	//? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId : 0;

	//public SlotItemApi Head;


	public unsafe SlotItemApi Head { get; set; } = null!;
	public unsafe SlotItemApi Body { get; set; } = null!;
	public unsafe SlotItemApi Hands { get; set; } = null!;
	public unsafe SlotItemApi Legs { get; set; } = null!;
	public unsafe SlotItemApi Feet { get; set; } = null!;
	public unsafe SlotItemApi Weapon { get; set; } = null!;
	public unsafe SlotItemApi Offhand { get; set; } = null!;
	public unsafe SlotItemApi Ears { get; set; } = null!;
	public unsafe SlotItemApi Neck { get; set; } = null!;
	public unsafe SlotItemApi Wrist { get; set; } = null!;
	public unsafe SlotItemApi RRing { get; set; } = null!;
	public unsafe SlotItemApi LRing { get; set; } = null!;

	//public unsafe SlotItemApi Necklace { get; set; } = null!;

	//public string HeadName => this.Head.SetName("Head");

	private static bool slotsLoaded = false;
	public string LoadSlots() {
		if (slotsLoaded) {
			//return "Slots already loaded";
		}
        Head.Name = Head.SetName("Head");
        Body.Name = Body.SetName("Body");
        Hands.Name = Hands.SetName("Hands");
        Legs.Name = Legs.SetName("Legs");
        Feet.Name = Feet.SetName("Feet");
        Weapon.Name = Weapon.SetName("Weapon");
        Offhand.Name = Offhand.SetName("Offhand");
        Ears.Name = Ears.SetName("Ears");
        Neck.Name = Neck.SetName("Neck");
        Wrist.Name = Wrist.SetName("Wrist");
        RRing.Name = RRing.SetName("RRing");
        LRing.Name = LRing.SetName("LRing");
		slotsLoaded = true;
		return "Slots Loaded";
	}
	//public unsafe string? HN => this.headName;

	private static int EquipAttemptLoops = 0;
	public unsafe void Equip(uint itemID, InventoryType? container = null, int? slot = null) {
		if (Inventory.HasItemEquipped(itemID))
			return;

		var pos = Inventory.GetItemLocationInInventory(itemID, Inventory.Equippable);
		if (pos == null) 			//Service.Log.Error($"Failed to find item {this.GetRow<Item>(itemID)?.Name} (ID: {itemID}) in inventory");
			return;

		container ??= pos.Value.inv;
		slot ??= pos.Value.slot;

		var agentId = Inventory.Armory.Contains(pos.Value.inv) ? AgentId.ArmouryBoard : AgentId.Inventory;
		var addonId = AgentModule.Instance()->GetAgentByInternalId(agentId)->GetAddonId();
		var ctx = AgentInventoryContext.Instance();
		ctx->OpenForItemSlot(pos.Value.inv, pos.Value.slot, addonId);

		var contextMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("ContextMenu");
		if (contextMenu != null) {
			for (var i = 0; i < contextMenu->AtkValuesCount; i++) {
				var firstEntryIsEquip = ctx->EventIds[i] == 25; // i'th entry will fire eventid 7+i; eventid 25 is 'equip'
				if (firstEntryIsEquip) 					Service.Log.Debug($"Equipping item #{itemID} from {pos.Value.inv} @ {pos.Value.slot}, index {i}");
			}
			//Callback.Fire(contextMenu, true, 0, -1, 0, 0, 0);
			EquipAttemptLoops++;

			if (EquipAttemptLoops >= 5) {
				DuoLog.Error($"Equip option not found after 5 attempts. Aborting.");
				return;
			}
		}
	}

	public unsafe void Remove(string name) {
		if (name == null) {
			Service.Log.Error("Name is null");
			return;
		}
		if (name == "Weapon") {
			Service.Log.Error("Cannot remove weapon");
			return;
		}
		if (name == "Offhand" && Offhand.Item > 0) {
            Offhand.Remove();
			return;
		}
		else if (name == "Head" && Head.Item > 0) {
            Head.Remove();
			return;
		}
		else if (name == "Body" && Body.Item > 0) {
            Body.Remove();
			return;
		}
		else if (name == "Hands" && Hands.Item > 0) {
            Hands.Remove();
			return;
		}
		else if (name == "Legs" && Legs.Item > 0) {
            Legs.Remove();
			return;
		}
		else if (name == "Feet" && Feet.Item > 0) {
            Feet.Remove();
			return;
		}
		else if (name == "Ears" && Ears.Item > 0) {
            Ears.Remove();
			return;
		}
		else if (name == "Neck" && Neck.Item > 0) {
            Neck.Remove();
			return;
		}
		else if (name == "Wrist" && Wrist.Item > 0) {
            Wrist.Remove();
			return;
		}
		else if (name == "RRing" && RRing.Item > 0) {
            RRing.Remove();
			return;
		}
		else if (name == "LRing" && LRing.Item > 0) {
            LRing.Remove();
			return;
		}
		else {
			Service.Log.Error($"Unable to remove {name}");
			return;
		}
	}
}
