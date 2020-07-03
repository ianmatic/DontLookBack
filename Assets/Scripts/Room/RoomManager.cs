using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{

    private List<GameObject> roomList;
    private List<GameObject> enemyList;
    private List<GameObject> enemyRoomList;
    private List<GameObject> ladderList;
    private List<GameObject> stairList;
    private List<GameObject> keyList;
    private List<GameObject> doorList;
    private List<GameObject> hidingSpotList;
    private GameObject currentPlayerRoom;
    private GameObject oldPlayerRoom;
    private GameObject player;
    private GameObject currentStair;
    private GameObject currentLadder;
    private GameObject currentHidingSpot;
    public string nameOfCurrentLevel;

    // Start is called before the first frame update
    void Awake()
    {
        roomList = new List<GameObject>();
        roomList.AddRange(GameObject.FindGameObjectsWithTag("Room"));
        enemyList = new List<GameObject>();
        enemyList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        enemyRoomList = new List<GameObject>();
        enemyRoomList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        enemyRoomList[0] = FindCurrentEnemyRoom();
        ladderList = new List<GameObject>();
        ladderList.AddRange(GameObject.FindGameObjectsWithTag("Ladder"));
        stairList = new List<GameObject>();
        stairList.AddRange(GameObject.FindGameObjectsWithTag("RightStair"));
        stairList.AddRange(GameObject.FindGameObjectsWithTag("LeftStair"));
        keyList = new List<GameObject>();
        keyList.AddRange(GameObject.FindGameObjectsWithTag("Key"));
        doorList = new List<GameObject>();
        doorList.AddRange(GameObject.FindGameObjectsWithTag("Door"));
        hidingSpotList = new List<GameObject>();
        hidingSpotList.AddRange(GameObject.FindGameObjectsWithTag("HidingSpot"));
        player = GameObject.FindGameObjectWithTag("Player");
        nameOfCurrentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        oldPlayerRoom = currentPlayerRoom;
        currentPlayerRoom = FindCurrentPlayerRoom();
        if (oldPlayerRoom != currentPlayerRoom)
        {
            TransitionToRoom(currentPlayerRoom);
        }

        enemyRoomList[0] = FindCurrentEnemyRoom();
    }

    /// <summary>
    /// Function to update current enemy position, written in this way so as to allow for multiple killers later in development while still being handled in the manager. As of now will be called from the backwall's box collider
    /// </summary>
    /// <param name="enemyCollision">Enemy object's collision</param>
    /// <param name="roomEntered">Room that the enemy entered</param>
    public void UpdateEnemyRoom(Collider enemyCollision, GameObject roomEntered)
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i] == enemyCollision.gameObject)
            {
                enemyRoomList[i] = roomEntered;
                Debug.Log(enemyList[i].name + "'s room is now " + roomEntered.name);
                break;
            }
        }
    }

    /// <summary>
    /// Disables the old room and enables the target room and sets it as the current
    /// </summary>
    /// <param name="room"></param>
    private void TransitionToRoom(GameObject room)
    {
        // disable light and camera in old room, if it is set (won't be for first frame)
        if (oldPlayerRoom)
        {
            foreach (Transform child in oldPlayerRoom.transform)
            {
                if (child.tag == "MainCamera" || child.name == "Spot Light")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }


        // turn on light and camera in new room
        foreach (Transform child in room.transform)
        {
            if (child.tag == "MainCamera" || child.name == "Spot Light")
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// For now simply loops through each room, may optimize later if needed
    /// </summary>
    /// <returns></returns>
    private GameObject FindCurrentPlayerRoom()
    {
        foreach (GameObject room in roomList)
        {
            if (room.GetComponent<BoxCollider>().bounds.Contains(player.transform.position))
            {
                return room;
            }
        }

        return null;
    }

    private GameObject FindCurrentEnemyRoom()
    {
        foreach (GameObject room in roomList)
        {
            if (room.GetComponent<BoxCollider>().bounds.Contains(GameObject.FindGameObjectWithTag("Enemy").GetComponent<Collider>().bounds.center))
            {
                return room;
            }
        }
        return null;
    }

    /// <summary>
    /// Builds a house for the enemy AI to navigate through
    /// </summary>
    /// <returns></returns>
    public List<List<RoomProperties>> BuildHouse()
    {
        List<List<RoomProperties>> house = new List<List<RoomProperties>>();

        // loop through all of the rooms in the level, and sort them into a house structure
        foreach (GameObject room in roomList)
        {
            RoomProperties roomProperties = room.GetComponent<RoomProperties>();


            // foreach "floor" of the house present, add a floor to Rooms
            while (roomProperties.floor >= house.Count)
            {
                house.Add(new List<RoomProperties>());
            }

            // foreach "floor" of the house, add rooms to the "floor"
            while (roomProperties.room >= house[roomProperties.floor].Count)
            {
                house[roomProperties.floor].Add(new RoomProperties());
            }

            // set the roomProperties within the House structure
            house[roomProperties.floor][roomProperties.room] = roomProperties;
        }

        return house;
    }

    /// <summary>
    /// Gets the adjacent rooms, corners included
    /// </summary>
    /// <param name="room"></param>
    /// <param name="house"></param>
    /// <returns></returns>
    public List<GameObject> GetAdjacentRooms(GameObject room, List<List<RoomProperties>> house)
    {
        // first get the floor/index of room
        int floor = room.GetComponent<RoomProperties>().floor;
        int roomIndex = room.GetComponent<RoomProperties>().room;

        List<GameObject> adjacentRooms = new List<GameObject>();
        for (int i = floor - 1; i <= floor + 1; i++)
        {
            // skip this floor if it doesn't exist
            if (i < 0 || i >= house.Count || house[i] == null)
            {
                continue;
            }
            for (int j = roomIndex - 1; j <= roomIndex + 1; j++)
            {
                if (j < 0 || j >= house[i].Count || house[i][j] == null) // skip this room if it doesn't exist
                {
                    continue;
                }
                adjacentRooms.Add(house[i][j].gameObject); // valid room
            }
        }

        return adjacentRooms;
    }

    /// <summary>
    /// Function to update what room the player is in currently. As of now will be called from the backwall's box collider
    /// </summary>
    /// <param name="roomEntered">Room that the player entered</param>
    public void UpdatePlayerRoom(GameObject roomEntered)
    {
        currentPlayerRoom = roomEntered;
        Debug.Log("The player's room is now " + roomEntered.name);
    }
    public GameObject PlayerRoom
    {
        get { return currentPlayerRoom; }
    }
    public List<GameObject> RoomList
    {
        get { return roomList; }
    }
    public List<GameObject> EnemyList
    {
        get { return enemyList; }
    }
    public List<GameObject> EnemyRoomList
    {
        get { return enemyRoomList; }
    }
    public List<GameObject> LadderList
    {
        get { return ladderList; }
    }
    public GameObject CurrentLadder
    {
        get { return currentLadder; }
        set { currentLadder = value; }
    }
    public GameObject CurrentHidingSpot
    {
        get { return currentHidingSpot; }
        set { currentHidingSpot = value; }
    }
    public List<GameObject> StairList
    {
        get { return stairList; }
    }
    public GameObject CurrentStair
    {
        get { return currentStair; }
        set { currentStair = value; }
    }

    public List<GameObject> DoorList
    {
        get { return doorList; }
    }

    public List<GameObject> HidingSpotList
    {
        get { return hidingSpotList; }
    }

    public List<GameObject> KeyList
    {
        get
        {
            for (int i = 0; i < keyList.Count; i++)
            {
                if (i < keyList.Count && keyList[i].GetComponent<Key>() == null)
                {
                    keyList.RemoveAt(i);
                    i--;
                }
            }
            return keyList;
        }
    }
}
