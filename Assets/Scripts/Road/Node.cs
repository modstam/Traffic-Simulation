using UnityEngine;
using System.Collections;
using System;

public class Node{

	
	public Node[] connections;
	public Vector3 pos;
	public Quaternion rot;

	public Node(Vector3 pos){
		this.pos = pos;
		connections = new Node[0];
	}

	public Node(Vector3 pos, Quaternion rot){
		this.pos = pos;
		this.rot = rot;
		connections = new Node[0];
	}
	
	public void Reset(){
		connections = new Node[0];

	}

	public void AddConnection(Node newConnection){
		Array.Resize(ref connections, connections.Length+1);
		connections[connections.Length-1] = newConnection;
	}
}
