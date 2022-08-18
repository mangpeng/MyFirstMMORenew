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
    const float POTION_COOL_TIME = 0.5f;

    public enum Images
    {
        SkillActive,
        SkillBuff,
        SkillPotion
    }


    public override void Init()
	{
        Bind<Image>(typeof(Images));

        GetImage((int)Images.SkillActive).gameObject.BindEvent(OnClickSKillActive);
        GetImage((int)Images.SkillActive).gameObject.GetComponent<UI_Skill_Item>().Init();

        GetImage((int)Images.SkillBuff).gameObject.BindEvent(OnClickSKillBuff);
        GetImage((int)Images.SkillBuff).gameObject.GetComponent<UI_Skill_Item>().Init();

        GetImage((int)Images.SkillPotion).gameObject.BindEvent(OnClickSKillPotion);
        GetImage((int)Images.SkillPotion).gameObject.GetComponent<UI_Skill_Item>().Init();

        //InitSkillItem();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            OnClickSKillActive(null);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            OnClickSKillBuff(null);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            OnClickSKillPotion(null);
        }
    }


    private void OnClickSKillActive(PointerEventData obj)
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

    private void OnClickSKillBuff(PointerEventData obj)
    {
        UI_Skill_Item skillItem = GetImage((int)Images.SkillBuff).GetComponent<UI_Skill_Item>();

        if (!skillItem.IsCool)
            skillItem.SetCool(Managers.Object.MyPlayer.BuffSkillData.cooldown);
        else
            Debug.Log("OnClickSKillBuff is cool time");
    }

    private void OnClickSKillPotion(PointerEventData obj)
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
                skillItem.SetCool(POTION_COOL_TIME);
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


}
