using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Skill : UI_Base
{
    const float HP_POTION_COOL_TIME = 0.5f;
    const int HP_POTION_ID = 1000;

    List<GameObject> _guidList;

    public enum Images
    {
        SkillActive,
        SkillBuff,
        SkillPotion
    }


    public override void Init()
	{
        Bind<Image>(typeof(Images));

        //GetImage((int)Images.SkillActive).gameObject.BindEvent(OnClickSKillActive, Define.UIEvent.Click);
        GetImage((int)Images.SkillActive).gameObject.BindEvent(OnEnterSkillActive, Define.UIEvent.Enter);
        GetImage((int)Images.SkillActive).gameObject.BindEvent(OnExitSkillActive, Define.UIEvent.Exit);
        GetImage((int)Images.SkillActive).gameObject.GetComponent<UI_Skill_Item>().Init();

        //GetImage((int)Images.SkillBuff).gameObject.BindEvent(OnClickSKillBuff, Define.UIEvent.Click);
        GetImage((int)Images.SkillBuff).gameObject.BindEvent(OnEnterSkillBuff, Define.UIEvent.Enter);
        GetImage((int)Images.SkillBuff).gameObject.BindEvent(OnExitSkillBuff, Define.UIEvent.Exit);
        GetImage((int)Images.SkillBuff).gameObject.GetComponent<UI_Skill_Item>().Init();


        //GetImage((int)Images.SkillPotion).gameObject.BindEvent(OnClickSKillPotion, Define.UIEvent.Click);
        GetImage((int)Images.SkillPotion).gameObject.BindEvent(OnEnterSkillPotion, Define.UIEvent.Enter);
        GetImage((int)Images.SkillPotion).gameObject.BindEvent(OnExitSkillPotion, Define.UIEvent.Exit);
        GetImage((int)Images.SkillPotion).gameObject.GetComponent<UI_Skill_Item>().Init();

        //InitSkillItem();
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Q))
        //{
        //    OnClickSKillActive(null);
        //}
        //else if (Input.GetKeyDown(KeyCode.W))
        //{
        //    OnClickSKillBuff(null);
        //}
        //else if (Input.GetKeyDown(KeyCode.E))
        //{
        //    OnClickSKillPotion(null);
        //}

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnClickSKillPotion(null);
        }
    }


    public void InitSkillItem()
    {
        Skill _skillData = null;

        _skillData = Managers.Object.MyPlayer.ActiveSkillData;
        GetImage((int)Images.SkillActive).GetComponent<UI_Skill_Item>().InitData(_skillData);

        _skillData = Managers.Object.MyPlayer.BuffSkillData;
        GetImage((int)Images.SkillBuff).GetComponent<UI_Skill_Item>().InitData(_skillData);

        _skillData = null;
        GetImage((int)Images.SkillPotion).GetComponent<UI_Skill_Item>().InitData(_skillData);
    }

    public void RefreshUI()
    {
        RefreshPotion();
    }

    private void RefreshPotion()
    {
        int count = Managers.Inven.GetPotionCount();
        UI_Skill_Item skillItem = GetImage((int)Images.SkillPotion).GetComponent<UI_Skill_Item>();
        skillItem.SetText(count.ToString());
    }

    #region Event


    public void OnClickSKillActive(PointerEventData evt)
    {
        UI_Skill_Item skillItem = GetImage((int)Images.SkillActive).GetComponent<UI_Skill_Item>();
        if (!skillItem.IsCool)
        {
            skillItem.SetCool(Managers.Object.MyPlayer.ActiveSkillData.cooldown);

            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            skill.Info.SkillId = Managers.Object.MyPlayer.ActiveSkillData.id;
            Managers.Network.Send(skill);
        }
        else
        {
            Debug.Log("OnClickSKillActive is cool time");
        }
    }

    private void OnEnterSkillActive(PointerEventData evt)
    {
        Debug.Log("OnEnterSkillActive");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        Data.Skill skillData;
        Managers.Data.SkillDict.TryGetValue(Managers.Object.MyPlayer.ActiveSkillData.id, out skillData);

        string format =
@"<color=yellow>{0} </color>
{1}";
        string description = string.Format(format, skillData.name, skillData.description);
        gameSceneUI.ShowTooltip(GetImage((int)Images.SkillActive).transform.position, description);

        ShowGuild(skillData.id);
    }

    public void OnExitSkillActive(PointerEventData evt)
    {
        Debug.Log("OnExitSkillActive");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.HideTooltip();

        HideGuid();
    }


    public void OnClickSKillBuff(PointerEventData evt)
    {
        UI_Skill_Item skillItem = GetImage((int)Images.SkillBuff).GetComponent<UI_Skill_Item>();

        if (!skillItem.IsCool)
            skillItem.SetCool(Managers.Object.MyPlayer.BuffSkillData.cooldown);
        else
            Debug.Log("OnClickSKillBuff is cool time");
    }

    private void OnEnterSkillBuff(PointerEventData evt)
    {
        Debug.Log("OnEnterSkillBuff");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        Data.Skill skillData;
        Managers.Data.SkillDict.TryGetValue(Managers.Object.MyPlayer.BuffSkillData.id, out skillData);

        string format =
@"<color=yellow>{0} </color>
{1}";
        string description = string.Format(format, skillData.name, skillData.description);
        gameSceneUI.ShowTooltip(GetImage((int)Images.SkillBuff).transform.position, description);
    }

    private void OnExitSkillBuff(PointerEventData evt)
    {
        Debug.Log("OnExitSkillBuff");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.HideTooltip();
    }


    public void OnClickSKillPotion(PointerEventData evt)
    {
        UI_Skill_Item skillItem = GetImage((int)Images.SkillPotion).GetComponent<UI_Skill_Item>();

        Item potion = Managers.Inven.FindPotion();
        if(potion == null)
        {
            Debug.Log("포션이 없습니다");
        }
        else
        {
            if (!skillItem.IsCool)
            {
                // todo 임시로 포션 쿨타임 값 지정
                skillItem.SetCool(HP_POTION_COOL_TIME);
                //// 임시로 서버 값쓰지 않고 줄임
                //int nextCount = Int32.Parse(skillItem.GetText()) - 1;
                //skillItem.SetText(nextCount.ToString());

                // to 서버에 알리고 => res 오면
                // 캐릭터 체력 갱신 필요, 인벤에서 제거등 필요
                C_UsePotion usePacket = new C_UsePotion();
                usePacket.ObjectId = Managers.Object.MyPlayer.Id;

                // 포션 사용시 db ID만 사용 하고 있음
                ItemInfo info = new ItemInfo();
                info.ItemDbId = potion.ItemDbId;
                usePacket.ItemInfo = info;

                Managers.Network.Send(usePacket);
            }
            else
            {
                Debug.Log("OnClickSKillPotion is cool time");
            }
        }
    }

    private void OnEnterSkillPotion(PointerEventData evt)
    {
        Debug.Log("OnEnterSkillPotion");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

        Data.ItemData itemData;
        Managers.Data.ItemDict.TryGetValue(HP_POTION_ID, out itemData);

        string format =
@"<color=yellow>{0} </color>
{1}";
        string description = string.Format(format, itemData.name, itemData.description);
        gameSceneUI.ShowTooltip(GetImage((int)Images.SkillPotion).transform.position, description);
    }

    private void OnExitSkillPotion(PointerEventData evt)
    {
        Debug.Log("OnExitSkillPotion");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.HideTooltip();
    }


    #endregion


    private void ShowGuild(int skillId)
    {
        switch(skillId)
        {
            case Const.SKILL_ARCHER_ACTIVE:
                ShowGuidArcherActive();
                break;
            case Const.SKILL_KNIGHT_ACTIVE:
                ShowGuidKnightActive();
                break;
            default:
                break;
        }
    }

    private void ShowGuidArcherActive()
    {
        Vector3Int playerCellPos = Managers.Object.MyPlayer.CellPos;
        MoveDir playerDIr = Managers.Object.MyPlayer.Dir;

        Vector3Int add = Vector3Int.zero;
        switch(playerDIr)
        { 
            case MoveDir.Left:
                add = Vector3Int.left;
                break;
            case MoveDir.Right:
                add = Vector3Int.right;
                break;
            case MoveDir.Up:
                add = Vector3Int.up;
                break;
            case MoveDir.Down:
                add = Vector3Int.down;
                break;
        }


        List<GameObject> guidList = new List<GameObject>();
        for(int i = 0; i < 100; i++)
        {
            Vector3Int cellPos = playerCellPos + (add * i);

            if (cellPos == playerCellPos)
                continue;

            if (!Managers.Map.CanGo(cellPos))
                continue;

            Vector3 worldPos = Managers.Map.Cell2World(cellPos);
            worldPos.y -= 0.5f;
            GameObject go = Managers.Resource.Instantiate("Guid");
            go.transform.position = worldPos;
            guidList.Add(go);
        }

        _guidList = guidList;
    }

    private void ShowGuidKnightActive()
    {
        Vector3Int playerCellPos = Managers.Object.MyPlayer.CellPos;
        MoveDir playerDIr = Managers.Object.MyPlayer.Dir;

        MoveDir ignoreDir = playerDIr;
        switch (playerDIr)
        {
            case MoveDir.Left:
                ignoreDir = MoveDir.Right;
                break;
            case MoveDir.Right:
                ignoreDir = MoveDir.Left;
                break;
            case MoveDir.Up:
                ignoreDir = MoveDir.Down;
                break;
            case MoveDir.Down:
                ignoreDir = MoveDir.Up;
                break;
        }

        Vector3Int ingnoreCell = Vector3Int.zero;
        switch (ignoreDir)
        {
            case MoveDir.Left:
                ingnoreCell = Vector3Int.left;
                break;
            case MoveDir.Right:
                ingnoreCell = Vector3Int.right;
                break;
            case MoveDir.Up:
                ingnoreCell = Vector3Int.up;
                break;
            case MoveDir.Down:
                ingnoreCell = Vector3Int.down;
                break;
        }


        List<GameObject> guidList = new List<GameObject>();
        int[] dy = { -1, 0, 1 };
        int[] dx = { -1, 0, 1 };

        foreach(int y in dy)
        {
            foreach (int x in dx)
            {
                Vector3Int cellPos = new Vector3Int(playerCellPos.x + x, playerCellPos.y + y, 0);

                // 캐릭터 위치 제외
                if (cellPos == playerCellPos)
                    continue;

                // 갈수 없는 지역 제외
                if (!Managers.Map.CanGo(cellPos))
                    continue;

                // 후방 제외
                if(cellPos == (playerCellPos + ingnoreCell))
                    continue;


                Vector3 worldPos = Managers.Map.Cell2World(cellPos);
                worldPos.y -= 0.5f;
                GameObject go = Managers.Resource.Instantiate("Guid");
                go.transform.position = worldPos;
                guidList.Add(go);

            }
        }

        _guidList = guidList;
    }

    public void HideGuid()
    {
        if (_guidList != null && _guidList.Count > 0)
        {
            foreach (GameObject go in _guidList)
            {
                Managers.Resource.DestroyImmediate(go);
            }
        }

        _guidList = null;
    }
}
