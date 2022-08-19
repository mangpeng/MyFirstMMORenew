using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Item
    {
        public ItemInfo Info { get; } = new ItemInfo();

        public int ItemDbId
        {
            get { return Info.ItemDbId; }
            set { Info.ItemDbId = value; }
        }

        public int TemplateId
        {
            get { return Info.TemplateId; }
            set { Info.TemplateId = value; }
        }

        public int Count
        {
            get { return Info.Count; }
            set { Info.Count = value; }
        }

        public int Slot
        {
            get { return Info.Slot; }
            set { Info.Slot = value; }
        }

        public bool Equipped
        {
            get { return Info.Equipped; }
            set { Info.Equipped = value; }
        }

        public ItemSetType ItemSetType = ItemSetType.None;
        public ItemType ItemType { get; private set; }
        public bool Stackable { get; protected set; }

        public Item(ItemType itemType)
        {
            ItemType = itemType;
        }

        public static Item MakeItem(ItemDb itemDb)
        {
            Item item = null;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(itemDb.TemplateId, out itemData);

            if (itemData == null)
                return null;

            switch (itemData.itemType)
            {
                case ItemType.Weapon:
                    item = new Weapon(itemDb.TemplateId);
                    break;
                case ItemType.Armor:
                    item = new Armor(itemDb.TemplateId);
                    break;
                case ItemType.Accessory:
                    item = new Accessory(itemDb.TemplateId);
                    break;
                case ItemType.Consumable:
                    item = new Consumable(itemDb.TemplateId);
                    break;
            }

            if (item != null)
            {
                item.ItemSetType = itemData.itemSetType;
                item.ItemDbId = itemDb.ItemDbId;
                item.Count = itemDb.Count;
                item.Slot = itemDb.Slot;
                item.Equipped = itemDb.Equipped;
            }

            return item;
        }
    }

    public class Weapon : Item
    {
        public WeaponType WeaponType { get; private set; }
        public ClassType ClassType { get; private set; }
        public int AddDamaged { get; private set; }

        public Weapon(int templateId) : base(ItemType.Weapon)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Weapon)
                return;

            WeaponData data = (WeaponData)itemData;
            {
                TemplateId = data.id;
                Count = 1;
                WeaponType = data.weaponType;
                ClassType = data.classType;
                AddDamaged = data.addAttack;
                Stackable = false;
            }
        }
    }

    public class Armor : Item
    {
        public ArmorType ArmorType { get; private set; }

        public int AddHp { get; private set; }
        public int AddDefence { get; private set; }
        public int AddMoveSpeed { get; private set; }

        public Armor(int templateId) : base(ItemType.Armor)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Armor)
                return;

            ArmorData data = (ArmorData)itemData;
            {
                TemplateId = data.id;
                Count = 1;
                ArmorType = data.armorType;
                AddHp = data.addHp;
                AddDefence = data.addDefense;
                AddMoveSpeed = data.addMoveSpeed;
                Stackable = false;
            }
        }
    }

    public class Accessory : Item
    {
        public AccessoryType AccessoryType { get; private set; }

        public int AddCritical { get; private set; }
        public int AddCriticalDamage { get; private set; }

        public Accessory(int templateId) : base(ItemType.Accessory)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Accessory)
                return;

            AccessoryData data = (AccessoryData)itemData;
            {
                TemplateId = data.id;
                Count = 1;
                AccessoryType = data.accessoryType;
                AddCritical = data.addCritical;
                AddCriticalDamage = data.addCriticalDamage;
                Stackable = false;
            }
        }
    }

    public class Consumable : Item
    {
        public ConsumableType ConsumableType { get; private set; }

        public int AddHp { get; private set; }
        public int AddMp { get; private set; }

        public Consumable(int templateId) : base(ItemType.Consumable)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Consumable)
                return;

            ConsumableData data = (ConsumableData)itemData;
            {
                TemplateId = data.id;
                Count = 1;

                AddHp = data.addHp;
                AddMp = data.addMp;

                ConsumableType = data.consumableType;
                Stackable = false;
            }
        }
    }
}
