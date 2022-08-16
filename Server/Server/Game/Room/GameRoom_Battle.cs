using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{
		public void HandleMove(Player player, C_Move movePacket)
		{
			if (player == null)
				return;

			// TODO : 검증
			PositionInfo movePosInfo = movePacket.PosInfo;
			ObjectInfo info = player.Info;

			// 다른 좌표로 이동할 경우, 갈 수 있는지 체크
			if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
			{
				if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
					return;
			}

			// 포탈 위치 했는지 체크
			if(Map.IsPrevProtal(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)))
            {
                Console.WriteLine("PrevPortal!!");

                GameRoom nextRoom = GameLogic.Instance.Find(RoomId - 1);
                if (nextRoom != null)
                {
					LeaveGame(player.Id);


					switch(nextRoom.RoomId)
                    {
						case 1:
							player.CellPos = new Vector2Int(-6, 22);
							break;
						case 2:
							player.CellPos = new Vector2Int(18, 21);
							break;
                    }
					
                    nextRoom.EnterGame(player, randomPos: false);
                    Console.WriteLine("이전 방으로 이동");
					return;
                }
                else
                {
                    Console.WriteLine("이전 방이 없습니다.");
                }
            }
            if (Map.IsNextProtal(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)))
            {
                Console.WriteLine("NextPortal!!");

                GameRoom nextRoom = GameLogic.Instance.Find(RoomId + 1);
                if (nextRoom != null)
                {
					LeaveGame(player.Id);

                    switch (nextRoom.RoomId)
                    {
                        case 2:
							player.CellPos = new Vector2Int(-11, -6);
							break;
                        case 3:
							player.CellPos = new Vector2Int(5, -3);
							break;
                    }

              
					nextRoom.EnterGame(player, randomPos: false);
					Console.WriteLine("다음 방으로 이동");
					return;
                }
                else
                {
                    Console.WriteLine("다음 방이 없습니다.");
                }
            }

            info.PosInfo.State = movePosInfo.State;
			info.PosInfo.MoveDir = movePosInfo.MoveDir;
			Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

			// 다른 플레이어한테도 알려준다
			S_Move resMovePacket = new S_Move();
			resMovePacket.ObjectId = player.Info.ObjectId;
			resMovePacket.PosInfo = movePacket.PosInfo;

			BroadCastVision(player.CellPos, resMovePacket);
		}

		public void HandleSkill(Player player, C_Skill skillPacket)
		{
			if (player == null)
				return;

			ObjectInfo info = player.Info;
			if (info.PosInfo.State != CreatureState.Idle)
				return;

			// TODO : 스킬 사용 가능 여부 체크
			info.PosInfo.State = CreatureState.Skill;
			S_Skill skill = new S_Skill() { Info = new SkillInfo() };
			skill.ObjectId = info.ObjectId;
			skill.Info.SkillId = skillPacket.Info.SkillId;
			BroadCastVision(player.CellPos, skill);

			Data.Skill skillData = null;
			if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
				return;

			switch (skillData.skillType)
			{
				case SkillType.SkillAuto:
					{
						Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
						GameObject target = Map.Find(skillPos);
						if (target != null)
						{
							Console.WriteLine("Hit GameObject !");
						}
					}
					break;
				case SkillType.SkillProjectile:
					{
						Arrow arrow = ObjectManager.Instance.Add<Arrow>();
						if (arrow == null)
							return;

						arrow.Owner = player;
						arrow.Data = skillData;
						arrow.PosInfo.State = CreatureState.Moving;
						arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
						arrow.PosInfo.PosX = player.PosInfo.PosX;
						arrow.PosInfo.PosY = player.PosInfo.PosY;
						arrow.Speed = skillData.projectile.speed;
						Push(EnterGame, arrow, false);
					}
					break;
			}
		}

        public void HandleChat(Player player, C_Chat chatPacket)
        {
            if (player == null)
                return;

            // 다른 플레이어한테도 알려준다
            S_Chat resChatPacket = new S_Chat();
			resChatPacket.ObjectId = player.Info.ObjectId;
			resChatPacket.Message = chatPacket.Message;

			BroadCastRoom(resChatPacket);
        }


        public void HandleUsePotion(Player player, C_UsePotion usePotionPacket)
        {
            if (player == null)
                return;


            // 아이템 리스트 새로 보냄 <= 문제 있다. 물약 소모는 게임 도중 빈번하게 일어나므로..
            // db에 의해 서버 로직이 느려진다..
            {

            }

            // 1. 인벤에서 아이템 제거 -> db에 저장 -> 아이템 리스트 새로 보냄
            // db에서 아이템 제거
            {
                using (AppDbContext db = new AppDbContext())
                {
					ItemDb potion = db.Items
						.Where(i => i.ItemDbId == usePotionPacket.ItemInfo.ItemDbId)
						.FirstOrDefault();

					// db에서 아이템 삭제
                    {
                        db.Remove(potion);
                        bool success = db.SaveChangesEx();
                        if (success)
                        {
                            Item removeItem = Item.MakeItem(potion);
                            player.Inven.Remove(removeItem);

                            // 클라에게 삭제된 아이템 정보 알림
                            {
                                Console.WriteLine("아이템 소모");
                                S_RemoveItem removePacket = new S_RemoveItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(removeItem.Info);
                                removePacket.Items.Add(itemInfo);
                                player.Session.Send(removePacket);
                            }
                            {
								S_UsePotion usePotionOkPacket = new S_UsePotion();
								usePotionPacket.ObjectId = player.Id;
                                Console.WriteLine($"S_UsePotion {usePotionPacket.ObjectId}");
								player.Session.Send(usePotionOkPacket);
							}
                        }
						else
                        {
                            Console.WriteLine("아이템 삭제 실패");
                        }
                    }

                }
            }



			// 2. 캐릭터 hp 증가
			// todo 임시로 체력 50회복
			int recoveryAmoount = 50;
			player.Stat.Hp = Math.Clamp(player.Stat.Hp + recoveryAmoount, 0, player.Stat.MaxHp);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = player.Id;
            changePacket.Hp = player.Stat.Hp;
            BroadCastVision(player.CellPos, changePacket);

        }
    }
}
