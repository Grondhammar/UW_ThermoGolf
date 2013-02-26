using UnityEngine;
using System.Collections;

public class GraphManager : MonoBehaviour
{
	//GlobalValues globalVals;
	//GUIManager guiManager;
	//public ThermoMath meshGenerator;
	
	Texture2D meshTexture;
	
	public GameObject tpvOrigin;
	public GameObject tuvOrigin;
	public GameObject tsvOrigin;
	
	Material meshMaterial;

	public void GenerateGraphs()
	{
		Mesh result = new Mesh();		

		// load up texture
		meshMaterial = Resources.Load("Materials/graphtexture") as Material;

		// generate graphs and throw into graphs arraylist
		result = tpvOrigin.GetComponent<MeshFilter>().mesh = ThermoMath.GenerateGraph( ThermoMath.graphSpaceEnums.TPV_Space );
		if(result == null)
			result = null;
		tpvOrigin.GetComponent<MeshRenderer>().renderer.material = meshMaterial;

		result = tuvOrigin.GetComponent<MeshFilter>().mesh = ThermoMath.GenerateGraph( ThermoMath.graphSpaceEnums.TUV_Space );
		if(result == null)
			result = null;
		tuvOrigin.GetComponent<MeshRenderer>().renderer.material = meshMaterial;

		result = tsvOrigin.GetComponent<MeshFilter>().mesh = ThermoMath.GenerateGraph( ThermoMath.graphSpaceEnums.TSV_Space );
		if(result == null)
			result = null;
		tsvOrigin.GetComponent<MeshRenderer>().renderer.material = meshMaterial;
	}
}
