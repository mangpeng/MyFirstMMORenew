using Google.Protobuf.Protocol;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Player : GameObject
	{
		public int PlayerDbId { get; set; }
		public ClientSession Session { get; set; }
		public Inventory Inven { get; private set; } = new Inventory();

		public int WeaponDamage { get; private set; }
		public int ArmorDefence { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public Player()
		{
			ObjectType = GameObjectType.Player;
		}

		public override void OnDamaged(GameObject attacker, int damage)
		{
			base.OnDamaged(attacker, damage);


		}

		public override void OnDead(GameObject attacker)
		{
			base.OnDead(attacker);
		}

	    public void OnLeaveGame()
        {
			// DB연동
			// 피가 깍일 때마다 DB 접근할 필요가 있을까?
			// 1) 서버가 다운되면 아직 저장되지 않은 정보 날아감
			// 2) 코드 흐름을 다 막아 버린다. (실행하고 있는 스레드)
			// - 비동기(Async) 방법 사용?
			// - 다른 스레드로 DB 읾감을 던지면?
			// -- 결과를 받아서 이어서 처리 해야 하는 경우가 많음..
			//        using (AppDbContext db = new AppDbContext())
			//        {
			//            // Find -> Save는 읽고->쓰고 두번의 db 접근
			//            // 이와 같은 방식은 한번만 접근해서 원하는 속성 수정
			//            PlayerDb playerDb = new PlayerDb();
			//playerDb.PlayerDbId = PlayerDbId;
			//            playerDb.Hp = Stat.Hp;

			//db.Entry(playerDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
			//db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
			//            db.SaveChangesEx();

			//            Console.WriteLine($"Hp Saved({playerDb.Hp})");
			//        }

			//DbTransaction.SavePlayerSatus_AllInOne(this, Room);
			DbTransaction.SavePlayerSatus_Step1(this, Room);
		}

		public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType == ItemType.Consumable)
                return;

            // 착용 쵸엉이라면, 겹치는 부위 해제
            if (equipPacket.Equipped)
            {
                Item unequipItem = null;
                if (item.ItemType == ItemType.Weapon)
                {
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Armor && ((Armor)i).ArmorType == armorType);
                }

                if (unequipItem != null)
                {
                    unequipItem.Equipped = false;

                    DbTransaction.EquipItemNoti(this, unequipItem);

                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            {
                // DB 연동
                // 별로 중요한 내용 아아니므로 'db 요청 -> db 저장 -> 로직 수행' 과 같이 처리 안함
                // 그냥 '메모리 적용하고, db에 노티' 하는 방식

                // 메모리 선 적용
                item.Equipped = equipPacket.Equipped;

                // DB에 Noti
                DbTransaction.EquipItemNoti(this, item); // 콜백 따로 안 받음

                // 클라에게 통보
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);
            }

            RefreshAdditionalStat();
        }

        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach(Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch(item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }
	}
}
