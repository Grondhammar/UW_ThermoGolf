using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{
	private GlobalValues globalVals;
	
	public Camera uiCam;
	public Camera hudCam;
	public Camera clubSetSelectorCam;
	public Camera heatclubCam;
	public Camera workclubCam;
	public Camera hitCam;
	public Camera energyCam;
	public Camera cam1;
	public Camera cam2;
	
	public GameObject globalsObject;
	
	public UILabel endGameMessage;
	public UILabel energyPickerReadout;
	
	public GameObject clubButtonHeat1;
	public GameObject clubButtonHeat2;
	public GameObject clubButtonHeat3;
	public GameObject clubButtonHeat4;
	public GameObject clubButtonHeat5;
	public GameObject clubButtonWork1;
	public GameObject clubButtonWork2;
	public GameObject clubButtonWork3;
	public GameObject clubButtonWork4;
	public GameObject clubButtonWork5;
	
	public GameObject clubSetSelectButtonLabel;
	private UILabel clubSetSelectLabelProps;
	
	public ArrayList heatClubButtons;
	public ArrayList workClubButtons;
	
	public bool heatClubsOn = true;
	
	public int energyHitCounter = 0;
	
	float screenHeight = 0.0f;
	float screenWidth = 0.0f;
	
	Vector2 directionalDiff;
	float scalingPercent = 0.0015f;
	
	// hides some GUI elements, sizes and moves GUI based on screen size
	void Start ()
	{
		globalsObject = GameObject.Find("Globals");
		
		globalVals = globalsObject.GetComponent("GlobalValues") as GlobalValues;
		
		// get ref to club set select button label
		clubSetSelectLabelProps = clubSetSelectButtonLabel.GetComponent("UILabel") as UILabel;
		
		// populate clubButtons arrays
		heatClubButtons = new ArrayList();
		heatClubButtons.Add(clubButtonHeat1);
		heatClubButtons.Add(clubButtonHeat2);
		heatClubButtons.Add(clubButtonHeat3);
		heatClubButtons.Add(clubButtonHeat4);
		heatClubButtons.Add(clubButtonHeat5);
		
		workClubButtons = new ArrayList();
		workClubButtons.Add(clubButtonWork1);
		workClubButtons.Add(clubButtonWork2);
		workClubButtons.Add(clubButtonWork3);
		workClubButtons.Add(clubButtonWork4);
		workClubButtons.Add(clubButtonWork5);
		
		// handle screen resolution
		screenHeight = cam1.pixelHeight + cam2.pixelHeight;
		screenWidth = cam1.pixelWidth;
		
		// scaler needs base value for screen size difference between design and reality
		directionalDiff.x = screenWidth - 648.0f;
		directionalDiff.y = screenHeight - 418.0f;
		
//Debug.Log ("height: " + screenHeight.ToString());
//Debug.Log ("width: " + screenWidth.ToString());
//Debug.Log ("dirperc X: " + directionalDiff.x.ToString());
//Debug.Log ("dirperc Y: " + directionalDiff.y.ToString());
		
		// lock club selection to bottom left of screen
		GameObject clubSetGUI = GameObject.Find ("ClubSetSelectorPanel");
		clubSetGUI.transform.position = new Vector3(
			clubSetGUI.transform.position.x - (directionalDiff.x * scalingPercent),
			clubSetGUI.transform.position.y - (directionalDiff.y * scalingPercent),
			clubSetGUI.transform.position.z);
		GameObject heatclubGUI = GameObject.Find ("HeatClubSelectorPanel");
		heatclubGUI.transform.position = new Vector3(
			heatclubGUI.transform.position.x - (directionalDiff.x * scalingPercent),
			heatclubGUI.transform.position.y - (directionalDiff.y * scalingPercent),
			heatclubGUI.transform.position.z);
		GameObject workclubGUI = GameObject.Find ("WorkClubSelectorPanel");
		workclubGUI.transform.position = new Vector3(
			workclubGUI.transform.position.x - (directionalDiff.x * scalingPercent),
			workclubGUI.transform.position.y - (directionalDiff.y * scalingPercent),
			workclubGUI.transform.position.z);

		// lock move counter to left just below mid
		GameObject counterGUI = GameObject.Find ("MoveCounterPanel");
		counterGUI.transform.position = new Vector3(
			counterGUI.transform.position.x - (directionalDiff.x * scalingPercent),
			counterGUI.transform.position.y,
			counterGUI.transform.position.z);
		
		// lock hit area to bottom right
		GameObject hitGUI = GameObject.Find ("HitRegionPanel");
		hitGUI.transform.position = new Vector3(
			hitGUI.transform.position.x + (directionalDiff.x * scalingPercent),
			hitGUI.transform.position.y - (directionalDiff.y * (2 * scalingPercent)),
			hitGUI.transform.position.z);
	}
	
	public void toggleClubVisibility(clubtypes toggleToClubType = clubtypes.notype)
	{
//Debug.Log(toggleToClubType.ToString() + "<--param:selected-->" + globalVals.selectedClubType.ToString());
		if (toggleToClubType == clubtypes.work || globalVals.selectedClubType == clubtypes.heat)
		{
			// switch selected and label to work clubs
			globalVals.selectedClubType = clubtypes.work;
			clubSetSelectLabelProps.text = "Work\nClubs";
			
			// first, turn off heat club cam and turn on work club cam
			heatclubCam.enabled = false;
			workclubCam.enabled = true;
			
			// next, turn off heat club select colliders and turn on work club colliders
			foreach (GameObject clubbutton in heatClubButtons)
				clubbutton.transform.collider.enabled = false;
			foreach (GameObject clubbutton in workClubButtons)
				clubbutton.transform.collider.enabled = true;
		}
		else
		{
			// switch to heat clubs, same process as above
			globalVals.selectedClubType = clubtypes.heat;
			clubSetSelectLabelProps.text = "Heat\nClubs";
			heatclubCam.enabled = true;
			workclubCam.enabled = false;
			foreach (GameObject clubbutton in workClubButtons)
				clubbutton.transform.collider.enabled = false;
			foreach (GameObject clubbutton in heatClubButtons)
				clubbutton.transform.collider.enabled = true;
		}
	}
}
