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
    const int POTION_COOL_TIME = 3;

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
        GetImage((int)Images.SkillBuff).gameObject.BindEvent(OnClickSKillBuff);
        GetImage((int)Images.SkillPotion).gameObject.BindEvent(OnClickSKillPotion);
        GetImage((int)Images.SkillPotion).gameObject.GetComponent<UI_Skill_Item>().Init();
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            OnClickSKillActive(null);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            OnClickSKillBuff(null);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            OnClickSKillPotion(null);
        }
    }


    private void OnClickSKillActive(PointerEventData obj)
    {

    }

    private void OnClickSKillBuff(PointerEventData obj)
    {

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
