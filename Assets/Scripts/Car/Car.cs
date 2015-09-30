using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Car : MonoBehaviour
{
	
	public CarControl carControl;
	Simulator simulator;
	
	Vector3 targetPos;
	
	public List<Edge> myPath;
	public int curEdgeIndex;
	public int myOriginId;
	public int myGoalId;
	//private float completeProgress = 0.99f; //Edge considered complete when t >= 0.99.
	private float trafficPauseProgress = 0.85f; //Check if traffic light is green at this distance.
	bool trafficChannelChecked = false;

    public int collCount = 0;

    public float CAR_EDGE_TIME = 5f;

    public Transform myBody;
    public Transform myCheck;

    public static float checkRadius = 0.25f;
    public bool checkStatus = false;
	
	void Start()
	{
		//carHandler.onCarReady(this, -1);
		//myGoal = new Vector3(60, 0, 0);
		carControl.SetCar(this);
        myBody.tag = "CarBody";
        transform.tag = "Car";
		restart();
	}
	
	void restart()
	{
		myPath = simulator.pathFromTo(myOriginId, myGoalId);
		
		string[] listString = new string[myPath.Count];
		for (int i = 0; i < myPath.Count; ++i)
		{
			listString[i] = myPath[i].ToString();
		}
		Debug.Log("Found path from " + myPath[0].n0 + " to " + myPath[myPath.Count - 1].n1 + "; List size: " + myPath.Count + ": " + string.Join(", ", listString));
		curEdgeIndex = 0;
		
		if (!myPath[0].reverse)
		{
			myOriginId = myPath[0].n0;
			TraverseEdge(myPath[0]);
			//GoTowards(simulator.getNodePosition(myPath[0].n0));
		}
		else
		{
			myOriginId = myPath[0].n1;
			TraverseEdge(myPath[0]);
			//GoTowards(simulator.getNodePosition(myPath[0].n1));
		}
	}
	
	
	public void GoTowards(Vector3 target)
	{
		targetPos = target;
		float distance = (targetPos - carControl.transform.position).magnitude;
		float speedLimit = 20; //TODO HARDCODED VALUE
		Vector3 direction = (targetPos - carControl.transform.position).normalized;
		
		carControl.Go(distance, speedLimit, direction);
	}
	
	public void TraverseEdge(Edge edge)
	{
        float edgeTime = CAR_EDGE_TIME;
		carControl.TraverseEdge(edge, edgeTime);
	}
	
	public void setPosition(Vector3 pos)
	{
		transform.position = pos;
	}
	
	public void setSimulator(Simulator simulator)
	{
		this.simulator = simulator;
	}
	
	public Vector3 getEdgePointOffset(Edge edge, float t, Quaternion carRotation)
	{
		return simulator.getEdgePointOffset(edge, t, carRotation * Vector3.right);
	}
	
	public Vector3 getEdgePoint(Edge edge, float t)
	{
		return simulator.getEdgePoint(edge, t);
	}
	
	public Vector3 getNodePosition(int nodeId)
	{
		return simulator.getNodePosition(nodeId);
	}
	
	void Update()
	{
		//if (!trafficChannelChecked)
		// {
		//Debug.Log("EdgeP: " + carControl.edgeProgress + " R: " + myPath[curEdgeIndex].reverse);
		if (carControl.traversing &&
		    ( (myPath[curEdgeIndex].reverse && (carControl.edgeProgress < (1-trafficPauseProgress) ) )
		 ||  (!myPath[curEdgeIndex].reverse && (carControl.edgeProgress > trafficPauseProgress) ))
		    && curEdgeIndex < myPath.Count-1)
		{
			trafficChannelChecked = true;
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
			
			if (simulator.isChannelOpen(firstNodeIndex, secondNodeIndex, viaNodeIndex, this))
			{
				//Continue going...
			}
			else
			{
				carControl.pause();
			}
		}

        
        if(!Physics.CheckSphere(myCheck.position, checkRadius))
        {
            if (collCount > 0)
            {
                collCount = 0;
                checkStatus = false;
                carControl.delayedResume();
            }
        } else
        {
            checkStatus = true;
        }
        
	}
	
	public void OnGreenLight()
	{
        // Debug.Log("Resuming...");
        if(collCount <= 0)
        carControl.resume();
	}
	
	public void onStop()
	{
		//Debug.Log("STOP!");
		//If we finnished the previous travel command
		if ( ( (carControl.goingReverse && (carControl.edgeProgress < (1-CarControl.EDGE_PROGRESS_REQ) ) )
		      || (carControl.edgeProgress > CarControl.EDGE_PROGRESS_REQ) )
		    || (transform.position - targetPos).magnitude < 1f)
		{
			curEdgeIndex += 1;
			if (!(curEdgeIndex >= myPath.Count)) //If we havn't reached end of path
			{
				trafficChannelChecked = false;
				TraverseEdge(myPath[curEdgeIndex]);
			} else
			{ //Make a journey back to beginning...
				/*Vector3 tmpVec = new Vector3(myOrigin.x, myOrigin.y, myOrigin.z);
                myOrigin = myGoal;
                myGoal = new Vector3(tmpVec.x, tmpVec.y, tmpVec.z);*/
				int oldOrigin = myOriginId;
				myOriginId = myGoalId;
				myGoalId = oldOrigin;
				restart();
			}
			//carHandler.onCarReady(this, targetNodeId);
		}
		else
		{
			GoTowards(targetPos);
		}
	}
	
	void OnTriggerEnter(Collider otherCollider)
	{
		if (otherCollider.tag == "CarBody")
		{
            //Debug.Log("CAR CROCK!");
            collCount++;
			carControl.pause();
		}
		
	}
	
	void OnTriggerExit(Collider otherCollider)
	{
		if (otherCollider.tag == "CarBody")
		{
            //Debug.Log("CAR DEEECROCK!");
            collCount--;
            if (collCount < 0)
                collCount = 0;
            //if (collCount == 0)
           // {
                carControl.delayedResume();
           // }
		}
	}
}