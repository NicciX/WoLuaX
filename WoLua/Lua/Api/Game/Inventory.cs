using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.Interop;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace WoLuaX.Lua.Api.Game;

#nullable disable
public class Inventory
{
    internal static Inventory Instance { get; } = new();

    private enum ItemRarity : byte
    {
        White = 1,
        Pink = 7,
        Green = 2,
        Blue = 3,
        Purple = 4
    }

	public static readonly InventoryType[] PlayerInventory =
[
	InventoryType.Inventory1,
		InventoryType.Inventory2,
		InventoryType.Inventory3,
		InventoryType.Inventory4,
		InventoryType.KeyItems,
	];

	public static readonly InventoryType[] MainOffHand =
	[
		InventoryType.ArmoryMainHand,
		InventoryType.ArmoryOffHand
	];

	public static readonly InventoryType[] LeftSideArmory =
	[
		InventoryType.ArmoryHead,
		InventoryType.ArmoryBody,
		InventoryType.ArmoryHands,
		InventoryType.ArmoryLegs,
		InventoryType.ArmoryFeets
	];

	public static readonly InventoryType[] RightSideArmory =
	[
		InventoryType.ArmoryEar,
		InventoryType.ArmoryNeck,
		InventoryType.ArmoryWrist,
		InventoryType.ArmoryRings
	];

	public static readonly InventoryType[] Armory = [.. MainOffHand, .. LeftSideArmory, .. RightSideArmory, InventoryType.ArmorySoulCrystal];
	public static readonly InventoryType[] Equippable = [.. PlayerInventory, .. Armory];

	public static unsafe (InventoryType inv, int slot)? GetItemLocationInInventory(uint itemId, IEnumerable<InventoryType> inventories) {
		foreach (var inv in inventories) {
			var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
			for (var i = 0; i < cont->Size; ++i) {
				if (cont->GetInventorySlot(i)->ItemId == itemId)
					return (inv, i);
			}
		}
		return null;
	}

	private static unsafe int InternalGetItemCount(uint itemId, bool isHq) => InventoryManager.Instance()->GetInventoryItemCount(itemId, isHq);
	public static unsafe int GetItemCount(uint itemId, bool includeHQ = true) => includeHQ ? InternalGetItemCount(itemId, true) + InternalGetItemCount(itemId, false) : InternalGetItemCount(itemId, false);

	public static unsafe bool HasItem(uint itemId) => GetItemInInventory(itemId, Equippable) != null;
	public static unsafe bool HasItemEquipped(uint itemId) {
		var cont = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
		for (var i = 0; i < cont->Size; ++i) {
			if (cont->GetInventorySlot(i)->ItemId == itemId)
				return true;
		}

		return false;
	}

	public static unsafe InventoryItem* GetItemInInventory(uint itemId, IEnumerable<InventoryType> inventories, bool mustBeHQ = false) {
		foreach (var inv in inventories) {
			var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
			for (var i = 0; i < cont->Size; ++i) {
				if (cont->GetInventorySlot(i)->ItemId == itemId && (!mustBeHQ || cont->GetInventorySlot(i)->Flags == InventoryItem.ItemFlags.HighQuality))
					return cont->GetInventorySlot(i);
			}
		}
		return null;
	}

	public static unsafe List<Pointer<InventoryItem>> GetHQItems(IEnumerable<InventoryType> inventories) {
		List<Pointer<InventoryItem>> items = [];
		foreach (var inv in inventories) {
			var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
			for (var i = 0; i < cont->Size; ++i) {
				if (cont->GetInventorySlot(i)->Flags == InventoryItem.ItemFlags.HighQuality)
					items.Add(cont->GetInventorySlot(i));
			}
		}
		return items;
	}

