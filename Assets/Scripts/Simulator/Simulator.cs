using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
[ExecuteInEditMode]
public class Simulator : MonoBehaviour
{

    public bool DEBUG_PATHFINDING = false;


    [SerializeField]
    Network network;
    public List<Car> cars; //All the cars of the simulation
    public List<int> trafficLightNodes; //Input in editor which nodes are to have traffic lights
    private List<TrafficLight> trafficLights;
    public float trafficLightChangeFrequency = 4f; //Input in editor how often the traffic lights change

    public List<int> endNodes; //The nodes that are at the edge of the network, which only have 1 connecting node.

    public List<Rigidbody> carPrefabs; //The different types of cars to spawn
    public int carsToSpawn = 0; //How many cars that should be spawned
    private int carsSpawned = 0; //How many cars we have spawned
    public float carSpawnIntensity = 1f; // how often we spawn new cars in seconds
    private float timeSinceCarSpawn = 0f;
    private int spawnEndNode; //the node from which we last spawned a car
    public float laneWidth = 1.5f; //the width of the lanes

    void Awake()
    {
        if (!Application.isPlaying)
        {
            network = gameObject.GetComponent<Network>();
            if (network == null)
                this.network = gameObject.AddComponent<Network>();
        }

   
        //Add traffic lights
        trafficLights = new List<TrafficLight>();
        if (trafficLightNodes != null)
        {
            for (int i = 0; i < trafficLightNodes.Count; i++)
            {
                trafficLights.Add(new TrafficLight(network.nodes[trafficLightNodes[i]], trafficLightNodes[i], trafficLightChangeFrequency));
            }

        }

        spawnEndNode = 0;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG_PATHFINDING && Application.isPlaying)
        {
            DEBUG_PATHFINDING = false;
            TestPathFinding();

        }

        //update the trafficlights
        foreach (TrafficLight trafficLight in trafficLights)
        {
            trafficLight.update(Time.deltaTime);
        }

