using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
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
					player.CellPos = new Vector2Int(-6, 22);
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
					player.CellPos = new Vector2Int(-11, -6);
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
    }
}
