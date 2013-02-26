using UnityEngine;
using System.Collections;

public class Button_NextLevel : MonoBehaviour
{
	GameMetaManager meta;
	GUIManager guiManager;
	
	void Start()
	{
		guiManager = GameObject.Find("Globals").GetComponent("GUIManager") as GUIManager;
		meta = GameObject.Find("Globals").GetComponent("GameMetaManager") as GameMetaManager;
	}
	
	void OnClick()
	{
		// disappear level buttons and start up new level
		guiManager.uiCam.enabled = false;
		meta.StartNewLevel(leveltype.newLevel);
	}
}