        //Spawn new cars
        if (carsSpawned < carsToSpawn)
        {

            if (timeSinceCarSpawn > carSpawnIntensity)
            {
                carsSpawned++;
                timeSinceCarSpawn = 0f;
                spawnCar(endNodes[spawnEndNode]);
                //Change the node from which the next car whould spawn from
                spawnEndNode = (spawnEndNode < endNodes.Count - 1) ? spawnEndNode + 1 : 0;
            }
            else
            {
                timeSinceCarSpawn += Time.deltaTime;
            }
        }

    }

    //Spawn a car at the node Id
    void spawnCar(int fromNodeId)
    {
        int carPrefabId = UnityEngine.Random.Range(0, carPrefabs.Count); //random car model amongst the possible ones
        Rigidbody carClone = (Rigidbody)Instantiate(carPrefabs[carPrefabId], getNodePosition(fromNodeId), transform.rotation);
        Car car = carClone.GetComponent<Car>();
        int goalNodeId = fromNodeId;
        while (goalNodeId == fromNodeId)
        {
            //Get a random goal node from the endnodes
            goalNodeId = endNodes[UnityEngine.Random.Range(0, endNodes.Count)];
        }
        car.myOriginId = fromNodeId;
        car.myGoalId = goalNodeId;
        car.setSimulator(this);
        cars.Add(car);
    }

    //Get a path from a vector3 position to another
    public List<Edge> pathFromTo(Vector3 position, Vector3 target)
    {
        Debug.Log("Looking for nodes corresponding to : " + position + " and " + target);
        int startNodeId = findClosestNodeId(position); //TODO temp
        int endNodeId = findClosestNodeId(target); //TODO temp
        Debug.Log("Found: " + getNodePosition(startNodeId) + " and " + getNodePosition(endNodeId));
        return network.PathTo(startNodeId, endNodeId);
    }

    //Get a path from a node to another
    public List<Edge> pathFromTo(int fromId, int toId)
    {
        return network.PathTo(fromId, toId);
    }

    //Find the closest node to a vec3 position
    public int findClosestNodeId(Vector3 position)
    {
        int bestIndex = -1;
        float minDistance = float.MaxValue;
        for (int i = 0; i < network.nodes.Count; ++i)
        {
            float distance = (position - network.nodes[i].pos).magnitude;
            if (distance < minDistance
                && !network.nodes[i].isControlPoint && network.nodes[i].isActive)
            {
                bestIndex = i;
                minDistance = distance;
            }
        }

        return bestIndex;
    }

    //Get the position of a node
    public Vector3 getNodePosition(int nodeId)
    {
        return network.nodes[nodeId].pos + transform.position;
    }

    //Get a point on the curved edge, which point is decided by the progress t (goes from 0.0 to 1.0)
    public Vector3 getEdgePoint(Edge edge, float t)
    {
        return Bezier.BezierCurve(getNodePosition(edge.n0), getNodePosition(edge.c0), getNodePosition(edge.c1), getNodePosition(edge.n1), t);
    }

    //Get a point on the curved edge, offset to the right of the car, to create two-lane-streets
    public Vector3 getEdgePointOffset(Edge edge, float t, Vector3 carRight)
    {

        Vector3 offset = Vector3.Scale(new Vector3(laneWidth, laneWidth, laneWidth), carRight);
        return offset + Bezier.BezierCurve(getNodePosition(edge.n0), getNodePosition(edge.c0), getNodePosition(edge.c1), getNodePosition(edge.n1), t);
    }


    void TestPathFinding()
    {
        Debug.Log("Testing pathfinding...this may take a while");
        int nr = 0;
        int nr_errors = 0;

        for (int x = 0; x < network.nodes.Count; ++x)
        {
            if (!network.nodes[x].isActive) continue;
            if (network.nodes[x].isControlPoint) continue;
            for (int y = 0; y < network.nodes.Count; ++y)
            {
                if (!network.nodes[y].isActive) continue;
                if (network.nodes[y].isControlPoint) continue;
                if (x == y) continue; //don't test path from and to the same node

                List<Edge> path = network.PathTo(x, y);
                bool pass = CheckPath(path, x, y);
                if (!pass) ++nr_errors;
                string[] listString = new string[path.Count];
                for (int i = 0; i < path.Count; ++i)
                {
                    listString[i] = path[i].ToString();
                }
                Debug.Log(pass + ": Test nr " + nr + ": from " + x + " to " + y + "; List size: " + path.Count + ": " + string.Join(", ", listString));
                ++nr;
            }
        }
        Debug.Log("Testing complete, errors: " + nr_errors);
    }

    private bool CheckPath(List<Edge> path, int start, int goal)
    {

        //check if we can reach our destination through the edges
        bool edge_check = true;

        if (path == null || path.Count == 0)
        {
            return true;
        }

        if (path.Count == 1)
        {
            if (path[0].reverse)
                return (path[0].n1 == start && path[0].n0 == goal);
            else
                return (path[0].n0 == start && path[0].n1 == goal);
        }

        Edge prevEdge;
        Edge curEdge;
        for (int i = 1; i < path.Count && edge_check; ++i)
        {
            prevEdge = path[i - 1];
            curEdge = path[i];

            if (prevEdge.reverse && curEdge.reverse)
            {
                if (!(prevEdge.n0 == curEdge.n1))
                    edge_check = false;
            }
            else if (prevEdge.reverse && !curEdge.reverse)
            {
                if (!(prevEdge.n0 == curEdge.n0))
                    edge_check = false;
            }
            else if (!prevEdge.reverse && curEdge.reverse)
            {
                if (!(prevEdge.n1 == curEdge.n1))
                    edge_check = false;
            }
            else if (!prevEdge.reverse && !curEdge.reverse)
            {
                if (!(prevEdge.n1 == curEdge.n0))
                    edge_check = false;
            }
        }

        Debug.Assert(edge_check);
        return edge_check;
    }

    //Use the traffic lights to decide if a traffic channel is open.
    //if there is no traffic light there, return true.
    public bool isChannelOpen(int fromNodeId, int toNodeId, int viaNodeId, Car car)
    {
        bool foundTrafficLight = false;
        foreach (TrafficLight trafficLight in trafficLights)
        {
            if (trafficLight.nodeId == viaNodeId)
            {
                foundTrafficLight = true;
            }
            if (trafficLight.nodeId == viaNodeId && trafficLight.checkTrafficLight(fromNodeId, toNodeId, car))
            {
                return true;
            }
        }

        return !foundTrafficLight; //return true if traffic light is not found
    }


}
