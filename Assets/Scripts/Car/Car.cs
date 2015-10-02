using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
/**
    This class represents a Car
**/
public class Car : MonoBehaviour
{
	//CarControl handles movement and direct control of the car object.
	public CarControl carControl;
	Simulator simulator; //Reference to the Simulator class.
	
	public List<Edge> myPath; //The path to travel
	public int curEdgeIndex; //The edge on the path that is currently being traveled
	public int myOriginId; //The node ID from which this car starts the path
	public int myGoalId; //The node ID to which the car is trying to go
	private float trafficPauseProgress = 0.85f; //Check if traffic light is green at this distance.
	bool trafficChannelChecked = false; //Whether the car is at a traffic light and has checked if it's green.
    bool waitingForGreen = false; //Waiting for green traffic light

    public int collCount = 0; //The number of currently colliding cars

    public float CAR_EDGE_TIME = 5f; //How long time an edge should take to travel

    public Transform myBody; //The body of the car
    public Transform myCheck; //A point in front of the car, used for checking collisions..
    public static float checkRadius = 0.10f; //.. in this radius
    public bool checkStatus = false; //if we found any collisions there
	
	void Start()
	{
		carControl.SetCar(this);
        myBody.tag = "CarBody"; //The body gets a collision tag of CarBody
        transform.tag = "Car"; //the car object gets a collision tag of Car
		restart();
	}
	
	void restart()
	{
        //Get a path from the simulator which takes us from our cur. pos. to our goal.
		myPath = simulator.pathFromTo(myOriginId, myGoalId);
		
        //Debug the path...
		string[] listString = new string[myPath.Count];
		for (int i = 0; i < myPath.Count; ++i)
		{
			listString[i] = myPath[i].ToString();
		}
		//Debug.Log("Found path from " + myPath[0].n0 + " to " + myPath[myPath.Count - 1].n1 + "; List size: " + myPath.Count + ": " + string.Join(", ", listString));
		curEdgeIndex = 0;
		
        //Start the travling of the first path.
		if (!myPath[0].reverse)
		{
			myOriginId = myPath[0].n0;
			TraverseEdge(myPath[0], myPath[1]);
		}
		else
		{
			myOriginId = myPath[0].n1;
			TraverseEdge(myPath[0], myPath[1]);
		}
	}
	

	//Use the carControl to traverse an edge
	public void TraverseEdge(Edge edge, Edge nextEdge)
	{
        float edgeTime = CAR_EDGE_TIME;
		carControl.TraverseEdge(edge, nextEdge, edgeTime);
	}
	
	public void setPosition(Vector3 pos)
	{
		transform.position = pos;
	}
	
	public void setSimulator(Simulator simulator)
	{
		this.simulator = simulator;
	}
	
    //Use the simulator to get a position of the curved edge
	public Vector3 getEdgePointOffset(Edge edge, float t, Quaternion carRotation)
	{
		return simulator.getEdgePointOffset(edge, t, carRotation * Vector3.right);
	}

    //Use the simulator to get a position of a node.
    public Vector3 getNodePosition(int nodeId)
	{
		return simulator.getNodePosition(nodeId);
	}
	
    //Called each frame
	void Update()
	{
        //If  we are traversing an edge are finnished with it, and if its not the last edge of the path
		if (carControl.traversing &&
		    ( (myPath[curEdgeIndex].reverse && (carControl.edgeProgress < (1-trafficPauseProgress) ) )
		 ||  (!myPath[curEdgeIndex].reverse && (carControl.edgeProgress > trafficPauseProgress) ))
		    && curEdgeIndex < myPath.Count-1)
		{ //In that case, we check the traffic light which are before the next edge
			trafficChannelChecked = true;
            //Our current edge starts at firstNode, the edge ends at secondNode
            // and the traffic light (node) between them is at viaNode.
            // Assign these values:
			int firstNodeIndex, secondNodeIndex, viaNodeIndex;
			if (!myPath[curEdgeIndex].reverse)
			{
				firstNodeIndex = myPath[curEdgeIndex].n0;
				viaNodeIndex = myPath[curEdgeIndex].n1;
			}   else
			{
				firstNodeIndex = myPath[curEdgeIndex].n1;
				viaNodeIndex = myPath[curEdgeIndex].n0;
			}
			
			
			if (!myPath[curEdgeIndex + 1].reverse)
				secondNodeIndex = myPath[curEdgeIndex + 1].n1;
			else
				secondNodeIndex = myPath[curEdgeIndex + 1].n0;
			
            //Check if the traffic light is green.
			if (simulator.isChannelOpen(firstNodeIndex, secondNodeIndex, viaNodeIndex, this))
			{
                //It it is, continue going...
                waitingForGreen = false;
			}
			else
			{   //If not, stop the car
                waitingForGreen = true;
				carControl.pause();
			}
		}

        //Check if the car is standing still for no reason, with an apparantly open path in front of it.
        if(!Physics.CheckSphere(myCheck.position, checkRadius))
        {   
            //if (collCount > 0)
            //{
                collCount = 0;
                checkStatus = false;
            //carControl.resume(); //resume the cars movement
            //}

            if(!waitingForGreen)
            {
                carControl.resume(); //resume the cars movement
            }
        } else
        {
            checkStatus = true;
        }
        
	}
	
    //Called by the traffic light, if we are waiting for it
    // to inform us that we can now continue our path
	public void OnGreenLight()
	{
        // resume if we don't have a car infront of us
        if(collCount <= 0)
        carControl.resume();

        waitingForGreen = false;
	}
	
	public void onStop()
	{
		//If we finnished the previous travel command
		if ( ( (carControl.goingReverse && (carControl.edgeProgress < (1-CarControl.EDGE_PROGRESS_REQ) ) )
		      || (carControl.edgeProgress > CarControl.EDGE_PROGRESS_REQ) ))
		{
			curEdgeIndex += 1;
			if (curEdgeIndex < myPath.Count) //If we havn't reached end of path...
			{ //Traverse the next edge
				trafficChannelChecked = false;
                if (curEdgeIndex+1 < myPath.Count)
                    TraverseEdge(myPath[curEdgeIndex], myPath[curEdgeIndex+1]);
                else
                    TraverseEdge(myPath[curEdgeIndex], null);
			} else
			{ //... if we have reached the end, take a journey back to beginning.
				int oldOrigin = myOriginId;
				myOriginId = myGoalId;
				myGoalId = oldOrigin;
				restart();
			}
		}
	}
	
	void OnTriggerEnter(Collider otherCollider)
	{
		if (otherCollider.tag == "CarBody")
		{   //A car appears to be in front of us, pause movement.
            collCount++;
			carControl.pause();
		}
		
	}
	
	void OnTriggerExit(Collider otherCollider)
	{
		if (otherCollider.tag == "CarBody")
		{
            //A car in front of us appears to have moved away, start movement
            // if no other car is blocking us.
            collCount--;
            if (collCount < 0)
                collCount = 0;
            //carControl.delayedResume();
		}
	}
}