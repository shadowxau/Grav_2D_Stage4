using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Com.LuisPedroFonseca.ProCamera2D;

public class GameController : MonoBehaviour {

    public static GameController gameControl;

    Scene scene;

    public enum NumberOfPlayers { One, Two, Three, Four}
    public NumberOfPlayers numberOfPlayers;

    [SerializeField]
    private GameObject playerObject;

    GameObject _currentCheckpoint;
    public GameObject currentCheckpoint { get; set; }

    RoomController _currentRoomControl;
    public RoomController currentRoomControl { get; set; }

    GameObject _activePlayer;
    public GameObject activePlayer { get; set; }

    private int _playerFacingDir;
    public int playerFacingDir { get; set; }

    private int spawnDir;
    private bool firstPlayerSpawn;

    public List<string> collectedItems;
    public List<string> openedDoors;
    public GameObject[] checkItems;


    private Vector3 _prevPlayerVelocity;
    public Vector3 prevPlayerVelocity { get; set; }

    private int _prevPlayerDirX;
    public int prevPlayerDirX { get; set; }

    public GameObject playerOneSpawn;
    public GameObject playerTwoSpawn;
    public GameObject playerThreeSpawn;
    public GameObject playerFourSpawn;

    int spawnPlayerNumber;

    void Awake()
    {
        if (gameControl == null)
        {
            DontDestroyOnLoad(gameObject);
            gameControl = this;
        }
        else if (gameControl != this)
        {
            DestroyObject(gameObject);
        }
    }

    // Use this for initialization
    void Start ()
    {
        prevPlayerDirX = 1;

        InitAllRoomData();
        //SpawnPlayerFromCheckPoint();
        SpawnAllPlayers();
	}
	
