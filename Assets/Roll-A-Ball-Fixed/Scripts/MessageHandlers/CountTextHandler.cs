using UnityEngine;
using System.Collections;

public class CountTextHandler : MessageHandler 
{
	GUIText guiText;

	void Start ()
	{
		guiText = GetComponent<GUIText> ();
		guiText.text = "Count: 0";
	}

	public override void HandleMessage (Message message)
	{
		guiText.text = "Count: " + Game.Instance.GetPoints ();
	}

}
