using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarBehaviour : MonoBehaviour
{
    public GameObject FR;
    public GameObject FL;
    public GameObject RL;
    public GameObject RR;



    //debug values
    public float currentSpeed;
    public float wheelRPM;
    public float engineRPM;
    public bool runnin = true;

    public float debugValue1;
    public float debugValue2;
    public float debugValue3;

    //tuneable stats
    public float engineREDLINE = 9000;//engine redline - REDLINE AT 6000 IF TRUCK
    public AnimationCurve engineTorque = new AnimationCurve(new Keyframe(0, 130), new Keyframe(5000, 250), new Keyframe(9000, 200));
    public Vector3 centerOfGravity = new Vector3(0,0,0);
    public float[] gears = new float[8] { -5.0f, 0.0f, 5.4f, 3.4f, 2.7f, 2.0f, 1.8f, 1.6f };//gears


    void Start()
    {

    }

    void FixedUpdate()
    {

        gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * currentSpeed, ForceMode.Impulse);


        gameObject.GetComponent<Rigidbody>().centerOfMass = centerOfGravity;

    }

    void Update()
    {




    }



    void Engine()
    {//engine
        



    }
    


}