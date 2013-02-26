using UnityEngine;
using System.Collections;
using Vectrosity;

public enum constraint_drawtype {FullLine,HalfLine,PrettyLine};

public enum quadrant {I,II,III,IV};

[RequireComponent(typeof(HUDText))]
// represents a single constraint line in one direction
// that is, there are two lines to handle the H constraint, +H and -H
public class Constraint_Line : MonoBehaviour
{
	public Vector3[] points;			// The set of world-space coordinates which define the line.
										// These are generated elsewhere and fed in.
	private Vector3[] renderedPoints;
	public string name;					// the constraint name, in the form of +H, -P, etc
	private float p_lineheading = 0.0f;			// the "up-the-middle" heading the line is traveling on, at least for its first segment
	public float selectionAngleWidth = 5.0f;	// used when calculating the left and right selection boundaries for this constraint
	private float camBorderAngleTolerance = 0.5f;	// used each frame to check if the cam angle is passing over a boundary
	public GUIManager guiManager;
	public GlobalValues globalVals;
	public VectorLine vectrosityLine;
	private ConstraintManager constraintManager;
	private bool glowing = false;
	private quadrant lineQuadrant;
	public Material basicLineMaterial;
	public Material glowingLineMaterial;
	public ThermoMath.pathType pathType;
	public int sign;
	private GameObject labelBase;
	private GameObject labelChild;
	private HUDText lineLabel;
	private GameObject hudChild;
	private System.DateTime lastGlowedTime;
	
	public void Awake()
	{
		guiManager = GameObject.FindGameObjectWithTag("GlobalTag").GetComponent("GUIManager") as GUIManager;
		globalVals = GameObject.FindGameObjectWithTag("GlobalTag").GetComponent("GlobalValues") as GlobalValues;
		constraintManager = GameObject.FindGameObjectWithTag("GlobalTag").GetComponent("ConstraintManager") as ConstraintManager;
	}

	public void Draw(constraint_drawtype drawtype)
	{
		Vector3 labelPosition = transform.position;

		// truncate render array
		// ****** WORK SPOT
		// ****** should also destroy previous version of this line here
		renderedPoints = new Vector3[0];
				
		// handle line drawing based on chosen line type
		if (drawtype == constraint_drawtype.FullLine)
		{
			// need all points possible for the line
			renderedPoints = points;
			vectrosityLine = RenderPoints();
		}
		else if (drawtype == constraint_drawtype.HalfLine)
		{
			//	need only half the points
			int halfSizeIndex = System.Int32.Parse(Mathf.Round(points.Length/2).ToString());
			renderedPoints = new Vector3[halfSizeIndex];

			for (int n=0;n<halfSizeIndex;n++)
				renderedPoints[n] = points[n];

			vectrosityLine = RenderPoints();
		}
		else if (drawtype == constraint_drawtype.PrettyLine)
		{
			// draw a transparent wide line with an arrow
		}

		CalculateLineHeading();

		// add label
		if (points.Length > 1)
		{			
			if ((p_lineheading >= 315 && p_lineheading <= 360) || (p_lineheading > -1 && p_lineheading <=45))
			{
				// augment x position
				labelPosition = new Vector3(transform.position.x-0.1f,transform.position.y+0.1f,transform.position.z);
			}
			else if (p_lineheading >= 225 && p_lineheading <= 315)
			{
				// decrement z position
				labelPosition = new Vector3(transform.position.x,transform.position.y+0.1f,transform.position.z-0.1f);
			}
			else if (p_lineheading >= 135 && p_lineheading <= 225)
			{
				// decrement x position
				labelPosition = new Vector3(transform.position.x+0.1f,transform.position.y+0.1f,transform.position.z);
			}
			else if (p_lineheading >= 45 && p_lineheading <= 135)
			{
				// increment z position
				labelPosition = new Vector3(transform.position.x,transform.position.y+0.1f,transform.position.z+0.1f);
			}
		}

		// generate label structure for NGUI HUDText
		if (HUDRoot.go != null)
		{
			labelBase = new GameObject("labelbase" + name);
			labelBase.transform.position = labelPosition;
			hudChild = NGUITools.AddChild(HUDRoot.go,globalVals.hudPrefab);
			lineLabel = hudChild.GetComponentInChildren<HUDText>();
			hudChild.AddComponent<UIFollowTarget>().target = labelBase.transform;
			hudChild.GetComponent<UIFollowTarget>().uiCamera = guiManager.hudCam;
			hudChild.GetComponent<UIFollowTarget>().gameCamera = guiManager.cam1;
		}
	}

