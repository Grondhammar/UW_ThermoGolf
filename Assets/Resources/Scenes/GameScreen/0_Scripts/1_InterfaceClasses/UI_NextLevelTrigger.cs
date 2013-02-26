using UnityEngine;
using System.Collections;

public class UI_NextLevelTrigger : MonoBehaviour
{
	public UILabel levelFeedbackLabel;
	GameMetaManager meta;
	GlobalValues globalVals;
	
	// Use this for initialization
	void Start ()
	{
		meta = GameObject.Find("Globals").GetComponent("GameMetaManager") as GameMetaManager;
		globalVals = GameObject.Find("Globals").GetComponent("GlobalValues") as GlobalValues;
	}
	
	void Update()
	{
		// simply check if the player has no moves left...
		if (globalVals.moveCounter < 1)
		{
			// ...and if they have none, show the fail-state screen
			meta.ShowEndLevelScreen("Try Again...");
		}
	}
}
