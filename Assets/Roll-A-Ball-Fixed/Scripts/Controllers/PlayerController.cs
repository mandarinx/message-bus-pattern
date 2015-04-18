using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{	
	public float Speed = 500.0f;

	Rigidbody rigidBody;
	Vector3 movement = Vector3.zero;

	Message positionMessage;

	void Start()
	{
		rigidBody = GetComponent<Rigidbody> ();
		CreatePositionMessage ();
	}

	void CreatePositionMessage ()
	{
		positionMessage = new Message ();
		positionMessage.Type = MessageType.PlayerPosition;
	}
	
	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		
		movement.Set (moveHorizontal, 0.0f, moveVertical);
		
		rigidBody.AddForce (movement * Speed * Time.deltaTime);
	}

	void Update ()
	{
		positionMessage.Vector3Value = transform.position;
		MessageBus.Instance.SendMessage (positionMessage);
	}
	
}
