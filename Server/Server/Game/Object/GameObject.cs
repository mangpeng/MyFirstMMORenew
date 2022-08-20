using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}

		public GameRoom Room { get; set; }

		public ObjectInfo Info { get; set; } = new ObjectInfo();
		public PositionInfo PosInfo { get; private set; } = new PositionInfo();
		public StatInfo Stat { get; private set; } = new StatInfo();

        public virtual int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, MaxHp); }
        }
        public virtual int MaxHp { get { return Stat.MaxHp; } }
        public virtual int Attack { get { return Stat.Attack;  } }
        public virtual int Defense { get { return Stat.Defense; } }
        public virtual int MoveSpeed { get { return Stat.MoveSpeed; } }
        public virtual int Critical { get { return Stat.Critical; } }
        public virtual int CriticalDamage { get { return Stat.CriticalDamage; } }
        public virtual int DamageRange { get { return Stat.DamageRange; } }


        public MoveDir Dir
		{
			get { return PosInfo.MoveDir; }
			set { PosInfo.MoveDir = value; }
		}

		public CreatureState State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}

		public GameObject()
		{
			Info.PosInfo = PosInfo;
			Info.StatInfo = Stat;

			SetType();
		}

		public virtual void Update()
		{

		}

		public virtual void SetType()
        {

        }

		public Vector2Int CellPos
		{
			get
			{
				return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
			}

			set
			{
				PosInfo.PosX = value.x;
				PosInfo.PosY = value.y;
			}
		}

		public Vector2Int GetFrontCellPos()
		{
			return GetFrontCellPos(PosInfo.MoveDir);
		}

		public Vector2Int GetFrontCellPos(MoveDir dir)
		{
			Vector2Int cellPos = CellPos;

			switch (dir)
			{
				case MoveDir.Up:
					cellPos += Vector2Int.up;
					break;
				case MoveDir.Down:
					cellPos += Vector2Int.down;
					break;
				case MoveDir.Left:
					cellPos += Vector2Int.left;
					break;
				case MoveDir.Right:
					cellPos += Vector2Int.right;
					break;
			}

			return cellPos;
		}

		public static MoveDir GetDirFromVec(Vector2Int dir)
		{
			if (dir.x > 0)
				return MoveDir.Right;
			else if (dir.x < 0)
				return MoveDir.Left;
			else if (dir.y > 0)
				return MoveDir.Up;
			else
				return MoveDir.Down;
		}

		public virtual void OnDamaged(GameObject attacker, int damage, bool isCritical)
		{
			if (Room == null)
				return;

			int totalDamage = damage - Defense;

			//Console.WriteLine($"defeat {attacker.ObjectType.ToString()} => {ObjectType.ToString()} {totalDamage}({Defense})");

			totalDamage = Math.Max(totalDamage, 0);
            Stat.Hp = Math.Max(Stat.Hp - totalDamage, 0);


			S_ChangeHp changePacket = new S_ChangeHp();
			changePacket.ObjectId = Id;
			changePacket.Hp = Stat.Hp;
			changePacket.IsCritical = isCritical;
			Room.BroadCastVision(CellPos, changePacket);

			if (Stat.Hp <= 0)
			{
				OnDead(attacker);
			}
		}

		public virtual void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.AttackerId = attacker.Id;
			Room.BroadCastVision(CellPos, diePacket);


			
			GameRoom room = Room;
			room.LeaveGame(Id);

			Hp = MaxHp;
			PosInfo.State = CreatureState.Idle;
			PosInfo.MoveDir = MoveDir.Down;

            if (ObjectManager.GetObjectTypeById(attacker.Id) == GameObjectType.Boss)
            {
				room = GameLogic.Instance.Find(2);
				room.EnterGame(this, randomPos: true);
			}
			else
            {
				room.EnterGame(this, randomPos: true);
            }
		}

		public virtual GameObject GetOwner()
		{
			return this;
		}

        public int CalculateDamage(out bool isCritical)
        {
			int totalDamage = Attack;

			// 데미지 오차
			Random rnd = new Random();
			float added = (100 + rnd.Next(-DamageRange, DamageRange)) / 100f;
			totalDamage = (int) (totalDamage* added);

            // 크리티컬 확률
            isCritical = rnd.Next(0, 100) < Critical;

            if (isCritical)// 크리티컬 데미지
            {
                totalDamage = (int) (totalDamage* (1f + CriticalDamage / 100f));
                return totalDamage;
            }
            else
            {
                return totalDamage;
            }
		}
    }
}
