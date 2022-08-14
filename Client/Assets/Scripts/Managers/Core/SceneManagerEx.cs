using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }
    private UnityAction<Scene, LoadSceneMode> _onLoaded = null;

	public void LoadScene(Define.Scene type, UnityAction<Scene, LoadSceneMode> onLoaded = null)
    {
        Managers.Clear();

        SceneManager.LoadScene(GetSceneName(type));
        if(_onLoaded != null)
            SceneManager.sceneLoaded -= _onLoaded;

        _onLoaded = onLoaded;
        SceneManager.sceneLoaded += _onLoaded;
    }

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
