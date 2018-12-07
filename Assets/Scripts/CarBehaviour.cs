using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarBehaviour : MonoBehaviour
{
    public Text speedDisplay;//output of speed to meter - by default MPH
    public Text gearDisplay;
    public Transform drivingWheel;
    public RectTransform pointer;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public WheelCollider engineCrank;
    public Transform wheelFLCalp;
    public Transform wheelFRCalp;
    public Transform wheelRLCalp;
    public Transform wheelRRCalp;
    public Transform wheelFLTrans;
    public Transform wheelFRTrans;
    public Transform wheelRLTrans;
    public Transform wheelRRTrans;
    //debug values
    public float currentSpeed;
    public float wheelRPM;
    public float engineRPM;
    public float unitOutput;
    public float turboSpool = 0.1f;//turbo boost value
    public bool spooled = false;//determine whether to play wastegate sound or not
    public int gear;//current gear
    public bool shifting = false;//shifter delay
    public float clutchPressure;//0-1. clutch in/out. float for pedal support later on
    public bool runnin = true;

    public float debugValue1;
    public float debugValue2;
    public float debugValue3;

    //tuneable stats
    public float aero = 15.0f; //aero - higher value = higher grip, but less accel/topspeed
    public float ratio; //final drive
    public bool manual = false; //manual(true) auto(false)
    /// <summary>
    /// How much power is being sent to the front wheels, as a ratio, can be used to change drivetrain
    /// 0: no power to front, 100% power to rear
    /// 0.5: half front, half rear
    /// 1: 100% front,  no rear
    /// </summary>
    public float FrontWheelDriveBias = 0.5f;
    // Having it as a ratio opens a whole lot of tricks, but mainly an easy way to allocate power for ALL drivetrains
    // FrontPower = engineOutput * FrontWheelDriveBias
    // RearPower = engineOutput * (1-FrontWheelDriveBias)
    // Chaning this while the car is driving is an effective way of having a center diff
    public float lsd = 1f;//clamped 0-1. 0 open 1 full lock
    public float engineREDLINE = 9000;//engine redline - REDLINE AT 6000 IF TRUCK
    public AnimationCurve engineTorque = new AnimationCurve(new Keyframe(0, 130), new Keyframe(5000, 250), new Keyframe(9000, 200));

    public float[] gears = new float[8] { -5.0f, 0.0f, 5.4f, 3.4f, 2.7f, 2.0f, 1.8f, 1.6f };//gears
    public int carIndex;//CARS INDEX IN DATA MANAGER
    public JointSpring springs = new JointSpring
    {
        spring = 8000,
        damper = 800,
        targetPosition = 0.1f,
    };
    public Vector3 CenterOfGravity = new Vector3(0, -0.4f, 0.5f);//CoG

    void Start()
    {

            engineRPM = 800;
            Physics.gravity = new Vector3(0, -aero, 0);
            GetComponent<Rigidbody>().centerOfMass = CenterOfGravity;
            gear = 1;

            wheelFR.ConfigureVehicleSubsteps(20, 30, 10);
            wheelFL.ConfigureVehicleSubsteps(20, 30, 10);
            wheelRL.ConfigureVehicleSubsteps(20, 30, 10);
            wheelRR.ConfigureVehicleSubsteps(20, 30, 10);


            HUDUpdate();

            manual = true;
        
    }

    void FixedUpdate()
    {
            StartCoroutine(Sparker());
            Engine();
            Differential();
            StartCoroutine(Clutch());

            wheelFR.steerAngle = 20 * Input.GetAxis("Steering");//steering
            wheelFL.steerAngle = 20 * Input.GetAxis("Steering");

            wheelRPM = (wheelFL.rpm + wheelRL.rpm) / 2; //speed counter
            currentSpeed = 2 * 22 / 7 * wheelFL.rpm * wheelFL.radius * 60 / 1000;
            currentSpeed = Mathf.Round(currentSpeed);
    }

    void Update()
    {
            StartCoroutine(Gearbox());//gearbox update 
            HUDUpdate();
            wheelFRTrans.Rotate(wheelFR.rpm / 60 * 360 * Time.deltaTime, 0, 0); //graphical updates
            wheelFLTrans.Rotate(wheelFL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelRRTrans.Rotate(wheelRR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelRLTrans.Rotate(wheelRL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelFRTrans.localEulerAngles = new Vector3(wheelFRTrans.localEulerAngles.x, wheelFR.steerAngle - wheelFRTrans.localEulerAngles.z, wheelFRTrans.localEulerAngles.z);
            wheelFLTrans.localEulerAngles = new Vector3(wheelFLTrans.localEulerAngles.x, wheelFL.steerAngle - wheelFLTrans.localEulerAngles.z, wheelFLTrans.localEulerAngles.z);
            wheelRRTrans.localEulerAngles = new Vector3(wheelRRTrans.localEulerAngles.x, wheelRR.steerAngle - wheelRRTrans.localEulerAngles.z, wheelRRTrans.localEulerAngles.z);
            wheelRLTrans.localEulerAngles = new Vector3(wheelRLTrans.localEulerAngles.x, wheelRL.steerAngle - wheelRLTrans.localEulerAngles.z, wheelRLTrans.localEulerAngles.z);

            wheelFLCalp.localEulerAngles = new Vector3(wheelFLCalp.localEulerAngles.x, wheelFL.steerAngle - wheelFLCalp.localEulerAngles.z, wheelFLCalp.localEulerAngles.z);
            wheelFRCalp.localEulerAngles = new Vector3(wheelFRCalp.localEulerAngles.x, wheelFR.steerAngle - wheelFRCalp.localEulerAngles.z, wheelFRCalp.localEulerAngles.z);
            wheelRLCalp.localEulerAngles = new Vector3(wheelRLCalp.localEulerAngles.x, wheelRL.steerAngle - wheelRLCalp.localEulerAngles.z, wheelRLCalp.localEulerAngles.z);
            wheelRRCalp.localEulerAngles = new Vector3(wheelRRCalp.localEulerAngles.x, wheelRR.steerAngle - wheelRRCalp.localEulerAngles.z, wheelRRCalp.localEulerAngles.z);

            WheelPosition(); //graphical update - wheel positions 
            drivingWheel.transform.localEulerAngles = new Vector3(drivingWheel.transform.rotation.x, drivingWheel.transform.rotation.y, -90 * Input.GetAxis("Steering"));
    }

    IEnumerator Sparker()
    {
        if (Input.GetKeyDown("p") && !runnin)
        {
            engineCrank.brakeTorque = 0;
            engineCrank.motorTorque = 700;
            yield return new WaitForSeconds(0.2f);
            runnin = true;
        }
        else if (Input.GetKeyDown("p") && runnin)
        {
            engineCrank.brakeTorque = 10;
            engineCrank.motorTorque = 0;
            runnin = false;
        }

    }

    void Engine()
    {//engine

        //Input.GetAxis("Throttle")
        if (runnin)
        {
            if (Input.GetAxis("Throttle") > 0)
            {
                engineCrank.motorTorque = engineTorque.Evaluate(engineRPM) * Input.GetAxis("Throttle");
            }
            else
            {
                engineCrank.motorTorque = 0;
            }

            if (engineRPM < 1000)
            {
                //idle
                engineCrank.motorTorque = 30;
                unitOutput = engineTorque.Evaluate(engineRPM);
            }
            if (engineRPM < 500)
            {
                //engine post
                runnin = false;
            }
            

        }

        if (engineRPM > engineREDLINE)
        {//rev limiter
            engineCrank.brakeTorque = 10;
            engineCrank.motorTorque = 0;
        }
        engineRPM = engineCrank.rpm;
        unitOutput = engineTorque.Evaluate(engineRPM);

        
        if (gear != 1)
        {
            debugValue1 = wheelRPM / ratio / gears[gear];
            debugValue2 = engineRPM / ratio / gears[gear];
        }
        debugValue3 = engineRPM * 2;






        //turbo manager
    }

    IEnumerator Clutch()
    {
        if (manual)
        {
            clutchPressure = Input.GetAxis("Clutch");
            //code for manual
            //float CLUTCH CLAMPED TO 0-1 DISENGAGED-ENGAGED
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            //code for auto clutch
        }
    }

    void Differential()
    {
        wheelRL.motorTorque = unitOutput * gears[gear];
        wheelRR.motorTorque = unitOutput * gears[gear];
    }
    //* ratio / gears[gear]
    //engineTorque.Evaluate(engineRPM) 
    // Gearbox managed, called each frame
    IEnumerator Gearbox()
    {
        if (manual)
        {
            if (Input.GetButtonDown("ShiftUp") && gear < gears.Length - 1 && !shifting && clutchPressure > 0.4f)
            {
                shifting = true;
                gear = gear + 1;
                yield return new WaitForSeconds(0.1f);
                shifting = false;
            }
            if (Input.GetButtonDown("ShiftDown") && gear > 0 && !shifting && clutchPressure > 0.4f)
            {
                shifting = true;
                gear = gear - 1;
                yield return new WaitForSeconds(0.1f);
                shifting = false;
            }
        }//END MANUAL
        else
        {
            //autotragic
        }
    }


    // Graphical update - wheel positions 
    void WheelPosition()
    {
        RaycastHit hit;
        Vector3 wheelPos;
        //FL
        if (Physics.Raycast(wheelFL.transform.position, -wheelFL.transform.up, out hit, wheelFL.radius + wheelFL.suspensionDistance))
        {
            wheelPos = hit.point + wheelFL.transform.up * wheelFL.radius;
        }
        else
        {
            wheelPos = wheelFL.transform.position - wheelFL.transform.up * wheelFL.suspensionDistance;
        }
        wheelFLCalp.position = wheelPos;
        wheelFLTrans.position = wheelPos;
        //FR
        if (Physics.Raycast(wheelFR.transform.position, -wheelFR.transform.up, out hit, wheelFR.radius + wheelFR.suspensionDistance))
        {
            wheelPos = hit.point + wheelFR.transform.up * wheelFR.radius;
        }
        else
        {
            wheelPos = wheelFR.transform.position - wheelFR.transform.up * wheelFR.suspensionDistance;
        }
        wheelFRCalp.position = wheelPos;
        wheelFRTrans.position = wheelPos;
        //RL
        if (Physics.Raycast(wheelRL.transform.position, -wheelRL.transform.up, out hit, wheelRL.radius + wheelRL.suspensionDistance))
        {
            wheelPos = hit.point + wheelRL.transform.up * wheelRL.radius;
        }
        else
        {
            wheelPos = wheelRL.transform.position - wheelRL.transform.up * wheelRL.suspensionDistance;
        }
        wheelRLCalp.position = wheelPos;
        wheelRLTrans.position = wheelPos;
        //RR
        if (Physics.Raycast(wheelRR.transform.position, -wheelRR.transform.up, out hit, wheelRR.radius + wheelRR.suspensionDistance))
        {
            wheelPos = hit.point + wheelRR.transform.up * wheelRR.radius;
        }
        else
        {
            wheelPos = wheelRR.transform.position - wheelRR.transform.up * wheelRR.suspensionDistance;
        }
        wheelRRCalp.position = wheelPos;
        wheelRRTrans.position = wheelPos;

    }

    // Updates the HUD
    void HUDUpdate()
    {
        float speedFactor = engineRPM / engineREDLINE;//dial rotation
        float rotationAngle = 0;
        if (engineRPM >= 0)
        {
            rotationAngle = Mathf.Lerp(70, -160, speedFactor);
            pointer.eulerAngles = new Vector3(0, 0, rotationAngle);
        }//end dial rot

        if (currentSpeed < 0)//cancelling negative integers, speed
        {
            speedDisplay.text = (currentSpeed * -1).ToString();
        }
        else
        {
            speedDisplay.text = currentSpeed.ToString();
        }
        if (shifting)
        {
            gearDisplay.text = "-";
        }
        else
        {
            //gears
            if (gear == 0)
            {
                gearDisplay.text = "R".ToString();//reverse gear
            }
            else if (gear == 1)
            {
                gearDisplay.text = "N".ToString();//neutral
            }
            else
            {
                gearDisplay.text = (gear - 1).ToString();//array value, minus 1
            }
        }
    }
}