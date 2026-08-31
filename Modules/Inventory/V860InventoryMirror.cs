using System;
using System.Collections.Generic;

namespace OpenNEL.WinUI.Modules.Inventory;

internal sealed class V860InventoryMirror
{
	internal sealed record ItemStack(int RuntimeId, int Count, int NetworkId, byte[] Metadata)
	{
		public static ItemStack Empty { get; } = new(0, 0, 0, Array.Empty<byte>());
		public bool IsAir => RuntimeId == 0 || Count <= 0;
		public string Text => string.Empty;
	}

	public const int PlayerInventoryContainer = 0;
	public const byte ArmorSlotType = 6;
	public const byte HotbarAndInventorySlotType = 12;
	public const byte DynamicContainerSlotType = 64;

	public static V860InventoryMirror Shared { get; } = new();

	public IReadOnlyList<(int Slot, ItemStack Item)> GetContainerItems(int containerId) =>
		Array.Empty<(int Slot, ItemStack Item)>();

	public IReadOnlyList<(int Slot, ItemStack Item)> GetPlayerItems() =>
		Array.Empty<(int Slot, ItemStack Item)>();

	public bool IsArmorSlotEmpty(int slot) => true;

	public bool TryGetFirstEmptyPlayerSlot(out int slot)
	{
		slot = -1;
		return false;
	}

	public bool TryMove(
		int sourceContainer,
		int sourceSlot,
		byte sourceType,
		int destinationContainer,
		int destinationSlot,
		byte destinationType) => false;

	public bool TryDrop(int sourceSlot) => false;
}
