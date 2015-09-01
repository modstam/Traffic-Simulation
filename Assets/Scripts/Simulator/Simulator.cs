using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Simulator : MonoBehaviour {
	
	Network network;
    public List<Car> cars;
    public Car testcar; //testcar

    void Awake(){
		if (!Application.isPlaying) {
			network = gameObject.GetComponent<Network>();
			if(network == null)
				this.network = gameObject.AddComponent<Network>();
		}
	}

	// Use this for initialization
	void Start () {
        testcar.setSimulator(this);
        /*foreach(Car car in cars)
        {
            car.setSimulator(this);
        }*/
	}
	
	// Update is called once per frame
	void Update () {

	}

    //
    public List<Edge> pathFromTo(Vector3 position, Vector3 target)
    {
        Node startNode = new Node(position); //TODO temp
        Node endNode = new Node(target); //TODO temp
        return network.pathTo(startNode, endNode);
    }

    public Node findClosestNode(Vector3 position)
    {
        int bestIndex = -1;
        float minDistance = float.MaxValue;
        for(int i = 0; i < network.nodes.Count; ++i)
        {
            if(((position - network.nodes[i].pos).magnitude) < minDistance)
                bestIndex = i;
        }

        return network.nodes[bestIndex];
    }
}
