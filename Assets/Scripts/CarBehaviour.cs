using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarBehaviour : MonoBehaviour
{
    public Transform drivingWheel;
    public RectTransform pointer;
    //debug values
    public float currentSpeed;
    public float wheelRPM;
    public float engineRPM;
    public int gear;//current gear
    public bool shifting = false;//shifter delay
    public float clutchPressure;//0-1. clutch in/out. float for pedal support later on
    public bool runnin = true;

    public float debugValue1;
    public float debugValue2;
    public float debugValue3;

    //tuneable stats
    public float ratio; //final drive
    public float FrontWheelDriveBias = 0.5f;
    public float lsd = 1f;//clamped 0-1. 0 open 1 full lock
    public float engineREDLINE = 9000;//engine redline - REDLINE AT 6000 IF TRUCK
    public AnimationCurve engineTorque = new AnimationCurve(new Keyframe(0, 130), new Keyframe(5000, 250), new Keyframe(9000, 200));

    public float[] gears = new float[8] { -5.0f, 0.0f, 5.4f, 3.4f, 2.7f, 2.0f, 1.8f, 1.6f };//gears


    void Start()
    {
        

        
    }

    void FixedUpdate()
    {




    }

    void Update()
    {




    }



    void Engine()
    {//engine
        



    }
    


}