	// Update is called once per frame
	void Update ()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // find checkpoint if none are set
        if (currentCheckpoint == null && activePlayer == null)
        {
            switch (spawnDir)
            {
                // player exits from left, enters from right
                case 0:
                    SpawnPlayerFromRight();
                    break;
                // player exits from right, enters from left
                case 1:
                    SpawnPlayerFromLeft();
                    break;
                // player exits from top, enters from bottom
                case 2:
                    SpawnPlayerFromBottom();
                    break;
                // player exits from bottom, enters from top
                case 3:
                    SpawnPlayerFromTop();
                    break;
            }
        }
        else if (activePlayer == null)
        {
            SpawnPlayerFromCheckPoint();
        }
	}

    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckItemRemove();
        CheckDoorOpen();
    }

    public void CollectItemsUpdate(string itemName)
    {
        collectedItems.Add(itemName);
    }

    public void CheckItemRemove()
    {
        foreach (string j in collectedItems)
        {
            Destroy(GameObject.Find(j));
        }
    }

    public void CheckDoorOpen()
    {
        foreach(string doorName in openedDoors)
        {
            if (GameObject.Find(doorName))
            {
                DoorController door = GameObject.Find(doorName).GetComponent<DoorController>();

                door.startingPos = DoorController.StartingPos.open;
            }
        }
    }

    public void NextScene(string nextScene, int doorDirection)
    {
        // save scene data for later
        SaveRoomData(SceneManager.GetActiveScene().name, collectedItems, openedDoors);

        // reset checkpoint
        currentCheckpoint = null;
        // set spawn direction based on previous exit
        spawnDir = doorDirection;

        // remove the old player from camera targets before transtitioning to next scene
        ProCamera2D.Instance.RemoveCameraTarget(transform);

        // load the next scene
        SceneManager.LoadScene(nextScene);

        //load the roomData
        print("LoadRoomData(" + nextScene + ");");
        LoadRoomData(nextScene);
    }

    //======================================================================

    #region Player Spawning methods
    public void SpawnPlayerFromCheckPoint()
    {
        if(currentCheckpoint == null)
        {
            currentCheckpoint = GameObject.FindGameObjectWithTag("Checkpoint");
        }
        SpawnPlayer(currentCheckpoint);
    }

    public void SpawnAllPlayers()
    {
        if (numberOfPlayers == NumberOfPlayers.One)
        {
            spawnPlayerNumber = 1;
            SpawnPlayer(playerOneSpawn);
        }
        else if (numberOfPlayers == NumberOfPlayers.Two)
        {
            spawnPlayerNumber = 1;
            SpawnPlayer(playerOneSpawn);
            spawnPlayerNumber = 2;
            SpawnPlayer(playerTwoSpawn);
        }
        else if (numberOfPlayers == NumberOfPlayers.Three)
        {
            spawnPlayerNumber = 1;
            SpawnPlayer(playerOneSpawn);
            spawnPlayerNumber = 2;
            SpawnPlayer(playerTwoSpawn);
            spawnPlayerNumber = 3;
            SpawnPlayer(playerThreeSpawn);
        }
        else if (numberOfPlayers == NumberOfPlayers.Four)
        {
            spawnPlayerNumber = 1;
            SpawnPlayer(playerOneSpawn);
            spawnPlayerNumber = 2;
            SpawnPlayer(playerTwoSpawn);
            spawnPlayerNumber = 3;
            SpawnPlayer(playerThreeSpawn);
            spawnPlayerNumber = 4;
            SpawnPlayer(playerFourSpawn);
        }
    }

    public void SpawnPlayerFromLeft()
    {
        currentCheckpoint = GameObject.FindGameObjectWithTag("LeftSpawn");
        SpawnPlayer(currentCheckpoint);
    }

    public void SpawnPlayerFromRight()
    {
        currentCheckpoint = GameObject.FindGameObjectWithTag("RightSpawn");
        SpawnPlayer(currentCheckpoint);
    }

    public void SpawnPlayerFromTop()
    {
        currentCheckpoint = GameObject.FindGameObjectWithTag("TopSpawn");
        SpawnPlayer(currentCheckpoint);
    }

    public void SpawnPlayerFromBottom()
    {
        currentCheckpoint = GameObject.FindGameObjectWithTag("BottomSpawn");
        SpawnPlayer(currentCheckpoint);
    }

    void SpawnPlayer(GameObject spawnPoint)
    {
        // move the camera before spawning the player to prevent the scene changing again
        Vector2 cameraPos = spawnPoint.transform.position;
        ProCamera2D.Instance.MoveCameraInstantlyToPosition(cameraPos);

        activePlayer = null;
        activePlayer = (GameObject)Instantiate(playerObject, spawnPoint.transform.position, Quaternion.identity);
        //activePlayer.GetComponent<Player>().velocity = prevPlayerVelocity;

        Player currentPlayer = activePlayer.GetComponent<Player>();

        // automatically setup player number when spawning
        switch (spawnPlayerNumber)
        {
            case 1:
                currentPlayer.playerNumber = Player.PlayerNumber.One;
                break;
            case 2:
                currentPlayer.playerNumber = Player.PlayerNumber.Two;
                break;
            case 3:
                currentPlayer.playerNumber = Player.PlayerNumber.Three;
                break;
            case 4:
                currentPlayer.playerNumber = Player.PlayerNumber.Four;
                break;
        }

        ProCamera2D.Instance.Reset(true, true, true);
        ProCamera2D.Instance.CenterOnTargets();

        // reset room controller
        currentRoomControl = GameObject.FindGameObjectWithTag("RoomController").GetComponent<RoomController>();
    }
    #endregion

    //======================================================================

    #region Saving Room Data methods

    public void InitAllRoomData()
    {
        string dirPath = Application.persistentDataPath + "/roomDatatemp";

        if (!Directory.Exists(dirPath))
        {
            print(dirPath + " folder doesn't exist. Creating...");
        }
        else
        {
            print(dirPath + " folder exists. Deleting..."); ;
            Directory.Delete(dirPath,true);
        }

        Directory.CreateDirectory(Application.persistentDataPath + "/roomDatatemp");

    }


    public void SaveRoomData(string roomID, List<string> items, List<string> openDoors)
    {
        // access file and binary formatter
        BinaryFormatter bf = new BinaryFormatter();

        using (FileStream file = File.Open(Application.persistentDataPath + "/roomDatatemp/" + roomID + "-roomInfo.dat", FileMode.OpenOrCreate))
        {
            // create new room data container
            RoomData data = new RoomData();

            // update room data container to current room data
            data.roomID = roomID;
            data.items = items;
            data.doors = openDoors;

            // save the file
            bf.Serialize(file, data);
            file.Close();
        }

    }

    public void LoadRoomData(string roomID)
    {
        if (File.Exists(Application.persistentDataPath + "/roomDatatemp/" + roomID + "-roomInfo.dat"))
        {
            // access file and binary formatter
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/roomDatatemp/" + roomID + "-roomInfo.dat", FileMode.Open);

            RoomData data = (RoomData)bf.Deserialize(file);
            file.Close();

            // load stored data to roomController
            collectedItems.Clear();
            collectedItems = data.items;

            openedDoors.Clear();
            print("openedDoors.Clear();");
            foreach(string openDoor in openedDoors)
            {
                print("openedDoors contains " + openDoor);
            }

            foreach (string door in data.doors)
            {
                print(door + " has been added to openedDoors");
            }
            openedDoors = data.doors;
        }
        else
        {
            // clear the lists if there is no previous save to prevent previous room saving over it
            collectedItems.Clear();
            openedDoors.Clear();
        }
    }
    #endregion

    //======================================================================
}



// Container for room data save
[Serializable]
class RoomData
{
    private string _roomID;
    public string roomID { get; set; }

    private List<string> _items;
    public List<string> items { get; set; }

    private List<string> _doors;
    public List<string> doors { get; set; }
}
