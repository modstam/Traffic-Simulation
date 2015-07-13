using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Network))] //This lets unity know that we want to use this class to inspect road-objects in the editor
public class NetworkInspector : Editor { //Use editor instead of monobehaviour to set the correct context (extending editor)
	
	public Network network; 
	public int roadIndex;
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
			roadIndex = r;
			Road road = network.GetRoad(roadIndex);
			Vector3 p0 = ShowPoint(0,true);

			for (int i = 1; i < road.ControlPointCount; i += 3) {
				Vector3 p1 = ShowPoint(i, false);
				Vector3 p2 = ShowPoint(i + 1, false);
				Vector3 p3 = ShowPoint(i + 2, true);
				
				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);
				
				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}
			
			//ShowDirections();

		}



	}
	
	
	private void ShowDirections () {
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
	
	private Vector3 ShowPoint(int index, bool canAddRoad){
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
			Repaint ();
		}
		if (selectedIndex == index) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(network, "Move Point");
				EditorUtility.SetDirty(network);
				road.SetControlPoint(index,handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}
	
	
	//Draw add-button
	public override void OnInspectorGUI () {
		Road road = network.GetRoad(roadIndex);
		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", road.Loop);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(network, "Toggle Loop");
			EditorUtility.SetDirty(network);
			road.Loop = loop;
		}
		if (selectedIndex >= 0 && selectedIndex < road.ControlPointCount) {
			DrawSelectedPointInspector();
		}
		if (GUILayout.Button("Add Road")) {
			Undo.RecordObject(network, "Add Road");
			network.AddRoad(roadIndex,selectedIndex);
			EditorUtility.SetDirty(network);
		}
	}
	
	private void DrawSelectedPointInspector() {
		Road road = network.GetRoad(roadIndex);
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", road.GetControlPoint(selectedIndex));
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
	}
	
	
	
	
	
	
	
}
