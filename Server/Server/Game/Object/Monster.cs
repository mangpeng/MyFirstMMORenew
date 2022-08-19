﻿using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Server.Game
{
	public class Monster : GameObject
	{
		public int TemplateId { get; private set; }

		public Monster()
		{
		}

        public override void SetType()
        {
			ObjectType = GameObjectType.Monster;
        }


        public virtual void Init(int templateId)
		{

			TemplateId = templateId;

			MonsterData monsterData = null;
			DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
			Stat.MergeFrom(monsterData.stat);
			Stat.Hp = monsterData.stat.MaxHp;
			State = CreatureState.Idle;
		}

		// FSM (Finite State Machine)
		protected IJob job;
		public override void Update()
		{
			switch (State)
			{
				case CreatureState.Idle:
					UpdateIdle();
					break;
				case CreatureState.Moving:
					UpdateMoving();
					break;
				case CreatureState.Skill:
					UpdateSkill();
					break;
				case CreatureState.Dead:
					UpdateDead();
					break;
			}

			// 5프레임 (0.2초마다 한번씩 Update)
			if (Room != null)
				job = Room.PushAfter(200, Update);
		}

		Player _target;
		int _searchCellDist = 10;
		int _chaseCellDist = 20;

		long _nextSearchTick = 0;
		protected virtual void UpdateIdle()
		{
			if (_nextSearchTick > Environment.TickCount64)
				return;
			_nextSearchTick = Environment.TickCount64 + 1000;

			Player target = Room.FindClosestPlayer(CellPos, _searchCellDist);

			if (target == null)
				return;

			_target = target;
			State = CreatureState.Moving;
		}

		int _skillRange = 1;
		long _nextMoveTick = 0;
		protected virtual void UpdateMoving()
		{
			if (_nextMoveTick > Environment.TickCount64)
				return;
			int moveTick = (int)(1000f / MoveSpeed);
			_nextMoveTick = Environment.TickCount64 + moveTick;

			if (_target == null || _target.Room != Room)
			{
				_target = null;
				State = CreatureState.Idle;
				BroadCastVisionMove();
				return;
			}

			Vector2Int dir = _target.CellPos - CellPos;
			int dist = dir.cellDistFromZero;
			if (dist == 0 || dist > _chaseCellDist)
			{
				_target = null;
				State = CreatureState.Idle;
				BroadCastVisionMove();
				return;
			}

			List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: true);
			if (path.Count < 2 || path.Count > _chaseCellDist)
			{
				_target = null;
				State = CreatureState.Idle;
				BroadCastVisionMove();
				return;
			}

			// 스킬로 넘어갈지 체크
			if (dist <= _skillRange && (dir.x == 0 || dir.y == 0))
			{
				_coolTick = 0;
				State = CreatureState.Skill;
				return;
			}

			// 이동
			Dir = GetDirFromVec(path[1] - CellPos);
			Room.Map.ApplyMove(this, path[1]);
			BroadCastVisionMove();
		}

		protected void BroadCastVisionMove()
		{
			// 다른 플레이어한테도 알려준다
			S_Move movePacket = new S_Move();
			movePacket.ObjectId = Id;
			movePacket.PosInfo = PosInfo;
			Room.BroadCastVision(CellPos, movePacket);
		}

		long _coolTick = 0;
		protected virtual void UpdateSkill()
		{
			if (_coolTick == 0)
			{
				// 유효한 타겟인지
				if (_target == null || _target.Room != Room)
				{
					_target = null;
					State = CreatureState.Moving;
					BroadCastVisionMove();
					return;
				}

				// 스킬이 아직 사용 가능한지
				Vector2Int dir = (_target.CellPos - CellPos);
				int dist = dir.cellDistFromZero;
				bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
				if (canUseSkill == false)
				{
					State = CreatureState.Moving;
					BroadCastVisionMove();
					return;
				}

				// 타게팅 방향 주시
				MoveDir lookDir = GetDirFromVec(dir);
				if (Dir != lookDir)
				{
					Dir = lookDir;
					BroadCastVisionMove();
				}

				Skill skillData = null;
				DataManager.SkillDict.TryGetValue(Const.SKILL_FIST, out skillData);
				bool isCritical = false;
				int totalDamage = skillData.damage + CalculateDamage(out isCritical);

				_target.OnDamaged(this, totalDamage, isCritical);

				// 스킬 사용 Broadcast
				S_Skill skill = new S_Skill() { Info = new SkillInfo() };
				skill.ObjectId = Id;
				skill.Info.SkillId = skillData.id;
				Room.BroadCastVision(CellPos, skill);

				// 스킬 쿨타임 적용
				int coolTick = (int)(1000 * skillData.cooldown);
				_coolTick = Environment.TickCount64 + coolTick;
			}

			if (_coolTick > Environment.TickCount64)
				return;

			_coolTick = 0;
		}

		protected virtual void UpdateDead()
		{

        }

        public override void OnDead(GameObject attacker)
        {
			if (job != null)
			{
				job.Cancel = true;
				job = null;
			}

			base.OnDead(attacker);

			GameObject owner = attacker.GetOwner();
			if (owner.ObjectType == GameObjectType.Player)
			{
				RewardData rewardData = GetRandomReward();
				if (rewardData != null)
				{
					Player player = (Player)owner;
					DbTransaction.RewardPlayer(player, rewardData, Room);
				}
			}
		}

		RewardData GetRandomReward()
		{
			MonsterData monsterData = null;
			DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

			int rand = new Random().Next(0, 101);

			int sum = 0;
			foreach (RewardData rewardData in monsterData.rewards)
			{
				sum += rewardData.probability;

				if (rand <= sum)
				{
					return rewardData;
				}
			}

			return null;
		}
	}
}
