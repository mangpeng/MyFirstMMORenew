using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat : UI_Base
{
	enum Images
	{
		Slot_Armor_Helmet,
		Slot_Armor_Upper,
		Slot_Armor_Boots,
		Slot_Weapon,
		Slot_Accessory_Necklace,
		Slot_Accessory_Ring,
	}

	enum Texts
	{
		NameText,
        TotalHpValueText,
        TotalAttackValueText,
        TotalDefenseValueText,
        TotalCriticalValueText,
        TotalCriticalDamageValueText,
        TotalMoveSpeedValueText,
        AddHpValueText,
        AddAttackValueText,
        AddDefenseValueText,
        AddCriticalValueText,
        AddCriticalDamageValueText,
        AddMoveSpeedValueText,
    }

    bool _init = false;
	public override void Init()
	{
		Bind<Image>(typeof(Images));
		Bind<Text>(typeof(Texts));

		_init = true;
		RefreshUI();
	}

	public void RefreshUI()
	{
		if (_init == false)
			return;

		// 우선은 다 가린다
		Get<Image>((int)Images.Slot_Armor_Helmet).enabled = false;
		Get<Image>((int)Images.Slot_Armor_Upper).enabled = false;
		Get<Image>((int)Images.Slot_Armor_Boots).enabled = false;
		Get<Image>((int)Images.Slot_Weapon).enabled = false;
		Get<Image>((int)Images.Slot_Accessory_Necklace).enabled = false;
		Get<Image>((int)Images.Slot_Accessory_Ring).enabled = false;

		// 채워준다
		foreach (Item item in Managers.Inven.Items.Values)
		{
			if (item.Equipped == false)
				continue;

			ItemData itemData = null;
			Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
			Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);

			if (item.ItemType == ItemType.Weapon)
			{
				Get<Image>((int)Images.Slot_Weapon).enabled = true;
				Get<Image>((int)Images.Slot_Weapon).sprite = icon;
			}
			else if (item.ItemType == ItemType.Armor)
			{
				Armor armor = (Armor)item;
				switch (armor.ArmorType)
				{
					case ArmorType.Helmet:
						Get<Image>((int)Images.Slot_Armor_Helmet).enabled = true;
						Get<Image>((int)Images.Slot_Armor_Helmet).sprite = icon;
						break;
					case ArmorType.Upper:
						Get<Image>((int)Images.Slot_Armor_Upper).enabled = true;
						Get<Image>((int)Images.Slot_Armor_Upper).sprite = icon;
						break;
					case ArmorType.Boots:
						Get<Image>((int)Images.Slot_Armor_Boots).enabled = true;
						Get<Image>((int)Images.Slot_Armor_Boots).sprite = icon;
						break;
				}
			}
            else if (itemData.itemType == ItemType.Accessory)
            {
                Accessory accessory = (Accessory)item;
                switch (accessory.AccessoryType)
                {
					case AccessoryType.Necklace:
                        Get<Image>((int)Images.Slot_Accessory_Necklace).enabled = true;
                        Get<Image>((int)Images.Slot_Accessory_Necklace).sprite = icon;
                        break;
					case AccessoryType.Ring:
						Get<Image>((int)Images.Slot_Accessory_Ring).enabled = true;
                        Get<Image>((int)Images.Slot_Accessory_Ring).sprite = icon;
                        break;
                }
            }
        }

		// Text
		MyPlayerController player = Managers.Object.MyPlayer;
		player.RefreshAdditionalStat();

		Get<Text>((int)Texts.NameText).text = player.name;

		int totalDamage = player.Stat.Attack + player.AddDamage;


		Get<Text>((int)Texts.TotalHpValueText).text = $"{player.Stat.MaxHp + player.AddHp}<color=yellow>(+{player.AddHp})</color>";
		Get<Text>((int)Texts.TotalAttackValueText).text = $"{player.Attack}<color=yellow>(+{player.AddDamage})</color>";
		Get<Text>((int)Texts.TotalDefenseValueText).text = $"{player.Defense}<color=yellow>(+{player.AddDefence})</color>";
		Get<Text>((int)Texts.TotalCriticalValueText).text = $"{player.Critical}<color=yellow>(+{player.AddCritical})</color>";
		Get<Text>((int)Texts.TotalCriticalDamageValueText).text = $"{player.CriticalDamage}<color=yellow>(+{player.AddCriticalDamage})</color>";
		Get<Text>((int)Texts.TotalMoveSpeedValueText).text = $"{player.MoveSpeed}<color=yellow>(+{player.AddMoveSpeed})</color>";


		Get<Text>((int)Texts.AddHpValueText).text = $"{player.AddHp}";
		Get<Text>((int)Texts.AddAttackValueText).text = $"{player.AddDamage}";
		Get<Text>((int)Texts.AddDefenseValueText).text = $"{player.AddDefence}";
		Get<Text>((int)Texts.AddCriticalValueText).text = $"{player.AddCritical}";
		Get<Text>((int)Texts.AddCriticalDamageValueText).text = $"{player.AddCriticalDamage}";
        Get<Text>((int)Texts.AddMoveSpeedValueText).text = $"{player.AddMoveSpeed}";
    }
}
