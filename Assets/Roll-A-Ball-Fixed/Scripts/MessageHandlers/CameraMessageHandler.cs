using UnityEngine;
using System.Collections;

public class CameraMessageHandler : MessageHandler 

{
	Vector3 offset;
	
	void Start () 
	{
		offset = transform.position;
	}

	public override void HandleMessage (Message message)
	{
		transform.position = message.Vector3Value + offset;
	}

}
