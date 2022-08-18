using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
        Enter,
        Exit
    }

    public enum MAP
    {
        EMPTY = 0,
        OBSTACLE,
        PORTAL_PREV,
        PORTAL_NEXT,
        RESPAWN,
        BOSS
    }
}
