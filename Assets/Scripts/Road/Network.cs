using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class Network : MonoBehaviour {
	
	[SerializeField]
	public int numCars = 0;

	[SerializeField]
	public List<Node> nodes;

	[SerializeField]
	public EdgeMatrix edges;

	public bool DEBUG_PRINT = false;
	

	// Use this for initialization
	void Start () {

		if (DEBUG_PRINT) {
			for (int i = 0; i < nodes.Count; ++i)
				Debug.Log (nodes [i]);

			Edge[] edgeList = edges.GetList ();
			for (int i = 0; i < edgeList.Length; ++i)
				Debug.Log (i + " " + edgeList [i]);
		}
	}

	// Update is called once per frame
	void Update () {

	}


	public void Reset(){
		nodes = new List<Node> ();
		edges = new EdgeMatrix ();

		Debug.Log("Reset network");

		Edge edge = new Edge ();
		//start node
		nodes.Add (new Node (new Vector3 (1f, 0f, 0f)));
		edge.n0 = 0;

		nodes.Add ( new Node (new Vector3 (2f, 0f, 0f)));
		nodes[1].isControlPoint = true;
		edge.c0 = 1;
	
		nodes.Add ( new Node (new Vector3 (3f, 0f, 0f)));
		nodes[2].isControlPoint = true;
		edge.c1 = 2;
		//end node
		nodes.Add (new Node (new Vector3 (4f, 0f, 0f)));
		edge.n1 = 3;

		//add entry for startnode to endnode connection
		edges [0, 3] = edge;

		AddConnection(0,1);


	}

	public Edge AddEdge(int nodeIndex){

		Vector3 point = nodes [nodeIndex].pos;

		Edge edge = new Edge ();




		point.x += 1f;
		nodes.Add (new Node (point));
		point.x += 1f;
		nodes.Add (new Node (point));
		point.x += 1f;
		nodes.Add (new Node (point));

		nodes[nodes.Count - 3].isControlPoint = true;
		nodes[nodes.Count - 2].isControlPoint = true;

		edge.n0 = nodeIndex;
		edge.c0 = nodes.Count - 3;
		edge.c1 = nodes.Count - 2;
		edge.n1 = nodes.Count - 1;


		//add entry for startnode to endnode connection
		if (edges [nodeIndex, nodes.Count - 1] == null) {
			edges[nodeIndex, nodes.Count-1] = edge;
		}
			

		AddConnection (nodeIndex, nodes.Count - 1);


		//TODO -> BezierModes 

		return edge;
	}


	public Node GetNode(int nodeIndex){
		if(nodes[nodeIndex] == null) throw new IndexOutOfRangeException ("that node was not found");			
		return nodes[nodeIndex];
	}

	public int GetIndex(Node node){
		for (int i = 0; i < nodes.Count; ++i) {
			if(node == nodes[i]) return i;
		}
		return -1;
	}


	public void Merge(Edge fromEdge, int fromNode, int toNode){

		Debug.Log ("Trying to merge n"+fromNode + " and n"+toNode);

		int x = fromEdge.n0;
		int y = toNode;
	
		if (edges [x, y] == null) {

			AddConnections(toNode, nodes[fromNode].connections);

			fromEdge.n1 = toNode;
			
			edges [x, toNode] = fromEdge;
			edges [x, fromNode] = null; 

					
			Node selectedNode = GetNode(fromNode);
			for(int i = 0; i < selectedNode.NumConnections(); ++i ){
				int oldConnection = selectedNode.GetConnectionIndex(i);
				nodes[oldConnection].RemoveConnection(fromNode);
				
			}

			nodes[fromNode] = null;


			for(int i = 0; i < nodes[toNode].NumConnections(); ++i){
				Debug.Log("Connection: " + nodes[x].connections[i]);
			}


		} else {
			Debug.Log ( "Duplicate edges are currently not supported");
		}


	}

	public void UnMerge(int nodeIndex){
		Debug.Log ("Unmerging n"+nodeIndex);
		//TODO
	}

	public void AddConnections(int fromNode, List<int> toNodes){
		for (int i = 0; i < toNodes.Count; ++i) {
			AddConnection(fromNode,toNodes[i]);
		}
	}

	public void AddConnection(List<int> nodeIndexes){
		for( int i = 1; i < nodeIndexes.Count; ++i){
			AddConnection(nodeIndexes[i-1],nodeIndexes[i]);
		}
		
	}
	public void AddConnection(int index1, int index2){
		nodes[index1].AddConnection(index2);
		nodes[index2].AddConnection(index1);
	}

	public void setNodePos(int index, Vector3 pos){

	}

	public List<Edge> pathTo(Node source, Node destination){
		//TODO
		return new List<Edge>();
	}


	public bool addCars(int numCars){
		Debug.Log ("Resizing car population to " + numCars);
		//TODO
		
		return false;
	}



}