using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyPathfinding : MonoBehaviour
{
    //TESTING:
    public int playerRealFloor; //TESTING: Player's Actual Floor

    //Public:
    public GameObject enemy;  //Enemy GameObject (Incase not directly attached)
    public GameObject player; //Player GameObject (Until middleman)

    //Private:
    private
    Vector2 enemyPosition; //Enemy's 2D position
    float enemyZPosition;
    int enemyFloor; //Enemy's current Floor 
    Vector2 playerPosition;//Player's 2D position
    int playerFloor; //Player's current Floor as known by the Enemy (Won't always be in sync with actual player's floor)
    int playerRoom; //Player's current Room as known by the Enemy (Won't always be in sync with actual player's room)
    Vector2 enemyDestination; //Immidiate Position that Enemy is walking to.
    int enemyTarget; //What type of Destination Enemy is walking to: 0: Player 1: Ladder ?: Eventually a hiding spot or such
    float enemySpeed = 0.04f; //Enemy Speed    Climbing speed is half.
    float roomHeight = 5f; //Room Height: Used only in Testing with Player object
    RoomManager roomManager;
    bool enemyClimbing = false;
    /// <summary>
    /// House Consists of several Lists:   From Outer to Inner:
    /// 1. List of Floors
    /// 2. List of Rooms
    /// 3. List of Room Variables
    ///     3a. Vector2D (Ladder: 0=No/1=Yes, Direction: 0=Down/1=Up)
    ///     3b. Vector2D (Center of Room)
    /// </summary>
    public List<List<List<Vector2>>> House; 

    // Start is called before the first frame update
    void Start()
    {
        //Retrive Room details from (getRooms)  Will pull from Room script in the future.
        roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        House = roomManager.buildHouse();

        //Set Initial enemy Floor, set enemyPosition (Certain Room), and set GameObject position
        enemyFloor = 2;
        enemyPosition = House[enemyFloor][2][1];
        enemy.transform.position = new Vector3(enemyPosition.x, enemyPosition.y, 0);
        enemyZPosition = 0;
        //TESTING:  sets test player floor,position, and gameobject position
        //playerRealFloor = 2;
        //playerRoom = 1;
        //playerPosition = player.transform.position + new Vector3(0.49f,0, 0);

    }

    // Update is called once per frame
    void Update()
    {
        
        //Grabs the player's Position
        playerPosition = player.transform.position;

        if(player.transform.position.y < 2.5)
        {
            playerFloor = 0;
        }
        else if(player.transform.position.y > 7.5)
        {
            playerFloor = 2;
        }
        else
        {
            playerFloor = 1;
        }
        //Create new Destination if not on ladder
        if (enemyPosition.y == (enemyFloor * roomHeight) - 2f){
            enemyClimbing = false;
            createPathToPLayer();
        }
        else {
            enemyClimbing = true;
        }
        //Move to Destination
        moveToDestination();


        //Once done all code: Update enemy Gameobject position
        enemy.transform.position = new Vector3(enemyPosition.x, enemyPosition.y, enemyZPosition);
    }

    void moveToDestination()
    {
        //Direction of movement (Distance between enemy and its destination, normalized))
        Vector2 direction;
        direction = (enemyDestination - enemyPosition).normalized;

        //Main Switch statement based on what the Destination is: player vs Ladder
        switch (enemyTarget)
        {
            //If Player: Move to Player at base speed untill within set buffer (Buffer code will be replaced by coliders most likely)
            case 0:
                if (Mathf.Abs(enemyPosition.x - enemyDestination.x) > 0.03f)
                {
                    enemyPosition.x += direction.x * enemySpeed;
                    //ONCE within buffer, Set x to exact x of Player  
                    if (Mathf.Abs(enemyPosition.x - enemyDestination.x) <= 0.03f)
                    {
                        enemyPosition.x = enemyDestination.x;
                    }
                }
                break;
            case 1:
                //If Ladder:
                //If X is different then move horizontally to Ladder
                if (Mathf.Abs(enemyPosition.x - enemyDestination.x) > 0.03f)
                {
                    enemyPosition += direction * enemySpeed;
                    if (Mathf.Abs(enemyPosition.x - enemyDestination.x) <= 0.03f)
                    {
                        enemyPosition.x = enemyDestination.x;
                    }
                }
                //If At Ladder, Change Destination to Connected of Next Floor closer to Player
                else
                {
                    //If Y is different, climb.  Else: set Y exactly to Destination and adjust enemyFloor to new Floor
                    if (Mathf.Abs(enemyPosition.y - enemyDestination.y) > 0.03f)
                    {
                        enemyPosition += direction * (enemySpeed * 0.5f);

                    }
                    else
                    {
                        enemyPosition.y = enemyDestination.y;
                        enemyFloor += (int)direction.y;
                    }
                }
                break;
            case 2:
                //If Stairs:
                if ( Mathf.Abs(enemyPosition.x - enemyDestination.x) > 0.03f)
                {
                    if(enemyPosition.y != enemyDestination.y){enemyZPosition = 1;}
                    enemyPosition += direction * enemySpeed;

                    if (Mathf.Abs(enemyPosition.x - enemyDestination.x) <= 0.03f)
                    {
                        enemyPosition.x = enemyDestination.x;
                    }
                }
                else
                {
                    enemyPosition.x = enemyDestination.x;
                    enemyZPosition = 1;
                   /* 
                    if (playerFloor > enemyFloor)
                    {
                        enemyDestination = findLadder(enemyFloor + 1, 0);
                    }
                    else
                    {
                        enemyDestination = findLadder(enemyFloor - 1, 1);
                    }
                    */
                    //Reset Direction going upwards
                    //direction = (enemyDestination - enemyPosition).normalized;
                    //If Y is different, climb.  Else: set Y exactly to Destination and adjust enemyFloor to new Floor
                    if (Mathf.Abs(enemyPosition.y - enemyDestination.y) > 0.03f)
                    {
                        enemyPosition += direction * (enemySpeed * 0.5f);
                    }
                    else
                    {
                        enemyPosition.y = enemyDestination.y;
                        enemy.transform.position.Set(enemyPosition.x,enemyPosition.y,0);
                        enemyFloor += (int)direction.y;
                        enemyZPosition = 0;
                    }
                }
                break;
            //No Default: Print Error if Target is ever NOT accounted for by Switch
            default: Debug.Log("ERROR: INVALID EnemyTarget: " + enemyTarget);
                break;
        }
    }

    /// <summary>
    /// Get location of player.
    /// If on same floor: set player as Destination
    /// If on different floor: set closest ladder as Desination
    /// </summary>
    void createPathToPLayer()
    {
        //Get player floor (Will get information less directly in future)
        //playerFloor = playerRealFloor;
        //Floor Check
        if( Mathf.Abs(enemyPosition.y - playerPosition.y) < 0.1f)
        {
            //Set Destination
            enemyDestination = playerPosition;
            //Set Target as Player
            enemyTarget = 0;
            
        }
        else
        {
            //Determine which Ladder (Up or Down) to go to / Set Destination
            if(playerPosition.y > enemyPosition.y)
            {
                enemyDestination = findLadder(enemyFloor, 1);
                if (enemyPosition.x == enemyDestination.x )
                    {
                        enemyDestination = findLadder(enemyFloor + 1, 0);
                    }
            }
            else
            {
                enemyDestination = findLadder(enemyFloor, 0);

                if (enemyPosition.x == enemyDestination.x )
                    {
                        enemyDestination = findLadder(enemyFloor - 1, 1);
                    }
            }
        }
    }
    /// <summary>
    /// Finds Closest Ladder going correct direction
    /// </summary>
    /// <param name="floor">Floor Ladder is located on</param>
    /// <param name="direction">Direction Ladder is going</param>
    /// <returns></returns>
    Vector2 findLadder(int floor, int direction)
    {
        if(floor < 0){floor = 0;}
        //Initial Ladder Room is invalid. POSSIBLE BREAK: If no floor on floor
        int ladderRoom = -1;
        
        //Loop: Each Room on Floor
        /*Debug.Log("floor " + floor);
        Debug.Log("playerFloor " + playerFloor);
        Debug.Log("enemyFloor " + enemyFloor);
        Debug.Log("direction " + direction);*/
        for (int i = 0; i < House[floor].Count; i++)
        {
            //Check: Room is Ladder or Stairs AND going correct direction 
            if( (House[floor][i][0].x >= 1 && (House[floor][i][0].y == direction || House[floor][i][0].y == 2)  ) || House[floor][i][0].x == 3)
            {
                //Check: If NOT first ladderRoom found
                if (ladderRoom != -1)
                {
                    if( Mathf.Abs(enemyFloor - playerFloor) < 2){
                        //Check: If distance to Ladder is less than previous ladderRoom (Based on Player  X then Y)
                        if (Mathf.Abs(House[floor][i][1].x - playerPosition.x) < Mathf.Abs(House[floor][ladderRoom][1].x - playerPosition.x))
                        {
                            ladderRoom = i;
                        }
                    }
                    else {
                        //Check: If distance to Ladder is less than previous ladderRoom (Based on Enemy Y then X)
                        if (Mathf.Abs(House[floor][i][1].x - enemyPosition.x) < Mathf.Abs(House[floor][ladderRoom][1].x - enemyPosition.x))
                        {
                            ladderRoom = i;
                        }
                    }
                                      
                }
                //If first ladder found
                else 
                {
                    ladderRoom = i;
                }
            }
        }
        
        //Ladder
        if( House[floor][ladderRoom][0].x == 1 || (House[floor][ladderRoom][0].x >= 3 && House[floor][ladderRoom][0].y == direction)){
            enemyTarget = 1;
        }
        else {
        //Stairs
            enemyTarget = 2;
            return House[floor][ladderRoom][2+direction];
        }
        // Return LadderRoom Position
        return House[floor][ladderRoom][1];
    }
}
