using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isFlickering = false;
    void Start()
    {
        if (isFlickering)
        {
            InvokeRepeating("lightSwitchAttempt", 0.5f, 0.075f);
        }
           
    }
    void lightSwitchAttempt()
    {
        if (Random.Range(1,6) <= 2)
        {
            FindObjectOfType<AudioManager>().Play("lightFlicker", gameObject);
            gameObject.GetComponent<Light>().enabled = !gameObject.GetComponent<Light>().enabled;
        }
    }
}
