using UnityEngine;
using System.Collections;

public class BallBehaviour : MonoBehaviour
{
	GameMetaManager meta;
	public bool insideTarget = false;
	
	void Start()
	{
		meta = GameObject.Find("Globals").GetComponent("GameMetaManager") as GameMetaManager;	
	}
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.name == "EndPoint")
		{
			insideTarget = true;
			
			// endgame is triggered in the BallManager.cs EndMove function
		}
	}
	
	private void OnTriggerExit(Collider other)
	{
		if (other.name == "EndPoint")
		{
			insideTarget = false;	
		}
	}
}
