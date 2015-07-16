using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Network))] //This lets unity know that we want to use this class to inspect road-objects in the editor
public class NetworkInspector : Editor { //Use editor instead of monobehaviour to set the correct context (extending editor)
	
	public Network network; 
	public int selectedRoad = -1;
	private Transform handleTransform;
	private Quaternion handleRotation;
	
	private const int lineSteps = 10;
	private const int stepsPerRoad = 10;
	
	
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;
	
	private int selectedIndex = -1;
	
	private static Color[] modeColors = {
		Color.white,
		Color.yellow,
		Color.cyan
	};
	
	
	void OnEnable(){
		network = (Network)target;
		SceneView.onSceneGUIDelegate -= OnScene;
		SceneView.onSceneGUIDelegate += OnScene;
		
	}
	private void OnScene(SceneView sceneview)
	{
		if(this)OnSceneGUI();
		
	}
	
	private const float directionScale = 0.5f;
	
	
	
	
	private void OnSceneGUI () {
		handleTransform = network.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;


		for(int r = 0; r < network.roads.Length; ++r){
		//		roadIndex = r;
			Road road = network.GetRoad(r);
			Vector3 p0 = ShowPoint(r, 0,true);

			for (int i = 1; i < road.ControlPointCount; i += 3) {
				Vector3 p1 = ShowPoint(r,i, false);
				Vector3 p2 = ShowPoint(r,i + 1, false);
				Vector3 p3 = ShowPoint(r,i + 2, true);
				
				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);
				
				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}		
			//ShowDirections(r);
		}

	}
	
	
	private void ShowDirections (int roadIndex) {
		Road road = network.GetRoad(roadIndex);
		Handles.color = Color.green;
		Vector3 point = network.GetPoint(road,0f);
		Handles.DrawLine(point, point + network.GetDirection(road,0f) * directionScale);
		int steps = stepsPerRoad * road.RoadCount;
		for (int i = 1; i <= steps; i++) {
			point = network.GetPoint(road,i / (float)steps);
			Handles.DrawLine(point, point + network.GetDirection(road,i / (float)steps) * directionScale);
		}
	}
	
	private Vector3 ShowPoint(int roadIndex, int index, bool canAddRoad){
		Road road = network.GetRoad(roadIndex);
		Vector3 point = handleTransform.TransformPoint(road.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 2f;
		}
		if(canAddRoad)Handles.color = modeColors[(int)road.GetControlPointMode(index)];
		else Handles.color = Color.magenta;
		
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			selectedIndex = index;
			selectedRoad = roadIndex;
			Repaint ();
		}
		if (selectedIndex == index && selectedRoad == roadIndex) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(network, "Move Point");
				EditorUtility.SetDirty(network);
				road.SetControlPoint(index,handleTransform.InverseTransformPoint(point));
			}
	
			//draw all the connecting points in a different color
			int conLength = network.GetNode(selectedRoad,selectedIndex).NumConnections();
			Vector3 selectedNodePos = network.GetNode(selectedRoad,selectedIndex).pos;
			Handles.color = Color.green;
			for(int i = 0; i < conLength; ++i){
				Node conNode = network.GetNode(selectedRoad,selectedIndex).GetConnection(i);
				Vector3 conPointTransformed = handleTransform.TransformPoint(conNode.pos);
				float circleSize = 0.025f*( Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, conPointTransformed));
				Handles.CircleCap(0, conPointTransformed, SceneView.currentDrawingSceneView.rotation, circleSize);

			}


			//Lets find the closest point to the selected point and draw it
			Handles.color = Color.cyan; //let blue denote the node to connect to
			float closestDistance = float.MaxValue;
			Node curNode;
			Node closestNode = new Node(new Vector3(0,0,0));
			for(int r = 0; r < network.roads.Length; ++r){
				for(int n = 0; n < network.GetRoad(r).points.Length; n += 3){

					if(r == selectedRoad && n == selectedIndex) continue;
				
					curNode = network.GetNode(r,n);
					float curDistance = Vector3.Distance(curNode.pos, selectedNodePos); 
					if(curDistance < closestDistance){
						closestDistance = curDistance;
						closestNode = curNode;
					}
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
		if(selectedRoad >= 0){

			Road road = network.GetRoad(selectedRoad);
			EditorGUI.BeginChangeCheck();
			bool loop = EditorGUILayout.Toggle("Loop", road.Loop);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(network, "Toggle Loop");
				EditorUtility.SetDirty(network);
				road.Loop = loop;
			}
			if (selectedIndex >= 0 && selectedIndex < road.ControlPointCount) {
				DrawSelectedPointInspector(selectedRoad);
			}
			if(selectedIndex % 3 == 0){
				if (GUILayout.Button("Add Road")) {
					Undo.RecordObject(network, "Add Road");
					network.AddRoad(selectedRoad,selectedIndex);
					EditorUtility.SetDirty(network);
				}

				Node node = network.GetNode(selectedRoad, selectedIndex);

				if( node.NumConnections() == 1){
					if (GUILayout.Button("Connect Node")) {
						Undo.RecordObject(network, "Connect Node");
						network.Merge(selectedRoad,selectedIndex, 0, 0);
						EditorUtility.SetDirty(network);
						
					}
				}
				else{
					if (GUILayout.Button("Disconnect Node")) {
						Undo.RecordObject(network, "Disconnect Node");
						network.UnMerge(selectedRoad,selectedIndex);
						EditorUtility.SetDirty(network);
						
					}				
				}
			}
		}
	}
	
	private void DrawSelectedPointInspector(int roadIndex) {
		Road road = network.GetRoad(roadIndex);

		//Selected point in inspector
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", road.GetControlPoint(selectedIndex));
		Debug.Log("RdIdx: " + roadIndex + " ptIdx: " + selectedIndex);
		if (EditorGUI.EndChangeCheck()) { 
			Undo.RecordObject(network, "Move Point");
			EditorUtility.SetDirty(network);
			road.SetControlPoint(selectedIndex, point);

		}
		
		EditorGUI.BeginChangeCheck();
		Road.BezierControlPointMode mode = (Road.BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", road.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(network, "Change Point Mode");
			road.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(network);
		}


		//Number of connections in inspector
		EditorGUILayout.IntField("Connections", network.GetNode(roadIndex,selectedIndex).NumConnections());

	}
	
	
	
	
	
	
	
}
