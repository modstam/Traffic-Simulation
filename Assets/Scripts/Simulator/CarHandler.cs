using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarHandler : MonoBehaviour
{

    public List<Road> roads;
    public List<Car> cars;

    List<Node> testNodes;

    public Car car;



    // Use this for initialization
    void Start()
    {
        testNodes = new List<Node>();
        testNodes.Add(new Node(new Vector3(0, 0, 0)));
        testNodes.Add(new Node(new Vector3(85, 0, 0)));
        testNodes.Add(new Node(new Vector3(85, 0, 30)));
        testNodes.Add(new Node(new Vector3(65, 0, -30)));
        testNodes.Add(new Node(new Vector3(15, 0, -90)));
        testNodes.Add(new Node(new Vector3(-40, 0, -100)));
        testNodes.Add(new Node(new Vector3(40, 0, -40)));
        //car.setHandler(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onCarReady(Car car, int previousNodeId)
    {
        int nextNodeId;
        if (previousNodeId == -1)
        {
            nextNodeId = Random.Range(1, testNodes.Count - 1);
            car.setPosition(testNodes[nextNodeId - 1].pos);
        }
        else if (previousNodeId == testNodes.Count - 1)
        {
            nextNodeId = 0;
        }
        else
        {
            nextNodeId = previousNodeId + 1;
        }
        car.GoTowards(testNodes[nextNodeId], nextNodeId);
    }
}