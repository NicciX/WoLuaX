using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Inventory;

using Lumina;
using Lumina.Data;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using Lumina.Excel.Exceptions;
using System.Linq;

using MoonSharp.Interpreter;

using NicciX.WoLua;
using NicciX.WoLua.Api;
using NicciX.WoLua.Constants;

using FFXIVClientStructs;
using Dalamud.Game;
using Lumina.Excel.Sheets;
using System.Reflection;
using System.Collections.Generic;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace NicciX.WoLua.Lua.Api.Game;

[MoonSharpUserData]
public class SlotItemApi : ApiBase {
	[MoonSharpHidden]
	internal SlotItemApi(ScriptContainer source) : base(source) { }
	
	public bool Loaded => !this.Disposed
		&& Service.ClientState.LocalPlayer is not null
		&& Service.ClientState.LocalContentId is not 0;
	public static implicit operator bool(SlotItemApi? player) => player?.Loaded ?? false;

	private readonly ExcelSheet<Stain> stain = Service.DataManager.GetExcelSheet<Stain>()!;

	public Inventory invHandler = new Inventory();

	//private readonly TypeInfo tinfo = typeof(SlotItemApi).GetTypeInfo();

	//private string? _Name;


	public string? Name { get; set; } = "Slot not loaded. Use 'Game.Player.Equipped.LoadSlots()' to initialize.";
	public uint? SlotId { get; set; } = null!;
	//public unsafe string? PName => this.Name;

	public unsafe string SetName(string name) {
		this.Name = name;
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

	public unsafe int ItemSlot => GetSlot(this.Name!);

	public unsafe uint StorageSlot => DstSlot(this.Name!);	

	//public unsafe uint Slot => GetSlot(this.Name);
	public unsafe uint Item => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.ItemSlot)->ItemId != 0
		? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.ItemSlot)->ItemId : 0;
	public unsafe uint DyeIdA => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.ItemSlot)->GetStain(0);	
	public unsafe string DyeA => this.stain.GetRow(this.DyeIdA).Name.ToString();
	public unsafe uint DyeIdB => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.ItemSlot)->GetStain(1);
	public unsafe string DyeB => this.stain.GetRow(this.DyeIdB).Name.ToString();
	public unsafe uint GlamId => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.ItemSlot)->GlamourId;

	public unsafe string? ItemName => Service.DataManager.GetExcelSheet<Item>()!.GetRow(this.Item).Name.ExtractText();
	public unsafe string? GlamName => Service.DataManager.GetExcelSheet<Item>()!.GetRow(this.GlamId).Name.ExtractText();
	//public unsafe void Remove() => this.invHandler.MoveItemToContainer(this.Item, 1000, this.StorageSlot);

	public unsafe string Remove() {
		if (this.Item > 0) {
			uint pInv = 1000;
			uint dstSlot = this.StorageSlot;
			this.invHandler.MoveItemToContainer(this.Item, pInv, dstSlot);
			return $"Removed {this.ItemName}.";
		}
		return $"Unable to remove {this.ItemName}.";
	}

	//public unsafe uint GlamDyeIdA => this.GlamId->GetStain(1);
	//public unsafe uint GlamDyeA => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot((int)this.GlamId)->GetStain(0);
}