	public static unsafe uint GetEmptySlots(IEnumerable<InventoryType> inventories = null) {
		if (inventories == null) {
			return InventoryManager.Instance()->GetEmptySlotsInBag();
		}
		else {
			uint count = 0;
			foreach (var inv in inventories) {
				var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
				for (var i = 0; i < cont->Size; ++i) {
					if (cont->GetInventorySlot(i)->ItemId == 0)
						count++;
				}
			}
			return count;
		}
	}

	public unsafe int GetItemCount(int itemID, bool includeHQ = true)
       => includeHQ ? InventoryManager.Instance()->GetInventoryItemCount((uint)itemID, true) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000)
       : InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000);

    public unsafe int GetItemCountInContainer(uint itemID, uint container) => GetItemInInventory(itemID, (InventoryType)container)->Quantity;

    public unsafe int GetInventoryFreeSlotCount()
    {
        InventoryType[] types = [InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4];
        var slots = 0;
        foreach (var x in types)
        {
            var cont = InventoryManager.Instance()->GetInventoryContainer(x);
            for (var i = 0; i < cont->Size; i++) {
				if (cont->Items[i].ItemId == 0)
                    slots++;
			}
		}
        return slots;
    }

    public unsafe uint GetItemIdInSlot(uint container, uint slot)
        => InventoryManager.Instance()->GetInventoryContainer((InventoryType)container)->GetInventorySlot((ushort)slot)->ItemId;

    public unsafe int GetItemCountInSlot(uint container, uint slot)
        => InventoryManager.Instance()->GetInventoryContainer((InventoryType)container)->GetInventorySlot((ushort)slot)->Quantity;

    public unsafe List<uint> GetItemIdsInContainer(uint container)
    {
        var cont = InventoryManager.Instance()->GetInventoryContainer((InventoryType)container);
        var list = new List<uint>();
        for (var i = 0; i < cont->Size; i++) {
			if (cont->Items[i].ItemId != 0)
                list.Add(cont->Items[i].ItemId);
		}

		return list;
    }

    public unsafe int GetFreeSlotsInContainer(uint container)
    {
        var inv = InventoryManager.Instance();
        var cont = inv->GetInventoryContainer((InventoryType)container);
        var slots = 0;
        for (var i = 0; i < cont->Size; i++) {
			if (cont->Items[i].ItemId == 0)
                slots++;
		}

		return slots;
    }

    public unsafe void MoveItemToContainer(uint itemID, uint srcContainer, uint dstContainer)
        => InventoryManager.Instance()->MoveItemSlot((InventoryType)srcContainer, (ushort)GetItemInInventory(itemID, (InventoryType)srcContainer)->Slot, (InventoryType)dstContainer, GetFirstAvailableSlot((InventoryType)dstContainer), 1);

    private static unsafe InventoryItem* GetItemInInventory(uint itemId, InventoryType inv, bool mustBeHQ = false)
    {
        var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
        for (var i = 0; i < cont->Size; ++i) {
			if (cont->GetInventorySlot(i)->ItemId == itemId && (!mustBeHQ || cont->GetInventorySlot(i)->Flags == InventoryItem.ItemFlags.HighQuality))
                return cont->GetInventorySlot(i);
		}

		return null;
    }

    private static unsafe ushort GetFirstAvailableSlot(InventoryType container)
    {
        var cont = InventoryManager.Instance()->GetInventoryContainer(container);
        for (var i = 0; i < cont->Size; i++) {
			if (cont->Items[i].ItemId == 0)
                return (ushort)i;
		}

		return 0;
    }

    private static unsafe InventoryItem* GetItemForSlot(InventoryType type, int slot)
        => InventoryManager.Instance()->GetInventoryContainer(type)->GetInventorySlot(slot);

    public List<uint> GetTradeableWhiteItemIDs() => Service.DataManager.GetExcelSheet<Item>()!.Where(x => !x.IsUntradable && x.Rarity == (byte)ItemRarity.White).Select(x => x.RowId).ToList();

    public string GetItemName(uint itemId) => Service.DataManager.GetExcelSheet<Item>()!.GetRow(itemId).Name.ExtractText();
}
