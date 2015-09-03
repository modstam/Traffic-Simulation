using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[System.Serializable]
[CustomEditor (typeof(Network))] //This lets unity know that we want to use this class to inspect road-objects in the editor
public class NetworkInspector : Editor { //Use editor instead of monobehaviour to set the correct context (extending editor)
	[SerializeField]
	Network network; 

	private Transform handleTransform;
	private Quaternion handleRotation;
	
	private const int lineSteps = 10;
	private const int stepsPerRoad = 10;
	
	
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;
	
	private int selectedIndex = -1;
	private int closestNodeIndex = -1;
	private Edge selectedEdge = null;
	private Node selectedNode = null;

	private const float directionScale = 0.5f;

	

	void OnEnable(){


		network = (Network)target;
		if(network == null){
			Debug.Log("No network to target");
		}
	}


	private void OnScene(SceneView sceneview)
	{
		if(this)OnSceneGUI();
		
	}

	
	private void OnSceneGUI () {
		handleTransform = network.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		for (int x = 0; x < network.nodes.Count; ++x) {
			for(int y = 0; y < network.nodes.Count; ++y){
				if(x==y) continue;

				Edge curEdge = network.edges[x,y];
				if(curEdge == null) continue;

				Vector3 p0 = ShowNode(network.nodes[curEdge.n0], x, curEdge);
				Vector3 p1 = ShowControlNode(network.nodes[curEdge.c0], curEdge);
				Vector3 p2 = ShowControlNode(network.nodes[curEdge.c1], curEdge);
				Vector3 p3 = ShowNode (network.nodes[curEdge.n1], y, curEdge);

				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);
				
				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;

				//Debug.Log (" Drew " + x + "," + y );

			}
		}
	}

	private Vector3 ShowControlNode(Node node, Edge edge){
		Vector3 point = handleTransform.TransformPoint(node.pos);
		Handles.color = Color.magenta;


		if (Handles.Button(point, handleRotation, handleSize, pickSize, Handles.DotCap)) {
			selectedEdge = edge;
			selectedNode = node;

			Repaint ();
		}

		if (selectedNode == node && node.isControlPoint) {

			EditorGUI.BeginChangeCheck ();
			point = Handles.DoPositionHandle (point, handleRotation);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject (network, "Move Control Point");
				EditorUtility.SetDirty (network);
				Bezier.SetControlPoint(network, selectedEdge ,selectedNode, handleTransform.InverseTransformPoint(point));
			}		

		}
		return point;

	}

	
	private Vector3 ShowNode(Node node, int index, Edge edge){

		Vector3 point = handleTransform.TransformPoint(node.pos);
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 2f;
		}
		Handles.color = Color.white; //Handles.color = modeColors[(int)Bezier.GetControlPointMode(network,roadIndex,index)];


		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			selectedIndex = index;
			selectedEdge = edge;
			selectedNode = node;
			Repaint ();
		}

		GUIStyle style = new GUIStyle ();
		style.normal.textColor = Color.white;
		style.fontSize = 15;
		Handles.Label (point + new Vector3(0,0.5f,0), "" + index,style );

		if (selectedIndex == index && !selectedNode.isControlPoint) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(network, "Move Point");
				EditorUtility.SetDirty(network);
				Bezier.SetControlPoint(network, selectedEdge, selectedNode,handleTransform.InverseTransformPoint(point));
			}
		

				//draw all the connecting points in a different color
				int conLength = network.GetNode(selectedIndex).NumConnections();
				Handles.color = Color.green;
				for(int i = 0; i < conLength; ++i){
					Node conNode = network.GetNode(selectedIndex).GetConnectionNode(network, i);
					Vector3 conPointTransformed = handleTransform.TransformPoint(conNode.pos);
					float circleSize = 0.025f*( Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, conPointTransformed));
					Handles.CircleCap(0, conPointTransformed, SceneView.currentDrawingSceneView.rotation, circleSize);

				}


				//Lets find the closest point to the selected point and draw it
				Handles.color = Color.cyan; //let blue denote the node to connect to
				float closestDistance = float.MaxValue;
				Node curNode;
				Node closestNode = new Node(new Vector3(0,0,0));

				for(int i = 0; i < network.nodes.Count; ++i){
					if (selectedIndex == i) continue;
					if (network.nodes[i].isControlPoint) continue;

					curNode = network.nodes[i];

					float curDistance = Vector3.Distance(curNode.pos, node.pos);

					if(curDistance < closestDistance){
						closestDistance = curDistance;
						closestNode = curNode;
						closestNodeIndex = i;
					}

				}
				Vector3 closePointTransformed = handleTransform.TransformPoint(closestNode.pos);
				float closeCircleSize = 0.035f * ( Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, closePointTransformed));
				Handles.CircleCap(0, closePointTransformed, SceneView.currentDrawingSceneView.rotation, closeCircleSize);


			}

		return point;
	}
	
	
	//Draw add-button
	public override void OnInspectorGUI () {


		if(selectedIndex >= 0 && !selectedNode.isControlPoint){

				
			DrawSelectedPointInspector();
			Edge edge = null;
				if (GUILayout.Button("Add Road")) {
					Undo.RecordObject(network, "Add Road");
					//Debug.Log (selectedIndex);
					edge = network.AddEdge(selectedIndex);
					EditorUtility.SetDirty(network);
				}

				Node node = network.GetNode(selectedIndex);

				if( node.NumConnections() == 1){
					if (GUILayout.Button("Connect Node")) {
						Undo.RecordObject(network, "Connect Node");
						network.Merge(selectedEdge, selectedIndex, closestNodeIndex );
						EditorUtility.SetDirty(network);
						selectedIndex = -1;
						selectedEdge = null;
						selectedNode = null;
						closestNodeIndex = -1;
					}
				}
				else{
					if (GUILayout.Button("Disconnect Node")) {
						Undo.RecordObject(network, "Disconnect Node");
						network.UnMerge(selectedIndex);
						EditorUtility.SetDirty(network);
						
					}				
				}
		}
	}
	
	private void DrawSelectedPointInspector() {

		//Selected point in inspector
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", network.nodes[selectedIndex].pos);
		//Debug.Log("RdIdx: " + roadIndex + " ptIdx: " + selectedIndex);
		if (EditorGUI.EndChangeCheck()) { 
			Undo.RecordObject(network, "Move Point");
			EditorUtility.SetDirty(network);
			Bezier.SetControlPoint(network, selectedEdge, selectedNode, point);

		}

		/**
		EditorGUI.BeginChangeCheck();
		Bezier.BezierControlPointMode mode = (Bezier.BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", Bezier.GetControlPointMode(network,selectedEdge,selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(network, "Change Point Mode");
			Bezier.SetControlPointMode(network, selectedEdge,selectedNode,  mode);
			EditorUtility.SetDirty(network);
		}
		**/


		//Number of connections in inspector
		EditorGUILayout.IntField("Connections", network.GetNode(selectedIndex).NumConnections());

	}
	
	
	
	
	
	
	
}
