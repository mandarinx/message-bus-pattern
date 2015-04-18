using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour 
{
	public Vector3 Rotation = new Vector3(15, 30, 45);

	void Update () 
	{
		transform.Rotate(Rotation * Time.deltaTime);
	}
}
