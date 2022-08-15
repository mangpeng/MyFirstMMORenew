using Google.Protobuf.Protocol;
using System;
using System.Collections;
using UnityEngine;
using static Define;

public class BossController2 : CreatureController
{
	Coroutine _coSkill;

	protected override void Init()
	{
		//base.Init();
	}

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

	public override void OnDamaged()
	{
		//Managers.Object.Remove(Id);
		//Managers.Resource.Destroy(gameObject);
	}

	public override void UseSkill(int skillId)
	{
		if (skillId == 1)
		{
			State = CreatureState.Skill;
		}
	}



	const float IDLE_LENGTH = 18f;
	const float ATTACK_LENGTH = 18f;

	const float IDLE_SPEED_SCALE = 3f;
	const float ATTACK_NORMAL_SPEED_SCALE = 7f;
	const float ATTACK_SPLASH_SPEED_SCALE = 5f;
	const float ATTACK_SUMMON_SPEED_SCALE = 3f;

	private Animator _anim;

    [SerializeField]
	private SpriteRenderer _warningSrr;
    [SerializeField]
    private GameObject _normalSkillParticle;

    [SerializeField]
	private Transform[] _targets;

    private void Awake()
    {
        _anim = GetComponent<Animator>();

		_anim.Play("IDLE");
		_anim.speed = IDLE_SPEED_SCALE;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) // 평타
        {
            UseSkillQ();
        }
        else if (Input.GetKeyDown(KeyCode.W)) // 광역기
        {
            UseSkillW();
        }
        else if (Input.GetKeyDown(KeyCode.E)) // 정령 소환
        {
            UseSkillE();
        }
        else if (Input.GetKeyDown(KeyCode.R)) // 정령 소환
        {
            UseSkillR();
        }
    }

	Coroutine _coPlayAttack = null;
	private void UseSkillQ()
    {
		ResetAttack();
		_coPlayAttack = StartCoroutine(CPlayAttackAnim(ATTACK_NORMAL_SPEED_SCALE));

		Transform target = GetRanTarget();
		float[] dx = { -1, 0, +1 };
		float[] dy = { -1, 0, +1 };
		foreach(float x in dx)
        {
			foreach(float y in dy)
            {
				Vector3 pos = new Vector3(target.position.x + x, target.position.y + y, 0);
				Vector3 temp = pos;
                ShowWarning(
					pos, 
					10,
					0.1f,
					0f,
					() =>
                    {
                        GameObject particle = GameObject.Instantiate(_normalSkillParticle);
                        particle.transform.position = temp;
                        GameObject.Destroy(particle, 1f);
                    });
            }
        }
    }

    private void UseSkillW()
    {
		ResetAttack();
		_coPlayAttack = StartCoroutine(CPlayAttackAnim(ATTACK_SPLASH_SPEED_SCALE));
		StartCoroutine(CDominoDiagonal(6, 0.2f));
    }

    private void UseSkillE()
    {
        ResetAttack();
        _coPlayAttack = StartCoroutine(CPlayAttackAnim(ATTACK_SPLASH_SPEED_SCALE));
        StartCoroutine(CDominoVerHor(6, 0.2f));
    }

    private void UseSkillR()
    {
        ResetAttack();
        _coPlayAttack = StartCoroutine(CPlayAttackAnim(ATTACK_SPLASH_SPEED_SCALE));
        StartCoroutine(CDominoRect(3, 0.2f));
    }



    IEnumerator CPlayAttackAnim(float speed, Action after = null)
    {
		_anim.speed = speed;
		_anim.Play("ATTACK", -1, 0f);

		float delay = ATTACK_LENGTH / speed;
		yield return new WaitForSeconds(delay);

		after?.Invoke();

		_anim.speed = IDLE_SPEED_SCALE;
		_coPlayAttack = null;

	}

    private void ResetAttack()
    {
		if (_coPlayAttack != null)
			StopCoroutine(_coPlayAttack);
	}

	public void ShowWarning(Vector3 pos, int blinkCount, float blinkInterval, float afterDelay = 0f, Action after = null)
    {
		SpriteRenderer warningSrr = SpriteRenderer.Instantiate(_warningSrr);
		warningSrr.transform.position = pos;

		StartCoroutine(CBlinck(warningSrr, blinkCount, blinkInterval,afterDelay, () =>
		{
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

			if(flag)
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

	IEnumerator CDominoDiagonal(int count, float interval)
    {
		int elapsed = 0;

		while(elapsed < count)
        {
			elapsed += 1;

			Transform target = GetRanTarget();
            float[] dx = { -1, +1 };
            float[] dy = { -1, +1 };
            foreach (float x in dx)
            {
                foreach (float y in dy)
                {
                    Vector3 pos = new Vector3(transform.position.x + x * elapsed, transform.position.y + y * elapsed, 0);
                    Vector3 temp = pos;
                    ShowWarning(
						pos, 
						2,
						0.1f,
						1f,
						() =>
						{
							GameObject particle = GameObject.Instantiate(_normalSkillParticle);
							particle.transform.position = temp;
							GameObject.Destroy(particle, 1f);
						});
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }


    IEnumerator CDominoVerHor(int count, float interval)
    {
        int elapsed = 0;

        while (elapsed < count)
        {
            elapsed += 1;

            float[] dx = { 0, 0, -1, 1 };
            float[] dy = { -1, 1, 0, 0 };

			for(int i =0; i<4; i++)
            {
                Vector3 pos = new Vector3(transform.position.x + dx[i] * elapsed, transform.position.y + dy[i] * elapsed, 0);
                Vector3 temp = pos;
                ShowWarning(
                    pos,
                    2,
                    0.1f,
					1f,
                    () =>
                    {
                        GameObject particle = GameObject.Instantiate(_normalSkillParticle);
                        particle.transform.position = temp;
                        GameObject.Destroy(particle, 1f);
                    });
            }

            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator CDominoRect(int count, float interval)
    {
        int elapsed = 0;

        while (elapsed < count)
        {
            elapsed += 1;

            int range = elapsed + 1;
            for(int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    if (Mathf.Abs(x) != range && Mathf.Abs(y) != range)
                        continue;

                    Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
                    Vector3 temp = pos;
                    ShowWarning(
                        pos,
                        2,
                        0.1f,
                        1f,
                        () =>
                        {
                            GameObject particle = GameObject.Instantiate(_normalSkillParticle);
                            particle.transform.position = temp;
                            GameObject.Destroy(particle, 1f);
                        });
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }

    public Transform GetRanTarget()
    {
		int ranIdx = UnityEngine.Random.Range(0, _targets.Length);
		return _targets[ranIdx];
    }
}
