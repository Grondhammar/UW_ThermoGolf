using UnityEngine;
using System.Collections;

public class Button_HeatClubSelect : MonoBehaviour
{
	GUIManager globalGui;
	GlobalValues globalVals;
	
	void Start()
	{
		globalGui = GameObject.FindGameObjectWithTag("GlobalTag").GetComponent("GUIManager") as GUIManager;	
		globalVals = GameObject.FindGameObjectWithTag("GlobalTag").GetComponent("GlobalValues") as GlobalValues;
	}
	
	void OnClick()
	{
		UIButton uiButtonScript;
		
		// make club selection available to all scripts
		globalVals.selectedClub = this.name;
		
		// clear perma-select on other clubs
		foreach (GameObject button in globalGui.heatClubButtons)
		{
			uiButtonScript = button.GetComponent("UIButton") as UIButton;

			// find this button and perma-select it
			if (button.name == this.name)
				uiButtonScript.isEnabled = false;
			else
				uiButtonScript.isEnabled = true;
		}
	}
}
