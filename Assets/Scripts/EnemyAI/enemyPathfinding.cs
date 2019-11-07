using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class enemyPathfinding : MonoBehaviour
{

    //Public:
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
    float enemySpeed = 4.5f; //Enemy Speed    Climbing speed is half.
    RoomManager roomManager;

    bool exitUnlocked = false;
    float huntTimerMax = 7.5f;
    float huntTimer = 0;
    GameObject searchSpot;
    float searchTimerMax = 5f;
    float searchTimer = 0;
    RoomProperties.Type pathTarget = RoomProperties.Type.none;


    class Point
    {
        public Point()
        {
            position = new Vector2(0.0f, 0.0f);
            type = RoomProperties.Type.none;
        }
        public Point(Vector2 position, RoomProperties.Type type)
        {
            this.position = position;
            this.type = type;
        }
        public Vector2 position;
        public RoomProperties.Type type;
    }
    List<Point> enemyPath = new List<Point>();
    RoomProperties wanderDestination;
    RoomProperties previousWanderDestination;
    /// <summary>
    /// House Consists of several Lists:   From Outer to Inner:
    /// 1. List of Floors
    /// 2. List of Rooms
    /// 3. List of Room Variables
    ///     3a. Vector2D (Ladder: 0=No/1=Yes, Direction: 0=Down/1=Up)
    ///     3b. Vector2D (Center of Room)
    /// </summary>
    public List<List<RoomProperties>> house = new List<List<RoomProperties>>();

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
    public State EnemyState
    {
        get { return enemyState; }
        set { enemyStoredState = enemyState; enemyState = value; }
    }

    public State EnemyStoredState
    {
        get { return enemyStoredState; }
    }

    public float HuntTimer
    {
        get { return huntTimer; }
        set { huntTimer = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        house = roomManager.BuildHouse();
        enemyState = State.Wandering;
        previousWanderDestination = roomManager.EnemyRoomList[0].GetComponent<RoomProperties>();

        enemyPosition = transform.position;
        enemyZPosition = -0.25f;

        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        huntTimer = huntTimerMax;


        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        foreach (Transform child in transform)
        {
            if (child.name == "enemyModel")
            {
                animator = child.GetComponent<Animator>();
                break;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // wandered out of the game world, should not happen
        if (enemyRoom == null)
        {
            enemyPosition = house[0][0].center;
            enemyState = State.Wandering;
            enemyPath.Clear();
            wanderDestination = null;
        }

        // different behaviors depending on state
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
            case State.Searching: // NOT IN USE
                Search();
                break;
            default:
                enemyState = State.Wandering;
                break;
        }

        //Once done all code: Update enemy Gameobject position
        transform.position = new Vector3(enemyPosition.x, enemyPosition.y, enemyZPosition);
    }

    void moveToDestination()
    {
        if (enemyPath.Count == 0) { return; } // nowhere to go to

        // debug lines showing route
        Color color;
        for (int i = 0; i < enemyPath.Count - 1; i++)
        {
            if (enemyPath[i].type == RoomProperties.Type.stairs)
            {
                color = new Color(0, 0, 255);
            }
            else if (enemyPath[i].type == RoomProperties.Type.ladder)
            {
                color = new Color(0, 255, 0);
            }
            else
            {
                color = new Color(255, 0, 0);
            }
            if (i == 0)
            {
                Debug.DrawLine(transform.position, enemyPath[i].position, Color.red, .01f);
            }
            Debug.DrawLine(enemyPath[i].position, enemyPath[i + 1].position, color, .01f);
        }


        Vector2 direction;

        // animation
        Quaternion currentOrientation;
        Quaternion targetOrientation;
        switch (enemyState)
        {
            case State.Wandering:
                direction = new Vector2(enemyPath[0].position.x - enemyPosition.x, 0).normalized;
                enemyPosition.x += direction.x * (enemySpeed / 1.5f) * Time.deltaTime;
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
                Point reachedDestination = null;

                switch (pathTarget)
                {
                    case RoomProperties.Type.none:
                        if (Mathf.Abs(enemyPosition.x - enemyPath[0].position.x) < 0.05f)
                        {
                            enemyPath.RemoveAt(0);
                        }
                        break;
                    case RoomProperties.Type.ladder:
                        if (Mathf.Abs(enemyPosition.x - enemyPath[0].position.x) < 0.05f)
                        {
                            animator.speed = 1.0f;
                            enemyPath.RemoveAt(0);
                            enemyStoredState = State.Wandering;
                            enemyState = State.Climbing;
                        }
                        break;
                    case RoomProperties.Type.stairs:
                        if (Mathf.Abs(enemyPosition.x - enemyPath[0].position.x) < 0.05f)
                        {
                            animator.speed = 1.0f;
                            enemyPath.RemoveAt(0);
                            enemyStoredState = State.Wandering;
                            enemyState = State.Climbing;
                        }
                        break;
                    default:
                        break;
                }
                previousWanderDestination = enemyRoom.GetComponent<RoomProperties>();
                enemyZPosition = -.25f;
                break;
            case State.Hunting:
                direction = new Vector2(enemyPath[0].position.x - enemyPosition.x, 0).normalized;
                enemyPosition.x += direction.x * (enemySpeed) * Time.deltaTime;

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

                        if (Mathf.Abs(enemyPosition.x - enemyPath[0].position.x) < 0.05f)
                        {
                            enemyPath.RemoveAt(0);
                        }
                        break;
                    case RoomProperties.Type.ladder:
                        if (Mathf.Abs(enemyPosition.x - enemyPath[0].position.x) < 0.05f)
                        {
                            animator.speed = 1.5f;
                            enemyPath.RemoveAt(0);
                            enemyStoredState = State.Hunting;
                            enemyState = State.Climbing;
                        }
                        break;
                    case RoomProperties.Type.stairs:
                        if (Mathf.Abs(enemyPosition.x - enemyPath[0].position.x) < 0.05f)
                        {
                            animator.speed = 1.5f;
                            enemyPath.RemoveAt(0);
                            enemyStoredState = State.Hunting;
                            enemyState = State.Climbing;
                        }
                        break;
                    default:
                        break;
                }
                enemyZPosition = -.25f;
                break;
            case State.Climbing:
                float movementModifier = 2.0f; // move faster when hunting, slower when wandering
                if (enemyStoredState == State.Hunting)
                {
                    movementModifier = 1.5f;
                    animator.speed = 1.5f;
                    HuntGrunt();
                    if (!DetectPlayerHunt()) // doesn't directly see the player
                    {
                        huntTimer -= Time.deltaTime;
                    }
                    else
                    {
                        huntTimer = huntTimerMax;
                    }

                    // lost track of the player completely
                    if (huntTimer <= 0)
                    {
                        // audio
                        audioManager.Play("enemyLost", gameObject);
                        enemyStoredState = State.Wandering;
                        huntTimer = huntTimerMax;
                        animator.speed = 1.0f;
                        movementModifier = 2.0f;
                    }
                }
                else if (enemyStoredState == State.Wandering)
                {
                    movementModifier = 2.0f;
                    animator.speed = 1.0f;
                    WanderGrunt();
                    if (DetectPlayerWander()) // detects the player, time to hunt
                    {
                        // audio
                        audioManager.Play("enemyDetect", gameObject);
                        enemyStoredState = State.Hunting;
                        movementModifier = 1.5f;
                    }
                }
                switch (pathTarget)
                {
                    case RoomProperties.Type.ladder:
                        enemyZPosition = 0.5f;
                        direction = new Vector2(0, enemyPath[0].position.y - enemyPosition.y).normalized;
                        // animation
                        animator.SetBool("isWalking", false);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", true);
                        animator.SetBool("isScaling", false);
                        // face correct direction
                        transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);

                        enemyPosition.y += direction.y * (enemySpeed / movementModifier) * Time.deltaTime;
                        if (Mathf.Abs(enemyPosition.y - enemyPath[0].position.y) < 0.05f)
                        {
                            enemyPosition = enemyPath[0].position;
                            enemyPath.RemoveAt(0);
                            enemyState = enemyStoredState;
                            pathTarget = RoomProperties.Type.none;
                        }

                        // audio
                        audioManager.EndLoopSound("enemyRun");
                        audioManager.EndLoopSound("enemyWalk");
                        audioManager.EndLoopSound("enemyStairs");
                        audioManager.PlayLoopSound("enemyLadder", gameObject);
                        break;
                    case RoomProperties.Type.stairs:
                        enemyZPosition = 1.25f;
                        direction = (enemyPath[0].position - enemyPosition).normalized;
                        // animation
                        animator.SetBool("isWalking", false);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isScaling", true);
                        // face correct direction
                        if (direction.x > 0)
                        {
                            transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
                        }
                        else
                        {
                            transform.rotation = Quaternion.Euler(transform.rotation.x, -90, transform.rotation.z);
                        }

                        enemyPosition += direction * (enemySpeed / movementModifier) * Time.deltaTime;
                        if ((enemyPosition - enemyPath[0].position).magnitude < 0.05f)
                        {
                            enemyPosition = enemyPath[0].position;
                            enemyPath.RemoveAt(0);
                            enemyState = enemyStoredState;
                            pathTarget = RoomProperties.Type.none;
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
                break;
            default:
                break;
        }

        if (enemyPath.Count == 0) { wanderDestination = null; }
    }

    /// <summary>
    /// Determine which climbables are the best to get to target floor.
    /// difference = target - current
    /// </summary>
    /// <param name="difference"></param>
    void DetermineClimables(int difference)
    {
        // a list of all of the climables the enemy has to use to get to the desired floor
        // the inner list contains [0] == starting position of that climable, [1] == ending position of that climable
        List<List<Point>> climbables = new List<List<Point>>(new List<Point>[Mathf.Abs(difference)]);

        // going down
        if (difference < 0)
        {
            // loop until the target floor, go down
            for (int i = 0; i < Mathf.Abs(difference); i++)
            {
                climbables[i] = new List<Point>(new Point[2]);
                climbables[i][0] = new Point();
                climbables[i][1] = new Point();
                // loop through each room on this floor
                foreach (RoomProperties room in house[-i + enemyFloor])
                {
                    // can go down by ladder
                    if (room.downPath && room.downType == RoomProperties.Type.ladder)
                    {
                        LadderProperties newLadderProperties = room.downProperties.GetComponent<LadderProperties>();
                        Vector2 newLadderTop = newLadderProperties.topLadder.transform.position;
                        Vector2 newLadderBottom = newLadderProperties.bottomLadder.transform.position;

                        if (climbables[i][0].position != new Vector2(0, 0)) // already set, check if a closer one is found
                        {
                            bool closer;
                            if (i > 0) // compare to previous climable instead of enemyPosition;
                            {
                                closer = Mathf.Abs(climbables[i - 1][1].position.x - climbables[i][0].position.x) > Mathf.Abs(climbables[i - 1][1].position.x - newLadderBottom.x);
                            }
                            else // compare to enemy position
                            {
                                closer = Mathf.Abs(enemyPosition.x - climbables[i][0].position.x) > Mathf.Abs(enemyPosition.x - newLadderTop.x);
                            }

                            if (closer)
                            {
                                // found a closer ladder
                                climbables[i][0] = new Point(newLadderTop, RoomProperties.Type.ladder);
                                climbables[i][1] = new Point(newLadderBottom, RoomProperties.Type.none);
                            }
                        }
                        else // not set, so set as the first one found (not necessarily the closest)
                        {
                            // first ladder found
                            climbables[i][0] = new Point(newLadderTop, RoomProperties.Type.ladder);
                            climbables[i][1] = new Point(newLadderBottom, RoomProperties.Type.none);
                        }
                    }
                    else if (room.downPath && room.downType == RoomProperties.Type.stairs) // can go down by stairs
                    {
                        StairProperties newStairProperties = room.downProperties.GetComponent<StairProperties>();
                        Vector2 newStairTop = newStairProperties.topStair.transform.position;
                        Vector2 newStairBottom = newStairProperties.bottomStair.transform.position;


                        if (climbables[i][0].position != new Vector2(0, 0)) // already set, so check if a closer one is found
                        {
                            bool closer;
                            if (i > 0) // compare to previous climable instead of enemyPosition;
                            {
                                closer = Mathf.Abs(climbables[i - 1][1].position.x - climbables[i][0].position.x) > Mathf.Abs(climbables[i - 1][1].position.x - newStairBottom.x);
                            }
                            else // compare to enemy position
                            {
                                closer = Mathf.Abs(enemyPosition.x - climbables[i][0].position.x) > Mathf.Abs(enemyPosition.x - newStairTop.x);
                            }

                            if (closer)
                            {
                                // found a closer stair
                                climbables[i][0] = new Point(newStairTop, RoomProperties.Type.stairs);
                                climbables[i][1] = new Point(newStairBottom, RoomProperties.Type.none);
                            }
                        }
                        else // not set, so set as the first one found (not necessarily the closest)
                        {
                            // first stair found
                            climbables[i][0] = new Point(newStairTop, RoomProperties.Type.stairs);
                            climbables[i][1] = new Point(newStairBottom, RoomProperties.Type.none);
                        }
                    }
                }
            }
        }
        else if (difference > 0) // going up
        {
            // loop until the target floor, go up
            for (int i = 0; i < difference; i++)
            {
                climbables[i] = new List<Point>(new Point[2]);
                climbables[i][0] = new Point();
                climbables[i][1] = new Point();
                // loop through each room on this floor
                foreach (RoomProperties room in house[i + enemyFloor])
                {
                    // has a way to go up by ladder
                    if (room.upPath && room.upType == RoomProperties.Type.ladder)
                    {
                        LadderProperties newLadderProperties = room.upProperties.GetComponent<LadderProperties>();
                        Vector2 newLadderBottom = newLadderProperties.bottomLadder.transform.position;
                        Vector2 newLadderTop = newLadderProperties.topLadder.transform.position;

                        // already set top ladder, so find a closer one
                        if (climbables[i][0].position != new Vector2(0, 0))
                        {
                            bool closer;
                            if (i > 0) // compare to previous climable instead of enemyPosition;
                            {
                                closer = Mathf.Abs(climbables[i - 1][1].position.x - climbables[i][0].position.x) > Mathf.Abs(climbables[i - 1][1].position.x - newLadderTop.x);
                            }
                            else // compare to enemy position
                            {
                                closer = Mathf.Abs(enemyPosition.x - climbables[i][0].position.x) > Mathf.Abs(enemyPosition.x - newLadderBottom.x);
                            }
                            // found a closer ladder
                            if (closer)
                            {
                                climbables[i][0] = new Point(newLadderBottom, RoomProperties.Type.ladder);
                                climbables[i][1] = new Point(newLadderTop, RoomProperties.Type.none);
                            }
                        }
                        else // haven't found a ladder, so set the first one found (not necessarily the closest)
                        {
                            climbables[i][0] = new Point(newLadderBottom, RoomProperties.Type.ladder);
                            climbables[i][1] = new Point(newLadderTop, RoomProperties.Type.none);
                        }
                    }
                    else if (room.upPath && room.upType == RoomProperties.Type.stairs) // has a way to go up by stair
                    {
                        StairProperties newStairProperties = room.upProperties.GetComponent<StairProperties>();
                        Vector2 newStairBottom = newStairProperties.bottomStair.transform.position;
                        Vector2 newStairTop = newStairProperties.topStair.transform.position;

                        //already set top stair, so find a closer one
                        if (climbables[i][0].position != new Vector2(0, 0))
                        {
                            bool closer;
                            if (i > 0) // compare to previous climable instead of enemyPosition;
                            {
                                closer = Mathf.Abs(climbables[i - 1][1].position.x - climbables[i][0].position.x) > Mathf.Abs(climbables[i - 1][1].position.x - newStairTop.x);
                            }
                            else // compare to enemy position
                            {
                                closer = Mathf.Abs(enemyPosition.x - climbables[i][0].position.x) > Mathf.Abs(enemyPosition.x - newStairBottom.x);
                            }
                            // found a closer stair
                            if (closer)
                            {
                                climbables[i][0] = new Point(newStairBottom, RoomProperties.Type.stairs);
                                climbables[i][1] = new Point(newStairTop, RoomProperties.Type.none);
                            }
                        }
                        else // haven't found a stair, so set the first one found (not necessarily the closest)
                        {
                            climbables[i][0] = new Point(newStairBottom, RoomProperties.Type.stairs);
                            climbables[i][1] = new Point(newStairTop, RoomProperties.Type.none);
                        }
                    }
                }
            }
        }


        // loop through each floor in backward order (add the furthest away floor last, the closest floor first)
        for (int i = climbables.Count - 1; i >= 0; i--)
        {
            // the end of the climable should be the 2nd point on path
            // the start of the climable should be the 1st point on path
            enemyPath.Insert(0, climbables[i][1]);
            enemyPath.Insert(0, climbables[i][0]);
        }
        pathTarget = enemyPath[0].type;
    }

    private void GenerateNewWanderPath()
    {
        // empty the path
        enemyPath.Clear();

        // get a random floor
        int randomFloor = Random.Range(0, house.Count);
        // get a random room on floor
        int randomRoom = Random.Range(0, house[randomFloor].Count);
        wanderDestination = house[randomFloor][randomRoom];
        enemyPath.Add(new Point(wanderDestination.center, RoomProperties.Type.none));

        DetermineClimables(randomFloor - enemyFloor);
        return;
    }

    public void SendBack()
    {
        // reset path
        enemyPath.Clear();
        wanderDestination = previousWanderDestination; // set destination as the previous one
        enemyPath.Add(new Point(wanderDestination.center, RoomProperties.Type.none));

        DetermineClimables(wanderDestination.floor - enemyFloor);
    }

    private void AvoidWalls()
    {
        // prevent wall collision
        foreach (Transform child in enemyRoom.transform)
        {
            if (child.name == "RightWall" || child.name == "LeftWall")
            {
                if (child.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
                {
                    SendBack();
                }
            }
        }
    }

    void Wander()
    {
        // nowhere to go to
        if (enemyPath.Count == 0) { wanderDestination = null; }

        AvoidWalls();

        // somewhere to go to
        if (wanderDestination)
        {
            moveToDestination();
        }
        else // nowhere to go to
        {
            GenerateNewWanderPath();
        }

        //Debug.DrawRay(GetComponent<Collider>().bounds.center, transform.TransformDirection(Vector3.forward) * 20, Color.yellow, 10);
        if (DetectPlayerWander())
        {
            enemyState = State.Hunting;
            enemyPath.Clear();

            // audio
            audioManager.Play("enemyDetect", gameObject);
        }
    }

    void Hunt()
    {
        if (exitUnlocked) { huntTimer = huntTimerMax; }


        RoomProperties playerProp = roomManager.PlayerRoom.GetComponent<RoomProperties>();
        enemyPath.Clear();
        enemyPath.Add(new Point(new Vector2(player.transform.position.x, player.transform.position.y), RoomProperties.Type.none));
        DetermineClimables(roomManager.PlayerRoom.GetComponent<RoomProperties>().floor - enemyRoom.GetComponent<RoomProperties>().floor);

        // player on ladder/stair in same room
        if ((roomManager.CurrentLadder || roomManager.CurrentStair) && 
            (player.GetComponent<PlayerMovement>().SpecialPlayerState != SpecialPlayerState.OnLadder || 
            player.GetComponent<PlayerMovement>().SpecialPlayerState != SpecialPlayerState.Stairs) 
            && roomManager.PlayerRoom == enemyRoom)
        {
            Point endPoint = new Point();
            Point startPoint = new Point();

            if (roomManager.CurrentLadder) // ladder 
            {
                startPoint.type = RoomProperties.Type.ladder;
                // above 
                if (player.transform.position.y > GetComponent<Collider>().bounds.max.y)
                {
                    endPoint.position = roomManager.CurrentLadder.GetComponent<LadderProperties>().topLadder.transform.position;
                    startPoint.position = roomManager.CurrentLadder.GetComponent<LadderProperties>().bottomLadder.transform.position;
                }
                else if (player.transform.position.y < GetComponent<Collider>().bounds.min.y) // bellow
                {
                    endPoint.position = roomManager.CurrentLadder.GetComponent<LadderProperties>().bottomLadder.transform.position;
                    startPoint.position = roomManager.CurrentLadder.GetComponent<LadderProperties>().topLadder.transform.position;
                }
            }
            else if (roomManager.CurrentStair) // stairs
            {
                startPoint.type = RoomProperties.Type.stairs;
                // above 
                if (player.transform.position.y > GetComponent<Collider>().bounds.max.y)
                {
                    endPoint.position = roomManager.CurrentStair.GetComponent<StairProperties>().topStair.transform.position;
                    startPoint.position = roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.transform.position;
                }
                else if (player.transform.position.y < GetComponent<Collider>().bounds.min.y) // bellow
                {
                    endPoint.position = roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.transform.position;
                    startPoint.position = roomManager.CurrentStair.GetComponent<StairProperties>().topStair.transform.position;
                }
            }

            // have set the values
            if (startPoint.position != endPoint.position)
            {
                // no longer need old route
                enemyPath.Clear();
                // the end of the climable should be the 2nd point on path
                // the start of the climable should be the 1st point on path
                enemyPath.Add(startPoint);
                enemyPath.Add(endPoint);

                pathTarget = enemyPath[0].type;

            }
        }

        moveToDestination();

        // enemy can only lose track if he doesn't see the player or if the player isn't close for an extended period of time
        if (!DetectPlayerHunt())
        {
            Debug.Log("Counting down");
            huntTimer -= Time.deltaTime;
            if (huntTimer <= 0) // enemy hasn't seen player in a while
            {
                Debug.Log("Searching");
                // start searching
                enemyState = State.Searching; // NOT IN USE
                enemyState = State.Wandering;
                searchTimer = searchTimerMax;
                huntTimer = huntTimerMax;
                enemyPath.Clear();

                // audio
                audioManager.Play("enemyLost", gameObject);
            }

        }
        else // enemy can see the player
        {
            huntTimer = huntTimerMax;
        }
    }


    /// <summary>
    /// Detects the player if they aren't hiding and either the enemy can see the player or the player is relatively close to the enemy
    /// </summary>
    /// <returns></returns>
    private bool DetectPlayerWander()
    {
        float distance = 7.0f;
        if (enemyState == State.Climbing)
        {
            distance = 2.5f;
        }
        foreach (Transform child in player.transform)
        {
            if (child.name == "Flashlight")
            {
                if (child.GetComponent<Light>().enabled)
                {
                    distance = distance * 1.25f;
                }
                break;
            }
        }

        RaycastHit hit;
        int mask = ~(1 << 2);
        // this allows the enemy to detect the player if they are behind them in the same room
        bool inSameRoomAndClose = roomManager.PlayerRoom == enemyRoom && Vector3.Distance(player.transform.position, transform.position) < distance;
        // this allows the enemy to see the player through a door frame
        bool facingPlayer = Physics.Raycast(GetComponent<Collider>().bounds.center, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask) && //enemy shoots a ray in their forward direction
            hit.transform.tag == "Player" &&  //enemy ray hits the player
            Vector3.Distance(player.transform.position, transform.position) < distance * 1.35f;
        Debug.DrawRay(GetComponent<Collider>().bounds.center, transform.TransformDirection(Vector3.forward) * distance * 1.5f, Color.cyan , 10);

        if (!player.GetComponent<PlayerMovement>().IsHiding && (inSameRoomAndClose || facingPlayer))
        {
            Debug.Log("Spotted the player");
        }

        return !player.GetComponent<PlayerMovement>().IsHiding && (inSameRoomAndClose || facingPlayer);
    }
    /// <summary>
    /// Checks if the enemy can see the player or is in the same room and relatively close to the player
    /// </summary>
    /// <returns></returns>
    private bool DetectPlayerHunt()
    {
        float distance = 9.0f;
        if (enemyState == State.Climbing)
        {
            distance = 4.0f;
        }
        foreach (Transform child in player.transform)
        {
            if (child.name == "Flashlight")
            {
                if (child.GetComponent<Light>().enabled)
                {
                    distance = distance * 1.25f;
                }
                break;
            }
        }
        RaycastHit hit;
        int mask = ~(1 << 2);
        // this allows the enemy to detect the player if they are behind them in the same room
        bool inSameRoomAndClose = roomManager.PlayerRoom == enemyRoom && Vector3.Distance(player.transform.position, transform.position) < distance;
        // this allows the enemy to see the player through a door frame
        bool facingPlayer = Physics.Raycast(GetComponent<Collider>().bounds.center, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask) && //enemy shoots a ray in their forward direction
            hit.transform.tag == "Player" &&  //enemy ray hits the player
            Vector3.Distance(player.transform.position, transform.position) < distance * 1.35f;

        if (inSameRoomAndClose || facingPlayer)
        {
            Debug.Log("Spotted the player");
        }

        return inSameRoomAndClose || facingPlayer;
    }

    /// <summary>
    /// Not used currently, the enemy simply returns to wandering instead
    /// </summary>
    void Search()
    {
        HuntGrunt();
        // no area to search
        if (enemyPath.Count == 0)
        {
            List<GameObject> adjacentRooms = roomManager.GetAdjacentRooms(enemyRoom, house);
            List<RoomProperties> randomRooms = new List<RoomProperties>();
            while (randomRooms.Count < 3) // get 3 random rooms
            {
                GameObject randomRoom = adjacentRooms[Random.Range(0, adjacentRooms.Count)]; // get a random room

                if (!randomRooms.Contains(randomRoom.GetComponent<RoomProperties>())) // no duplicate rooms allowed
                {
                    randomRooms.Add(randomRoom.GetComponent<RoomProperties>());
                }
            }

            // create a new path
            for (int i = 0; i < randomRooms.Count; i++)
            {
                enemyPath.Add(new Point(randomRooms[i].center, RoomProperties.Type.none));
                if (i == 0)
                {
                    DetermineClimables(randomRooms[i].floor - enemyFloor); // get to the room from where you are right now
                }
                else
                {
                    DetermineClimables(randomRooms[i].floor - randomRooms[i - 1].floor); // get to the room from where you will be
                }

            }
        }
        else // search the spot
        {
            AvoidWalls();
            moveToDestination();
        }
        if (searchTimer <= 0)
        {
            Debug.Log("End Searching");
            enemyState = State.Wandering;
            enemyPath.Clear();
            wanderDestination = null;
            searchTimer = searchTimerMax;

            // audio
            audioManager.Play("enemyLost", gameObject);
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
            if (enemyFloor < house.Count && enemyRoom.GetComponent<RoomProperties>().room - direction < house[enemyFloor].Count)
            {
                enemyPath.Insert(0, new Point(house[enemyFloor][Mathf.Abs(enemyRoom.GetComponent<RoomProperties>().room - direction)].center, RoomProperties.Type.none));
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
