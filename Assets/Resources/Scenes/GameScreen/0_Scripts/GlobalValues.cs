using UnityEngine;
using System.Collections;

public enum clubtypes {notype = 0, heat = 1, work = 2};

public enum leveltype {newLevel = 0, redoLevel = 1};

public class GlobalValues : MonoBehaviour
{
	public int moveCounter = 0;
	public float hitEnergy = 0.0f;
	public float hitAccuracy = 0.0f;
	public clubtypes selectedClubType;
	public string selectedClub;
	public Constraint_Line selectedConstraint;
	public Material basicLineMaterial;
	public Material glowingLineMaterial;
	public string lastGlowedName;
	public Vector3 previousGameStartPoint;
	public Vector3 previousGameEndPoint;
	public GameObject hudPrefab;
	
	void Start()
	{
		moveCounter = Random.Range(3,10);
	}
	
	public float GetSelectedClubJoules()
	{
		float jouleVal = 0.0f;
		
		// heat buttons
		if (selectedClub == "Button_Heat1")
			jouleVal = 440.0f;		// 440j
		else if (selectedClub == "Button_Heat2")
			jouleVal = 4400.0f;
		else if (selectedClub == "Button_Heat3")
			jouleVal = 44000.0f;
		else if (selectedClub == "Button_Heat4")
			jouleVal = 440000.0f;
		else if (selectedClub == "Button_Heat5")
			jouleVal = 4400000.0f;	// 4400Kj
		else if (selectedClub == "Button_Work1")
			jouleVal = 440.0f;		// 440j
		else if (selectedClub == "Button_Work2")
			jouleVal = 4400.0f;
		else if (selectedClub == "Button_Work3")
			jouleVal = 44000.0f;
		else if (selectedClub == "Button_Work4")
			jouleVal = 440000.0f;
		else if (selectedClub == "Button_Work5")
			jouleVal = 4400000.0f;	// 4400Kj
		
		return jouleVal;
	}
}
