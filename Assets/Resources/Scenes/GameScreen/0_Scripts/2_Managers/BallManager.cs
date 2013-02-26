using UnityEngine;
using System.Collections;

public class BallManager : MonoBehaviour
{
	GUIManager guiManager;
	Camera ballCam;
	GlobalValues globalVals;
	public Player_Ball playerBall;
	public GameObject moveFlash;
	ConstraintManager pathManager;
	GraphManager graphManager;
	GameMetaManager meta;
	
	void Start ()
	{
		ballCam = Camera.main;
		guiManager = GameObject.Find("Globals").GetComponent("GUIManager") as GUIManager;
		globalVals = GameObject.Find("Globals").GetComponent("GlobalValues") as GlobalValues;
		playerBall = GameObject.Find("Globals").GetComponent("Player_Ball") as Player_Ball;
		pathManager = GameObject.Find("Globals").GetComponent("ConstraintManager") as ConstraintManager;
		graphManager = GameObject.Find("Globals").GetComponent("GraphManager") as GraphManager;
		meta = GameObject.Find("Globals").GetComponent("GameMetaManager") as GameMetaManager;
	}
	
	public void DoMove()
	{
		Vector3[] camPath = new Vector3[3];
	
		// set as moving for endgame testing
		playerBall.isMoving = true;
		
		// first parse the original cam heading so we can put it back on the same spot after the cam movement
		Quaternion originalCamHeading = guiManager.cam1.transform.rotation;
		
		Vector3 finalOffset = new Vector3(0.3f,0.5f,0);
		
		
/******** WORK SPOT
 * 
 * use above quaternion to create a final XYZ offset for the camera so it's looking at the ball with
 * the same orientation as it did before the hit, also taking into account the direction from the camera 
 * to the ground plane (graph)
 * 
 */
		
		// play flash
		Object tempobj = Instantiate (moveFlash, playerBall.ball.transform.position, playerBall.ball.transform.rotation);
		Destroy(tempobj,1.5f);
		
		// calculate the actual hit value taking into account the scale and accuracy relative to the energy pick
		float normalisedEnergyPick = globalVals.hitEnergy / globalVals.GetSelectedClubJoules();
//Debug.Log(globalVals.hitEnergy + "<--raw energy");
//Debug.Log(normalisedEnergyPick + "<--norm energy");
		float normalisedAccuracyPick = globalVals.hitAccuracy / 150;
//Debug.Log(globalVals.hitAccuracy + "<-- raw accuracy");
//Debug.Log(normalisedAccuracyPick + "<-- norm accuracy");
		float errorMargin = normalisedAccuracyPick - normalisedEnergyPick;
//Debug.Log(errorMargin + "<-- error margin");
		float maxAdjustment = globalVals.GetSelectedClubJoules() * normalisedEnergyPick;
//Debug.Log(maxAdjustment + "<-- max error adjust");
		float adjustedHitEnergy = (errorMargin * maxAdjustment) + globalVals.hitEnergy;
//Debug.Log(adjustedHitEnergy + "<-- adjusted energy hit");
		
		// float actual hit over ball
		if (playerBall.lineLabel != null)
			playerBall.lineLabel.Add(adjustedHitEnergy.ToString(),Color.green,1.0f);
		
		// create new array of points from the selectedConstraint.points array
		//	that only travels to the final destination point given hit power
		Vector3[] traveledPath = ThermoMath.GetConstraintPath(
			globalVals.selectedConstraint.pathType,
			adjustedHitEnergy * globalVals.selectedConstraint.sign,
			playerBall.ball.transform.position,
			graphManager.tpvOrigin.transform.position,
			ThermoMath.graphSpaceEnums.TPV_Space
		);
		
		// construct cam path
		Vector3 ballTarget = (Vector3) traveledPath[traveledPath.Length-1];
		camPath[0] = new Vector3(
						guiManager.cam1.gameObject.transform.position.x,
						guiManager.cam1.gameObject.transform.position.y+0.3f,
						guiManager.cam1.gameObject.transform.position.z);
		camPath[1] = new Vector3(
						guiManager.cam1.gameObject.transform.position.x,
						guiManager.cam1.gameObject.transform.position.y+0.7f,
						guiManager.cam1.gameObject.transform.position.z);
		camPath[2] = new Vector3(
						ballTarget.x + finalOffset.x,
						ballTarget.y + finalOffset.y,
						ballTarget.z + finalOffset.z);
		

		
/******** VERY HANDY PATH DEBUGGING BLOCK *********/
//Debug.Log("name: " + globalVals.selectedConstraint.name + ", pathtype: " + globalVals.selectedConstraint.pathType + ", hitval: " + (globalVals.hitEnergy * globalVals.hitAccuracy * globalVals.selectedConstraint.sign).ToString());
//Debug.Log("startpos: " + playerBall.ball.transform.position.ToString() + ", tpvOrigin: " + graphManager.tpvOrigin.transform.position + ", graphspace: graphSpaceEnums.TPV_Space");
//foreach (Vector3 point in traveledPath)
//		{
//Debug.Log("X:" + point.x + " Y:" + point.y + " Z:" + point.z);	
//		}
/******** END VERY HANDY DEBUGGING BLOCK **********/
		
		
		
		// start movement along with cam movement
		iTween.MoveTo(playerBall.ball, iTween.Hash("path", traveledPath, "time", 4));
		iTween.MoveTo(guiManager.cam1.gameObject,
			new Hashtable() {
				{"path",camPath},
				{"looktarget",playerBall.ball.transform},
				{"time",4.0f},
				{"easetype",iTween.EaseType.easeInOutQuad},
				{"oncomplete","EndMove"},
				{"oncompletetarget",gameObject}
			}
		);

		// re-appear UI
		if (guiManager.heatClubsOn)
			guiManager.heatclubCam.enabled = true;
		else
			guiManager.workclubCam.enabled = true;
		
		guiManager.clubSetSelectorCam.enabled = true;
		guiManager.hitCam.enabled = true;
	}
	
