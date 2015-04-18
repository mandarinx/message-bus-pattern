using UnityEngine;
using System.Collections;

public class PickupController : MonoBehaviour 
{

	void OnTriggerEnter ( Collider collider )
	{
		if(collider.gameObject.tag == "Player")
		{
			gameObject.SetActive(false);
			Game.Instance.AddPoint();
		}
	}

}
