using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TrafficLight {

    private bool INITIALIZED = false;
    private IntPair[] channels;
    public int nodeId;
    public int openChannel;

    private List<Car>[] waitingCars; //waitingCars[2] are cars waiting for channel 2 to open.

    public float frequency; //how often the traffic lights change
    private float timeSinceChange;

    /*public static TrafficLight CreateComponent(GameObject parent, Node node, int nodeId, float frequency)
    {
        TrafficLight tl = parent.AddComponent<TrafficLight>();
        tl.init(node, nodeId, frequency);
        return tl;
    }
    
    public void init(Node node, int nodeId, float frequency)
    {
        Debug.Log("INITING TFL");
        this.frequency = frequency;
        this.nodeId = nodeId;

        channels = new IntPair[(node.connections.Count * (node.connections.Count - 1))/2];
        //Only need one channel per pair, ie. don't need both (a,b) and (b,a)
        for (int i = 0; i < node.connections.Count - 1; i++)
        {
            for (int j = node.connections.Count - 1; j > i; j--)
            {
                channels[i] = new IntPair(node.connections[i], node.connections[j]);
            }
        }
        printChannels();

        waitingCars = new List<Car>[channels.Length];
        for (int i = 0; i < channels.Length; i++)
        {
            waitingCars[i] = new List<Car>();
        }

        openChannel = 0;
        INITIALIZED = true;
    }*/

    public TrafficLight(Node node, int nodeId, float frequency)
    {
        Debug.Log("INITING TFL");
        this.frequency = frequency;
        this.nodeId = nodeId;

        channels = new IntPair[(node.connections.Count * (node.connections.Count - 1)) / 2];
        //Only need one channel per pair, ie. don't need both (a,b) and (b,a)
        int index = 0;
        for (int i = 0; i < node.connections.Count - 1; i++)
        {
            for (int j = node.connections.Count - 1; j > i; j--)
            {
                Debug.Log("Adding when i=" + i + ", j=" + j + ". node#" + node.connections[i] + "<->node#" + node.connections[j]);
                channels[index] = new IntPair(node.connections[i], node.connections[j]);
                index++;
            }
        }
        printChannels();

        waitingCars = new List<Car>[channels.Length];
        for (int i = 0; i < channels.Length; i++)
        {
            waitingCars[i] = new List<Car>();
        }

        timeSinceChange = 0;
        openChannel = 0;
        INITIALIZED = true;
    }

	
	// Update is NOT called automatically, must be called every update() of parent.
	public void update (float deltaTime) {
        //Debug.Log("updating tfl");
        timeSinceChange += deltaTime;
        if (timeSinceChange > frequency)
        {
            Debug.Log("pre openchannel:" + openChannel);
            timeSinceChange = 0;
            openChannel = (openChannel < channels.Length - 1) ? openChannel + 1 : 0;
            foreach (Car car in waitingCars[openChannel])
            {
                Debug.Log("Resuming CAHRS");
                car.OnGreenLight(); //notify the cars waiting for this openChannel
            }
            int a = 2;
            Debug.Log("post openchannel:" + openChannel);
            Debug.Log("channels[openchannel]" + channels[openChannel]);
            Debug.Log("channels.length: " + channels.Length);
            Debug.Log("(openChannel < channels.Length - 1) is " + (openChannel < channels.Length - 1));
            Debug.Log("Changing trafficlight #" + nodeId + " to open [" + channels[openChannel].x + " <-> " + channels[openChannel].y + "].");
        }
	}

    //Will notify car using car.OnGreenLight() when its channel is open.
    public bool checkTrafficLight(int nodeFrom, int nodeTo, Car car)
    {
        if(isOpen(nodeFrom, nodeTo))
        {
            return true;
        }
        for(int i = 0; i < channels.Length; i++)
        {
            if(channels[i].containsBoth(nodeFrom, nodeTo))
            {
                Debug.Log("Adding car to waitlist");
                waitingCars[i].Add(car);
                return false;
            }
            
        }
        //add car to waitingCars[its desired channel]
        return false;
    }

    public bool isOpen(int nodeFrom, int nodeTo)
    {
        if(channels[openChannel].containsBoth(nodeFrom, nodeTo))
        {
            return true;
        } else
        {
            return false;
        }
    }

    public void printChannels()
    {
        foreach(IntPair channel in channels)
        {
            Debug.Log(" Channel at " + nodeId + " [" + channel.x + " <-> " + channel.y + "].");
        }
    }
}
