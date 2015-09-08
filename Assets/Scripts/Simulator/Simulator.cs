using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
[ExecuteInEditMode]
public class Simulator : MonoBehaviour {

	public bool DEBUG_PATHFINDING = false;


	[SerializeField]
	Network network;
    public List<Car> cars;
    public Car testcar; //testcar
    public List<int> trafficLightNodes; //Input in editor which nodes are to have traffic lights
    private List<TrafficLight> trafficLights;
    public float trafficLightChangeFrequency = 4f; //Input in editor how often the traffic lights change

    public List<int> endNodes;

    public Rigidbody carPrefab;
    public int carsToSpawn = 0;
    private int carsSpawned = 0;
    public float carSpawnIntensity = 1f; // seconds
    private float timeSinceCarSpawn = 0f;
    private int spawnEndNode;

    void Awake(){
		if (!Application.isPlaying) {
			network = gameObject.GetComponent<Network>();
			if(network == null)
				this.network = gameObject.AddComponent<Network>();
		}

        //testcar.setSimulator(this);

       // foreach (Car car in cars)
       // {
       //     car.setSimulator(this);
      //      Debug.Log("SET SIM!");
    //    }

        //Add traffic lights
        trafficLights = new List<TrafficLight>();
        if(trafficLightNodes != null)
        {
            for(int i = 0; i < trafficLightNodes.Count; i++)
            {
                trafficLights.Add(new TrafficLight(network.nodes[trafficLightNodes[i]], trafficLightNodes[i], trafficLightChangeFrequency));
            }
            
        }

        spawnEndNode = 0;
    }

	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("Uppadado?");
        if (DEBUG_PATHFINDING && Application.isPlaying) {
			DEBUG_PATHFINDING = false;
			TestPathFinding();

		}
        
        foreach (TrafficLight trafficLight in trafficLights)
        {
            trafficLight.update(Time.deltaTime);
        }

