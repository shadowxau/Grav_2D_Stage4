using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour {

    //public string roomID;

    [SerializeField]
    //public GameObject[] items;
    public GameObject[] doors;

    public List<GameObject> items;

    public List<string> collectedItems;

    Object thisRoom;

    public string moveSceneRightString;
    public string moveSceneLeftString;
    public string moveSceneUpString;
    public string moveSceneDownString;

    // Use this for initialization
    void Start () {
        //items = GameObject.FindGameObjectsWithTag("Item");

        items.AddRange(GameObject.FindGameObjectsWithTag("Item"));
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void CollectItemsUpdate(string itemName)
    {
        collectedItems.Add(itemName);
    }


}
