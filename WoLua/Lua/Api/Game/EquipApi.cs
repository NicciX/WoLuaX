using System;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

using MoonSharp.Interpreter;
using ECommons.Logging;
using ECommons.Automation;
using System.Globalization;

using Lumina.Excel.Sheets;
using System.Linq;
//using static FFXIVClientStructs.FFXIV.Client.Graphics.Render.ModelRenderer;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;

namespace NicciX.WoLua.Lua.Api.Game;

[MoonSharpUserData]
public class EquipApi: ApiBase {
	[MoonSharpHidden]
	internal EquipApi(ScriptContainer source) : base(source) { }

	//private readonly ExcelSheet<Stain> stain = Service.DataManager.GetExcelSheet<Stain>()!;

	//private unsafe void OnSetupContentsFinder(AddonEvent type, AddonArgs args) => Callback.Fire((AtkUnitBase*)args.Addon, true, 12, 0);

	public bool Loaded => !this.Disposed
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
			return "Slots already loaded";
		}
		this.Head.Name = this.Head.SetName("Head");
		this.Body.Name = this.Body.SetName("Body");
		this.Hands.Name = this.Hands.SetName("Hands");
		this.Legs.Name = this.Legs.SetName("Legs");
		this.Feet.Name = this.Feet.SetName("Feet");
		this.Weapon.Name = this.Weapon.SetName("Weapon");
		this.Offhand.Name = this.Offhand.SetName("Offhand");
		this.Ears.Name = this.Ears.SetName("Ears");
		this.Neck.Name = this.Neck.SetName("Neck");
		this.Wrist.Name = this.Wrist.SetName("Wrist");
		this.RRing.Name = this.RRing.SetName("RRing");
		this.LRing.Name = this.LRing.SetName("LRing");
		slotsLoaded = true;
		return "Slots Loaded";
	}
	//public unsafe string? HN => this.headName;

	private static int EquipAttemptLoops = 0;
	public unsafe void Equip(uint itemID) {
		if (Inventory.HasItemEquipped(itemID))
			return;

		var pos = Inventory.GetItemLocationInInventory(itemID, Inventory.Equippable);
		if (pos == null) {
			//Service.Log.Error($"Failed to find item {this.GetRow<Item>(itemID)?.Name} (ID: {itemID}) in inventory");
			return;
		}

		var agentId = Inventory.Armory.Contains(pos.Value.inv) ? AgentId.ArmouryBoard : AgentId.Inventory;
		var addonId = AgentModule.Instance()->GetAgentByInternalId(agentId)->GetAddonId();
		var ctx = AgentInventoryContext.Instance();
		ctx->OpenForItemSlot(pos.Value.inv, pos.Value.slot, addonId);

		var contextMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("ContextMenu");
		if (contextMenu != null) {
			for (var i = 0; i < contextMenu->AtkValuesCount; i++) {
				var firstEntryIsEquip = ctx->EventIds[i] == 25; // i'th entry will fire eventid 7+i; eventid 25 is 'equip'
				if (firstEntryIsEquip) {
					Service.Log.Debug($"Equipping item #{itemID} from {pos.Value.inv} @ {pos.Value.slot}, index {i}");
					//Callback.Fire(contextMenu, true, 0, i - 7, 0, 0, 0); // p2=-1 is close, p2=0 is exec first command
				}
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
		if (name == "Offhand" && this.Offhand.Item > 0) {
			this.Offhand.Remove();
			return;
		}
		else if (name == "Head" && this.Head.Item > 0) {
			this.Head.Remove();
			return;
		}
		else if (name == "Body" && this.Body.Item > 0) {
			this.Body.Remove();
			return;
		}
		else if (name == "Hands" && this.Hands.Item > 0) {
			this.Hands.Remove();
			return;
		}
		else if (name == "Legs" && this.Legs.Item > 0) {
			this.Legs.Remove();
			return;
		}
		else if (name == "Feet" && this.Feet.Item > 0) {
			this.Feet.Remove();
			return;
		}
		else if (name == "Ears" && this.Ears.Item > 0) {
			this.Ears.Remove();
			return;
		}
		else if (name == "Neck" && this.Neck.Item > 0) {
			this.Neck.Remove();
			return;
		}
		else if (name == "Wrist" && this.Wrist.Item > 0) {
			this.Wrist.Remove();
			return;
		}
		else if (name == "RRing" && this.RRing.Item > 0) {
			this.RRing.Remove();
			return;
		}
		else if (name == "LRing" && this.LRing.Item > 0) {
			this.LRing.Remove();
			return;
		}
		else {
			Service.Log.Error($"Unable to remove {name}");
			return;
		}
	}
}
