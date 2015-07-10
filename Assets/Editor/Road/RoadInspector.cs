using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Road))] //This lets unity know that we want to use this class to inspect road-objects in the editor
public class RoadInspector : Editor { //Use editor instead of monobehaviour to set the correct context (extending editor)

	public Road road; 
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
		road = (Road)target;
	}


	private const float directionScale = 0.5f;
	
	private void OnSceneGUI () {
		handleTransform = road.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;
		
		Vector3 p0 = ShowPoint(0);
		for (int i = 1; i < road.ControlPointCount; i += 3) {
			Vector3 p1 = ShowPoint(i);
			Vector3 p2 = ShowPoint(i + 1);
			Vector3 p3 = ShowPoint(i + 2);
			
			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);
			
			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}
		
		ShowDirections();
	}
	
	private void ShowDirections () {
		Handles.color = Color.green;
		Vector3 point = road.GetPoint(0f);
		Handles.DrawLine(point, point + road.GetDirection(0f) * directionScale);
		int steps = stepsPerRoad * road.RoadCount;
		for (int i = 1; i <= steps; i++) {
			point = road.GetPoint(i / (float)steps);
			Handles.DrawLine(point, point + road.GetDirection(i / (float)steps) * directionScale);
		}
	}

	private Vector3 ShowPoint(int index){
		Vector3 point = handleTransform.TransformPoint(road.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 2f;
        }
        Handles.color = modeColors[(int)road.GetControlPointMode(index)];
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			selectedIndex = index;
			Repaint ();
		}
		if (selectedIndex == index) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(road, "Move Point");
				EditorUtility.SetDirty(road);
				road.SetControlPoint(index,handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}


	//Draw add-button
	public override void OnInspectorGUI () {

		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", road.Loop);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(road, "Toggle Loop");
			EditorUtility.SetDirty(road);
            road.Loop = loop;
        }
        if (selectedIndex >= 0 && selectedIndex < road.ControlPointCount) {
			DrawSelectedPointInspector();
		}
		if (GUILayout.Button("Add Road")) {
			Undo.RecordObject(road, "Add Road");
			road.AddRoad(selectedIndex);
			EditorUtility.SetDirty(road);
		}
	}

	private void DrawSelectedPointInspector() {
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", road.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(road, "Move Point");
			EditorUtility.SetDirty(road);
			road.SetControlPoint(selectedIndex, point);
		}

		EditorGUI.BeginChangeCheck();
		Road.BezierControlPointMode mode = (Road.BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", road.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(road, "Change Point Mode");
			road.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(road);
		}
	}







}
