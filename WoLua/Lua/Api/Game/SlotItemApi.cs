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

	//private readonly TypeInfo tinfo = typeof(SlotItemApi).GetTypeInfo();

	public string Name { get; set; } = "SlotItem";

	public static int GetSlot(string name) {
		return name switch {
			"Head" => 2,
			"Body" => 3,
			"Hands" => 4,
			"Waist" => 0,
			"Legs" => 6,
			"Feet" => 7,
			_ => 0,
		};
	}
	//public new string Name { get; set; } = "SlotItem";
	public unsafe uint Item => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId != 0
		? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId : 0;

	//public unsafe int Slot => GetSlot(this.Name);
	//public unsafe uint Item => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId != 0
	//? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.Slot)->ItemId : 0;
	//public unsafe uint DyeIdA => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.Slot)->GetStain(0);	
	//public unsafe string DyeA => this.stain.GetRow(this.DyeIdA).Name.ToString();
	//public unsafe uint DyeIdB => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.Slot)->GetStain(0);
	//public unsafe string DyeB => this.stain.GetRow(this.DyeIdA).Name.ToString();
	//public unsafe uint Glam => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(this.Slot)->GlamourId;
}

