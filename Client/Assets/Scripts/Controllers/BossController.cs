using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BossController : CreatureController
{
    public enum SplashSkillType
    {
        None = 0,
        Normal,
        Splash_Cross,
        Splash_Diagnal,
        Splash_Area,
    }

    const float IDLE_LENGTH = 18f;
    const float ATTACK_LENGTH = 18f;

    const float IDLE_SPEED_SCALE = 3f;
    const float ATTACK_NORMAL_SPEED_SCALE = 7f;
    const float ATTACK_SPLASH_SPEED_SCALE = 5f;
    const float ATTACK_SUMMON_SPEED_SCALE = 3f;

    private Coroutine _coSkill;
    private int _nextSkillType;

    private SpriteRenderer _warningSrr;
    private GameObject _skillParticle;

    private Vector2Int _skillCellPos;

    public Skill NormalSkillData = null;
    public Skill CrossSkillData = null;
    public Skill DiagnalSkillData = null;
    public Skill RectSkillData = null;

    public override CreatureState State
    {
        get { return PosInfo.State; }
        set
        {
            PosInfo.State = value;
            UpdateAnimation();
            _updated = true;
        }
    }


    protected override void Init()
	{
		base.Init();

		
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		if(!gameSceneUI.bossUI.gameObject.activeSelf)
        {
            gameSceneUI.bossUI.gameObject.SetActive(true);
            gameSceneUI.bossUI.InitHpUI(Hp, Stat.MaxHp);
        }


        _warningSrr = Managers.Resource.Load<SpriteRenderer>("Prefabs/Warning");
        _skillParticle = Managers.Resource.Load<GameObject>("Prefabs/Particle/SkillSplashParticle");
    }


    protected override void UpdateAnimation()
    {
        if (_animator == null)
            return;

        if (State == CreatureState.Idle)
            _animator.Play("IDLE");
        else if (State == CreatureState.Moving)
            _animator.Play("IDLE");
        else if (State == CreatureState.Skill)
        {
            _animator.Play("ATTACK");
            PlaySkill(_nextSkillType);
        }
    }   

    protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

	public override void OnDamaged()
	{
        Debug.Log("defeat!!!");
		//Managers.Object.Remove(Id);
		//Managers.Resource.Destroy(gameObject);
		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.bossUI.ChangeHp(Hp);
	}

	public override void UseSkill(SkillInfo info)
	{
        switch(info.SkillId)
        {
            case Const.SKILL_BOSS_NORMAL:
            case Const.SKILL_BOSS_CROSS:
            case Const.SKILL_BOSS_DIAGNAL:
            case Const.SKILL_BOSS_RECT:
                _nextSkillType = info.SkillId;
                _skillCellPos = new Vector2Int(info.CellPosX, info.CellPosY);
                State = CreatureState.Skill;
                Debug.Log($"use skill! {info.SkillId}");
                break;
            default:
                Debug.Log($"유효하지 않은 스킬 id {info.SkillId}");
                State = CreatureState.Idle;
                break;
        }
	}


    private void PlaySkill(int nextSkillType)
    {
        switch (nextSkillType)
        {
            case Const.SKILL_BOSS_NORMAL:
                UseSkillNormal();
                break;
            case Const.SKILL_BOSS_CROSS:
                UseSkillCross();
                break;
            case Const.SKILL_BOSS_DIAGNAL:
                UseSkillDiagnal();
                break;
            case Const.SKILL_BOSS_RECT:
                UseSkillArea();
                break;
            default:
                Debug.Log($"유요하지 않은 스킬 타입 {nextSkillType}");
                break;
        }

        //switch (nextSkillType)
        //{
        //    case SplashSkillType.Normal:
        //        UseSkillNormal();
        //        break;
        //    case SplashSkillType.Splash_Cross:
        //        UseSkillDiagnal();
        //        break;
        //    case SplashSkillType.Splash_Diagnal:
        //        UseSkillCross();
        //        break;
        //    case SplashSkillType.Splash_Area:
        //        UseSkillArea();
        //        break;
        //    default:
        //        Debug.Log("유요하지 않은 스킬 타입");
        //        break;
        //}
    }

    Coroutine _coPlayAttack = null;
    private List<GameObject> _tempObjs = new List<GameObject>();

    private void UseSkillNormal()
    {
        Debug.Log("UseSkillNormal");
        PlayNormal();
    }

    private void UseSkillDiagnal()
    {
        Debug.Log("UseSkillDiagnal");
        StartCoroutine(CDominoDiagonal(6, 0.2f));
    }

    private void UseSkillCross()
    {
        Debug.Log("UseSkillCross");
        StartCoroutine(CDominoVerHor(6, 0.2f));
    }

    private void UseSkillArea()
    {
        Debug.Log("UseSkillArea");
        StartCoroutine(CDominoRect(3, 0.2f));
    }



    private void PlayNormal()
    {
        Vector3 skillWorldPos = Managers.Map.Cell2World(new Vector3Int(_skillCellPos.x, _skillCellPos.y, 0));

        float[] dx = { -1, 0, +1 };
        float[] dy = { -1, 0, +1 };
        foreach (float x in dx)
        {
            foreach (float y in dy)
            {
                Vector3 pos = new Vector3(skillWorldPos.x + x, skillWorldPos.y + y, 0);
                Vector3 temp = pos;
                ShowWarning(
                    pos,
                    10,
                    0.1f,
                    0f,
                    () =>
                    {
                        GameObject particle = GameObject.Instantiate(_skillParticle);
                        particle.transform.position = temp;
                        GameObject.Destroy(particle, 1f);

                        State = CreatureState.Idle;
                    });
            }
        }
    }

    IEnumerator CDominoVerHor(int count, float interval)
    {
        Vector3 skillWorldPos = Managers.Map.Cell2World(CellPos);

        int elapsed = 0;

        while (elapsed < count)
        {
            elapsed += 1;

            float[] dx = { 0, 0, -1, 1 };
            float[] dy = { -1, 1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(skillWorldPos.x + dx[i] * elapsed, skillWorldPos.y + dy[i] * elapsed, 0);
                Vector3 temp = pos;
                ShowWarning(
                    pos,
                    2,
                    0.1f,
                    1f,
                    () =>
                    {
                        GameObject particle = GameObject.Instantiate(_skillParticle);
                        particle.transform.position = temp;
                        GameObject.Destroy(particle, 1f);

                        State = CreatureState.Idle;
                    });
            }

            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator CDominoDiagonal(int count, float interval)
    {
        Vector3 skillWorldPos = Managers.Map.Cell2World(CellPos);

        int elapsed = 0;

        while (elapsed < count)
        {
            elapsed += 1;

            float[] dx = { -1, +1 };
            float[] dy = { -1, +1 };
            foreach (float x in dx)
            {
                foreach (float y in dy)
                {
                    Vector3 pos = new Vector3(skillWorldPos.x + x * elapsed, skillWorldPos.y + y * elapsed, 0);
                    Vector3 temp = pos;
                    ShowWarning(
                        pos,
                        2,
                        0.1f,
                        1f,
                        () =>
                        {
                            GameObject particle = GameObject.Instantiate(_skillParticle);
                            particle.transform.position = temp;
                            GameObject.Destroy(particle, 1f);

                            State = CreatureState.Idle;
                        });
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator CDominoRect(int count, float interval)
    {
        Vector3 skillWorldPos = Managers.Map.Cell2World(CellPos);


        int elapsed = 0;

        while (elapsed < count)
        {
            elapsed += 1;

            int range = elapsed + 1;
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    if (Mathf.Abs(x) != range && Mathf.Abs(y) != range)
                        continue;

                    Vector3 pos = new Vector3(skillWorldPos.x + x, skillWorldPos.y + y, 0);
                    Vector3 temp = pos;
                    ShowWarning(
                        pos,
                        2,
                        0.1f,
                        1f,
                        () =>
                        {
                            GameObject particle = GameObject.Instantiate(_skillParticle);
                            _tempObjs.Add(particle);
                            particle.transform.position = temp;
                            _tempObjs.Remove(particle);
                            GameObject.Destroy(particle, 1f);
                        });
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }


    public void ShowWarning(Vector3 pos, int blinkCount, float blinkInterval, float afterDelay = 0f, Action after = null)
    {
        if(_warningSrr == null)
            _warningSrr = Managers.Resource.Load<SpriteRenderer>("Prefabs/Warning");

        SpriteRenderer warningSrr = SpriteRenderer.Instantiate(_warningSrr);
        warningSrr.transform.position = pos;
        _tempObjs.Add(warningSrr.gameObject);

        StartCoroutine(CBlinck(warningSrr, blinkCount, blinkInterval, afterDelay, () =>
        {
            _tempObjs.Remove(warningSrr.gameObject);
            GameObject.DestroyImmediate(warningSrr.gameObject);
            after?.Invoke();
        }));

    }

    IEnumerator CBlinck(SpriteRenderer spriteRr, int count, float interval = 0.1f, float afterDelay = 0f, Action after = null)
    {
        int elapsed = 0;

        Color c = spriteRr.color;
        bool flag = true;
        float initAlpha = c.a;

        while (elapsed < count)
        {
            elapsed += 1;

            if (flag)
            {
                flag = false;
                c.a = 0f;
            }
            else
            {
                flag = true;
                c.a = initAlpha;
            }
            spriteRr.color = c;

            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(afterDelay);

        c.a = 0;
        spriteRr.color = c;

        after?.Invoke();
    }

    private void OnDestroy()
    {
        foreach(var temp in _tempObjs)
        {
            GameObject.DestroyImmediate(temp.gameObject);
        }
    }
}
