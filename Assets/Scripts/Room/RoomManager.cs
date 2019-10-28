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

    // Start is called before the first frame update
    void Awake()
    {
        roomList = new List<GameObject>();
        roomList.AddRange(GameObject.FindGameObjectsWithTag("Room"));
        enemyList = new List<GameObject>();
        enemyList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        enemyRoomList = new List<GameObject>();
        enemyRoomList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
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

    }

    void Update()
    {
        oldPlayerRoom = currentPlayerRoom;
        currentPlayerRoom = FindCurrentPlayerRoom();
        if (oldPlayerRoom != currentPlayerRoom)
        {
            TransitionToRoom(currentPlayerRoom);
        }
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

    /// <summary>
    /// Returns List of Rooms
    /// TESTING: Currently generates Room itself (Will grab room data from elsewhere in future)
    /// </summary>
    /// <returns>List of Floors->Rooms->Variables</returns>
    public List<List<List<Vector2>>> buildHouse()
    {
        //Initialize internal variable
        List<List<List<Vector2>>> Rooms = new List<List<List<Vector2>>>();
        //TESTING: House Generation
        //For each Floor: 3
        for (int i = 0; i < 3; i++)
        {
            //Add Floor
            Rooms.Add(new List<List<Vector2>>());
            //For each Room: 3
            for (int j = 0; j < 3; j++)
            {
                //Add Room
                Rooms[i].Add(new List<Vector2>());
                //For Each Variable
                for (int k = 0; k < 2; k++)
                {
                    //SETS first variable to (0,0) IE: Not a ladder.   Sets Position in incremtes of 2fx and 4fy
                    Rooms[i][j].Add(new Vector2(k * ((j * 10f) - 10f), k * (i * 5f) - 2f));
                    //Debug.Log(Rooms[i][j][k]); //TESTING: Print each Room
                }
                //Debug.Log("Room Done"); //TESTING: Room Done Creation
            }
            //Debug.Log("Floor Done"); //TESTING: Floor Done Creation
        }
        //Debug.Log("Rooms Done"); //TESTING: Rooms Done Creation

        //TESTING: Manually Set certain Rooms as Ladders.
        //For Stairs, add extra Vector 2s to store position of Stairs going: [2]Down, [3]UP
        //Column 0
        //Floor 0
        Rooms[0][0][0] = new Vector2(1f, 1f); //Ladder UP
        //Floor 1
        Rooms[1][0][0] = new Vector2(3f, 0f); //Ladder Down Stairs Up
        Rooms[1][0].Add(new Vector2(0, 0));
        Rooms[1][0].Add(new Vector2(-7f, Rooms[1][0][1].y)); //Stairs position
        //Floor 2
        Rooms[2][0][0] = new Vector2(2f, 0f); //Stairs Down
        Rooms[2][0].Add(new Vector2(-13f, Rooms[2][0][1].y)); //Stairs position
        //Column 1
        //Floor 0
        Rooms[0][1][0] = new Vector2(2f, 1f); //Stairs UP
        Rooms[0][1].Add(new Vector2(0f, 0f));
        Rooms[0][1].Add(new Vector2(-2.5f, Rooms[0][1][1].y)); //Stairs position
        //Floor 1
        Rooms[1][1][0] = new Vector2(2f, 0f); //Stairs Down
        Rooms[1][1].Add(new Vector2(3.6f, Rooms[1][1][1].y)); //Stairs position


        //Return Finished Rooms
        return Rooms;
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
        get {
            for(int i = 0; i < keyList.Count; i++)
            {
                if(i < keyList.Count && keyList[i].GetComponent<Key>() == null)
                {
                    keyList.RemoveAt(i);
                    i--;
                }
            }
            return keyList;
        }
    }
}
