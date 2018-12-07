using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireBehavior : MonoBehaviour
{
    public SpringJoint spring;
    public Transform strut;

    // Use this for initialization
    void Start()
    {



    }

    // Update is called once per frame
    void FixedUpdate()
    {
        spring.connectedAnchor = strut.position + new Vector3(0, 0.2f, 0);


    }
    void Update()
    {

    }


}