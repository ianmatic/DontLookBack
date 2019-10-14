using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private GameObject[] roomList;
    private GameObject[] enemyList;
    private GameObject[] enemyRoomList;
    private GameObject currentPlayerRoom;
    
    // Start is called before the first frame update
    void Start()
    {
        roomList = GameObject.FindGameObjectsWithTag("Room");
        enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        enemyRoomList = new GameObject[GameObject.FindGameObjectsWithTag("Enemy").Length];
    }

    /// <summary>
    /// Function to update current enemy position, written in this way so as to allow for multiple killers later in development while still being handled in the manager. As of now will be called from the backwall's box collider
    /// </summary>
    /// <param name="enemyCollision">Enemy object's collision</param>
    /// <param name="roomEntered">Room that the enemy entered</param>
    public void UpdateEnemyRoom(Collider enemyCollision,GameObject roomEntered)
    {
        for (int i = 0; i< enemyList.Length;i++)
        {
            if(enemyList[i] == enemyCollision.gameObject)
            {
                enemyRoomList[i] = roomEntered;
                Debug.Log(enemyList[i].name + "'s room is now " + roomEntered.name);
                break;
            }
        }
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
}
