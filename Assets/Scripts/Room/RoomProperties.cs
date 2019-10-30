using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomProperties : MonoBehaviour
{
    public enum Type {none,ladder,stairs}


    public bool upPath;
    public Type upType;
    public GameObject upProperties;
    public bool downPath;
    public Type downType;
    public GameObject downProperties;
    public int floor;
    public int room;

    private GameObject self;
    private Vector3 center3D;
    private Vector2 centerv;

	public Vector2 center
	{
		get { return centerv;}
	}

    private void Start()
    {
        self = gameObject;
        center3D = self.transform.Find("BottomWall").transform.position;
        centerv.x = center3D.x;
        centerv.y = center3D.y;
    }
}