	public void EndMove()
	{
		// clear old constraints
		pathManager.ClearConstraints();
		
		// draw new paths
		pathManager.DrawConstraints();
		
		// unset moving
		playerBall.isMoving = false;
		
		// check for end of game
		BallBehaviour ballScript = playerBall.ball.GetComponent("BallBehaviour") as BallBehaviour;
		if (ballScript.insideTarget)
		{
			// win state reached
			meta.ShowEndLevelScreen("You won!");
		}
	}
	
	public void Update()
	{
		// cheats, for endpoint testing
		// note that up (E), down (C), and forward (W) are the only movements implemented
		// redrawing constraint lines used to be here, but proved harder than it's worth given the very precise Y coordinates involved
		if (Input.GetKey(KeyCode.E))
		{
			playerBall.ball.transform.position = new Vector3(
				playerBall.ball.transform.position.x,
				playerBall.ball.transform.position.y + 0.05f,
				playerBall.ball.transform.position.z
			);
		}
		else if (Input.GetKey(KeyCode.C))
		{
			playerBall.ball.transform.position = new Vector3(
				playerBall.ball.transform.position.x,
				playerBall.ball.transform.position.y - 0.05f,
				playerBall.ball.transform.position.z
			);
		}
		else if (Input.GetKey(KeyCode.W))
		{
			Vector3 camRelativeForward = ballCam.transform.TransformDirection(Vector3.forward);
			playerBall.ball.transform.position = new Vector3(
				playerBall.ball.transform.position.x + (camRelativeForward.x * 0.1f),
				playerBall.ball.transform.position.y + (camRelativeForward.y * 0.1f),
				playerBall.ball.transform.position.z + (camRelativeForward.z * 0.1f)
			);
		}
	}
}
