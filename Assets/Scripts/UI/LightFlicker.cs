using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("lightSwitchAttempt", 1.0f, 3.0f);   
    }
    void lightSwitchAttempt()
    {
        if (Random.Range(1,10) <= 2)
        {
            gameObject.GetComponent<Light>().enabled = !gameObject.GetComponent<Light>().enabled;
        }
    }
}
