using UnityEngine;
using System.Collections;

public class Button_HeatWorkToggle : MonoBehaviour
{
	GUIManager globalGui;
	//GlobalValues globalVals;
	
	void Start()
	{
		globalGui = GameObject.Find("Globals").GetComponent("GUIManager") as GUIManager;	
		//globalVals = GameObject.Find("Globals").GetComponent("GlobalValues") as GlobalValues;	
	}

	void OnClick()
	{
		globalGui.toggleClubVisibility();
	}
}
