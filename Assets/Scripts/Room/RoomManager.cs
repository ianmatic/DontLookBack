using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<GameObject> roomList;
    private List<GameObject> enemyList;
    private List<GameObject> enemyRoomList;
    private List<GameObject> ladderList;
    private GameObject currentPlayerRoom;
    private GameObject oldPlayerRoom;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        roomList = new List<GameObject>();
        roomList.AddRange(GameObject.FindGameObjectsWithTag("Room"));
        enemyList = new List<GameObject>();
        enemyList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        enemyRoomList = new List<GameObject>();
        enemyRoomList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        ladderList = new List<GameObject>();
        ladderList.AddRange(GameObject.FindGameObjectsWithTag("Ladder"));
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


    void SetupCollision()
    {
        foreach(GameObject room in roomList)
        {
            foreach (Transform child in room.transform)
            {
                if (child.GetComponent<WallProperties>() && child.GetComponent<WallProperties>().isPasable) //ignore passable walls
                {
                    Physics.IgnoreCollision(player.GetComponent<BoxCollider>(), child.GetComponent<BoxCollider>());
                }
            }
            // ignore room collider
            Physics.IgnoreCollision(player.GetComponent<BoxCollider>(), room.GetComponent<BoxCollider>());
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
        //Room 0 of Floor 0 and 1
        Rooms[0][0][0] = new Vector2(1f, 1f); //UP
        Rooms[1][0][0] = new Vector2(1f, 2f); //UP/Down
        Rooms[2][0][0] = new Vector2(1f, 0f); //Down

        Rooms[1][1][0] = new Vector2(1f, 1f); //UP
        Rooms[2][1][0] = new Vector2(1f, 0f); //UP/Down

        Rooms[0][2][0] = new Vector2(1f, 1f); //UP
        Rooms[1][2][0] = new Vector2(1f, 2f); //UP/Down
        Rooms[2][2][0] = new Vector2(1f, 0f); //Down

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
}
