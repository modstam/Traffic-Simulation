using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Car : MonoBehaviour
{

    public CarControl carControl;
    Simulator simulator;

    Node targetNode;
    int targetNodeId;

    public List<Edge> myPath;
    public int curEdgeIndex;
    public Vector3 myGoal;


    void Start()
    {
        //carHandler.onCarReady(this, -1);
        carControl.SetCar(this);
        myPath = simulator.pathFromTo(transform.position, myGoal);
        curEdgeIndex = -1;
        GoTowards(myPath[0].node0, 0);
    }


    public void GoTowards(Node node, int nodeId)
    {
        targetNode = node;
        targetNodeId = nodeId;
        float distance = (targetNode.pos - carControl.transform.position).magnitude;
        float speedLimit = 20; //TODO HARDCODED VALUE
        Vector3 direction = (targetNode.pos - carControl.transform.position).normalized;

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
        if (carControl.edgeProgress > 0.99f || (transform.position - targetNode.pos).magnitude < 2)
        {
            curEdgeIndex += 1;
            if(!(curEdgeIndex >= myPath.Count)) //If we havn't reached end of path
            {
                TraverseEdge(myPath[curEdgeIndex]);
            }
            //carHandler.onCarReady(this, targetNodeId);
        }
        else
        {
            GoTowards(targetNode, targetNodeId);
        }
    }
}