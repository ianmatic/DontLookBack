using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityRoomUpdater : MonoBehaviour
{
    public GameObject Manager;
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Enemy":
                Manager.GetComponent<RoomManager>().UpdateEnemyRoom(other, gameObject);
                break;
            case "Player":
                Manager.GetComponent<RoomManager>().UpdatePlayerRoom(gameObject);
                break;
            default:
                break;
        }
    }
}
