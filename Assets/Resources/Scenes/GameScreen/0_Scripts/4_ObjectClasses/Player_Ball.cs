using UnityEngine;
using System.Collections;

public class Player_Ball : MonoBehaviour
{
	public GameObject ball;
	private bool instantiated = false;
	public bool isMoving = false;
	public HUDText lineLabel;
	private GameObject labelBase;
	private GameObject hudChild;
	GlobalValues globalVals;
	GUIManager guiManager;
	
	void Start ()
	{
		name = "player_ball";
		guiManager = GameObject.FindGameObjectWithTag("GlobalTag").GetComponent("GUIManager") as GUIManager;
		globalVals = GameObject.FindGameObjectWithTag("GlobalTag").GetComponent("GlobalValues") as GlobalValues;
	}
	
	void Update()
	{
		if (!instantiated)
		{
			// create HUD text heirarchy
			if (HUDRoot.go != null)
			{
				labelBase = new GameObject("labelbase" + name);
				labelBase.transform.position = ball.transform.position;
				hudChild = NGUITools.AddChild(HUDRoot.go,globalVals.hudPrefab);
				lineLabel = hudChild.GetComponentInChildren<HUDText>();
				hudChild.AddComponent<UIFollowTarget>().target = labelBase.transform;
				hudChild.GetComponent<UIFollowTarget>().uiCamera = guiManager.hudCam;
				hudChild.GetComponent<UIFollowTarget>().gameCamera = guiManager.cam1;
				
				instantiated = true;
			}	
		}
	}
}
