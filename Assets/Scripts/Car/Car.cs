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


    void Start()
    {
        //carHandler.onCarReady(this, -1);
        myGoal = new Vector3(60, 0, 0);
        carControl.SetCar(this);
        //Debug.Log("Car is set...");
       // Debug.Log("Nodepos[0]: " + simulator.getNodePosition(0));
        myPath = simulator.pathFromTo(transform.position, myGoal);
        Debug.Log("Found path: " + myPath[0].ToString());
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
        float edgeTime = 5f; //TODO HARDCODED VALUE
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

    public void onStop()
    {
        //If we finnished the previous travel command
        if (carControl.edgeProgress > 0.99f || (transform.position - targetPos).magnitude < 2)
        {
            curEdgeIndex += 1;
            if (!(curEdgeIndex >= myPath.Count)) //If we havn't reached end of path
            {
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