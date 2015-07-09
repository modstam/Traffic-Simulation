using UnityEngine;
using System.Collections;
using System;

public class Road : MonoBehaviour {
	
	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
    }
    
    [SerializeField]
	private Node[] points; //This is the points on our road
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
		points = new Node[]{
			new Node(new Vector3(1f, 0f, 0f)),
			new Node(new Vector3(2f, 0f, 0f)),
			new Node(new Vector3(3f, 0f, 0f)),
			new Node(new Vector3(4f, 0f, 0f))
        };

		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
        };

        
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
	//	Debug.Log("length: " + points.Length + " , index:" + index );
		return points[index].pos;
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

	
	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * RoadCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        
        //convert to world space
		return transform.TransformPoint(Bezier(
			points[i].pos, points[i + 1].pos, points[i + 2].pos, points[i + 3].pos, t));
    }

	public Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
				3f * oneMinusT * oneMinusT * t * p1 +
				3f * oneMinusT * t * t * p2 +
                t * t * t * p3;
	}


	//This is the velocity along the curve
	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * RoadCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
		return transform.TransformPoint(GetFirstDerivative(points[0].pos, points[1].pos, points[2].pos, points[3].pos, t)) -
			transform.position;
    }
    

	public Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
				6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
    }

	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
    }



	public void AddRoad () {
		Vector3 point = points[points.Length - 1].pos;
		Array.Resize(ref points, points.Length + 3);
		point.x += 1f;
		points[points.Length - 3] = new Node(point);
		point.x += 1f;
		points[points.Length - 2] = new Node(point);
		point.x += 1f;
		points[points.Length - 1] = new Node(point);


		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);

		if (loop) {
			points[points.Length - 1].pos = points[0].pos;
			modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }

	public int RoadCount {
		get {
			return (points.Length - 1) / 3;
        }
    }


	public BezierControlPointMode GetControlPointMode (int index) {
		return modes[(index + 1) / 3];
	}

}
