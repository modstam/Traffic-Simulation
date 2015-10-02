using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//A class for keeping track of a the traffic light which lies between at least 2 surrounding nodes.
// Each pair of these surrounding nodes forms a "channel", and the traffic light keeps 1 channel
// open at a time, rotating between the channels and notifying waiting cars when their channels are opened.
public class TrafficLight {

    private bool INITIALIZED = false;
    private IntPair[] channels; //All the possible channels that 
    public int nodeId; //the nodeId of the traffic light node
    public int openChannel; //the currently open channel

    private List<Car>[] waitingCars; //waitingCars[2] are the cars waiting for channel 2 to open.

    public float frequency; //how often the traffic lights change
    public float changeTime = 1f; //time between a channel closing and the next one opening
    bool changing = false; //whether or not the traffic light is currently chaning
    private float timeSinceChange;


    public TrafficLight(Node node, int nodeId, float frequency)
    {
        this.frequency = frequency;
        this.nodeId = nodeId;

        // Create the channels, using the node connections to discover surrounding nodes.
        channels = new IntPair[(node.connections.Count * (node.connections.Count - 1)) / 2];
        //Only need one channel per pair, ie. don't need both (a,b) and (b,a):
        int index = 0;
        for (int i = 0; i < node.connections.Count - 1; i++)
        {
            for (int j = node.connections.Count - 1; j > i; j--)
            {
                channels[index] = new IntPair(node.connections[i], node.connections[j]);
                index++;
            }
        }
        //printChannels();

        waitingCars = new List<Car>[channels.Length];
        for (int i = 0; i < channels.Length; i++)
        {
            waitingCars[i] = new List<Car>();
        }

        timeSinceChange = 0;
        openChannel = 0;
        INITIALIZED = true;
    }

	
	// update is NOT called automatically, must be called every Update() of parent.
    // This is the method that rotates the open channels, and notifies cars.
	public void update (float deltaTime) {
        timeSinceChange += deltaTime;
        if(changing && timeSinceChange > changeTime)
        {
            //Debug.Log("Done changin!");
            timeSinceChange = 0f;
            changing = false;
            foreach (Car car in waitingCars[openChannel])
            {
                car.OnGreenLight(); //notify the cars waiting for this openChannel
            }
        }
        if (!changing && timeSinceChange > frequency)
        {
            //Debug.Log("Changing!");
            timeSinceChange = 0f;
            changing = true;
            openChannel = (openChannel < channels.Length - 1) ? openChannel + 1 : 0;
            
            //Debug.Log("Changing trafficlight #" + nodeId + " to open [" + channels[openChannel].x + " <-> " + channels[openChannel].y + "].");
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
                //Debug.Log("Adding car to waitlist");
                waitingCars[i].Add(car);
                return false;
            }
            
        }
        return false;
    }

    //simply check if a channel is open.
    public bool isOpen(int nodeFrom, int nodeTo)
    {
        if(!changing && channels[openChannel].containsBoth(nodeFrom, nodeTo))
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
