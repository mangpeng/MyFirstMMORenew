using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	
	public static GameObjectType GetObjectTypeById(int id)
	{
		int type = (id >> 24) & 0x7F;
		return (GameObjectType)type;
	}

	public void Add(ObjectInfo info, bool myPlayer = false)
	{
		if (MyPlayer != null && MyPlayer.Id == info.ObjectId)
			return;
		if (_objects.ContainsKey(info.ObjectId))
			return;

		GameObjectType objectType = GetObjectTypeById(info.ObjectId);
		if (objectType == GameObjectType.Player)
		{
			if (myPlayer)
			{
				if((ClassType)info.ClassType == ClassType.Archer)
                {
					MyPlayerController pc = AddMyPlayer(info, "Creature/MyPlayer_Archer");

                    Skill skillData = null;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_ARROW, out skillData);
                    pc.NormalSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_ARCHER_ACTIVE, out skillData);
                    pc.ActiveSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_ARCHER_BUFF, out skillData);
                    pc.BuffSkillData = skillData;
                }
                else if ((ClassType)info.ClassType == ClassType.Knight)
                {
                    MyPlayerController pc = AddMyPlayer(info, "Creature/MyPlayer_Knight");


                    Skill skillData = null;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_FIST, out skillData);
                    pc.NormalSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_KNIGHT_ACTIVE, out skillData);
                    pc.ActiveSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_KNIGHT_BUFF, out skillData);
                    pc.BuffSkillData = skillData;
                }

                UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
                if (gameSceneUI != null)
                {
					gameSceneUI.SkillUI.InitSkillItem();
					gameSceneUI.SkillUI.RefreshUI();
                }
            }
			else
			{
                if ((ClassType)info.ClassType == ClassType.Archer)
                {
					PlayerController pc = AddPlayer(info, "Creature/Player_Archer");

                    Skill skillData = null;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_ARROW, out skillData);
                    pc.NormalSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_ARCHER_ACTIVE, out skillData);
                    pc.ActiveSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_ARCHER_BUFF, out skillData);
                    pc.BuffSkillData = skillData;
                }
                else if ((ClassType)info.ClassType == ClassType.Knight)
                {
					PlayerController pc = AddPlayer(info, "Creature/Player_Knight");

					Skill skillData = null;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_FIST, out skillData);
                    pc.NormalSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_KNIGHT_ACTIVE, out skillData);
					pc.ActiveSkillData = skillData;
                    Managers.Data.SkillDict.TryGetValue(Const.SKILL_KNIGHT_BUFF	, out skillData);
					pc.BuffSkillData = skillData;
                }
			}
		}
		else if (objectType == GameObjectType.Monster)
		{
			MonsterData monsterData;
			Managers.Data.MonsterDict.TryGetValue(info.MonsterTemplateId, out monsterData);

			Debug.Assert(monsterData != null, $"유효하지 않은 몬스터 데이터 입니다. {info.MonsterTemplateId}");

			GameObject go = Managers.Resource.Instantiate(monsterData.path);
			go.name = info.Name;
			_objects.Add(info.ObjectId, go);

			MonsterController mc = go.GetComponent<MonsterController>();
			mc.Id = info.ObjectId;
			mc.PosInfo = info.PosInfo;
			mc.Stat = info.StatInfo;
			mc.SyncPos();

            Skill skillData = null;
            Managers.Data.SkillDict.TryGetValue(Const.SKILL_FIST, out skillData);
            mc.NormalSkillData = skillData;
        }
        else if (objectType == GameObjectType.Boss)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Boss");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            BossController bc = go.GetComponent<BossController>();
            bc.Id = info.ObjectId;
            bc.PosInfo = info.PosInfo;
            bc.Stat = info.StatInfo;
            bc.SyncPos();

            Skill skillData = null;
            Managers.Data.SkillDict.TryGetValue(Const.SKILL_BOSS_NORMAL, out skillData);
            bc.NormalSkillData = skillData;
            Managers.Data.SkillDict.TryGetValue(Const.SKILL_BOSS_CROSS, out skillData);
            bc.CrossSkillData = skillData;
            Managers.Data.SkillDict.TryGetValue(Const.SKILL_BOSS_DIAGNAL, out skillData);
            bc.DiagnalSkillData = skillData;
            Managers.Data.SkillDict.TryGetValue(Const.SKILL_BOSS_RECT, out skillData);
            bc.RectSkillData = skillData;
        }
        else if (objectType == GameObjectType.Projectile)
		{
			GameObject go = Managers.Resource.Instantiate("Creature/Arrow");

			go.name = "Arrow";
			_objects.Add(info.ObjectId, go);

			ArrowController ac = go.GetComponent<ArrowController>();
			ac.PosInfo = info.PosInfo;
			ac.Stat = info.StatInfo;
			ac.SyncPos();
		}
	}

	private MyPlayerController AddMyPlayer(ObjectInfo info, string prefabPath)
    {
        GameObject go = Managers.Resource.Instantiate(prefabPath);
        go.name = info.Name;
        _objects.Add(info.ObjectId, go);

        MyPlayer = go.GetComponent<MyPlayerController>();
        MyPlayer.ClassType = (ClassType)info.ClassType;
        MyPlayer.Id = info.ObjectId;

        MyPlayer.PosInfo = info.PosInfo;
        MyPlayer.Stat.MergeFrom(info.StatInfo);
        MyPlayer.SyncPos();

		return MyPlayer;
    }

    private PlayerController AddPlayer(ObjectInfo info, string prefabPath)
	{
        GameObject go = Managers.Resource.Instantiate(prefabPath);
        go.name = info.Name;
        _objects.Add(info.ObjectId, go);

        PlayerController pc = go.GetComponent<PlayerController>();
        pc.ClassType = (ClassType)info.ClassType;
        pc.Id = info.ObjectId;


        Skill skillData = null;
        Managers.Data.SkillDict.TryGetValue(3, out skillData);
        pc.ActiveSkillData = skillData;
        Managers.Data.SkillDict.TryGetValue(4, out skillData);
        pc.BuffSkillData = skillData;

        pc.PosInfo = info.PosInfo;
        pc.Stat.MergeFrom(info.StatInfo);
        pc.SyncPos();

		return pc;
    }

    public void Remove(int id)
	{
		if (MyPlayer != null && MyPlayer.Id == id)
			return;
		if (_objects.ContainsKey(id) == false)
			return;

		GameObject go = FindById(id);
		if (go == null)
			return;

		_objects.Remove(id);
		Managers.Resource.Destroy(go);
	}

	public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
	}

	public GameObject FindCreature(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);
		_objects.Clear();
		MyPlayer = null;
	}
}
