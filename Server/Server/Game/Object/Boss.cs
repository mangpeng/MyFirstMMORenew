using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game.Object
{
    public enum SkillType
    {
        NONE,
        FIST,
        SPLASH
    }

    public class Boss : Monster
    {
        private SkillType _skillType = SkillType.NONE;

        public Boss()
        {
            ObjectType = GameObjectType.Boss;
        }

        public override void Init(int templateId)
        {
            base.Init(templateId);
        }

        IJob _job;
        public override void Update()
        {
            switch (State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
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
                _job = Room.PushAfter(200, Update);
        }

        Player _target;

        int _searchCellDist = 5;

        int _splashSkillCool = 10000; // 10초
        long _lastSplashSkill;

        long _nextSearchTick = 0;
        protected override void UpdateIdle()
        {
            // 1초 후에
            if (_nextSearchTick > Environment.TickCount64)
                return;

            // 1초마다 idle에서 체크
            _nextSearchTick = Environment.TickCount64 + 1000;

            // skill : 내려 찍기 가능한지 체크
            //{
            //    if(Environment.TickCount64 > _lastSplashSkill + _splashSkillCool)
            //    {
            //        _skillType = SkillType.SPLASH;
            //        State = CreatureState.Skill;
            //        _lastSplashSkill = Environment.TickCount64;
            //        return;
            //    }
            //    else
            //        _lastSplashSkill = Environment.TickCount64;
            //}

            // skill : 사용 가능한 스킬 없다면 평타 사용
            {
                _skillType = SkillType.FIST;
                State = CreatureState.Skill;
            }
        }

        long _coolTick = 0;
        protected override void UpdateSkill()
        {
            switch(_skillType)
            {
                case SkillType.FIST:
                    UseFistSkill();
                    break;
                case SkillType.SPLASH:
                    UseSplashSkill();
                    break;
            }
        }

        long _fistCoolTick = 0;
        public void UseFistSkill()
        {
            if (_fistCoolTick == 0)
            { 
                Player target = Room.FindClosestPlayer(CellPos, _searchCellDist);
                _target = target;

                // 유효한 타겟인지
                if (_target == null || _target.Room != Room)
                {
                    State = CreatureState.Idle;
                    return;
                }

                // 데미지 판정
                _target.OnDamaged(this, 100);

                // 스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = 2;
                Room.BroadCastVision(CellPos, skill);

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * 2);
                _fistCoolTick = Environment.TickCount64 + coolTick;
            }

            if (_fistCoolTick > Environment.TickCount64)
                return;

            _fistCoolTick = 0;
        }

        public void UseSplashSkill()
        {

        }

        public override void OnDead(GameObject attacker)
        {
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
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
