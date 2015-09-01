using UnityEngine;
using System.Collections;


[System.Serializable]
public class Edge{

	[SerializeField]
	public Node node0;
	[SerializeField]
	public Node node1;

    [SerializeField]
	public Node control0;
	[SerializeField]
	public Node control1;


	public Vector3 Progress(float t){

		return new Vector3();
	}
	
}
