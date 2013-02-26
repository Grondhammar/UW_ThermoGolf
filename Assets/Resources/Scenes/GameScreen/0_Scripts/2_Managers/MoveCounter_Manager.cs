using UnityEngine;
using System.Collections;

public class MoveCounter_Manager : MonoBehaviour
{
	GlobalValues globalVals;
	public UILabel moveCountUI;
	
	void Start()
	{
		globalVals = GameObject.Find("Globals").GetComponent("GlobalValues") as GlobalValues;
	}
	
	void Update()
	{
		moveCountUI.text = globalVals.moveCounter.ToString();
	}
}
