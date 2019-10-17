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
    int enemyFloor; //Enemy's current Floor 
    Vector2 playerPosition;//Player's 2D position
    int playerFloor; //Player's current Floor as known by the Enemy (Won't always be in sync with actual player's floor)
    int playerRoom; //Player's current Room as known by the Enemy (Won't always be in sync with actual player's room)
    Vector2 enemyDestination; //Immidiate Position that Enemy is walking to.
    int enemyTarget; //What type of Destination Enemy is walking to: 0: Player 1: Ladder ?: Eventually a hiding spot or such
    float enemySpeed = 0.04f; //Enemy Speed    Climbing speed is half.
    float roomHeight = 5f; //Room Height: Used only in Testing with Player object

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
        House = getRooms();

        //Set Initial enemy Floor, set enemyPosition (Certain Room), and set GameObject position
        enemyFloor = 2;
        enemyPosition = House[enemyFloor][2][1];
        enemy.transform.position = new Vector3(enemyPosition.x, enemyPosition.y, 0);

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

        if(player.transform.position.y < 2)
        {
            playerFloor = 0;
        }
        else if(player.transform.position.y > 7)
        {
            playerFloor = 2;
        }
        else
        {
            playerFloor = 1;
        }
        //Create new Destination if not on ladder
        if (enemyPosition.y == enemyFloor * roomHeight - 2){
            createPathToPLayer();
        }
        //Move to Destination
        moveToDestination();


        //Once done all code: Update enemy Gameobject position
        enemy.transform.position = new Vector3(enemyPosition.x, enemyPosition.y, 0);
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
                if (enemyPosition.x < enemyDestination.x - 0.03f || enemyPosition.x > enemyDestination.x + 0.03f)
                {
                    enemyPosition.x += direction.x * enemySpeed;
                    //ONCE within buffer, Set x to exact x of Player  
                    if (enemyPosition.x > enemyDestination.x - 0.03f && enemyPosition.x < enemyDestination.x + 0.03f)
                    {
                        enemyPosition.x = enemyDestination.x;
                    }
                }
                break;
            case 1:
                //If Ladder:
                //If X is different then move horizontally to Ladder
                if (enemyPosition.x < enemyDestination.x - 0.03f || enemyPosition.x > enemyDestination.x + 0.03f)
                {
                    enemyPosition += direction * enemySpeed;

                    if (enemyPosition.x > enemyDestination.x - 0.03f && enemyPosition.x < enemyDestination.x + 0.03f)
                    {
                        enemyPosition.x = enemyDestination.x;
                    }
                }
                //If At Ladder, Change Destination to Connected of Next Floor closer to Player
                else
                {
                    
                    if (playerFloor > enemyFloor)
                    {
                        enemyDestination = findLadder(enemyFloor + 1, 0);
                    }
                    else
                    {
                        enemyDestination = findLadder(enemyFloor - 1, 1);
                    }
                    //Reset Direction going upwards
                    direction = (enemyDestination - enemyPosition).normalized;
                    //If Y is different, climb.  Else: set Y exactly to Destination and adjust enemyFloor to new Floor
                    if (enemyPosition.y < enemyDestination.y - 0.03f || enemyPosition.y > enemyDestination.y + 0.03f)
                    {
                        enemyPosition += direction * (enemySpeed * 0.5f);

                    }
                    else
                    {
                        enemyPosition.y = enemyDestination.y;
                        enemyFloor += (int)direction.y;
                        //Debug.Log(enemyFloor);
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
        if(enemyFloor == playerFloor)
        {
            //Set Destination
            enemyDestination = playerPosition;
            //Set Target as Player
            enemyTarget = 0;
        }
        else
        {
            //Determine which Ladder (Up or Down) to go to / Set Destination
            if(playerFloor > enemyFloor)
            {
                enemyDestination = findLadder(enemyFloor, 1);
            }
            else
            {
                enemyDestination = findLadder(enemyFloor, 0);

            }
            //Set Target as Ladder
            enemyTarget = 1;
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
        //Initial Ladder Room is invalid. POSSIBLE BREAK: If no floor on floor
        int ladderRoom = -1;
        //Loop: Each Room on Floor
        Debug.Log(floor);
        Debug.Log(playerFloor);
        Debug.Log(enemyFloor);
        for (int i = 0; i < House[floor].Count; i++)
        {
            //Check: Room is Ladder AND going correct direction
            if(House[floor][i][0].x == 1 && House[floor][i][0].y == direction || House[floor][i][0].y == 2)
            {
                //Check: If NOT first ladderRoom found
                if (ladderRoom != -1)
                {
                    //Check: If distance to Ladder is less than previous ladderRoom
                    if (Mathf.Abs(House[floor][i][1].x - enemyPosition.x) < Mathf.Abs(House[floor][ladderRoom][1].x - enemyPosition.x))
                    {
                        ladderRoom = i;
                    }
                }
                //If first ladder found
                else 
                {
                    ladderRoom = i;
                }
            }
        }
        // Return LadderRoom Position
        return House[floor][ladderRoom][1];
    }
    /// <summary>
    /// Returns List of Rooms
    /// TESTING: Currently generates Room itself (Will grab room data from elsewhere in future)
    /// </summary>
    /// <returns>List of Floors->Rooms->Variables</returns>
    List<List<List<Vector2>>> getRooms()
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
                    Rooms[i][j].Add(new Vector2(k * ((j * 10f)-10f), k * (i * 5f)-2f));
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
}
