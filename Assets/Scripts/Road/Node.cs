using UnityEngine;
using System.Collections.Generic;
using System;

public class Node{

	
	public List<Node> connections;
	public Vector3 pos;
	public Quaternion rot;

	public Node(Vector3 pos){
		this.pos = pos;
		connections = new List<Node>();
	}

	public Node(Vector3 pos, Quaternion rot){
		this.pos = pos;
		this.rot = rot;
		connections = new List<Node>();
	}
	
	public void Reset(){
		connections = new List<Node>();

	}

	public void AddConnection(Node newConnection){
		if(!connections.Contains(newConnection))
			connections.Add (newConnection);
	}

	public int NumConnections(){
		return connections.Count;
	}

	public Node GetConnection(int index){
		return connections[index];
	}

	public void RemoveConnection(Node node){
		connections.Remove(node);
	}
}
