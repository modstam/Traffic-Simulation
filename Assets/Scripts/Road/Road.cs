

using UnityEngine;
using System.Collections;
using System;

public class Road {
	
	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
    }
    
    [SerializeField]
	public Node[] points; //This is the points on our road
	[SerializeField]
	private BezierControlPointMode[] modes;
	[SerializeField]
	private bool loop;
    
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Reset(){
		Debug.Log("Reset road");
		points = new Node[]{
			new Node(new Vector3(1f, 0f, 0f)),
			new Node(new Vector3(2f, 0f, 0f)),
			new Node(new Vector3(3f, 0f, 0f)),
			new Node(new Vector3(4f, 0f, 0f))
        };
		AddConnection(0,1);
		AddConnection(1,2);
		AddConnection(2,3);

		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
        };

        
    }

	public void Reset(Node[] start){
		if(start.Length == 4){
			points = start;

			AddConnection(0,1);
			AddConnection(1,2);
			AddConnection(2,3);

			modes = new BezierControlPointMode[] {
				BezierControlPointMode.Free,
				BezierControlPointMode.Free
			};
		}
		else throw new Exception(@"[Reset(Node[] start) -> Argument MUST be of length 4]");

	}
	
	public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
			if (value == true) {
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0].pos);
            }
        }
    }

	public int ControlPointCount {
		get {
			return points.Length;
		}
	}


	public Vector3 GetControlPoint (int index) {

		//Debug.Log (points[index].pos);

		try{
			return points[index].pos;
		}
		catch(Exception e){
			Debug.Log (e.StackTrace);
			Debug.Log("GetControlPoint error: length: " + points.Length + " , index:" + index );
		}
		return new Vector3(0,0,0);
	}


	public void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points[index].pos;
			if (loop) {
				if (index == 0) {
					points[1].pos += delta;
					points[points.Length - 2].pos += delta;
					points[points.Length - 1].pos = point;
				}
				else if (index == points.Length - 1) {
					points[0].pos = point;
					points[1].pos += delta;
					points[index - 1].pos += delta;
				}
				else {
					points[index - 1].pos += delta;
					points[index + 1].pos += delta;
				}
            }
            else {
                if (index > 0) {
					points[index - 1].pos += delta;
                }
                if (index + 1 < points.Length) {
					points[index + 1].pos += delta;
                }
            }
        }
		points[index].pos = point;
        EnforceMode(index);
    }
	
	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop) {
			if (modeIndex == 0) {
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1) {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }
	
	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) {
			return;
		}
		
		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = points.Length - 2;
			}
		}
		
		Vector3 middle = points[middleIndex].pos;
		Vector3 enforcedTangent = middle - points[fixedIndex].pos;
        if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex].pos);
        }
		points[enforcedIndex].pos = middle + enforcedTangent;
	}

	

    

	public Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
				6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
    }

	
	public void AddRoad (int index) {

		if(!(index > 0 && index < points.Length -1)){
			 

			Vector3 point = points[points.Length-1].pos;
			Array.Resize(ref points, points.Length + 3);
			point.x += 1f;
			points[points.Length-3] = new Node(point);
			point.x += 1f;
			points[points.Length-2] = new Node(point);
			point.x += 1f;
			points[points.Length-1] = new Node(point);

			AddConnection(index, index-1);   //add connnections between all the nodes
			AddConnection(index-1, index-2);
					
			
			Array.Resize(ref modes, modes.Length + 1);
			modes[modes.Length - 1] = modes[modes.Length - 2];
			EnforceMode(points.Length - 4);
			
			if (loop) {
				points[points.Length - 1].pos = points[0].pos;
				modes[modes.Length - 1] = modes[0];
				EnforceMode(0);
			}

		}
		else{


			//TODO : DO SOMETHING

		}



    }

	public int RoadCount {
		get {
			return (points.Length - 1) / 3;
        }
    }


	public BezierControlPointMode GetControlPointMode (int index) {
		//Debug.Log ("index: " + index + " size: " +  modes.Length);
		return modes[(index + 1) / 3];
	}

	public void AddConnection(int index, Road road){
		points[index].AddConnection(road.points[0]);
		road.points[0].AddConnection(points[index]);
	}
	public void AddConnection(int index1, int index2){
		points[index1].AddConnection(points[index2]);
		points[index2].AddConnection(points[index1]);
	}
}
