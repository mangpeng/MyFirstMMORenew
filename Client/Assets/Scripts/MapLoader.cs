using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    void Start()
    {
        Managers.Map.LoadMap(1, divideCount:1);
    }
}
