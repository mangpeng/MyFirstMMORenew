using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
    public UI_Stat StatUI { get; private set; }
    public UI_Inventory InvenUI { get; private set; }
    public UI_Skill SkillUI { get; private set; }
    public UI_FriendList FriendUI { get; private set; }
    public ChatScroll chatUI { get; private set; }
    public UI_GameInfo gameInfo{ get; private set; }
    public UI_Boss bossUI { get; private set; }
    public Tooltip tooltip { get; private set; }

    public override void Init()
	{
        base.Init();

        StatUI = GetComponentInChildren<UI_Stat>();
        InvenUI = GetComponentInChildren<UI_Inventory>();
        SkillUI = GetComponentInChildren<UI_Skill>();
        FriendUI = GetComponentInChildren<UI_FriendList>();
        chatUI = GetComponentInChildren<ChatScroll>();
        gameInfo = GetComponentInChildren<UI_GameInfo>();
        bossUI = GetComponentInChildren<UI_Boss>();
        tooltip = GetComponentInChildren<Tooltip>();

        StatUI.gameObject.SetActive(false);
        InvenUI.gameObject.SetActive(false);
        SkillUI.gameObject.SetActive(true);
        FriendUI.gameObject.SetActive(false);
        chatUI.gameObject.SetActive(true);
        gameInfo.gameObject.SetActive(true);
        bossUI.gameObject.SetActive(false);
        tooltip.gameObject.SetActive(false);
    }

    public void ShowTooltip(Vector3 pos, string text)
    {
        tooltip.gameObject.SetActive(true);
        tooltip.transform.position = pos;
        tooltip.SetText(text);
    }

    public void HideTooltip()
    {
        tooltip.gameObject.SetActive(false);
    }
}
