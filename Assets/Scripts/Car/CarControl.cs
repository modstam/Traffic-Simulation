using UnityEngine;
using System.Collections;

/**
    This class handles the movements and control of a Car.
**/
public class CarControl : MonoBehaviour
{
    public bool traversing = false;  //When traveling along a curved edge
    private bool paused = false; //when temporarily stopping because of obstacle/traffic light

    private Edge curEdge; //currently traveling on this edge
	private Edge nextEdge;
    public float edgeProgress = 0f; //going form 0 to 100% 
    public float edgeTime = 0f; //how long time it takes to complete an edge
    public bool goingReverse; //If the current edge is from B to A rather than A to B.

    //public Transform graphicTransform; //
    public Car myCar; //The Car script managing higher level car tasks

    private Vector3 previousPos; //used for calculating a delta vektor between to points.

    private int rotationUpdateCounter; //how long a go the car rotation was updated
    

	//members for transfering the car between two edges
	private bool transferMode = false;
	private Vector3 transferEnd;
	private float transferProgress = 0f;
	private float transferDistance = 0f;
    private Vector3 transferStartPos;

    private float nextEdgeStartProgress = 0f; //How far ahead we start the next edge

    //When an edged is to be considered traversed
    public static float EDGE_PROGRESS_REQ = 0.95f;
    //The speed units/s of a transfer
    public static float TRANSFER_SPEED = 10f;
    //rotate car every 7th frame
    private static int ROTATION_INTENSITY = 7;
    //Minimum distance required to change rotation
    private static float MIN_ROT_DIST = 0.2f;
    //Start edge travel a bit later
    private static float START_EDGE_PROGRESS = 0.1f;

    private static float INTERSECT_DIFF = 0.02f;
    private static float RIGHT_TURN_MIN_VAL = 0.1f;

    // Use this for initialization
    void Start()
    {}

    //Stop the car
    void Stop()
    {
        traversing = false;
        myCar.onStop();
    }

    //API function for pausing the cars movement
    public void pause()
    {
        paused = true;
    }

    //API function for resuming the cars movement
    public void resume()
    {
        paused = false;
    }

    //Resume after 1 second
    public void delayedResume()
    {
        Invoke("resume", 1);
    }

    // Update is called once per frame
    void Update()
    {
        //DrawCheckTurnDirection();
        if (!paused && traversing)
        {
            if ((!goingReverse && edgeProgress > EDGE_PROGRESS_REQ) || //finnished
                goingReverse && edgeProgress < 1 - EDGE_PROGRESS_REQ)
            {
                //Debug.Log("GON STOP!");
                checkTurnDirection();
                Stop();
            }
            MovementTraverse(Time.deltaTime); //update movement
            
        }

    }

    //Update the movement of the car when traversing
    void MovementTraverse(float deltaTime)
    {
        //If we are transfering the car between two edges
		if (transferMode) {

			//Debug.Log("transferMode");
			transferProgress += (1/transferDistance)*deltaTime*TRANSFER_SPEED;

			if(transferProgress >= 1.0f){
				//Debug.Log ("transferprogress: " + transferProgress);
				transferProgress = 0;
				transferMode = false;
                startEdgeTravel(curEdge, edgeTime);
            }
			else{

				//Debug.Log ("interpolating");

				Vector3 newPos = Vector3.Lerp (transferStartPos,transferEnd, transferProgress);

                //rotation fix
                transform.rotation = Quaternion.LookRotation((transferEnd - transferStartPos).normalized);
                //Debug.Log("transferEnd: " + transferEnd + ", rotation: " + (transferEnd - transferStartPos).normalized);

                transform.position = newPos;
			}
		} else { //If moving the car along an edge
            //If we are going in reverse, progres goes from 100% to 0 % instead of the other way around.
			if (!goingReverse)
			{   
				edgeProgress += deltaTime / edgeTime;
			}
			else
			{
				edgeProgress -= deltaTime / edgeTime;
			}
			//What's our new position
			Vector3 newPos = myCar.getEdgePointOffset(curEdge,edgeProgress, transform.rotation);

			rotationUpdateCounter++; //Update the rotation very ROTATION_INTENSITY:th frame
            if (rotationUpdateCounter > ROTATION_INTENSITY)
			{
                if ((newPos - previousPos).magnitude > MIN_ROT_DIST) //If we have traveled long enough since last rotation
                {
                    rotationUpdateCounter = 0;
                    transform.rotation = Quaternion.LookRotation((newPos - previousPos).normalized);
                    previousPos = transform.position;
                }
			}
            //Update car position:
			transform.position = newPos;
		}
	}
	
    //Assign a Car-object
	public void SetCar(Car car)
	{
		myCar = car;
	}
	
