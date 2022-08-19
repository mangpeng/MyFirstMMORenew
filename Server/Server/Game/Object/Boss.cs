using Google.Protobuf.Protocol;
using Server.Data;
using Server.Utils;
using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class Boss : Monster
    {
        public enum SplashSkillType
        {
            None = 0,
            Normal,
            Splash_Cross,
            Splash_Diagnal,
            Splash_Area,
        }

        public Boss() 
        {
            
        }

        public override void SetType()
        {
            ObjectType = GameObjectType.Boss;
        }

        const int IDLE_DELAY_MIN_TIME = 2;
        const int IDLE_DELAY_MAX_TIME = 3;

        const int PATROL_SEARCH_MIN_RANGE = 20;
        const int PATROL_SEARCH_MAX_RANGE = 30;

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;

        int _splashType = -1;

        long _nextSearchTick = 0;
        protected override void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + GetRanIdleDealyTime();

            State = CreatureState.Moving;
        }

        int _defaultSkillRange = 10;
        long _nextMoveTick = 0;
        protected override void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / MoveSpeed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            // Patrol
            Vector2Int ranDestPos = GetRanPatrolPos();
            List<Vector2Int> path = Room.Map.FindPath(CellPos, ranDestPos, checkObjects: true);
            if (path.Count < 2 || path.Count > _chaseCellDist)
            {
                State = CreatureState.Idle;
                BroadCastVisionMove();
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
            BroadCastVisionMove();
        }

        long _coolTick = 0;
        const float skillCoolDown = 2;
        Skill _skillData = null;
        Vector2Int _targetCellPos;
        protected override void UpdateSkill()
        {
            if(_splashType != -1)
            {
                switch (_splashType)
                {
                    case Const.SKILL_BOSS_NORMAL:
                        PlayNormal();
                        break;
                    case Const.SKILL_BOSS_CROSS:
                        PlayCross();
                        break;
                    case Const.SKILL_BOSS_DIAGNAL:
                        PlayDiagnal();
                        break;
                    case Const.SKILL_BOSS_RECT:
                        PlayArea();
                        break;
                }

                return;
            }

            if (_coolTick == 0)
            {
                // 유효한 타겟인지
                if (_target == null || _target.Room != Room)
                {
                    _target = null;
                    State = CreatureState.Idle;
                    BroadCastVisionMove();
                    return;
                }

        

                // TODO random 스킬 사용
                Random rnd = new Random();
                int ranSkillIdx = rnd.Next(Const.SKILL_BOSS_NORMAL, Const.SKILL_BOSS_RECT + 1);
                DataManager.SkillDict.TryGetValue(ranSkillIdx, out _skillData);

                // 스킬이 아직 사용 가능한지
                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;

                // todo 스킬 사용 가능함 범위 10으로 임의 세팅
                bool canUseSkill = (dist <= 10);
                if (canUseSkill == false)
                {
                    State = CreatureState.Idle;
                    BroadCastVisionMove();
                    return;
                }

                // 스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = _skillData.id;
                skill.Info.CellPosX = _target.CellPos.x;
                skill.Info.CellPosY = _target.CellPos.y;

                Room.BroadCastVision(CellPos, skill);

                _splashType = _skillData.id;
                _targetCellPos = _target.CellPos;

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillCoolDown);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }

        long _splashSkillDelayTick = 0;
        long _splashSkillIntervalTick = 0;
        bool isStartWarningWait = false;
        bool isStartIntervalWait = false;
        int elapsedCount = 0;

        private void PlayNormal()
        {
            if(isStartWarningWait == false)
            {
                isStartWarningWait = true;
                _splashSkillDelayTick = Environment.TickCount64 + (long)(_skillData.splash.warningDelay * 1000);
                Console.WriteLine(Environment.TickCount64 / 1000f);
            }


            if (_splashSkillDelayTick > Environment.TickCount64)
                return;

            Console.WriteLine(Environment.TickCount64 / 1000f);

            int[] dx = { -1, 0, +1 };
            int[] dy = { -1, 0, +1 };
            foreach (int x in dx)
            {
                foreach (int y in dy)
                {
                    Vector2Int pos = new Vector2Int(_targetCellPos.x + x, _targetCellPos.y + y);

                    Player p = Room.FindPlayer(p =>
                    {
                        return p.CellPos == pos;
                    });

                    if(p != null)
                    {
                        p.OnDamaged(this, _skillData.damage + TotalAttack);
                    }
                }
            }

            _splashSkillDelayTick = 0;
            _splashSkillIntervalTick = 0;
            isStartWarningWait = false;
            isStartIntervalWait = false;
            elapsedCount = 0;
            _splashType = -1;
        }

        private void PlayCross()
        {           

            if(isStartWarningWait == false)
            {
                _splashSkillDelayTick = Environment.TickCount64 + (long)(_skillData.splash.warningDelay * 1000);
                isStartWarningWait = true;
            }

            if (Environment.TickCount64 < _splashSkillDelayTick)
                return;

            if (elapsedCount < _skillData.splash.hitCount)
            {
                if (isStartIntervalWait == false)
                {
                    _splashSkillIntervalTick = Environment.TickCount64 + (long)(_skillData.splash.hitInterval * 1000);
                    isStartIntervalWait = true;
                }

                if (Environment.TickCount64 < _splashSkillIntervalTick)
                    return;

                ++elapsedCount;

                int[] dx = { 0, 0, -1, 1 };
                int[] dy = { -1, 1, 0, 0 };

                for (int i = 0; i < 4; i++)
                {
                    Vector2Int pos = new Vector2Int(CellPos.x + dx[i] * elapsedCount, CellPos.y + dy[i] * elapsedCount);
                    Console.WriteLine($"splash {pos.x} {pos.y}");
                    Player p = Room.FindPlayer(p =>
                    {
                        return p.CellPos == pos;
                    });

                    if (p != null)
                    {
                        p.OnDamaged(this, _skillData.damage + TotalAttack);
                    }
                }

                _splashSkillIntervalTick = Environment.TickCount64 + (long)(_skillData.splash.hitInterval * 1000);
            }
            else
            {
                _splashSkillDelayTick = 0;
                _splashSkillIntervalTick = 0;
                isStartWarningWait = false;
                isStartIntervalWait = false;
                elapsedCount = 0;
                _splashType = -1;
            }
        }

        private void PlayDiagnal()
        {
            if (isStartWarningWait == false)
            {
                _splashSkillDelayTick = Environment.TickCount64 + (long)(_skillData.splash.warningDelay * 1000);
                isStartWarningWait = true;
            }

            if (Environment.TickCount64 < _splashSkillDelayTick)
                return;

            if (elapsedCount < _skillData.splash.hitCount)
            {
                if (isStartIntervalWait == false)
                {
                    _splashSkillIntervalTick = Environment.TickCount64 + (long)(_skillData.splash.hitInterval * 1000);
                    isStartIntervalWait = true;
                }

                if (Environment.TickCount64 < _splashSkillIntervalTick)
                    return;

                ++elapsedCount;

                int[] dx = { -1, +1 };
                int[] dy = { -1, +1 };

                for(int i = 0; i< 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2Int pos = new Vector2Int(CellPos.x + dx[i] * elapsedCount, CellPos.y + dy[j] * elapsedCount);
                        Console.WriteLine($"splash {pos.x} {pos.y}");
                        Player p = Room.FindPlayer(p =>
                        {
                            return p.CellPos == pos;
                        });

                        if (p != null)
                        {
                            p.OnDamaged(this, _skillData.damage + TotalAttack);
                        }
                    }
                }

                _splashSkillIntervalTick = Environment.TickCount64 + (long)(_skillData.splash.hitInterval * 1000);
            }
            else
            {
                _splashSkillDelayTick = 0;
                _splashSkillIntervalTick = 0;
                isStartWarningWait = false;
                isStartIntervalWait = false;
                elapsedCount = 0;
                _splashType = -1;
            }
        }

        private void PlayArea()
        {
            if (isStartWarningWait == false)
            {
                _splashSkillDelayTick = Environment.TickCount64 + (long)(_skillData.splash.warningDelay * 1000);
                isStartWarningWait = true;
            }

            if (Environment.TickCount64 < _splashSkillDelayTick)
                return;

            if (elapsedCount < _skillData.splash.hitCount)
            {
                if (isStartIntervalWait == false)
                {
                    _splashSkillIntervalTick = Environment.TickCount64 + (long)(_skillData.splash.hitInterval * 1000);
                    isStartIntervalWait = true;
                }

                if (Environment.TickCount64 < _splashSkillIntervalTick)
                    return;

                ++elapsedCount;

                int range = elapsedCount + 1;
                for (int x = -range; x <= range; x++)
                {
                    for (int y = -range; y <= range; y++)
                    {
                        if (Math.Abs(x) != range && Math.Abs(y) != range)
                            continue;

                        Vector2Int pos = new Vector2Int(CellPos.x + x * elapsedCount, CellPos.y + y * elapsedCount);
                        Player p = Room.FindPlayer(p =>
                        {
                            return p.CellPos == pos;
                        });

                        if (p != null)
                        {
                            p.OnDamaged(this, _skillData.damage + TotalAttack);
                        }
                    }
                }

                _splashSkillIntervalTick = Environment.TickCount64 + (long)(_skillData.splash.hitInterval * 1000);
            }
            else
            {
                _splashSkillDelayTick = 0;
                _splashSkillIntervalTick = 0;
                isStartWarningWait = false;
                isStartIntervalWait = false;
                elapsedCount = 0;
                _splashType = -1;
            }
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
