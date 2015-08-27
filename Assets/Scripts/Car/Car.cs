using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour {

    public CarControl carControl;
    CarHandler carHandler;
    Node targetNode;
    int targetNodeId;

    void Start()
    {
        carHandler.onCarReady(this, -1);
        carControl.SetCar(this);
    }
    

	public void GoTowards(Node node, int nodeId)
    {
        targetNode = node;
        targetNodeId = nodeId;
        float distance = (targetNode.pos - carControl.transform.position).magnitude;
        float speedLimit = 20; //HARDCODED VALUE
        Vector3 direction = (targetNode.pos - carControl.transform.position).normalized;

        carControl.Go(distance, speedLimit, direction);
    }

    public void setPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void setHandler(CarHandler carHandler)
    {
        this.carHandler = carHandler;
    }

    public void onStop()
    {
        if((transform.position - targetNode.pos).magnitude < 2)
        {
            carHandler.onCarReady(this, targetNodeId);
        } else
        {
            GoTowards(targetNode, targetNodeId);
        }
    }
}
