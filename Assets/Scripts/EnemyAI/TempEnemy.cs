using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEnemy : MonoBehaviour
{
    GameObject player;
    RoomManager roomManager;
    Vector3 playerPos;

    bool onLadder;
    GameObject ladderTarget;
    Vector3 velocity;
    public float speed;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        playerPos = player.transform.position;
    }

    void Update()
    {
        MoveTowardsPlayer();
        if(onLadder)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    void MoveTowardsPlayer()
    {
        playerPos = player.transform.position;
        if(playerPos.y - gameObject.transform.position.y > 0.15)
        {
            MoveToLadder(roomManager.LadderList, true);
        }
        else if(playerPos.y - gameObject.transform.position.y < -0.15)
        {
            MoveToLadder(roomManager.LadderList, false);
        }
        else
        {
            onLadder = false;
            Move(new Vector3(playerPos.x - gameObject.transform.position.x, 0));
        }
    }

    void MoveToLadder(List<GameObject> ladders, bool goUp)
    {
        if(onLadder)
        {
            MoveToLadder(ladderTarget, goUp);
            return;
        }

        GameObject closestLadder;
        List<GameObject> filtered = new List<GameObject>();
        filtered = FilterLadders(ladders, goUp);
        if(!(filtered.Count <= 1))
        {
            closestLadder = filtered[0];

            for (int i = 1; i < filtered.Count; i++)
            {
                if ((closestLadder.transform.position - gameObject.transform.position).magnitude > (filtered[i].transform.position - gameObject.transform.position).magnitude)
                {
                    closestLadder = filtered[i];
                }
            }
        }
        else if (filtered.Count == 0)
        {
            onLadder = false;
            Move(new Vector3(playerPos.x - gameObject.transform.position.x, 0));
            return;
        }
        else
        {
            closestLadder = filtered[0];
        }

        if (Mathf.Abs(gameObject.transform.position.x - closestLadder.transform.position.x) < 0.3f)
        {
            ladderTarget = closestLadder;
            OnLadder(goUp);
            onLadder = true;
            return;
        }
        else
        {
            onLadder = false;
        }

        Move(new Vector3(closestLadder.transform.position.x - gameObject.transform.position.x, 0));
    }

    void MoveToLadder(GameObject ladder, bool goUp)
    {
        if (Mathf.Abs(gameObject.transform.position.x - ladder.transform.position.x) < 0.3f)
        {
            OnLadder(goUp);
        }
        else
        {
            ladderTarget = null;
            onLadder = false;
        }
    }

    List<GameObject> FilterLadders(List<GameObject> ladders, bool goUp)
    {
        List<GameObject> newLadders = new List<GameObject>();
        if(goUp)
        {
            for (int i = 0; i < ladders.Count; i++)
            {
                if (Mathf.Abs(ladders[i].transform.position.y - gameObject.transform.position.y) < 1)
                {
                    newLadders.Add(ladders[i]);
                }
            }
                
                    
        }
        else
        {
            for (int i = 0; i < ladders.Count; i++)
                if (Mathf.Abs(ladders[i].transform.position.y - gameObject.transform.position.y) > 0.7)
                {
                    newLadders.Add(ladders[i]);
                }
                    
        }

        return newLadders;
    }

    void Move(Vector3 target)
    {
        gameObject.transform.position += (target.normalized * Time.deltaTime * speed);
    }

    void OnLadder(bool goUp)
    {
        if(goUp)
        {
            Move(new Vector3(0, 1, 0));
        }
        else
        {
            if(gameObject.transform.position.y - ladderTarget.transform.position.y < 1)
            {
                onLadder = false;
            }
            Move(new Vector3(0, -1, 0));
        }
    }
}
