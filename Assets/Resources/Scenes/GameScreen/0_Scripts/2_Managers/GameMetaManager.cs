using UnityEngine;
using System.Collections;

/***
 * Class to handle starting, restarting and ending the game along
 * 	with hiding/showing related sets of UI
 */
public class GameMetaManager : MonoBehaviour
{
	ConstraintManager lineManager;
	GraphManager graphManager;
	GUIManager guiManager;
	GlobalValues globalVals;
	Player_Ball ballClass;
	GameObject endPoint;
	public bool gameInitialised = false;
	
	void Start ()
	{
		GameObject globalObj = GameObject.FindGameObjectWithTag("GlobalTag");
		
		lineManager = globalObj.GetComponent("ConstraintManager") as ConstraintManager;
		graphManager = globalObj.GetComponent("GraphManager") as GraphManager;
		guiManager = globalObj.GetComponent("GUIManager") as GUIManager;
		globalVals = globalObj.GetComponent("GlobalValues") as GlobalValues;
		ballClass = globalObj.GetComponent("Player_Ball") as Player_Ball;
		endPoint = GameObject.Find("EndPoint");
	}

	void Update()
	{
		/**
		 * Note that this is the main controller for the entire game startup
		 */
		if (!gameInitialised)
		{
			InitialiseGame();
			StartNewLevel(leveltype.newLevel);
			gameInitialised = true;	
		}
	}
	
	public void InitialiseGame()
	{
		// hide start/restart UI
		HideEndLevelScreen();
		
		// hide energy picker
		HideEnergyPicker();
		
		// draw graphs
		graphManager.GenerateGraphs();
		
	}
	
	public void StartNewLevel(leveltype newLevelType)
	{		
		// clear any leftover constraint lines
		lineManager.ClearConstraints();
		
		// reset move counter and collision
		globalVals.moveCounter = Random.Range(3,10);
		ballClass.ball.GetComponent<BallBehaviour>().insideTarget = false;
		
		// make sure things are visible
		ballClass.ball.renderer.enabled = true;
		endPoint.renderer.enabled = true;

		// position ball
		if (newLevelType == leveltype.newLevel)
		{
			// save position for 'Redo Level' button click
			ballClass.ball.transform.position = ThermoMath.RandomStart(graphManager.tpvOrigin.transform.position,ThermoMath.graphSpaceEnums.TPV_Space);
			globalVals.previousGameStartPoint = ballClass.ball.transform.position;
		}
		else
			ballClass.ball.transform.position = globalVals.previousGameStartPoint;
			
		// draw constraint lines
		lineManager.DrawConstraints();

		// position hole
		if (newLevelType == leveltype.newLevel)
		{
			// save this position for 'Redo Level' button
			// this isn't currently needed, but possibly needed for later "moving target" type game
			endPoint.transform.position = ThermoMath.RandomStart(graphManager.tpvOrigin.transform.position,ThermoMath.graphSpaceEnums.TPV_Space);
			globalVals.previousGameEndPoint = endPoint.transform.position;
		}
		else
			endPoint.transform.position = globalVals.previousGameEndPoint;
		
		// make sure ui is ready for the player
		guiManager.uiCam.enabled = false;
		guiManager.hitCam.enabled = true;
		guiManager.clubSetSelectorCam.enabled = true;
		guiManager.toggleClubVisibility(clubtypes.heat);
		
		// get the camera to where it should be
		guiManager.cam1.transform.position = new Vector3(ballClass.ball.transform.position.x+0.3f,ballClass.ball.transform.position.y+0.5f,ballClass.ball.transform.position.z);
		
		// glow initial selected so the colours are right
		globalVals.selectedConstraint = lineManager.constraints[0] as Constraint_Line;
		globalVals.selectedConstraint.Glow();	
	}
	
	public void ShowEndLevelScreen(string endMessage)
	{
		// set up message
		guiManager.endGameMessage.text = endMessage;
		
		// show next/restart/quit buttons and message GUI
		guiManager.uiCam.enabled = true;
		
		// hide energy hit region, clubs, move counter
		guiManager.hitCam.enabled = false;
		guiManager.clubSetSelectorCam.enabled = false;
		guiManager.heatclubCam.enabled = false;
		guiManager.workclubCam.enabled = false;
		
		// allow player to fly around --- maybe. This will require:
		//	- allowing the camera full freedom of movement in easytouch script
		//	- creating script to go in update somewhere to fly player with WASD
		//	- opens up the ugly probability of needing to use all of the EasyTouch movement library for mobile devices
		
	}
	
	public void HideEndLevelScreen()
	{
		// disallow free cam movement
		
		// hide next/restart/quit buttons
		guiManager.uiCam.enabled = false;
	}
	
	public void HideEnergyPicker()
	{
		// turn off energy picker cam
		guiManager.energyCam.enabled = false;
	}
}
