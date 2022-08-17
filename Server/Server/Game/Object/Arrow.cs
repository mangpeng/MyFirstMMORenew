using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class Arrow : Projectile
	{
		public GameObject Owner { get; set; }
		public bool Penetration = false;


        public override void Update()
		{
			if (Data == null || Data.projectile == null || Owner == null || Room == null)
				return;

			int tick = (int)(1000 / Data.projectile.speed);
			Room.PushAfter(tick, Update);

			Vector2Int destPos = GetFrontCellPos();

			if(Penetration)
            {
                if (Room.Map.ApplyMove(this, destPos, checkObjects: false, collision: false))
                {
                    S_Move movePacket = new S_Move();
                    movePacket.ObjectId = Id;
                    movePacket.PosInfo = PosInfo;
                    Room.BroadCastVision(CellPos, movePacket);

                    GameObject target = Room.Map.Find(destPos);
                    if (target != null)
                    {
                        target.OnDamaged(this, Data.damage + Owner.TotalAttack);
                    }
                }
                else
                {
                    Room.Push(Room.LeaveGame, Id);
                }
            }
			else
            {
                if (Room.Map.ApplyMove(this, destPos, checkObjects: true, collision: false))
                {
                    S_Move movePacket = new S_Move();
                    movePacket.ObjectId = Id;
                    movePacket.PosInfo = PosInfo;
                    Room.BroadCastVision(CellPos, movePacket);
                }
                else
                {
                    GameObject target = Room.Map.Find(destPos);
                    if (target != null)
                    {
                        target.OnDamaged(this, Data.damage + Owner.TotalAttack);
                    }

                    // 소멸
                    Room.Push(Room.LeaveGame, Id);
                }
            }
		}

		public override GameObject GetOwner()
		{
			return Owner;
		}
	}
}
