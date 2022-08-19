using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{ 
	#region Skill
	[Serializable]
	public class Skill
	{
		public int id;
		public string name;
		public string description;
		public float cooldown;
		public int damage;
		public string skillIcon;
		public SkillType skillType;
		public ProjectileInfo projectile;
		public SmashInfo smash;
		public BuffInfo buff;
	}

	public class ProjectileInfo
	{
		public string name;
		public float speed;
		public int range;
		public string prefab;
	}

    public class SplashInfo
    {
        public string name;
        public float warningDelay;
        public int hitCount;
        public float hitInterval;
    }

    public class SmashInfo
    {
        public string prefab;
    }

    public class BuffInfo
    {
        public string prefab;
    }

    [Serializable]
	public class SkillData : ILoader<int, Skill>
	{
		public List<Skill> skills = new List<Skill>();

		public Dictionary<int, Skill> MakeDict()
		{
			Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
			foreach (Skill skill in skills)
				dict.Add(skill.id, skill);
			return dict;
		}
	}
	#endregion

	#region Item
	[Serializable]
	public class ItemData
	{
		public int id;
		public ItemSetType itemSetType;
		public string name;
		public string description;
		public ItemType itemType;
		public string iconPath;
	}

	[Serializable]
	public class WeaponData : ItemData
	{
		public WeaponType weaponType;
		public ClassType classType;
		public int addAttack;
	}

    [Serializable]
    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int addHp;
        public int addDefense;
		public int addMoveSpeed;
	}


    [Serializable]
    public class AccessoryData : ItemData
    {
        public AccessoryType accessoryType;
        public int addCritical;
		public int addCriticalDamage;
	}



    [Serializable]
	public class ConsumableData : ItemData
	{
		public ConsumableType consumableType;
		public int addHp;
		public int addMp;
	}


	[Serializable]
	public class ItemLoader : ILoader<int, ItemData>
	{
		public List<WeaponData> weapons = new List<WeaponData>();

		public List<ArmorData> helmets = new List<ArmorData>();
		public List<ArmorData> uppers = new List<ArmorData>();
		public List<ArmorData> boots = new List<ArmorData>();

		public List<AccessoryData> necklaces = new List<AccessoryData>();
		public List<AccessoryData> rings = new List<AccessoryData>();

		public List<ConsumableData> consumables = new List<ConsumableData>();

		public Dictionary<int, ItemData> MakeDict()
		{
			Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
			foreach (ItemData item in weapons)
			{
				item.itemType = ItemType.Weapon;
				dict.Add(item.id, item);
			}

			foreach (ItemData item in helmets)
			{
				item.itemType = ItemType.Armor;
				dict.Add(item.id, item);
			}
            foreach (ItemData item in uppers)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in boots)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }

            foreach (ItemData item in necklaces)
            {
				item.itemType = ItemType.Accessory;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in rings)
            {
                item.itemType = ItemType.Accessory;
                dict.Add(item.id, item);
            }

            foreach (ItemData item in consumables)
			{
				item.itemType = ItemType.Consumable;
				dict.Add(item.id, item);
			}
			return dict;
		}
	}
	#endregion

	#region Monster

	[Serializable]
	public class MonsterData
	{
		public int id;
		public string name;
		public string path;
		public StatInfo stat;
	}

	[Serializable]
	public class MonsterLoader : ILoader<int, MonsterData>
	{
		public List<MonsterData> monsters = new List<MonsterData>();

		public Dictionary<int, MonsterData> MakeDict()
		{
			Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
			foreach (MonsterData monster in monsters)
			{
				dict.Add(monster.id, monster);
			}
			return dict;
		}
	}

	#endregion
}