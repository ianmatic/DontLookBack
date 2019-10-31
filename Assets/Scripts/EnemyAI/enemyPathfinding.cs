using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class enemyPathfinding : MonoBehaviour
{

    //Public:
    public GameObject enemy;  //Enemy GameObject (Incase not directly attached)
    public GameObject player; //Player GameObject (Until middleman)

    //Private:
    AudioManager audioManager;
    Animator animator;
    float gruntTimer = 10.0f;
    Vector2 enemyPosition; //Enemy's 2D position
    float enemyZPosition;
    //int enemyFloor; //Enemy's current Floor 
    Vector2 playerPosition;//Player's 2D position
    //int playerFloor; //Player's current Floor as known by the Enemy (Won't always be in sync with actual player's floor)
    int playerRoom; //Player's current Room as known by the Enemy (Won't always be in sync with actual player's room)
    Vector2 enemyDestination; //Immidiate Position that Enemy is walking to.
    int enemyTarget; //What type of Destination Enemy is walking to: 0: Player 1: Ladder ?: Eventually a hiding spot or such
    float enemySpeed = 0.08f; //Enemy Speed    Climbing speed is half.
    RoomManager roomManager;
    bool enemyClimbing = false;

    bool exitUnlocked = false;
    float huntTimerMax = 10f;
    float huntTimer = 0;
    GameObject searchSpot;
    float searchTimerMax = 10f;
    float searchTimer = 0;
    float searchTimer2Max = 8;
    float searchTimer2 = 0;
    RoomProperties.Type pathTarget = RoomProperties.Type.none;
    RoomProperties.Type pathPreviousTarget = RoomProperties.Type.none;
    List<Vector2> enemyPath = new List<Vector2>();
    RoomProperties wanderDestination;
    /// <summary>
    /// House Consists of several Lists:   From Outer to Inner:
    /// 1. List of Floors
    /// 2. List of Rooms
    /// 3. List of Room Variables
    ///     3a. Vector2D (Ladder: 0=No/1=Yes, Direction: 0=Down/1=Up)
    ///     3b. Vector2D (Center of Room)
    /// </summary>
    public List<List<List<Vector2>>> House = new List<List<List<Vector2>>>();
    public List<List<RoomProperties>> HouseNew = new List<List<RoomProperties>>();

    public Door Door { get; set; }
    private GameObject enemyRoom
    {
        get { return roomManager.EnemyRoomList[0]; }
    }
    //Enemy's current Floor 
    public int enemyFloor
    {
        //Get Floor from roomManager's enemyRoom  (0) cause only 1 enemy
        get { return enemyRoom.GetComponent<RoomProperties>().floor; }
    }

    private int playerFloor
    {
        //Get Floor from roomManager's PlayerRoom
        get { return roomManager.PlayerRoom.GetComponent<RoomProperties>().floor; }
    }

    public enum State
    {
        Wandering,
        Hunting,
        Climbing,
        Opening,
        Searching,
    }
    private State enemyStoredState;
    private State enemyState;
    public State enemyStateProp
    {
        get { return enemyState; }
        set { enemyStoredState = enemyState; enemyState = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        HouseNew = roomManager.buildHouseNew();
        /*
        for (int i = 0; i < houseData.Length; i++)
		{
            House.Add(new List<List<Vector2>>());
            for (int j = 0; j < houseData[i].floorData.Length; j++)
		    {
                House[i].Add(new List<Vector2>());
                for (int k = 0; k < houseData[i].floorData[j].roomData.Length; k++)
		            {
                        House[i][j].Add(houseData[i].floorData[j].roomData[k]);
		            }
		    }
		}

        //Retrive Room details from (getRooms)  Will pull from Room script in the future.
        //roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        //House = roomManager.buildHouse();

        //Set Initial enemy Floor, set enemyPosition (Certain Room), and set GameObject position
        enemyFloor = 3;
        
        //TESTING:  sets test player floor,position, and gameobject position
        //playerRealFloor = 2;
        //playerRoom = 1;
        //playerPosition = player.transform.position + new Vector3(0.49f,0, 0);
        */
        enemyState = State.Wandering;

        enemyPosition = enemy.transform.position;
        enemy.transform.position = new Vector3(enemyPosition.x, enemyPosition.y, 0);
        enemyZPosition = -0.25f;

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        foreach (Transform child in transform)
        {
            if (child.name == "enemyModel")
            {
                animator = child.GetComponent<Animator>();
                break;
            }
        }
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(enemyRoom == null){
            enemyPosition = HouseNew[0][0].center;
            enemyState = State.Wandering;
            enemyPath.Clear();
            wanderDestination = null;
        }
        //Debug.Log(enemyRoom);
        //Debug.Log(enemyFloor);
        switch (enemyState)
        {
            case State.Wandering:
                Wander();
                break;
            case State.Hunting:
                Hunt();
                break;
            case State.Climbing:
                moveToDestination();
                break;
            case State.Opening:
                Bang();
                break;
            case State.Searching:
                Search();
                break;
            default:
                enemyState = State.Wandering;
                break;
        }

        if (huntTimer > 0) { huntTimer -= Time.deltaTime; }

        //Once done all code: Update enemy Gameobject position
        enemy.transform.position = new Vector3(enemyPosition.x, enemyPosition.y, enemyZPosition);
    }

    void moveToDestination()
    {
        if (enemyPath.Count == 0) { return; }


        Vector2 direction;
        Quaternion currentOrientation;
        Quaternion targetOrientation;
        switch (enemyState)
        {
            case State.Wandering:
                direction = new Vector2(enemyPath[0].x - enemyPosition.x, 0).normalized;
                enemyPosition.x += direction.x * (enemySpeed / 1.5f);
                // animation
                animator.speed = 1.0f;
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isScaling", false);
                // face correct direction
                currentOrientation = transform.rotation;
                if (direction.x > 0)
                {
                    targetOrientation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
                }
                else
                {
                    targetOrientation = Quaternion.Euler(transform.rotation.x, -90, transform.rotation.z);
                }
                transform.rotation = Quaternion.Lerp(currentOrientation, targetOrientation, .1f);
                // audio
                audioManager.EndLoopSound("enemyRun");
                audioManager.EndLoopSound("enemyStairs");
                audioManager.EndLoopSound("enemyLadder");
                audioManager.PlayLoopSound("enemyWalk", gameObject);
                WanderGrunt();

                switch (pathTarget)
                {
                    case RoomProperties.Type.none:
                        if (Mathf.Abs((enemyPosition.x - enemyPath[0].x)) < 0.03f)
                        {
                            enemyPath.RemoveAt(0);
                        }
                        break;
                    case RoomProperties.Type.ladder:
                        if (Mathf.Abs((enemyPosition.x - enemyPath[0].x)) < 0.03f)
                        {
                            animator.speed = 1.0f;
                            enemyPath.RemoveAt(0);
                            enemyState = State.Climbing;
                        }
                        break;
                    case RoomProperties.Type.stairs:
                        if (Mathf.Abs((enemyPosition.x - enemyPath[0].x)) < 0.03f)
                        {
                            animator.speed = 1.0f;
                            enemyPath.RemoveAt(0);
                            enemyState = State.Climbing;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case State.Hunting:
                direction = new Vector2(enemyPath[0].x - enemyPosition.x, 0).normalized;
                enemyPosition.x += direction.x * (enemySpeed);

                // animation
                animator.speed = 1.0f;
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", true);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isScaling", false);
                // face correct direction
                currentOrientation = transform.rotation;
                if (direction.x > 0)
                {
                    targetOrientation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
                }
                else
                {
                    targetOrientation = Quaternion.Euler(transform.rotation.x, -90, transform.rotation.z);
                }
                transform.rotation = Quaternion.Lerp(currentOrientation, targetOrientation, .1f);

                // audio
                audioManager.EndLoopSound("enemyWalk");
                audioManager.EndLoopSound("enemyStairs");
                audioManager.EndLoopSound("enemyLadder");
                audioManager.PlayLoopSound("enemyRun", gameObject);
                HuntGrunt();

                switch (pathTarget)
                {
                    case RoomProperties.Type.none:

                        if (Mathf.Abs((enemyPosition.x - enemyPath[0].x)) < 0.03f)
                        {
                            enemyPath.RemoveAt(0);
                        }
                        break;
                    case RoomProperties.Type.ladder:
                        if (Mathf.Abs((enemyPosition.x - enemyPath[0].x)) < 0.03f)
                        {
                            animator.speed = 1.5f;
                            enemyPath.RemoveAt(0);
                            enemyState = State.Climbing;
                        }
                        break;
                    case RoomProperties.Type.stairs:
                        if (Mathf.Abs((enemyPosition.x - enemyPath[0].x)) < 0.03f)
                        {
                            animator.speed = 1.5f;
                            enemyPath.RemoveAt(0);
                            enemyState = State.Climbing;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case State.Climbing:
                switch (pathTarget)
                {
                    case RoomProperties.Type.ladder:
                        enemyZPosition = 0.5f;
                        direction = new Vector2(0, enemyPath[0].y - enemyPosition.y).normalized;
                        // animation
                        animator.SetBool("isWalking", false);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", true);
                        animator.SetBool("isScaling", false);
                        // face correct direction
                        transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);

                        if (huntTimer > 0) { enemyPosition.y += direction.y * (enemySpeed / 1.5f); }
                        else { enemyPosition.y += direction.y * (enemySpeed / 2f); }
                        if (Mathf.Abs((enemyPosition.y - enemyPath[0].y)) < 0.03f)
                        {
                            enemyZPosition = -0.25f;
                            enemyPosition = enemyPath[0];
                            enemyPath.RemoveAt(0);
                            enemyState = State.Wandering;
                            pathTarget = RoomProperties.Type.none;

                            if (huntTimer > 0 || exitUnlocked) { enemyState = State.Hunting; }
                        }

                        // audio
                        audioManager.EndLoopSound("enemyRun");
                        audioManager.EndLoopSound("enemyWalk");
                        audioManager.EndLoopSound("enemyStairs");
                        audioManager.PlayLoopSound("enemyLadder", gameObject);
                        break;
                    case RoomProperties.Type.stairs:
                        enemyZPosition = 1.25f;
                        direction = (enemyPath[0] - enemyPosition).normalized;
                        // animation
                        animator.SetBool("isWalking", false);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isScaling", true);
                        // face correct direction
                        if (direction.x > 0)
                        {
                            transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
                        } else
                        {
                            transform.rotation = Quaternion.Euler(transform.rotation.x, -90, transform.rotation.z);
                        }

                        if (huntTimer > 0) { enemyPosition += direction * (enemySpeed / 1.5f); }
                        else { enemyPosition += direction * (enemySpeed / 2f); }
                        if ((enemyPosition - enemyPath[0]).magnitude < 0.03f)
                        {
                            enemyPosition = enemyPath[0];
                            enemyPath.RemoveAt(0);
                            enemyState = State.Wandering;
                            pathTarget = RoomProperties.Type.none;
                            enemyZPosition = -0.25f;
                            if (huntTimer > 0 || exitUnlocked) { enemyState = State.Hunting; }
                        }

                        // audio
                        audioManager.EndLoopSound("enemyRun");
                        audioManager.EndLoopSound("enemyWalk");
                        audioManager.EndLoopSound("enemyLadder");
                        audioManager.PlayLoopSound("enemyStairs", gameObject);
                        break;
                    default:
                        break;
                }

                //Debug.Log(enemyFloor);

                break;
            default:
                break;
        }

        if (enemyPath.Count == 0) { wanderDestination = null; }
    }

    /// <summary>
    /// Get location of player.
    /// If on same floor: set player as Destination
    /// If on different floor: set closest ladder as Desination
    /// </summary>
    void createPathToPLayer()
    {
        enemyZPosition = 0;
        //Get player floor (Will get information less directly in future)
        //playerFloor = playerRealFloor;
        //Floor Check
        if (Mathf.Abs(enemyPosition.y - playerPosition.y) < 0.2f)
        {
            //Set Destination
            enemyDestination = playerPosition;
            //Set Target as Player
            enemyTarget = 0;

        }
        else
        {
            //Determine which Ladder (Up or Down) to go to / Set Destination
            if (playerPosition.y > enemyPosition.y)
            {
                enemyDestination = findLadder(enemyFloor, 1);
                if (Mathf.Abs(enemyPosition.x - enemyDestination.x) < 0.1f)
                {
                    enemyDestination = findLadder(enemyFloor + 1, 0);
                }
            }
            else
            {
                enemyDestination = findLadder(enemyFloor, 0);
                if (Mathf.Abs(enemyPosition.x - enemyDestination.x) < 0.1f)
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
        if (floor < 0) { floor = 0; }
        //Initial Ladder Room is invalid. POSSIBLE BREAK: If no floor on floor
        int ladderRoom = -1;

        //Loop: Each Room on Floor
        for (int i = 0; i < House[floor].Count; i++)
        {
            //Check: Room is Ladder or Stairs AND going correct direction 
            if ((House[floor][i][0].x >= 1 && (House[floor][i][0].y == direction || House[floor][i][0].y == 2)) || House[floor][i][0].x == 3)
            {
                //Check: If NOT first ladderRoom found
                if (ladderRoom != -1)
                {
                    //Check: If distance to Ladder is less than previous ladderRoom (Based on Enemy Y then X)
                    if (Mathf.Abs(House[floor][i][1].x - enemyPosition.x) < Mathf.Abs(House[floor][ladderRoom][1].x - enemyPosition.x))
                    {
                        ladderRoom = i;
                    }
                    //}

                }
                //If first ladder found
                else
                {
                    ladderRoom = i;
                }
            }
        }

        //Ladder
        if (House[floor][ladderRoom][0].x == 1 || (House[floor][ladderRoom][0].x >= 3 && House[floor][ladderRoom][0].y == direction))
        {
            enemyTarget = 1;
        }
        else
        {
            //Stairs
            enemyTarget = 2;
            return House[floor][ladderRoom][2 + direction];
        }
        // Return LadderRoom Position
        return House[floor][ladderRoom][1];
    }

    void findladder(int direction)
    {
        Vector2 firstDestination = new Vector2(0, 0);
        Vector2 secondDestination = new Vector2(0, 0);
        switch (direction)
        {
            case 0:
                foreach (RoomProperties room in HouseNew[enemyFloor])
                {
                    if (room.downPath)
                    {
                        switch (room.downType)
                        {
                            case RoomProperties.Type.ladder:
                                LadderProperties newLadderProp = room.downProperties.GetComponent<LadderProperties>();
                                Vector2 newLadder = newLadderProp.topLadder.transform.position;
                                if (firstDestination != new Vector2(0, 0))
                                {
                                    if (Mathf.Abs(enemyPosition.x - firstDestination.x) > Mathf.Abs(enemyPosition.x - newLadder.x))
                                    {
                                        firstDestination = newLadder;
                                        secondDestination = newLadderProp.bottomLadder.transform.position;
                                        pathTarget = RoomProperties.Type.ladder;
                                    }
                                }
                                else
                                {
                                    firstDestination = newLadder;
                                    secondDestination = newLadderProp.bottomLadder.transform.position;
                                    pathTarget = RoomProperties.Type.ladder;
                                }
                                break;
                            case RoomProperties.Type.stairs:
                                StairProperties newStairProp = room.downProperties.GetComponent<StairProperties>();
                                Vector2 newStair = newStairProp.topStair.transform.position;
                                if (firstDestination != new Vector2(0, 0))
                                {
                                    if (Mathf.Abs(enemyPosition.x - firstDestination.x) > Mathf.Abs(enemyPosition.x - newStair.x))
                                    {
                                        firstDestination = newStair;
                                        secondDestination = newStairProp.bottomStair.transform.position;
                                        pathTarget = RoomProperties.Type.stairs;
                                    }
                                }
                                else
                                {
                                    firstDestination = newStair;
                                    secondDestination = newStairProp.bottomStair.transform.position;
                                    pathTarget = RoomProperties.Type.stairs;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                break;
            case 1:
                foreach (RoomProperties room in HouseNew[enemyFloor])
                {
                    if (room.upPath)
                    {
                        switch (room.upType)
                        {
                            case RoomProperties.Type.ladder:
                                LadderProperties newLadderProp = room.upProperties.GetComponent<LadderProperties>();
                                Vector2 newLadder = newLadderProp.bottomLadder.transform.position;
                                if (firstDestination != null)
                                {
                                    if (Mathf.Abs(enemyPosition.x - firstDestination.x) > Mathf.Abs(enemyPosition.x - newLadder.x))
                                    {
                                        firstDestination = newLadder;
                                        secondDestination = newLadderProp.topLadder.transform.position;
                                        pathTarget = RoomProperties.Type.ladder;
                                    }
                                }
                                else
                                {
                                    firstDestination = newLadder;
                                    secondDestination = newLadderProp.topLadder.transform.position;
                                    pathTarget = RoomProperties.Type.ladder;

                                }
                                break;
                            case RoomProperties.Type.stairs:
                                StairProperties newStairProp = room.upProperties.GetComponent<StairProperties>();
                                Vector2 newStair = newStairProp.bottomStair.transform.position;
                                if (firstDestination != null)
                                {
                                    if (Mathf.Abs(enemyPosition.x - firstDestination.x) > Mathf.Abs(enemyPosition.x - newStair.x))
                                    {
                                        firstDestination = newStair;
                                        secondDestination = newStairProp.topStair.transform.position;
                                        pathTarget = RoomProperties.Type.stairs;
                                    }
                                }
                                else
                                {
                                    firstDestination = newStair;
                                    secondDestination = newStairProp.topStair.transform.position;
                                    pathTarget = RoomProperties.Type.stairs;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                break;
            default:
                break;


        }
        enemyPath.Insert(0, secondDestination);
        enemyPath.Insert(0, firstDestination);
    }

    void Wander()
    {
        if (enemyPath.Count == 0) { wanderDestination = null; }
        if (wanderDestination)
        {

            moveToDestination();
        }
        else
        {
            enemyPath.Clear();
            int chance = Random.Range(0, 2);
            //Debug.Log("Chance: " + chance);
            int randomRoom = 0;
            switch (chance)
            {
                case 0:
                    randomRoom = Random.Range(0, HouseNew[enemyFloor].Count);
                    wanderDestination = HouseNew[enemyFloor][randomRoom];
                    enemyPath.Add(wanderDestination.center);
                    pathTarget = RoomProperties.Type.none;
                    break;
                case 1:
                    chance = Random.Range(0, 2);
                    switch (chance)
                    {
                        case 0:
                            if (enemyFloor - 1 >= 0)
                            {
                                randomRoom = Random.Range(0, HouseNew[enemyFloor - 1].Count);
                                wanderDestination = HouseNew[enemyFloor - 1][randomRoom];
                                findladder(0);
                                enemyPath.Add(wanderDestination.center);
                            }
                            break;
                        case 1:
                            if (enemyFloor + 1 < HouseNew.Count)
                            {
                                randomRoom = Random.Range(0, HouseNew[enemyFloor + 1].Count);
                                wanderDestination = HouseNew[enemyFloor + 1][randomRoom];
                                findladder(1);
                                enemyPath.Add(wanderDestination.center);
                            }
                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }
        }

        if (roomManager.PlayerRoom == enemyRoom && player.GetComponent<PlayerMovement>().IsHiding == false)
        {
            enemyState = State.Hunting;

            // audio
            audioManager.Play("enemyDetect", gameObject);
        }
    }

    void Hunt()
    {
        if (exitUnlocked) { huntTimer = huntTimerMax; }
        bool hiding = player.GetComponent<PlayerMovement>().IsHiding;
        if (roomManager.PlayerRoom == enemyRoom && !hiding)
        {
            huntTimer = huntTimerMax;
            enemyPath.Clear();
        }


        RoomProperties playerProp = roomManager.PlayerRoom.GetComponent<RoomProperties>();
        if (/*(roomManager.CurrentStair || roomManager.CurrentLadder) &&*/ player.transform.position.y > enemyPosition.y + 2f)
        {
            findladder(1);
        }
        else if (/*(roomManager.CurrentStair || roomManager.CurrentLadder) &&*/ player.transform.position.y < enemyPosition.y - 2f)
        {
            findladder(0);
        }
        else
        {
            pathTarget = RoomProperties.Type.none;

        }
        if (!hiding)
        {
            enemyPath.Add(new Vector2(player.transform.position.x, player.transform.position.y));
        }
        moveToDestination();
        if (enemyPath.Count == 0)
        {
            enemyState = State.Searching;
            searchTimer = searchTimerMax;
            searchTimer2 = searchTimer2Max;
        }
        else if (huntTimer < 0) 
        { 
            huntTimer = 0; enemyState = State.Wandering; wanderDestination = null;

            // audio
            audioManager.Play("enemyLost", gameObject);
        }
    }

    void Search()
    {
        if (!searchSpot)
        {
            Vector2 closeSpot = new Vector2(0, 0);
            foreach (GameObject hidingSpot in roomManager.HidingSpotList)
            {
                Vector2 Spot = new Vector2(hidingSpot.transform.position.x, hidingSpot.transform.position.y);

                if (Mathf.Abs(Spot.y - enemyPosition.y) < 2f)
                {
                    if (!searchSpot)
                    {
                        closeSpot = Spot;
                        searchSpot = hidingSpot;
                    }
                    else
                    {
                        if (Mathf.Abs(Spot.x - enemyPosition.x) < Mathf.Abs(closeSpot.x - enemyPosition.x))
                        {
                            closeSpot = Spot;
                            searchSpot = hidingSpot;
                        }
                    }
                    enemyPath.Clear();
                    enemyPath.Insert(0, closeSpot);
                }
            }

        }

        if (searchSpot)
        {
            moveToDestination();
            if (Mathf.Abs(enemyPosition.x - searchSpot.transform.position.x) < 0.6f)
            {
                enemyPosition.x = searchSpot.transform.position.x;
                searchTimer2 -= Time.deltaTime;
                if (searchTimer2 < 0)
                {
                    enemyZPosition = 1f;
                }
            }
        }
        if (huntTimer < 0)
        {
            enemyState = State.Wandering;
            enemyPath.Clear();
            wanderDestination = null;
            enemyZPosition = -0.25f;
        }
        searchTimer -= Time.deltaTime;
    }

    void Bang()
    {
        // animation
        animator.speed = 1.0f;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isClimbing", false);
        animator.SetBool("isScaling", false);
        // audio
        audioManager.EndLoopSound("enemyRun");
        audioManager.EndLoopSound("enemyWalk");
        audioManager.EndLoopSound("enemyStairs");
        audioManager.EndLoopSound("enemyLadder");
        if (Door == null) { return; }
        int direction = 0;
        if (Door.needKey)
        {
            if (enemyPosition.x > Door.gameObject.transform.position.x) { direction = -1; }
            else { direction = 1; }
            revertState();
            if (enemyFloor < HouseNew.Count && enemyRoom.GetComponent<RoomProperties>().room - direction < HouseNew[enemyFloor].Count)
            {
                enemyPath.Insert(0, HouseNew[enemyFloor][Mathf.Abs(enemyRoom.GetComponent<RoomProperties>().room - direction)].center);
            }
            wanderDestination = null;
        }
    }

    public void revertState()
    {
        switch (enemyState)
        {
            case State.Wandering:
                break;
            case State.Hunting:
                break;
            case State.Climbing:
                break;
            case State.Opening:
                Door = null;
                break;
            default:
                break;
        }
        if (enemyState == enemyStoredState && enemyStoredState == State.Opening) { enemyStoredState = State.Wandering; }
        enemyState = enemyStoredState;
    }

    public void changeState(State newState)
    {
        switch (newState)
        {
            case State.Wandering:
                break;
            case State.Hunting:
                break;
            case State.Climbing:
                break;
            default:
                break;
        }
    }

    void WanderGrunt()
    {
        if (gruntTimer <= 0.0f)
        {
            audioManager.PlayRandomWanderGrunt(gameObject);
            gruntTimer = 10.0f;
        }
        gruntTimer -= Time.deltaTime;
    }

    void HuntGrunt()
    {
        if (gruntTimer <= 0.0f)
        {
            audioManager.PlayRandomHuntGrunt(gameObject);
            gruntTimer = 3.0f;
        }
        gruntTimer -= Time.deltaTime;
    }
}
