using UnityEngine;
using System.Collections;

public class WinTextHandler : MessageHandler 
{
	GUIText guiText;

	void Start ()
	{
		guiText = GetComponent<GUIText> ();
		guiText.text = "";
	}

	public override void HandleMessage (Message message)
	{
		if (Game.Instance.HasWon())
			guiText.text = "YOU WIN!";
	}

}
