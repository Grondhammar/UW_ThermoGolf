using UnityEngine;
using System.Collections;

public class ConstraintManager : MonoBehaviour
{
	public Material lineMaterial;
	public ArrayList constraints;
	GlobalValues globalVals;
	GraphManager graphManager;
	
	void Start()
	{
		// have to use FindGameObjectWithTag instead of just Find because Unity dies trying to Find from this class...
		constraints = new ArrayList();
		GameObject globalObj = GameObject.FindGameObjectWithTag("GlobalTag");
		
		globalVals = globalObj.GetComponent("GlobalValues") as GlobalValues;
		graphManager = globalObj.GetComponent("GraphManager") as GraphManager;
	}

	public void DrawConstraints()
	{
		string[] namearr = {"V-","V+","T-","T+","U-","U+","S-","S+"};
		double[] strengths = {-10000.0,10000.0,-10000.0,10000.0,-10000.0,10000.0,-10000.0,10000.0};
		int[] sign = {-1,1,-1,1,-1,1,-1,1};
		ThermoMath.pathType[] pathtypes = {
			ThermoMath.pathType.CONSTANT_V,
			ThermoMath.pathType.CONSTANT_V,
			ThermoMath.pathType.CONSTANT_T,
			ThermoMath.pathType.CONSTANT_T,
			ThermoMath.pathType.CONSTANT_U,
			ThermoMath.pathType.CONSTANT_U,
			ThermoMath.pathType.CONSTANT_S,
			ThermoMath.pathType.CONSTANT_S			
		};

		// get player ball for positioning
		GameObject playerBall = GameObject.Find("PlayerBall");

		Vector3[] tempvector;		// holds temporary coords while vectors are generated
		Constraint_Line templine;	// holds temp constraint_line object

		for (int n=0;n<sign.Length;n++)
		{
			tempvector = ThermoMath.GetConstraintPath(pathtypes[n],strengths[n],playerBall.transform.position,
							graphManager.tpvOrigin.transform.position,ThermoMath.graphSpaceEnums.TPV_Space);
			
			templine = playerBall.AddComponent("Constraint_Line") as Constraint_Line;
			templine.name = namearr[n];
			templine.points = tempvector;
//Debug.Log(templine.points.Length + "<-- number of path points");
			if (templine.points.Length > 0)
				templine.Draw(constraint_drawtype.FullLine);
			templine.basicLineMaterial = globalVals.basicLineMaterial;
			templine.glowingLineMaterial = globalVals.glowingLineMaterial;
			templine.transform.renderer.material = globalVals.basicLineMaterial;
			templine.pathType = pathtypes[n];
			templine.sign = sign[n];
						
			constraints.Add(templine);
		}
	}
	
	public void ClearConstraints()
	{
		// clear old paths, counting down so removeat works correctly
		Constraint_Line temp;
		for (int n=constraints.Count-1;n>-1;n--)
		{
			temp = (Constraint_Line) constraints[n];
			temp.Dispose();
			temp = null;
			constraints.RemoveAt(n);
		}
	}
}
