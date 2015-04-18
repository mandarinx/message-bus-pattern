using UnityEngine;
using System.Collections;

public class Game 
{
	int PointsToWin = 14;

	int Points = 0;

	Message addPointMessage;

	public void AddPoint ()
	{
		Points++;
		MessageBus.Instance.SendMessage (addPointMessage);
	}

	public int GetPoints ()
	{
		return Points;
	}

	public bool HasWon ()
	{
		return Points == PointsToWin;
	}

	/* Singleton */
	static Game instance;

	public static Game Instance
	{
		get 
		{
			if(instance == null)
				instance = new Game();

			return instance;
		}
	}

	private Game ()
	{
		addPointMessage = new Message ();
		addPointMessage.Type = MessageType.PointAdded;
	}
}