	public void Glow()
	{
//Debug.Log(name + "<-- just selected");
		// FIRST check to see if this line is even selectable
		if (points.Length < 2)
			return;
		
		// if this glowed less than 2 seconds ago, don't do it again
		System.TimeSpan interval = System.DateTime.Now - lastGlowedTime;
		if (interval.TotalMilliseconds < 2000)
			return;
		
		// if we got past the gatekeepers, set lastGlowed to this constraint's name
		globalVals.lastGlowedName = name;
		lastGlowedTime = System.DateTime.Now;

		// does a visual change on this line, and sets it as the globally selected constraint
		globalVals.selectedConstraint = this;
		glowing = true;
		if (vectrosityLine != null)
			vectrosityLine.SetColor(Color.yellow);

//		Debug.Log("glowed " + name);

		// remove glow from all other constraints
		foreach (Constraint_Line temp in constraintManager.constraints)
		{
			if (temp.name != name)
				temp.Unglow();
		}

		// show constraint name for a few seconds
		if (lineLabel != null)
			lineLabel.Add(name,Color.white,0.1f);
	}
	
	public void Unglow()
	{
		// knocks the visual effect off this line
		if (vectrosityLine != null)
			vectrosityLine.SetColor(new Color(0.2f,0.5f,0.1f,1.0f));
		
		glowing = false;
	}
	
	private VectorLine RenderPoints()
	{	
		VectorLine templine = null;
		
		// draws the line using Vectrosity library
		if (renderedPoints.Length > 1)
		{
			templine = new VectorLine("Line", renderedPoints, basicLineMaterial, 2.0f, LineType.Continuous);
			templine.Draw3DAuto();
		}
		
		return templine;
	}
	
	private void Update()
	{
		// check for camera move between left heading and right heading
		//	if that happens, select this constraint
		if (!glowing)
		{
			//if (guiManager.cam1.transform.rotation.eulerAngles.y > LeftSelectionHeading() && guiManager.cam1.transform.rotation.eulerAngles.y < RightSelectionHeading())
			if (
				(
				guiManager.cam1.transform.rotation.eulerAngles.y + camBorderAngleTolerance > LeftSelectionHeading()
				&&
				guiManager.cam1.transform.rotation.eulerAngles.y - camBorderAngleTolerance < LeftSelectionHeading()
				)
					||
				(
				guiManager.cam1.transform.rotation.eulerAngles.y + camBorderAngleTolerance > RightSelectionHeading()
				&&
				guiManager.cam1.transform.rotation.eulerAngles.y - camBorderAngleTolerance < RightSelectionHeading()
				)
			)
				Glow();
		}
	}
	
