using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public Door door;
    private bool grabbed;

    // Start is called before the first frame update
    void Start()
    {
        grabbed = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GrabKey()
    {
        if(!grabbed)
        {
            grabbed = true;
            door.OpenLock();
            Destroy(gameObject);
            Destroy(this);
        }
    }
}
