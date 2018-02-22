using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTriggerScript : MonoBehaviour {

    public Object moveToThisScene;

    public enum DoorDirection { left, right, top, bottom};
    public DoorDirection doorDirection;

    [HideInInspector]
    public int doorDir;

    private void Start()
    {
        doorDir = (int)doorDirection;
    }

}
