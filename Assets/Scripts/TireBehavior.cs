using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{
    public Rigidbody wheel;
    public float torks;
    // Use this for initialization
    void Start()
    {
        wheel.maxAngularVelocity = 9000;


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey("n"))
        {
            wheel.AddTorque(new Vector3(torks,0,0), ForceMode.Acceleration);


        }


    }
    void Update()
    {

    }


}