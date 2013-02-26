using UnityEngine;
using System.Collections;

public class Button_EnergyHit : MonoBehaviour
{
	GUIManager guiManager;
	GlobalValues globalVals;
	BallManager ballManager;
	
	public UILabel buttonLabel;
	public GameObject greenMeter;
	public GameObject redMeter;
	public GameObject yellowMeter;
	
	float scaleDivisor = 150.0f;
	float energyDivisor = 1;
	
	// Use this for initialization
	void Start ()
	{
		guiManager = GameObject.Find("Globals").GetComponent("GUIManager") as GUIManager;
		globalVals = GameObject.Find("Globals").GetComponent("GlobalValues") as GlobalValues;
		ballManager = GameObject.Find("Globals").GetComponent("BallManager") as BallManager;
	}
	
	void Update()
	{
		// if we're choosing the energy level, update the readout label
		if (guiManager.energyHitCounter == 1)
			guiManager.energyPickerReadout.text = Mathf.RoundToInt(globalVals.GetSelectedClubJoules() * (greenMeter.transform.localScale.x/150)).ToString() + " j";
	}
	
	void OnClick()
	{
		// increment the hit counter
		guiManager.energyHitCounter++;
		
		if (guiManager.energyHitCounter == 1)
		{
			// disappear yellow scale
			yellowMeter.renderer.enabled = false;
			
			// start up the picker anim
			iTween.ScaleTo(greenMeter,new Hashtable() {
				{"easetype",iTween.EaseType.linear},
				{"looptype",iTween.LoopType.pingPong},
				{"x",10},
				{"z",10}
			});
		}
		
		// branches based on the number of times the hit zone has been tapped
		switch (guiManager.energyHitCounter)
		{
			case 1:
				// disappear UI, show energy picker and start it up
				guiManager.heatclubCam.enabled = false;
				guiManager.workclubCam.enabled = false;
				guiManager.clubSetSelectorCam.enabled = false;
				guiManager.energyCam.enabled = true;
			
				buttonLabel.text = "Select\nEnergy";
			
				break;
			case 2:
				// lock in energy selection, move to accuracy selection
				globalVals.hitEnergy = globalVals.GetSelectedClubJoules() * (greenMeter.transform.localScale.x/scaleDivisor);
				if (globalVals.hitEnergy > 10000.0f)
					globalVals.hitEnergy = 10000.0f;
			
				// draw line on UI selector for energy choice by setting the yellow meter to the green meter value
				yellowMeter.transform.localScale = greenMeter.transform.localScale;
				yellowMeter.renderer.enabled = true;
			
				// start back up tween but only the green one
				// this way the yellow remains to show Energy choice
				greenMeter.transform.localScale = new Vector3(150,2,150);
				greenMeter.renderer.material.color = new Color(21,200,21,0.5f);
				iTween.ScaleTo(greenMeter,new Hashtable() {
					{"easetype",iTween.EaseType.linear},
					{"looptype",iTween.LoopType.pingPong},
					{"x",10},
					{"z",10}
				});
			
				buttonLabel.text = "Select\nAccuracy";
			
				break;
			case 3:
				// lock in accuracy selection, disappear meter, switch to ball movement,
				//	reset hit count, and decrement number of moves available
				globalVals.hitAccuracy = greenMeter.transform.localScale.x;
			
				// stop the meter tween and reset green, yellow to full size
				iTween.Stop();
				greenMeter.transform.localScale = new Vector3(150,2,150);
				yellowMeter.transform.localScale = new Vector3(150,2,150);
				greenMeter.renderer.material.color = new Color(21,200,21,1);

				guiManager.energyCam.enabled = false;
				guiManager.hitCam.enabled = false;
				
				guiManager.energyHitCounter = 0;
				buttonLabel.text = "Add\nEnergy";

				globalVals.moveCounter--;
				ballManager.DoMove();
			
				break;
			default:
				// should never get here
				guiManager.energyHitCounter = 0;
				break;
		}
	}
}
