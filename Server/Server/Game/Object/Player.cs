using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Player : GameObject
	{
		public int PlayerDbId { get; set; }
		public ClientSession Session { get; set; }
		public VisionCube Vision { get; private set; }

		public Inventory Inven { get; private set; } = new Inventory();

        public override int MaxHp { get { return Stat.MaxHp + AddHp; } }
        public override int Attack { get { return Stat.Attack + AddDamage; } }
        public override int Defense { get { return Stat.Defense + AddDefence; } }
        public override int MoveSpeed { get { return Stat.MoveSpeed + AddMoveSpeed; } }
        public override int Critical { get { return Stat.Critical + AddCritical; } }
        public override int CriticalDamage { get { return Stat.CriticalDamage + AddCriticalDamage; } }
		public override int DamageRange { get { return Stat.DamageRange + AddDamageRange; } }

		public int AddHp { get; private set; }
        public int AddDamage { get; private set; }
        public int AddDefence { get; private set; }
        public int AddMoveSpeed { get; private set; }
        public int AddCritical { get; private set; }
        public int AddCriticalDamage { get; private set; }
        public int AddDamageRange { get; private set; }

        public Player()
		{
			Vision = new VisionCube(this);
		}

        public override void SetType()
        {
			ObjectType = GameObjectType.Player;
		}

        public override void OnDamaged(GameObject attacker, int damage, bool isCritical)
		{
			base.OnDamaged(attacker, damage, isCritical);
		}

		public override void OnDead(GameObject attacker)
		{
			base.OnDead(attacker);
		}

		public void OnLeaveGame()
		{
			// TODO
			// DB 연동?
			// -- 피가 깎일 때마다 DB 접근할 필요가 있을까?
			// 1) 서버 다운되면 아직 저장되지 않은 정보 날아감
			// 2) 코드 흐름을 다 막아버린다 !!!!
			// - 비동기(Async) 방법 사용?
			// - 다른 쓰레드로 DB 일감을 던져버리면 되지 않을까?
			// -- 결과를 받아서 이어서 처리를 해야 하는 경우가 많음.
			// -- 아이템 생성

			DbTransaction.SavePlayerStatus_Step1(this, Room);
		}

		public void HandleEquipItem(C_EquipItem equipPacket)
		{
			Item item = Inven.Get(equipPacket.ItemDbId);
			if (item == null)
				return;

			if (item.ItemType == ItemType.Consumable)
				return;

			// 착용 요청이라면, 겹치는 부위 해제
			if (equipPacket.Equipped)
			{
				Item unequipItem = null;

				if (item.ItemType == ItemType.Weapon)
				{
					unequipItem = Inven.Find(
						i => i.Equipped && i.ItemType == ItemType.Weapon);
				}
				else if (item.ItemType == ItemType.Armor)
				{
					ArmorType armorType = ((Armor)item).ArmorType;
					unequipItem = Inven.Find(
						i => i.Equipped && i.ItemType == ItemType.Armor
							&& ((Armor)i).ArmorType == armorType);
				}
                else if (item.ItemType == ItemType.Accessory)
                {
					AccessoryType accessoryType = ((Accessory)item).AccessoryType;
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Accessory
                            && ((Accessory)i).AccessoryType == accessoryType);
                }

                if (unequipItem != null)
				{
                    Console.WriteLine("착용해제");
					// 메모리 선적용 
					unequipItem.Equipped = false;

					// DB에 Noti
					DbTransaction.EquipItemNoti(this, unequipItem);

					// 클라에 통보
					S_EquipItem equipOkItem = new S_EquipItem();
					equipOkItem.ItemDbId = unequipItem.ItemDbId;
					equipOkItem.Equipped = unequipItem.Equipped;
					Session.Send(equipOkItem);
				}
			}

			{
                Console.WriteLine("착용");
				// 메모리 선적용
				item.Equipped = equipPacket.Equipped;

				// DB에 Noti
				DbTransaction.EquipItemNoti(this, item);

				// 클라에 통보
				S_EquipItem equipOkItem = new S_EquipItem();
				equipOkItem.ItemDbId = equipPacket.ItemDbId;
				equipOkItem.Equipped = equipPacket.Equipped;
				Session.Send(equipOkItem);
			}

			RefreshAdditionalStat();
		}

		public void RefreshAdditionalStat()
		{
            AddHp = 0;

            AddDamage = 0;
            AddDefence = 0;

            AddMoveSpeed = 0;

            AddCritical = 0;
            AddCriticalDamage = 0;

            foreach (Item item in Inven.Items.Values)
			{
				if (item.Equipped == false)
					continue;

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        AddDamage += ((Weapon)item).AddDamaged;
                        break;
                    case ItemType.Armor:
                        AddDefence += ((Armor)item).AddDefence;
                        AddHp += ((Armor)item).AddHp;
                        AddMoveSpeed += ((Armor)item).AddMoveSpeed;
                        break;
                    case ItemType.Accessory:
                        AddCritical += ((Accessory)item).AddCritical;
                        AddCriticalDamage += ((Accessory)item).AddCriticalDamage;
                        break;
                }
            }
		}
    }
}
