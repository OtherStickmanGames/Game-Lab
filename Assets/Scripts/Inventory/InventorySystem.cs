using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;

namespace DwarfClone.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        public event Action OnInventoryChanged;

        [Header("Capacity")]
        [SerializeField] private int maxSlots = Constants.DEFAULT_INVENTORY_SLOTS;

        [Header("Equipment Slots")]
        [SerializeField] private ItemData equippedWeapon;
        [SerializeField] private ItemData equippedArmor;
        [SerializeField] private ItemData equippedHelmet;
        [SerializeField] private ItemData equippedTool;

        private readonly List<ItemStack> backpack = new List<ItemStack>();

        public IReadOnlyList<ItemStack> Backpack => backpack;
        public ItemData EquippedWeapon => equippedWeapon;
        public ItemData EquippedArmor => equippedArmor;
        public ItemData EquippedHelmet => equippedHelmet;
        public ItemData EquippedTool => equippedTool;
        public int MaxCapacity => maxSlots;
        public int TotalItemCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < backpack.Count; i++)
                {
                    if (!backpack[i].IsEmpty) count += backpack[i].count;
                }
                return count;
            }
        }

        private void Awake()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            backpack.Clear();
            for (int i = 0; i < maxSlots; i++)
            {
                backpack.Add(ItemStack.Empty);
            }
        }

        public bool AddItem(ItemData item, int count = 1)
        {
            if (item == null || count <= 0) return false;

            int remaining = count;

            // 1. Try stacking into existing non-full slots
            for (int i = 0; i < backpack.Count; i++)
            {
                if (!backpack[i].IsEmpty && backpack[i].item.id == item.id)
                {
                    int space = item.stackLimit - backpack[i].count;
                    if (space > 0)
                    {
                        int toAdd = Mathf.Min(remaining, space);
                        backpack[i].count += toAdd;
                        remaining -= toAdd;
                        if (remaining <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }

            // 2. Put into empty slots
            for (int i = 0; i < backpack.Count; i++)
            {
                if (backpack[i].IsEmpty)
                {
                    int toAdd = Mathf.Min(remaining, item.stackLimit);
                    backpack[i] = new ItemStack(item, toAdd);
                    remaining -= toAdd;
                    if (remaining <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return remaining < count;
        }

        public bool RemoveItem(ItemData item, int count = 1)
        {
            if (item == null || count <= 0) return false;
            if (GetItemCount(item.id) < count) return false;

            int remaining = count;
            for (int i = backpack.Count - 1; i >= 0; i--)
            {
                if (!backpack[i].IsEmpty && backpack[i].item.id == item.id)
                {
                    if (backpack[i].count <= remaining)
                    {
                        remaining -= backpack[i].count;
                        backpack[i].Clear();
                    }
                    else
                    {
                        backpack[i].count -= remaining;
                        remaining = 0;
                    }

                    if (remaining <= 0) break;
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public int GetItemCount(string itemId)
        {
            int total = 0;
            for (int i = 0; i < backpack.Count; i++)
            {
                if (!backpack[i].IsEmpty && backpack[i].item.id == itemId)
                {
                    total += backpack[i].count;
                }
            }
            return total;
        }

        public bool HasItem(string itemId, int count = 1)
        {
            return GetItemCount(itemId) >= count;
        }

        public bool HasTool(ToolType type)
        {
            if (equippedTool != null && equippedTool.toolType == type) return true;
            if (equippedWeapon != null && equippedWeapon.toolType == type) return true;

            for (int i = 0; i < backpack.Count; i++)
            {
                if (!backpack[i].IsEmpty && backpack[i].item.toolType == type) return true;
            }
            return false;
        }

        public void Equip(ItemData item)
        {
            if (item == null) return;

            if (item.IsTool) equippedTool = item;
            else if (item.IsWeapon) equippedWeapon = item;
            else if (item.IsArmor)
            {
                if (item.id.Contains("helmet")) equippedHelmet = item;
                else equippedArmor = item;
            }
            OnInventoryChanged?.Invoke();
        }
    }
}