        if (carsSpawned <= carsToSpawn)
        {
           
            if (timeSinceCarSpawn > carSpawnIntensity)
            {
                carsSpawned++;
                timeSinceCarSpawn = 0f;
                spawnCar(endNodes[spawnEndNode]);
                spawnEndNode = (spawnEndNode < endNodes.Count - 1) ? spawnEndNode + 1 : 0;
            }
            else
            {
                timeSinceCarSpawn += Time.deltaTime;
            }
        }
        
	}

    void spawnCar(int fromNodeId)
    {
        Rigidbody carClone = (Rigidbody)Instantiate(carPrefab, getNodePosition(fromNodeId), transform.rotation);
        Car car = carClone.GetComponent<Car>();
        int goalNodeId = fromNodeId;
        //Debug.Log("From(" + fromNodeId + ") is at: " + getNodePosition(fromNodeId) + ".");
        while (goalNodeId == fromNodeId)
        {
            goalNodeId = endNodes[UnityEngine.Random.Range(0, endNodes.Count)];
        }
        //Debug.Log("Goal(" + goalNodeId + ") is at: " + getNodePosition(goalNodeId) + ".");
        car.myGoal = getNodePosition(goalNodeId);
        car.setSimulator(this);
        cars.Add(car);
    }

    
    public List<Edge> pathFromTo(Vector3 position, Vector3 target)
    {
        //Debug.Log("Looking for nodes corresponding to : " + position + " and " + target);
        int startNodeId = findClosestNodeId(position); //TODO temp
        int endNodeId = findClosestNodeId(target); //TODO temp
        //Debug.Log("Found: " + getNodePosition(startNodeId) + " and " + getNodePosition(endNodeId));
        return network.PathTo(startNodeId, endNodeId);
    }

    public int findClosestNodeId(Vector3 position)
    {
        int bestIndex = -1;
        float minDistance = float.MaxValue;
        for (int i = 0; i < network.nodes.Count; ++i)
        {
            //Debug.Log("Looking at node #" + i + ", distance: " + (position - network.nodes[i].pos).magnitude + ", min: " + minDistance);
            if (((position - network.nodes[i].pos).magnitude) < minDistance
                && !network.nodes[i].isControlPoint && network.nodes[i].isActive)
            {
                bestIndex = i;
                minDistance = (position - network.nodes[i].pos).magnitude;
            }
        }

        return bestIndex;
    }

    public Vector3 getNodePosition(int nodeId)
    {
        return network.nodes[nodeId].pos;
    }

    public Vector3 getEdgePoint(Edge edge, float t)
    {
        return Bezier.BezierCurve(getNodePosition(edge.n0), getNodePosition(edge.c0), getNodePosition(edge.c1), getNodePosition(edge.n1), t);
    }

    public Vector3 getEdgePointOffset(Edge edge, float t, Vector3 carRight)
    {
        Vector3 offset = Vector3.Scale(new Vector3(2, 2, 2), carRight);
        return offset + Bezier.BezierCurve(getNodePosition(edge.n0), getNodePosition(edge.c0), getNodePosition(edge.c1), getNodePosition(edge.n1), t);
    }


    void TestPathFinding(){
		Debug.Log ("Testing pathfinding...this may take a while");
		int nr = 0;
		int nr_errors = 0;

		for (int x = 0; x < network.nodes.Count; ++x) {
			if(!network.nodes[x].isActive) continue;
			if(network.nodes[x].isControlPoint) continue;
			for (int y = 0; y < network.nodes.Count; ++y) {
				if(!network.nodes[y].isActive) continue;
				if(network.nodes[y].isControlPoint) continue;
				if(x==y) continue; //don't test path from and to the same node
			
				List<Edge> path = network.PathTo(x,y);
				bool pass = CheckPath(path,x,y);
				if(!pass) ++nr_errors; 
				string[] listString = new string[path.Count];
				for(int i = 0; i < path.Count; ++i){
					listString[i] = path[i].ToString();
				}
				Debug.Log (pass + ": Test nr " + nr + ": from " + x + " to " + y + "; List size: " + path.Count + ": " + string.Join(", ",listString) );
				++nr;
			}	
		}
		Debug.Log ("Testing complete, errors: " + nr_errors); 
	}

	private bool CheckPath(List<Edge> path, int start, int goal){

		//check if we can reach our destination through the edges
		bool edge_check = true;

		if (path == null || path.Count == 0) {
			return true;
		}

		if (path.Count == 1) {
			if(path[0].reverse)
				return (path[0].n1 == start && path[0].n0 == goal);
			else
				return (path[0].n0 == start && path[0].n1 == goal);
		}

		Edge prevEdge;
		Edge curEdge; 
		for (int i = 1; i < path.Count && edge_check; ++i) {
			prevEdge = path[i-1];
			curEdge = path[i];

			if(prevEdge.reverse && curEdge.reverse){
				if(!(prevEdge.n0 == curEdge.n1))
					edge_check = false;
			}
			else if(prevEdge.reverse && !curEdge.reverse){
				if(!(prevEdge.n0 == curEdge.n0))
					edge_check = false;
			}
			else if(!prevEdge.reverse && curEdge.reverse){
				if(!(prevEdge.n1 == curEdge.n1))
					edge_check = false;
			}
			else if(!prevEdge.reverse && !curEdge.reverse) {
				if(!(prevEdge.n1 == curEdge.n0))
					edge_check = false;
			}
		}

		Debug.Assert (edge_check);
		return edge_check;
	}

    public bool isChannelOpen(int fromNodeId, int toNodeId, int viaNodeId, Car car)
    {
        //Debug.Log("isChannelOpen? trafficLights.count: " + trafficLights.Count);
        foreach(TrafficLight trafficLight in trafficLights) {
            if(trafficLight.nodeId == viaNodeId && trafficLight.checkTrafficLight(fromNodeId, toNodeId, car))
            {
                return true;
            }
        }
        return false;
    }


}