    //API function for traversing an edge in edgeTime amount of seconds
	public void TraverseEdge(Edge edge, Edge nextEdge, float edgeTime)
    {
        this.nextEdge = nextEdge;
        this.curEdge = edge;
        if (curEdge != null) {
			//commence transfer
			transferMode = true;

            //Debug.Log("Edge: " + edge + "nextEdge: " + nextEdge);

			

            //Find out where the next edge starts
            //Debug.Log("transferEnd, NextStartProg: " + nextEdgeStartProgress);
            if (edge.reverse) transferEnd = myCar.getEdgePointOffset(edge, (1 - nextEdgeStartProgress), Quaternion.LookRotation((myCar.getNodePosition(edge.c1) - transform.position).normalized));
			else transferEnd = myCar.getEdgePointOffset(edge, nextEdgeStartProgress, Quaternion.LookRotation((myCar.getNodePosition(edge.c0) - transform.position).normalized));

            transferProgress = 0f;
            transferStartPos = transform.position;
            transferDistance = Vector3.Distance(transferStartPos,transferEnd);

		}
        //Set up the edge travling to be done after the transfer between the two edges.
        startEdgeTravel(edge, edgeTime);
        


    }

    //Start an edge travel, which will be done in edgeTime amount of seconds
    private void startEdgeTravel(Edge edge, float edgeTime)
    {
        this.edgeTime = edgeTime;
        this.curEdge = edge;

        //where are we now?
        Vector3 startPos = transform.position;
        Vector3 firstLook;
        //where do we look? :
        if (!edge.reverse)
        {
            firstLook = myCar.getNodePosition(edge.c0); //use the first control node of the edge as look target
            edgeProgress = nextEdgeStartProgress;
            goingReverse = false;
        }
        else
        {
            firstLook = myCar.getNodePosition(edge.c1);
            edgeProgress = 1- nextEdgeStartProgress;
            goingReverse = true;
        }

        //Set look-at-rotation of the car
        transform.rotation = Quaternion.LookRotation((firstLook - startPos).normalized);
        //Get the point of the curve on which we position ourselves
        transform.position = myCar.getEdgePointOffset(curEdge, edgeProgress, transform.rotation);

        previousPos = transform.position;
        traversing = true;
        rotationUpdateCounter = 0;
    }

    //Determine if the car is turning left or right
    //Important to avoid collisions with cars traveling the oposite direction in the turn
    //cars turning right will take an inner curve, while left turns generates outer, larger turns.
    void checkTurnDirection()
    {
        if (nextEdge != null)
        {
            Vector3 targetRoad; //A location at START_EDGE_PROGRESS % on the next edge.
            if (nextEdge.reverse)
            {
                targetRoad = myCar.getEdgePointOffset(nextEdge, 1 - (START_EDGE_PROGRESS), Quaternion.LookRotation((myCar.getNodePosition(nextEdge.c1) - transform.position)));
            }
            else
            {
                targetRoad = myCar.getEdgePointOffset(nextEdge, START_EDGE_PROGRESS, Quaternion.LookRotation((myCar.getNodePosition(nextEdge.c0) - transform.position)));
            }

            Vector3 r = transform.rotation * Vector3.right + transform.position;
            Vector3 l = transform.rotation * Vector3.left + transform.position;

            if ((targetRoad - r).magnitude > (targetRoad - l).magnitude)
            { //If we are turning left
                //Debug.Log("Gon turn left!");
                nextEdgeStartProgress = 0;
            }
            else
            { //If we are turning left
                //Debug.Log("Gon turn right!");
                nextEdgeStartProgress = START_EDGE_PROGRESS;
            }
        }

    }

    //Debug drawing lines for determening turn diretion.
    void DrawCheckTurnDirection()
    {
        if (nextEdge != null)
        {
            Vector3 targetRoad;
            if (nextEdge.reverse)
            {
                targetRoad = myCar.getEdgePointOffset(nextEdge, 1 - (START_EDGE_PROGRESS), Quaternion.LookRotation((myCar.getNodePosition(nextEdge.c1) - transform.position)));
            }
            else
            {
                targetRoad = myCar.getEdgePointOffset(nextEdge, START_EDGE_PROGRESS, Quaternion.LookRotation((myCar.getNodePosition(nextEdge.c0) - transform.position)));
            }

            //Vector3 localRight = transform.worldToLocalMatrix.MultiplyVector(transform.right);
            Vector3 r = transform.rotation * Vector3.right + transform.position;
            Vector3 l = transform.rotation * Vector3.left + transform.position;


            Debug.DrawLine(r, targetRoad, Color.red);
            Debug.DrawLine(l, targetRoad, Color.magenta);
        }
    }


}