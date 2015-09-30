using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


/**
* This class is the wrapper class for the graph that contains all the roads.
* It has functionality for managing the graph as well as to find a path from a node to another node.
* 
*/


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
		//Stick graph-related frame sensitive code here
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

		AddConnection(0,3	);


	}

	/**
	 * This method will add an edge at a specified node in the graph.
	 * The edge will start from the node and end with a newly created node. 
	 * */
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

		Debug.Log ("added " + edge.ToString ());


		//add entry for startnode to endnode connection
		if (edges [nodeIndex, nodes.Count - 1] == null) {
			edges[nodeIndex, nodes.Count-1] = edge;
		}
			

		AddConnection (nodeIndex, nodes.Count - 1);

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

	/**
	 * Merges an edge with a target node, 
	 * the last node on the edge will be replaced by the target node.
	**/
	public void Merge(Edge fromEdge, int fromNode, int toNode){

		Debug.Log ("Trying to merge n"+fromNode + " and n"+toNode);

		int x = fromEdge.n0;
		int y = toNode;

		//Check that this edge does not exist already
		if (edges [x, y] == null) {

			//transfer connections
			AddConnections(toNode, nodes[fromNode].connections);

			fromEdge.n1 = toNode;

			//Connect edges and remove old edge. 
			edges [x, toNode] = fromEdge;
			edges [x, fromNode] = null; 
			edges.Remove (x,fromNode);

			//Remove all the old connections	
			Node selectedNode = GetNode(fromNode);
			for(int i = 0; i < selectedNode.NumConnections(); ++i ){
				int oldConnection = selectedNode.GetConnectionIndex(i);
				nodes[oldConnection].RemoveConnection(fromNode);
				
			}
			//disable the node that was replaced (setting == null is unreliable)
			nodes[fromNode].isActive = false;



			for(int i = 0; i < nodes[toNode].NumConnections(); ++i){
				Debug.Log("Connection: " + nodes[x].connections[i]);
			}


		} else {
			Debug.Log ( "Duplicate edges are currently not supported");
		}


	}

	/**
	 * This method will disconnect an edge from another edge
	 * NOTE: currently not implemented
	 * */ 
	public void UnMerge(int nodeIndex){
		Debug.Log ("Unmerging n"+nodeIndex);
		//TODO
	}

	/**
	 * Connects a node to all nodes in the inputlist
	*/
	public void AddConnections(int fromNode, List<int> toNodes){
		for (int i = 0; i < toNodes.Count; ++i) {
			AddConnection(fromNode,toNodes[i]);
		}
	}

	/**
	 * Connects all nodes in the list with eachother
	*/
	public void AddConnection(List<int> nodeIndexes){
		for( int i = 1; i < nodeIndexes.Count; ++i){
			AddConnection(nodeIndexes[i-1],nodeIndexes[i]);
		}
		
	}

	/**
	 * Connects two specified nodes
	*/
	public void AddConnection(int index1, int index2){
		nodes[index1].AddConnection(index2);
		nodes[index2].AddConnection(index1);
	}
	

	public List<Edge> PathTo(int source, int destination){
		/**
		 *	this A* variant was implemented straight off of wikipedias version
		 *	https://en.wikipedia.org/wiki/A*_search_algorithm
		 *
		 *  See wiki for more information
		**/

		if (nodes [source].isControlPoint || nodes [destination].isControlPoint) {
			throw new ArgumentException("Cannot use control points as start or goal");
			return new List<Edge>();
		}


		HashSet<int> closedSet = new HashSet<int> ();
		HashSet<int> openSet = new HashSet<int> ();
		openSet.Add (source);
		int count = 1;

		Dictionary<int, int> cameFrom = new Dictionary<int,int> ();
		Dictionary<int, float> gScore = new Dictionary<int,float> ();
		Dictionary<int, float> fScore = new Dictionary<int,float> ();



		//initialize scores with max int values
		for (int i = 0; i < nodes.Count; ++i) {
			if(!nodes[i].isActive) continue;
			gScore.Add (i, float.MaxValue);
			fScore.Add (i, float.MaxValue);
		}

		gScore [source] = 0.0f;
		fScore [source] = Vector3.Distance(nodes[source].pos, nodes[destination].pos);


		while (count > 0) {
			int current = FindLowestScore(fScore);
			if(current == destination){
				//Debug.Log ("found path from " + source + " to " + destination);
				return ConstructPath (cameFrom, current, source);
			}
				

			openSet.Remove(current);
			fScore.Remove (current);
			--count;
			closedSet.Add(current);

			foreach (int neighbor in nodes[current].connections){
				if(closedSet.Contains(neighbor)) continue;
				//Debug.Log("checking neighbor " + neighbor);
				float tentativeGScore = gScore[current] + Vector3.Distance(nodes[current].pos, nodes[neighbor].pos);

				if(!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor]){
					cameFrom[neighbor] = current;
					gScore[neighbor] = tentativeGScore;
					fScore[neighbor] = tentativeGScore + Vector3.Distance (nodes[neighbor].pos, nodes[destination].pos);
					if (!openSet.Contains(neighbor)){
						openSet.Add(neighbor);
						++count;
					}
				}
			}

		}

		return new List<Edge>();
	}



	private int FindLowestScore(Dictionary<int, float> inMap){
		int lowestIndex = -1;
		float lowestValue = float.MaxValue;
		foreach(KeyValuePair<int,float> kvp in inMap){
			if(kvp.Value < lowestValue){
				lowestValue = kvp.Value;
				lowestIndex = kvp.Key;
			}
		}
		//Debug.Log ("lowest index " + lowestIndex + " ; lowest value " + lowestValue);
		return lowestIndex;
	}

	private List<Edge> ConstructPath(Dictionary<int, int> map, int current, int start){
	
		List<Edge> path = new List<Edge> ();
		int maxIter = map.Count * 2;
		int iter = 0;
		while (current != start && iter < maxIter) {
			Edge edge = edges[ map[current], current];
			if(edge == null){ //if edge is null, it must be reversed
				//Debug.Log ("didnt find edge " + map[current] + "->" + current);
				edge = edges[current, map[current]];
				Edge reverseEdge = new Edge();
				reverseEdge.n0 = edge.n0;
				reverseEdge.c0 = edge.c0;
				reverseEdge.c1 = edge.c1;
				reverseEdge.n1 = edge.n1;
				reverseEdge.reverse = true;
				path.Add (reverseEdge);
			}
			else{
				//Debug.Log ("Found edge " + edge.n0 + "->" + edge.n1 + " Reversed: " + edge.reverse);
				path.Add (edge);
			}
			current = map[current];
			++iter;
		}
		path.Reverse ();

		return path;
	}



}