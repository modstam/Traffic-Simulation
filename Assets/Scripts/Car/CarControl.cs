using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour
{

    private float speedLimit = 0;
    //private Rigidbody myBody;
    public float distanceTraveled = 0;
    public float targetDistance = 0;
    private Vector3 startPos;
    private Vector3 targetDirection;
    private bool going = false; //When freely moving straight towards a node
    public bool traversing = false;  //When traveling along a curved edge
    private bool paused = false; //when temporarily stopping because of obstacle/traffic light

    private Edge curEdge;
    public float edgeProgress = 0f;
    public float edgeTime = 0f;
    public bool goingReverse;

    public Transform graphicTransform;
    public Car myCar;

    private Vector3 previousPos;
    private int rotationUpdateCounter;
    private int rotationIntensity = 7; //rotate car every 7th frame


    void Awake()
    {
        startPos = transform.position;
    }

    // Use this for initialization
    void Start()
    {
        //transform.rotation = Quaternion.LookRotation(new Vector3(1,0,0));
        //Go (1000, 20, new Vector3(0,0,1));
    }

    public void Go(float distance, float speedLimit, Vector3 direction)
    {
        targetDirection = direction;
        //if (direction.y != 0f)
            //Debug.Log("New direction: " + direction);
        transform.rotation = Quaternion.LookRotation(direction);
        startPos = transform.position;
        distanceTraveled = 0;
        this.speedLimit = speedLimit;
        targetDistance = distance;
        //myBody.velocity = speedLimit * direction;
        going = true;
    }

    void Stop()
    {
        //myBody.velocity = Vector3.zero;
        speedLimit = 0f;
        traversing = false;
        going = false;
        myCar.onStop();
    }

    public void pause()
    {
        paused = true;
    }

    public void resume()
    {
        paused = false;
    }

    public void delayedResume()
    {
        Invoke("resume", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused)
        {
            if (going)
            {
                distanceTraveled = (startPos - transform.position).magnitude;


                if (distanceTraveled > targetDistance)
                {
                    Stop();
                    //Go(Random.Range(500f, 1500f), Random.Range(10f, 25f), new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
                }
                else
                {
                    MovementGo();
                }
            }
            else if (traversing)
            {
                if (!goingReverse)
                {
                    if (edgeProgress > 0.99f) //finnished
                    {
                        Stop();
                    }
                } else
                {
                    if (edgeProgress < 0.01f) //finnished
                    {
                        Stop();
                    }
                }
                MovementTraverse(Time.deltaTime);
            }

        }

    }

    void MovementGo()
    {
        transform.Translate(transform.forward * 1 * Time.deltaTime * speedLimit, Space.World);
    }

    void MovementTraverse(float deltaTime)
    {
        if (!goingReverse)
        {
            edgeProgress += deltaTime / edgeTime;
        }
        else
        {
            edgeProgress -= deltaTime / edgeTime;
        }
        Vector3 newPos = myCar.getEdgePointOffset(curEdge,edgeProgress, transform.rotation);
        //Debug.Log("NewPos: " + newPos + ", EdgeProgress: " + edgeProgress);
        rotationUpdateCounter++;
        if(rotationUpdateCounter > rotationIntensity)
        {
            rotationUpdateCounter = 0;
            transform.rotation = Quaternion.LookRotation((newPos - previousPos).normalized);
            previousPos = transform.position;
        }
        //transform.rotation = Quaternion.LookRotation((transform.position - newPos).normalized);
        transform.position = newPos;
    }

    public void SetCar(Car car)
    {
        myCar = car;
    }

    public void TraverseEdge(Edge edge, float edgeTime)
    {
        this.edgeTime = edgeTime;
        this.curEdge = edge;
        
        //Debug.Log("Edgetime: " + edgeTime + ". Edge: " + edge);

        Vector3 startPos = transform.position;
        Vector3 firstLook;
        if(!edge.reverse)
        {
            firstLook = myCar.getNodePosition(edge.c0);
            edgeProgress = 0;
            goingReverse = false;
        } else
        {
            firstLook = myCar.getNodePosition(edge.c1);
            edgeProgress = 1;
            goingReverse = true;
        }
        transform.rotation = Quaternion.LookRotation((firstLook - startPos).normalized);
        transform.position = myCar.getEdgePointOffset(curEdge, edgeProgress, transform.rotation);

        previousPos = transform.position;
        //rotationUpdateCounter = (int) (rotationIntensity * 0.9);
        traversing = true;
    }


}