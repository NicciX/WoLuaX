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

using MoonSharp.Interpreter;

using NicciX.WoLua;
using NicciX.WoLua.Api;
using NicciX.WoLua.Constants;

using FFXIVClientStructs;
using Dalamud.Game;
using Lumina.Excel.Sheets;
using static NicciX.WoLua.Lua.Api.Game.SlotItemApi;

namespace NicciX.WoLua.Lua.Api.Game;

[MoonSharpUserData]
public class EquipApi: ApiBase {
	[MoonSharpHidden]
	internal EquipApi(ScriptContainer source) : base(source) { }

	//private readonly ExcelSheet<Stain> stain = Service.DataManager.GetExcelSheet<Stain>()!;

	public bool Loaded => !this.Disposed
		&& Service.ClientState.LocalPlayer is not null
		&& Service.ClientState.LocalContentId is not 0;
	public static implicit operator bool(EquipApi? player) => player?.Loaded ?? false;

	//public SlotItemApi head { get; set; } = null!;
	//private unsafe SlotItem Head.ItemId => this.head;

	//public unsafe uint? Head => this.Loaded
	//? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId : 0;

	//public SlotItemApi Head;


	public SlotItemApi Head { get; set; } = null!;

	







	//? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId : 0;


	//public unsafe uint Item => InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId != 0
	//? InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->GetInventorySlot(2)->ItemId : 0;

}
