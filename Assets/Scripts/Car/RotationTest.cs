using UnityEngine;
using System.Collections;

public class RotationTest : MonoBehaviour {

	private float speed = 2;
	public Transform playergraphic;
	public Vector3 dir;

	void Start(){
		//transform.forward = new Vector3(1,0,0);
		//transform.up = new Vector3(0,1,0);
		//transform.right = new Vector3(0,0,1);
		dir = new Vector3(1,0,0);
		transform.rotation = Quaternion.LookRotation(dir);
	}
	
	void  Update (){
		Movement();
		Rotation();
	}

	void Rotation(){
		dir = new Vector3(dir.x, dir.y, dir.z + 0.005f);
		transform.rotation = Quaternion.LookRotation(dir);
		Debug.Log(transform.forward);
	}
	
	void  Movement (){
		

		//Player graphic rotation
		//Vector3 moveDirection= new Vector3 (0, 0, 1);    
	//	if (moveDirection != Vector3.zero){
		//	Quaternion newRotation = Quaternion.LookRotation(moveDirection);
		//	playergraphic.transform.rotation = Quaternion.Slerp(playergraphic.transform.rotation, newRotation, Time.deltaTime * 8);
		//}
	//		transform.Rotate()
		transform.Translate(transform.forward * 1 * Time.deltaTime * speed, Space.World);

	}
}