using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class Boss : Monster
    {
        public Boss() 
        {
            
        }

        public override void SetType()
        {
            ObjectType = GameObjectType.Boss;
        }

        const int IDLE_DELAY_MIN_TIME = 2;
        const int IDLE_DELAY_MAX_TIME = 3;

        const int PATROL_SEARCH_MIN_RANGE = 10;
        const int PATROL_SEARCH_MAX_RANGE = 20;

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;

        long _nextSearchTick = 0;
        protected override void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + GetRanIdleDealyTime();

            State = CreatureState.Moving;
        }

        int _defaultSkillRange = 4;
        long _nextMoveTick = 0;
        protected override void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            // Patrol
            Vector2Int ranDestPos = GetRanPatrolPos();
            List<Vector2Int> path = Room.Map.FindPath(CellPos, ranDestPos, checkObjects: true);
            if (path.Count < 2 || path.Count > _chaseCellDist)
            {
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 스킬로 넘어갈지 체크
            _target = Room.FindClosestPlayer(CellPos, _defaultSkillRange);
            if(_target != null)
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            // 이동
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);
            BroadcastMove();
        }

        long _coolTick = 0;
        protected override void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                // 유효한 타겟인지
                if (_target == null || _target.Room != Room)
                {
                    _target = null;
                    State = CreatureState.Idle;
                    BroadcastMove();
                    return;
                }

                Skill skillData = null;

                // TODO random 스킬 사용
                Random rnd = new Random();
                int ranSkillIdx = rnd.Next(3, 6);

                DataManager.SkillDict.TryGetValue(ranSkillIdx, out skillData);

                // 스킬이 아직 사용 가능한지
                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= skillData.splash.maxRange);
                if (canUseSkill == false)
                {
                    State = CreatureState.Idle;
                    BroadcastMove();
                    return;
                }


                // 데미지 판정
                //_target.OnDamaged(this, skillData.damage + TotalAttack);
                _target.OnDamaged(this, 15);

                // 스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                skill.Info.CellPosX = _target.CellPos.x;
                skill.Info.CellPosY = _target.CellPos.y;
                
                Room.BroadCastVision(CellPos, skill);

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }


        public override void OnDead(GameObject attacker)
        {
            if (job != null)
            {
                job.Cancel = true;
                job = null;
            }

            if (Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.BroadCastVision(CellPos, diePacket);

            GameRoom room = Room;
            room.LeaveGame(Id);

            //GameObject owner = attacker.GetOwner();
            //if (owner.ObjectType == GameObjectType.Player)
            //{
            //    RewardData rewardData = GetRandomReward();
            //    if (rewardData != null)
            //    {
            //        Player player = (Player)owner;
            //        DbTransaction.RewardPlayer(player, rewardData, Room);
            //    }
            //}
        }


        private long GetRanIdleDealyTime()
        {
            Random rnd = new Random();

            return rnd.Next(IDLE_DELAY_MIN_TIME, IDLE_DELAY_MAX_TIME + 1);
        }

        private Vector2Int GetRanPatrolPos()
        {
            Random rnd = new Random();

            while(true)
            {
                int ranX = rnd.Next(Room.Map.MinX, Room.Map.MaxX);
                int ranY = rnd.Next(Room.Map.MinY, Room.Map.MaxY);

                int rangeRange = Math.Abs(ranX-CellPos.x) + Math.Abs(ranY-CellPos.y);
                if (rangeRange < PATROL_SEARCH_MIN_RANGE || rangeRange > PATROL_SEARCH_MAX_RANGE)
                    continue;

                Vector2Int ranCellPos = new Vector2Int(ranX, ranY);
                if (Room.Map.CanGo(ranCellPos, checkObjects: true))
                    return ranCellPos;
            }
        }

    }
}
