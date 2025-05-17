using ECommons.Automation;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

using Lumina.Excel.Sheets;
using Lumina.Excel;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using WoLua.Lua;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
public unsafe class SlotItemApi : ApiBase {
	[MoonSharpHidden]
	internal SlotItemApi(ScriptContainer source) : base(source) { }
	
	public bool Loaded => !Disposed
        && Service.ClientState.LocalPlayer is not null
		&& Service.ClientState.LocalContentId is not 0;
	public static implicit operator bool(SlotItemApi? player) => player?.Loaded ?? false;

	private readonly ExcelSheet<Stain> stain = Service.DataManager.GetExcelSheet<Stain>()!;

	public Inventory invHandler = new();

	//private readonly TypeInfo tinfo = typeof(SlotItemApi).GetTypeInfo();

	//private string? _Name;


	public string? Name { get; set; } = "Slot not loaded. Use 'Game.Player.Equipped.LoadSlots()' to initialize.";
	public uint? SlotId { get; set; } = null!;
	//public unsafe string? PName => this.Name;

	public unsafe string SetName(string name) {
        Name = name;
		return name;
	}

	public static int GetSlot(string name) {
		return name switch {
			"Weapon" => 0,
			"Offhand" => 1,
			"Head" => 2,
			"Body" => 3,
			"Hands" => 4,
			//"Waist" => 5, Was belt slot
			"Legs" => 6,
			"Feet" => 7,
			"Ears" => 8,
			"Neck" => 9,
			"Wrist" => 10,
			"RRing" => 11,
			"LRing" => 12,
			_ => 0,
		};
	}

	public static uint DstSlot(string name) {
		return name switch {
			//"Weapon" => 0, -- Weapon is not removable.
			"Offhand" => 3200,
			"Head" => 3201,
			"Body" => 3202,
			"Hands" => 3203,
			//"Waist" => 5, Was belt slot
			"Legs" => 3205,
			"Feet" => 3206,
			"Ears" => 3207,
			"Neck" => 3208,
			"Wrist" => 3209,
			"RRing" => 3300,
			"LRing" => 3300,
			_ => 0,
		};
	}
	//public new string Name { get; set; } = "SlotItem";

	public unsafe int ItemSlot => GetSlot(Name!);

	public unsafe uint StorageSlot => DstSlot(Name!);	

	//public unsafe uint Slot => GetSlot(this.Name);
	public unsafe uint Item => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(ItemSlot)->ItemId != 0
		? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(ItemSlot)->ItemId : 0;
	public unsafe uint DyeIdA => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(ItemSlot)->GetStain(0);	
	public unsafe string DyeA => stain.GetRow(DyeIdA).Name.ToString();
	public unsafe uint DyeIdB => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(ItemSlot)->GetStain(1);
	public unsafe string DyeB => stain.GetRow(DyeIdB).Name.ToString();
	public unsafe uint GlamId => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(ItemSlot)->GlamourId;

	public unsafe string? ItemName => Service.DataManager.GetExcelSheet<Item>()!.GetRow(Item).Name.ExtractText();
	public unsafe string? GlamName => Service.DataManager.GetExcelSheet<Item>()!.GetRow(GlamId).Name.ExtractText();
	//public unsafe void Remove() => this.invHandler.MoveItemToContainer(this.Item, 1000, this.StorageSlot);

	public unsafe string Remove() {
		if (Item > 0) {
			uint pInv = 1000;
			uint dstSlot = StorageSlot;
            invHandler.MoveItemToContainer(Item, pInv, dstSlot);
			return $"Removed {ItemName}.";
		}
		return $"Unable to remove {ItemName}.";
	}


	private static int EquipAttemptLoops = 0;
	public static void Equip(uint itemID, InventoryType? container = null, int? slot = null) {
		if (Inventory.HasItemEquipped(itemID))
			return;

		var pos = Inventory.GetItemLocationInInventory(itemID, Inventory.Equippable);
		if (pos == null) {
			Service.Log.Error($"Failed to find item (ID: {itemID}) in inventory");
			return;
		}

		container ??= pos.Value.inv;
		slot ??= pos.Value.slot;

		var agentId = Inventory.Armory.Contains(container.Value) ? AgentId.ArmouryBoard : AgentId.Inventory;
		var addonId = AgentModule.Instance()->GetAgentByInternalId(agentId)->GetAddonId();
		var ctx = AgentInventoryContext.Instance();
		ctx->OpenForItemSlot(container.Value, slot.Value, addonId);

		var contextMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("ContextMenu");
		if (contextMenu != null) {
			for (var i = 0; i < contextMenu->AtkValuesCount; i++) {
				var firstEntryIsEquip = ctx->EventIds[i] == 25; // i'th entry will fire eventid 7+i; eventid 25 is 'equip'
				if (firstEntryIsEquip) {
					Service.Log.Info($"Equipping item #{itemID} from {container.Value} @ {slot.Value}, index {i}");
					//Callback.Fire(contextMenu, true, 0, i - 7, 0, 0, 0); // p2=-1 is close, p2=0 is exec first command
				}
			}
			//Callback.Fire(contextMenu, true, 0, -1, 0, 0, 0);
			EquipAttemptLoops++;

			if (EquipAttemptLoops >= 5) 				//Service.Log.Error($"Equip option not found after 5 attempts. Aborting.");
				return;
		}
	}




	//public unsafe uint GlamDyeIdA => this.GlamId->GetStain(1);
	//public unsafe uint GlamDyeA => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot((int)this.GlamId)->GetStain(0);
}