	private float CalculateLineHeading()
	{
		// default to zero
		p_lineheading = 0;
		
		int farPoint = 0;
		if (points.Length > 9)
			farPoint = 9;
		else if (points.Length > 4)
			farPoint = 4;
		else if (points.Length > 1)
			farPoint = 1;
		else
			farPoint = 0;

		// it's possible for the line to have ZERO points in the case of a boundary
		// *check for array size*
		if (points.Length > 0)
		{
			// calculate the camera heading for this line's first segment, taking into account only an XZ plane
			Vector3 ballPosition = (Vector3) points[0];	// the position of the ball
			Vector3 linePosition = (Vector3) points[farPoint];	// the position of the first line segment point after the ball
			float adjacentLength = linePosition.x - ballPosition.x;
			float oppositeLength = linePosition.z - ballPosition.z;
//Debug.Log(linePosition.x + "<--X:Z-->" + linePosition.z);
			
			// determine quadrant and set
			// NOTE: the assignment when one or the other length is 0 is arbitrary, just needed those lines to be in a quadrant rather than two
			if (oppositeLength > 0 && adjacentLength > 0)
				lineQuadrant = quadrant.I;
			else if (oppositeLength < 0 && adjacentLength > 0)
				lineQuadrant = quadrant.II;
			else if (oppositeLength < 0 && adjacentLength < 0)
				lineQuadrant = quadrant.III;
			else if (oppositeLength > 0 && adjacentLength < 0)
				lineQuadrant = quadrant.IV;
			else if (oppositeLength == 0 && adjacentLength > 0)
				lineQuadrant = quadrant.II;
			else if (oppositeLength == 0 && adjacentLength < 0)
				lineQuadrant = quadrant.IV;
			else if (oppositeLength > 0 && adjacentLength == 0)
				lineQuadrant = quadrant.I;
			else if (oppositeLength < 0 && adjacentLength == 0)
				lineQuadrant = quadrant.III;

			// calculate angle --- non-zero lengths are simple
			float nonadjustedAngle = 0.0f;
//Debug.Log("div: " + (oppositeLength/adjacentLength).ToString());
//Debug.Log("arctan of 1: " + RadianToDegree((Mathf.Atan(1))).ToString());
			// convert lengths to both be positive for the angle calculation
			if (adjacentLength != 0 && oppositeLength != 0)
				nonadjustedAngle = RadiansToDegrees(Mathf.Atan(Mathf.Abs(oppositeLength)/Mathf.Abs(adjacentLength)));
			else
			{
				// zero length on one of the sides, need to set by quadrant
				// NOTE that these values don't make any sense without taking into account the adjustments just below this block
				if (lineQuadrant == quadrant.I)
					nonadjustedAngle = 90;
				else if (lineQuadrant == quadrant.IV)
					nonadjustedAngle = 0;
				else if (lineQuadrant == quadrant.III)
					nonadjustedAngle = 90;
				else
					nonadjustedAngle = 0;
			}

//Debug.Log("opp:" + oppositeLength.ToString() + ", adj:" + adjacentLength.ToString() + ", calc heading for " + name + " --> " + nonadjustedAngle.ToString());

			// determine quadrant and adjust...
			if (lineQuadrant == quadrant.I)
				p_lineheading = 90 - nonadjustedAngle;
			else if (lineQuadrant == quadrant.IV)
				p_lineheading = 270 + nonadjustedAngle;
			else if (lineQuadrant == quadrant.III)
				p_lineheading = 270 - nonadjustedAngle;
			else
				p_lineheading = 90 + nonadjustedAngle;
		}
		
		// finally, check for an overlapping line!
		foreach (Constraint_Line otherConstraint in constraintManager.constraints)
		{
			// if an existing line has precisely the same heading, offset this one by a couple of degrees
			// 	so they are both selectable with a little camera tweaking
			if (otherConstraint.LineHeading() == p_lineheading)
				p_lineheading -= 3;
		}
		
//Debug.Log("calc'd heading for " + name + ":" + p_lineheading.ToString());
		return p_lineheading;
	}

	public float LeftSelectionHeading()
	{
		if (p_lineheading >= 0)
			return p_lineheading - selectionAngleWidth;
		else
			return CalculateLineHeading() - selectionAngleWidth;
	}
	
	public float RightSelectionHeading()
	{
		if (p_lineheading >= 0)
			return p_lineheading + selectionAngleWidth;
		else
			return CalculateLineHeading() + selectionAngleWidth;
	}
	
	public float LineHeading()
	{
		return p_lineheading;	
	}
	
	private float DegreesToRadians(float angle) { return Mathf.PI * angle / 180.0f; }
	private float RadiansToDegrees(float angle) { return angle * (180.0f / Mathf.PI); }
	
	public void Dispose()
	{
		// wipe all visible traces of this constraint in preparation for nullification
		VectorLine.Destroy(ref vectrosityLine);
		
		// also destroy the HUD base
		labelBase = null;
		lineLabel = null;
	}
}
