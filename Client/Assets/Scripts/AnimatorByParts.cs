using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorByParts : MonoBehaviour
{
    public enum State
    {
        IDLE = 0,
        WALK,
        RUN,
        ATTACK,
        END
    };

    Transform[] _roots;

    State _state;
    MoveDir _dir;

    Transform _curActiveRoot = null;
    GameObject _curDirObject = null;
    
    private void Awake()
    {
        _roots = new Transform[transform.childCount];
        for(int i =0; i< transform.childCount; i++)
            _roots[i] = transform.GetChild(i);

        Debug.Assert(_roots.Length == (int)State.END, "Animator Parts 정보가 잘못 됐습니다.");
    }

    private void Start()
    {
        SetSate(State.IDLE, MoveDir.Down);
    }

    public void SetSate(State state, MoveDir dir)
    {
        _state = state;
        _dir = dir;

        if (_curDirObject != null)
            _curDirObject.SetActive(false);
        if (_curActiveRoot != null)
            _curActiveRoot.gameObject.SetActive(false);

        _curActiveRoot = _roots[(int)state];
        _curDirObject = _curActiveRoot.GetChild((int)dir).gameObject;

        _curActiveRoot.gameObject.SetActive(true);
        _curDirObject.SetActive(true);
    }
}
