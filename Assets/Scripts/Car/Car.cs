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
    public Vector3 myGoal;
    public float completeProgress = 0.99f; //Edge considered complete when t >= 0.99.
    public float trafficPauseProgress = 0.85f; //Check if traffic light is green at this distance.
    bool trafficChannelChecked = false;



    void Start()
    {
        //carHandler.onCarReady(this, -1);
        myGoal = new Vector3(60, 0, 0);
        carControl.SetCar(this);
        //Debug.Log("Car is set...");
       // Debug.Log("Nodepos[0]: " + simulator.getNodePosition(0));
        myPath = simulator.pathFromTo(transform.position, myGoal);
        string[] listString = new string[myPath.Count];
        for (int i = 0; i < myPath.Count; ++i)
        {
            listString[i] = myPath[i].ToString();
        }
        Debug.Log( "Found path from " + myPath[0].n0 + " to " + myPath[myPath.Count -1].n0 + "; List size: " + myPath.Count + ": " + string.Join(", ", listString));
        curEdgeIndex = -1;
        
        GoTowards(simulator.getNodePosition(myPath[0].n0));
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
        float edgeTime = 10f; //TODO HARDCODED VALUE
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

    public Vector3 getEdgePoint(Edge edge, float t)
    {
        return simulator.getEdgePoint(edge, t);
    }

    void Update()
    {
        if (!trafficChannelChecked)
        {
            if (carControl.edgeProgress > trafficPauseProgress && curEdgeIndex < myPath.Count-1)
            {
                trafficChannelChecked = true;
                if (simulator.isChannelOpen(myPath[curEdgeIndex].n0, myPath[curEdgeIndex + 1].n1, myPath[curEdgeIndex].n1, this))
                {
                    //Continue going...
                }
                else
                {
                    carControl.pause();
                }
            } 
        }
    }

    public void OnGreenLight()
    {
        Debug.Log("Resuming...");
        carControl.resume();
    }

    public void onStop()
    {
        Debug.Log("STOP!");
        //If we finnished the previous travel command
        if (carControl.edgeProgress > completeProgress || (transform.position - targetPos).magnitude < 1f)
        {
            curEdgeIndex += 1;
            if (!(curEdgeIndex >= myPath.Count)) //If we havn't reached end of path
            {
                trafficChannelChecked = false;
                TraverseEdge(myPath[curEdgeIndex]);
            }
            //carHandler.onCarReady(this, targetNodeId);
        }
        else
        {
            GoTowards(targetPos);
        }
    }
}