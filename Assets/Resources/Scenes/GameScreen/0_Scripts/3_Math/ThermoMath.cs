using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ThermoPoint
{
    public double T;
	public double V;
	public double P;
	public double U;
	public double H;
	public double S;
	public ThermoMath.area area;
	public int index;
	
	public ThermoPoint(double t, double v, double p, double u, double h, double s, ThermoMath.area a, int i){
		T = t;
		V = v;
		P = p;
		U = u;
		H = h;
		S = s;
		area = a;
		index = i;
	}
	
	public override string ToString(){
		return "Temperature: " + this.T + ", Pressure: " + this.P + ", Volume: " + this.V + ", Internal Energy: " + this.U + ", Enthalpy: " + this.H + ", Entropy" + this.S;
	}
}


public class ThermoMath : MonoBehaviour
{
	
	public enum area {
		VAPOR_DOME = 0, 
		SUPER_HEATED_VAPOR = 1, 
		SUPER_COOLED_LIQUID = 2,
		SATURATED_LIQUID = 3, 
		SATURATED_VAPOR = 4, 
		UNKNOWN = 5
	};
		
	public enum pathType {
		CONSTANT_T = 0, 	//Constant Temperature
		CONSTANT_V = 1,		//Constant Volume
		CONSTANT_P = 2, 	//Constant Pressure
		CONSTANT_U = 3, 	//Constant Internal Energy
		CONSTANT_H = 4,		//Constant Enthalpy
		CONSTANT_S = 5,		//Constant Entropy
		POLY = 6,			//Polytropic 
	};
	
	public enum direction {
		POSITIVE = 0,
		NEGATIVE = 0
	};
	
	/*static GameObject TPV;
	static GameObject TUV;
	static GameObject TPU;
	static GameObject THV;
	static GameObject TSV;
	static GameObject TPS;
	*/
	
	/*Type of surface,
	 * axes X, Y, Z 
	 * --------------
	 * 0 -> T, P, V
	 * 1 -> T, U, V
	 * 2 -> T, P, U
	 * 3 -> T, H, V
	 * 4 -> T, S, V
	 * 5 -> T, P, S
	 * */
	public enum graphSpaceEnums {TPV_Space = 0, TUV_Space = 1, TPU_Space = 2, THV_Space = 3, TSV_Space = 4, TPS_Space = 5};
	
	//List of all ThermoPoints for mesh
	static ArrayList modelData;
	
	//True if we have not yet done the calculations for the Mesh
	static bool calculate = false;
	
	//Will contain an array of the unique Temperature and Volume values
	//used to create the array
	static Grid TM_Grid;
	static double[] T_Vals;	//Temperature values from the Grid
	static double[] V_Vals;	//Volume values from the Grid
	
	static int t_count;		//number of values in T_Vals
	static int v_count;		//number of values in V_Vals
	
	static Vector2[] mesh_uvs;			//UV array for Mesh
	static int[] mesh_triangles;		//Triangle array for Mesh
	
	static Mesh water_surface;			//local Mesh variable to modify referenced Meshes
	
	//As new values of T and V are provided, rho, delta, and tao
	//will be changing accordingly
	static double T = 273.16;
	static double V = .001588;
	static double rho = 0;
	static double delta = 0;
	static double tao = 0;
	
	//Used in equations but not used in this iteration yet
	//double M = 18.015268;
	//double Rm = 8.314472;
	static float celKelConverter = 273.15f;

	static double Tc = 647.096;
	static double rhoc = 322;
	static double R = .46151805;
	
	static float[] ci = new float[]{
		0,0,0,0,0,0,0,//not used 1 to 7
		1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,//1 == 8 to 22 ==> 15 vals
		2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,2f,//2 == 23 to 42 ==> 20 vals
		3f,3f,3f,3f,//43 to 46
		4f,
		6f,6f,6f,6f//48 to 51
		
	};
	
	static float[] di = new float[]{
		1f,1f,1f,	2f,2f,	3f, 4f,	1f,1f,1f,	2f,2f,	3f,	4f,4f,	5f, 7f, 9f, 10f, 11f, 13f, 15f, 1f,
		2f,2f,2f,	3f,	4f,4f,4f,	5f,	6f,6f,	7f,	9f,9f,9f,9f,9f,	10f,10f,	12f, 3f,	4f,4f,	5f, 14f, 3f,	6f,6f,6f
	};
	
	static float[] ti = new float[]{
		-.5f, .875f, 1f, .5f, .75f, .375f, 1f, 4f, 6f, 12f, 1f, 5f, 4f, 2f, 13f, 9f, 3f, 4f, 11f, 4f, 13f, 1f, 7f, 1f, 9f, 10f,10f, 
		3f, 7f, 10f,10f, 6f, 10f,10f, 1f, 2f, 3f, 4f, 8f, 6f, 9f, 8f, 16f, 22f, 23f,23f, 10f, 50f, 44f, 46f, 50f
	};
	
	static double[] ni = new double[]{
		.012533547935523,
		7.8957634722828, 
		-8.7803203303561,
		0.31802509345418,
		-0.26145533859358,
		-.0078199751687981,
		.0088089493102134,
		-0.66856572307965,
		0.20433810950965,
		-.000066212605039687,
		-0.19232721156002,
		-0.25709043003438,
		0.16074868486251,
		-.040092828925807,
		.00000039343422603254,
		-.0000075941377088144,
		.00056250979351888,
		-.000015608652257135,
		.0000000011537996422951,
		.00000036582165144204,
		-.0000000000013251180074668,
		-.00000000062639586912454,
		-0.10793600908932,
		.017611491008752,
		0.22132295167546,
		-0.40247669763528,
		0.58083399985759,
		.0049969146990806,
		-.031358700712549,
		-0.74315929710341,
		0.47807329915480,
		.020527940895948,
		-0.13636435110343,
		.014180634400617,
		.0083326504880713,
		-.029052336009585,
		.038615085574206,
		-.020393486513704,
		-.0016554050063734,
		.0019955571979541,
		.00015870308324157,
		-.000016388568342530,
		.043613615723811,
		.034994005463765,
		-.076788197844621,
		.022446277332006,
		-.000062689710414685,
		-.00000000055711118565645,
		-0.19905718354408,
		0.31777497330738,
		-0.11841182425981
		
	};
	
		
	static float[] di2 = new float[]{3f,3f,3f};
	
	static float[] ti2 = new float[]{0,1f,4f};
	
	static double[] ni2 = new double[]{
		-31.306260323435, 
		31.546140237781, 
		-2521.3154341695
	};
	
	static double[] ni2a = new double[]{	
		-.14874640856724,
		.31806110878444
	};
	
	static float[] alphai = new float[]{20f,20f,20f};
	
	static float[] betai = new float[]{150f,150f, 250f};
	
	static float[] betaia = new float[]{.3f,.3f};
	
	static float[] gammai = new float[]{1.21f,1.21f, 1.25f};
	
	static float[] epsiloni = new float[]{1f,1f,1f};
	
	static float[] ai = new float[]{3.5f,3.5f};
	
	static float[] bi = new float[]{.85f, .95f};
	
	static float[] Bi = new float[]{.2f,.2f};
	
	static float[] Ci = new float[]{28f, 32f};
	
	static float[] Di = new float[]{700f, 800f};
	
	static float[] Ai = new float[]{.32f,.32f};
	
	static float Tr;
	
	static double[] nio = new double[]{
		-8.32044648201, 
		6.6832105268,
		3.00632,
		.012436, 
		.97315,
		1.2795,
		.96956,
		.24873
	};
	
	static double[] gammaio = new double[]{
		0,
		0,
		0,
		1.28728967, 
		3.53734222,
		7.74073708,
		9.24437796,
		27.5075105
	};
	
	static double vf = 0;
	static double vg = 0;
	static double hf = 0;
	static double hg = 0;
	static double sf = 0;
	static double sg = 0;
	static double uf = 0;
	static double ug = 0;

	// Use this for initialization
	void Start ()
	{
		//Create the Grid and retrieve the Volume and Temperature Array
		//As well as there respective counts
		TM_Grid = new Grid();
		T_Vals = TM_Grid.T_Vals;
		V_Vals = TM_Grid.V_Vals;
		t_count = T_Vals.Length;
		v_count = V_Vals.Length;
		
		//Using the number of unique Temperatures and Volumes
		//set the size of the Mesh members
		mesh_triangles = new int[(t_count-1)*(v_count-1)*6];
		mesh_uvs = new Vector2[t_count*v_count];
		
		modelData = new ArrayList();
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		//If calculated Mesh already return
		if(calculate)
			return;
		
		//Temperature and Volume holders
		double t;
		double v;
		
		int n = 0; 		//Used for ThermoPoint based counting
		int m = 0; 		//Used for specifying indexes into triangle array
		
		//For every unique Temperature use each unique Volume to construct
		//the modelData ArrayList
		for(int i = 0; i < t_count; i++){
			t = T_Vals[i] + celKelConverter;	//Convert the provided Temperature to Kelvin
			
			for(int j = 0; j < v_count; j++){
				v = V_Vals[j];
				
				//Add a new ThermoPoint, for the P, U, H, and S values call the functions directly
				//-----!FindArea function!--------
				modelData.Add(new ThermoPoint(t-celKelConverter, v, ThermoMath.GetP_TV(t, v, true), ThermoMath.GetU_TV(t, v, true), 
					ThermoMath.GetH_TV(t, v, true), ThermoMath.GetS_TV(t, v, true), ThermoMath.area.UNKNOWN, n));				
				n++;
			}
		}
		
		n = modelData.Count;
		
		//Create Triangle strips using indecies into modelData/mesh_vertices, 
		//For each point excluding the last of a strip create 2 triangles
		for(int i = 0; i < n-v_count-1; i++){		//Do not create a strip for the last set of unique Volumes
			
			//If reached the end of a strip (have indexed every unique Volume for a unique Temperature)
			//Do not create a set of Triangles for that point, move to the next strip
			if((((i+1) % v_count) != 0) || i == 0 ){
				//Create each Triangle with a clockwise indexing
				mesh_triangles[m] = i;
				mesh_triangles[m+2] = i + 1;
				mesh_triangles[m+1] = i + v_count;
				m+=3;		//Increase base index into the triangle array
				
				mesh_triangles[m] = i + 1;
				mesh_triangles[m+1] = i + v_count;
				mesh_triangles[m+2] = i + v_count + 1;
				m+=3;
			}
		}
		
		
		calculate = true; 		//Have finished calculations
		
	}
	
	/*RandomStart 
	 * Description - Returns a random point for positioning the ball / particle at the
	 * start of the game.
	 * 
	 * Params: 	graphOrigin -> The origin(Translation) of the Mesh specified by graphSpace
	 * 			graphSpace -> The type of graph to create the point from
	 * 
	 * Error: Will return Vector3(-1, -1, -1) if graphSpace doesn't exist
	 * */
	public static Vector3 RandomStart(Vector3 graphOrigin, graphSpaceEnums graphSpace){
		int random_point = Random.Range(0, modelData.Count);
		
		switch(graphSpace){
		case graphSpaceEnums.TPV_Space:
			return graphOrigin + new Vector3(Mathf.Log((float)((modelData[random_point] as ThermoPoint).T)), 
				Mathf.Log((float)((modelData[random_point] as ThermoPoint).P)), 
				-Mathf.Log((float)((modelData[random_point] as ThermoPoint).V)));
		
		case graphSpaceEnums.TUV_Space:
			return graphOrigin + new Vector3(Mathf.Log((float)((modelData[random_point] as ThermoPoint).T)), 
				Mathf.Log((float)((modelData[random_point] as ThermoPoint).U)), 
				-Mathf.Log((float)((modelData[random_point] as ThermoPoint).V)));
			
		case graphSpaceEnums.TPU_Space:
			return graphOrigin + new Vector3(Mathf.Log((float)((modelData[random_point] as ThermoPoint).T)), 
				Mathf.Log((float)((modelData[random_point] as ThermoPoint).P)), 
				-Mathf.Log((float)((modelData[random_point] as ThermoPoint).U)));
		
		case graphSpaceEnums.TPS_Space:
			return graphOrigin + new Vector3(Mathf.Log((float)((modelData[random_point] as ThermoPoint).T)), 
				Mathf.Log((float)((modelData[random_point] as ThermoPoint).P)), 
				-Mathf.Log((float)((modelData[random_point] as ThermoPoint).S)));
			
		case graphSpaceEnums.THV_Space:
			return graphOrigin + new Vector3(Mathf.Log((float)((modelData[random_point] as ThermoPoint).T)), 
				Mathf.Log((float)((modelData[random_point] as ThermoPoint).H)), 
				-Mathf.Log((float)((modelData[random_point] as ThermoPoint).V)));
			
		case graphSpaceEnums.TSV_Space:
			return graphOrigin + new Vector3(Mathf.Log((float)((modelData[random_point] as ThermoPoint).T)), 
				Mathf.Log((float)((modelData[random_point] as ThermoPoint).S)), 
				-Mathf.Log((float)((modelData[random_point] as ThermoPoint).V)));
		}
		
		return new Vector3(-1, -1, -1);
	}
	
	/*GenerateGraph 
	 * Description - Returns a Mesh object with valid points for the 
	 * space specified.s
	 * 
	 * Params: 	graphSpace -> The type of graph to create (axes)
	 * */
	public static Mesh GenerateGraph(graphSpaceEnums graphSpace){
		water_surface = new Mesh();
		
		switch(graphSpace){
		case graphSpaceEnums.TPV_Space:
			water_surface.Clear();
			water_surface.vertices = TPVverts();
			break;
		
		case graphSpaceEnums.TUV_Space:
			water_surface.Clear();
			water_surface.vertices = TUVverts();
			break;
			
		case graphSpaceEnums.TPU_Space:
			water_surface.Clear();
			water_surface.vertices = TPUverts();
			break;
		
		case graphSpaceEnums.TPS_Space:
			water_surface.Clear();
			water_surface.vertices = TPSverts();
			break;
			
		case graphSpaceEnums.THV_Space:
			water_surface.Clear();
			water_surface.vertices = THVverts();
			break;
			
		case graphSpaceEnums.TSV_Space:
			water_surface.Clear();
			water_surface.vertices = TSVverts();
			break;	
		}		
		
		water_surface.triangles = mesh_triangles;
		water_surface.uv = mesh_uvs;
		water_surface.Optimize();
		water_surface.RecalculateBounds();
		water_surface.RecalculateNormals();		
		
		return water_surface;
	}
	
	/*Description - Returns a Vector3[] of the points along a requested path 
	 * to the requested target value. This will also include the current 
	 * specified location.
	 * 
	 * Params: 	pathType -> The type of path to calculate
	 * 			strength -> The energy to add / remove if negative
	 * 			currentLocation -> The points current location
	 * 			graphOrigin -> The origin(Translation) of the Mesh specified by graphSpace
	 * 			graphSpace -> The type of graph to create the point from
	 * */
	public static Vector3[] GetConstraintPath(pathType pathType, double strength, Vector3 currentLocation, Vector3 graphOrigin, graphSpaceEnums graphSpace){
		ArrayList pathInThermoPoints = new ArrayList();		//Used to easily store all of a point's atributes, then depending on graph type use specific attributes
		bool inPositive = (strength > 0);					//Will be used to switch between logic for adding or removing energy, "very nifty this is"
		Vector3[] pathPoints;								//Returned to caller
		int gridPointStart = 0;								//Index in V/T_Vals that provided point starts from
		currentLocation = currentLocation - graphOrigin;				//currentLocation must be adjusted out of unity space
		double currentT = (double)Mathf.Exp(currentLocation.x);			//Real temperature of current point
		double currentV = (double)Mathf.Exp(-currentLocation.z);			//Real volume of current point
		double currentU = ThermoMath.GetU_TV(currentT, currentV, false);		//Real energy at current point	
		double currentP = ThermoMath.GetP_TV(currentT, currentV, false);		//Real Pressure at current point
		double currentH = ThermoMath.GetH_TV(currentT, currentV, false);		//Real Enthalpy at current point
		double currentS = ThermoMath.GetS_TV(currentT, currentV, false);		//Real Entropy at current point
		double targetU = currentU + strength;							//Target Internal Energy based on the strength
		double finalV = ThermoMath.VOTX(currentT, targetU, pathType.CONSTANT_U);				//Volume at targetU
		double finalT = ThermoMath.TOVX(currentV, targetU, pathType.CONSTANT_U);				//Temperature at targetU
		
		//If no positive path exists return an empty array
		if(inPositive ? currentU > targetU : currentU < targetU)
			return new Vector3[]{};
		
		//Construct path ThermoPoints depending on constraint
		switch(pathType){
		/*CONSTANT TEMPERATURE*/
		case pathType.CONSTANT_T:
			//Find the first specific volume on the grid that is greater than the specific volume of the current point
			for(gridPointStart = 0; gridPointStart < V_Vals.Length; gridPointStart++){
				if(V_Vals[gridPointStart] > currentV)
					break;
			}
			
			//Adjust counter depending on if we are adding or removing energy
			gridPointStart = inPositive ? gridPointStart : (gridPointStart - 1);
			
			//Set the first point in the path to be the current point
			pathInThermoPoints.Add(new ThermoPoint(currentT, currentV, ThermoMath.GetP_TV(currentT, currentV, false), 
				ThermoMath.GetU_TV(currentT, currentV, false), ThermoMath.GetH_TV(currentT, currentV, false), 
				ThermoMath.GetS_TV(currentT, currentV, false), ThermoMath.area.UNKNOWN, 0)); 
			
			//Find all points in the path that line up with the grid (spacing) and are below the target energy value
			for(gridPointStart = gridPointStart + 0; (inPositive ? (gridPointStart <  V_Vals.Length) : (gridPointStart >= 0)) && 
				(inPositive ? (ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false) < targetU) : (ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false) > targetU)); 
				gridPointStart = (inPositive ? (gridPointStart+1) : (gridPointStart-1))){
				
				pathInThermoPoints.Add(new ThermoPoint(currentT, V_Vals[gridPointStart], ThermoMath.GetP_TV(currentT, V_Vals[gridPointStart], false), 
				ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false), ThermoMath.GetH_TV(currentT, V_Vals[gridPointStart], false), 
				ThermoMath.GetS_TV(currentT, V_Vals[gridPointStart], false), ThermoMath.area.UNKNOWN, pathInThermoPoints.Count)); 	
			}
			
			//Set the final point to the point with the target energy value
			if((inPositive ? (gridPointStart != 0) : (gridPointStart != -1)) && gridPointStart != V_Vals.Length)
				pathInThermoPoints.Add(new ThermoPoint(currentT, finalV, ThermoMath.GetP_TV(currentT, finalV, false), 
				targetU, ThermoMath.GetH_TV(currentT, finalV, false), 
				ThermoMath.GetS_TV(currentT, finalV, false), ThermoMath.area.UNKNOWN, pathInThermoPoints.Count));
			break;
			
		/*CONSTANT VOLUME*/	
		case pathType.CONSTANT_V:	
			//Find the first Temperature on the grid that is greater than the Temperature of the current point
			for(gridPointStart = 0; gridPointStart < T_Vals.Length; gridPointStart++){
				if(T_Vals[gridPointStart] > currentT)
					break;
			}
			
			//Adjust counter depending on if we are adding or removing energy
			gridPointStart = inPositive ? gridPointStart : (gridPointStart - 1);
			
			//Set the first point in the path to be the current point
			pathInThermoPoints.Add(new ThermoPoint(currentT, currentV, ThermoMath.GetP_TV(currentT, currentV, false), 
				ThermoMath.GetU_TV(currentT, currentV, false), ThermoMath.GetH_TV(currentT, currentV, false), 
				ThermoMath.GetS_TV(currentT, currentV, false), ThermoMath.area.UNKNOWN, 0)); 
			
			//Find all points in the path that line up with the grid (spacing) and are below the target energy value
			for(gridPointStart = gridPointStart + 0; (inPositive ? (gridPointStart <  T_Vals.Length) : (gridPointStart >= 0)) && 
				(inPositive ? (ThermoMath.GetU_TV(T_Vals[gridPointStart], currentV, false) < targetU) : (ThermoMath.GetU_TV(T_Vals[gridPointStart], currentV, false) > targetU)); 
				gridPointStart = (inPositive ? (gridPointStart+1) : (gridPointStart-1))){
				
				pathInThermoPoints.Add(new ThermoPoint(T_Vals[gridPointStart], currentV, ThermoMath.GetP_TV(T_Vals[gridPointStart], currentV, false), 
				ThermoMath.GetU_TV(T_Vals[gridPointStart], currentV, false), ThermoMath.GetH_TV(T_Vals[gridPointStart], currentV, false), 
				ThermoMath.GetS_TV(T_Vals[gridPointStart], currentV, false), ThermoMath.area.UNKNOWN, pathInThermoPoints.Count)); 
			}
			
			//If we ended on a point not equal to the target energy value set the final point to the point with the 
			//target energy value
			if((inPositive ? (gridPointStart != 0) : (gridPointStart != -1)) && gridPointStart != T_Vals.Length)
				pathInThermoPoints.Add(new ThermoPoint(finalT, currentV, ThermoMath.GetP_TV(finalT, currentV, false), 
				targetU, ThermoMath.GetH_TV(finalT, currentV, false), 
				ThermoMath.GetS_TV(finalT, currentV, false), ThermoMath.area.UNKNOWN, pathInThermoPoints.Count));
			break;
		case pathType.CONSTANT_P:
			//Find the first Volume on the grid that is greater than the Volume of the current point
			for(gridPointStart = 0; gridPointStart < V_Vals.Length; gridPointStart++){
				if(V_Vals[gridPointStart] > currentV)
					break;
			}
			
			//Adjust counter depending on if we are adding or removing energy
			gridPointStart = inPositive ? gridPointStart : (gridPointStart - 1);
			
			//Set the first point in the path to be the current point
			pathInThermoPoints.Add(new ThermoPoint(currentT, currentV, ThermoMath.GetP_TV(currentT, currentV, false), 
				ThermoMath.GetU_TV(currentT, currentV, false), ThermoMath.GetH_TV(currentT, currentV, false), 
				ThermoMath.GetS_TV(currentT, currentV, false), ThermoMath.area.UNKNOWN, 0)); 
			
			//As we will be using currentT throughout the path calculation, set it based on the first volume
			//greater than that of the current position
			currentT = ThermoMath.TOVX(V_Vals[gridPointStart], currentP, pathType.CONSTANT_P);
			
			//Find all points in the path that line up with the grid (spacing) and are below the target energy value
			for(gridPointStart = gridPointStart + 0; (inPositive ? (gridPointStart <  V_Vals.Length) : (gridPointStart >= 0)) && 
				(inPositive ? (ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false) < targetU && currentT < T_Vals[T_Vals.Length-1]) : 
				(ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false) > targetU && currentT > T_Vals[0])); 
				gridPointStart = (inPositive ? (gridPointStart+1) : (gridPointStart-1))){
				
				pathInThermoPoints.Add(new ThermoPoint(
					currentT, 
					V_Vals[gridPointStart], 
					currentP, 
					ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false), 
					ThermoMath.GetH_TV(currentT, V_Vals[gridPointStart], false), 
					ThermoMath.GetS_TV(currentT, V_Vals[gridPointStart], false), 
					ThermoMath.area.UNKNOWN, pathInThermoPoints.Count)
					); 
				
				//Update currentT unless we are on the first/last Volume of the grid
				if(inPositive ? (gridPointStart+1 != V_Vals.Length) : (gridPointStart != 0))
					currentT = ThermoMath.TOVX(V_Vals[gridPointStart+1], currentP, pathType.CONSTANT_P);
			}
			
			//Adjust final Temperature and Volume values in case we've extended beyond the surface bounds
			if(inPositive ? (finalV > V_Vals[V_Vals.Length-1]) : (finalV < V_Vals[0]))
				finalV = inPositive ? V_Vals[V_Vals.Length-1] : V_Vals[0];
			
			if(inPositive ? (finalT > T_Vals[T_Vals.Length-1]) : (finalT < T_Vals[0]))
				finalT = inPositive ? T_Vals[T_Vals.Length-1] : T_Vals[0];
			
			//If we ended on a point not equal to the target energy value set the final point to the point with the 
			//target energy value
			if(inPositive ? (gridPointStart == V_Vals.Length) : (gridPointStart == -1))
				pathInThermoPoints.Add(new ThermoPoint(finalT, finalV, currentP, 
				targetU, ThermoMath.GetH_TV(finalT, finalV, false), 
				ThermoMath.GetS_TV(finalT, finalV, false), ThermoMath.area.UNKNOWN, pathInThermoPoints.Count));
			break;
			
		//CONSTANT INTERNAL ENERGY
		 case pathType.CONSTANT_U:
			targetU = currentU;
			finalT = -1;
			finalV = -1;
			double t;
			double v;
			//double u;
			int length = 0;
			
			//Find where constant Internal Energy path will go out of bounds.
			for (gridPointStart = 0; gridPointStart < V_Vals.Length; gridPointStart++) {
                t = ThermoMath.TOVX(V_Vals[gridPointStart], targetU, pathType.CONSTANT_U);
                //t = ThermoMath.TOVU(V_Vals[gridPointStart], targetU);
				length++;
                if (t < .011) {	
                    break;		
                }				
            }
			
			//Find the first specific volume on the grid that is greater / less than the specific volume of the current point
			for(gridPointStart = 0; gridPointStart < V_Vals.Length; gridPointStart++){
				if(V_Vals[gridPointStart] > currentV)
					break;
			}
			
			//Adjust depending on whether adding / removing energy
			gridPointStart = (inPositive ? gridPointStart : gridPointStart - 1);
			
			//Set the first point in the path to be the current point
			pathInThermoPoints.Add(new ThermoPoint(currentT, currentV, ThermoMath.GetP_TV(currentT, currentV, false), 
				targetU, ThermoMath.GetH_TV(currentT, currentV, false), 
				ThermoMath.GetS_TV(currentT, currentV, false), ThermoMath.area.UNKNOWN, 0)); 
			
			//Find all points in the path that line up with the grid (spacing) and are within the surface / formula bounds
			for (int volumeIndex = gridPointStart; (inPositive ? (volumeIndex < length) : (volumeIndex >= 0)); volumeIndex = (inPositive ? (volumeIndex+1) : (volumeIndex-1))) {
                v = V_Vals[volumeIndex];
				t = ThermoMath.TOVX(v, targetU, pathType.CONSTANT_U);	//Retrieve the best Temperature for the constant Internal energy and grid aligned Volume point
				//t = ThermoMath.TOVU(v, targetU);
				//Add a new point to the path based on the retrieved temperature value
				pathInThermoPoints.Add(new ThermoPoint(t, v, ThermoMath.GetP_TV(t, v, false), 
				targetU, ThermoMath.GetH_TV(t, v, false), 
				ThermoMath.GetS_TV(t, v, false), ThermoMath.area.UNKNOWN, pathInThermoPoints.Count)); 
            }
			
			
			break;
		/*case pathType.CONSTANT_H:
			
			break;*/
		case pathType.CONSTANT_S:
			bool lowTemp, highTemp, lowVol, highVol;
			
			//Find the first Volume on the grid that is greater than the Volume of the current point
			for(gridPointStart = 0; gridPointStart < V_Vals.Length; gridPointStart++){
				if(V_Vals[gridPointStart] > currentV)
					break;
			}
			
			//Adjust counter depending on if we are adding or removing energy
			gridPointStart = inPositive ? gridPointStart : (gridPointStart - 1);
			
			//Set the first point in the path to be the current point
			pathInThermoPoints.Add(new ThermoPoint(currentT, currentV, ThermoMath.GetP_TV(currentT, currentV, false), 
				ThermoMath.GetU_TV(currentT, currentV, false), ThermoMath.GetH_TV(currentT, currentV, false), 
				ThermoMath.GetS_TV(currentT, currentV, false), ThermoMath.area.UNKNOWN, 0)); 
			
			//As we will be using currentT and currentV throughout the path calculation, set it based on the first volume
			//greater than that of the current position
			currentT = ThermoMath.TOVX(V_Vals[gridPointStart], currentS, pathType.CONSTANT_S);
			currentV = V_Vals[gridPointStart];
			
			lowTemp = currentT > T_Vals[1];
			highVol = currentV < V_Vals[V_Vals.Length-1];
			lowVol = currentV > V_Vals[1];
			highTemp = currentT < T_Vals[T_Vals.Length-1];
			
			//Catches the case where we are at a position so close the surface edge, that we will not enter the path calculation
			//loop. 
			if(inPositive ? gridPointStart+1 != V_Vals.Length && !lowTemp || !highVol : gridPointStart != 0 && !lowVol || !highTemp)
				pathInThermoPoints.Add(new ThermoPoint(
				currentT, 
				V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], 
				ThermoMath.GetP_TV(currentT, V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], false), 
				ThermoMath.GetU_TV(currentT, V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], false), 
				ThermoMath.GetH_TV(currentT, V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], false), 
				currentS, 
				//ThermoMath.GetS_TV(currentT, V_Vals[gridPointStart], false), 
				ThermoMath.area.UNKNOWN, pathInThermoPoints.Count)
				); 	
			
			//Find all points in the path that line up with the grid (spacing) and are below the target energy value
			for(gridPointStart = gridPointStart + 0; (inPositive ? (gridPointStart <  V_Vals.Length) : (gridPointStart >= 0)) && 
				(inPositive ? (ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false) < targetU && lowTemp && highVol) : 
				(ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false) > targetU && lowVol && highTemp)); 
				gridPointStart = (inPositive ? (gridPointStart+1) : (gridPointStart-1))){
				
				pathInThermoPoints.Add(new ThermoPoint(
					currentT, 
					V_Vals[gridPointStart], 
					ThermoMath.GetP_TV(currentT, V_Vals[gridPointStart], false), 
					ThermoMath.GetU_TV(currentT, V_Vals[gridPointStart], false), 
					ThermoMath.GetH_TV(currentT, V_Vals[gridPointStart], false), 
					currentS, 
					//ThermoMath.GetS_TV(currentT, V_Vals[gridPointStart], false), 
					ThermoMath.area.UNKNOWN, pathInThermoPoints.Count)
					); 
				
				//Update currentT unless we are on the first/last Volume of the grid
				if(inPositive ? (gridPointStart+1 != V_Vals.Length) : (gridPointStart != 0)){
					currentT = ThermoMath.TOVX(V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], currentS, pathType.CONSTANT_S);
					currentV = V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1];
					
					//Edge check
					lowTemp = currentT >= T_Vals[1];
					highVol = currentV < V_Vals[V_Vals.Length-1];
					lowVol = currentV >= V_Vals[1];
					highTemp = currentT < T_Vals[T_Vals.Length-1];
				}
				
				//If close to the edge calculate the last point before exiting
				if(inPositive ? !lowTemp || !highVol : !lowVol || !highTemp)
					pathInThermoPoints.Add(new ThermoPoint(
					currentT, 
					V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], 
					ThermoMath.GetP_TV(currentT, V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], false), 
					ThermoMath.GetU_TV(currentT, V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], false), 
					ThermoMath.GetH_TV(currentT, V_Vals[inPositive ? gridPointStart+1 : gridPointStart-1], false), 
					currentS, 
					//ThermoMath.GetS_TV(currentT, V_Vals[gridPointStart], false), 
					ThermoMath.area.UNKNOWN, pathInThermoPoints.Count)
					); 	
			}
			
			//Adjust final Temperature and Volume values in case we've extended beyond the surface bounds
			//if(inPositive ? (finalV > V_Vals[V_Vals.Length-1]) : (finalV < V_Vals[0]))
			//	finalV = inPositive ? V_Vals[V_Vals.Length-1] : V_Vals[0];
			
			//if(inPositive ? (finalT > T_Vals[T_Vals.Length-1]) : (finalT < T_Vals[0]))
			//	finalT = inPositive ? T_Vals[T_Vals.Length-1] : T_Vals[0];
			//finalT = ThermoMath.TOVX(V_Vals[inPositive ? gridPointStart-1 : gridPointStart+1], currentS, pathType.CONSTANT_S);
			//finalV = ThermoMath.VOTX(finalT, currentS, pathType.CONSTANT_S);
			
			//If we ended on a point not equal to the target energy value set the final point to the point with the 
			//target energy value
			//if(inPositive ? (gridPointStart != V_Vals.Length) : (gridPointStart != -1)) //Add catch here for finalT/V
			//	pathInThermoPoints.Add(new ThermoPoint(finalT, finalV, ThermoMath.GetP_TV(finalT, finalV, false), 
			//	targetU, ThermoMath.GetH_TV(finalT, finalV, false), 
			//	ThermoMath.GetS_TV(finalT, finalV, false), ThermoMath.area.UNKNOWN, pathInThermoPoints.Count));
			break;
		}
		
		pathPoints = new Vector3[pathInThermoPoints.Count];
		
		//Depending on graphSpace set the accurate order of quantities defining each point
		//offset by the graphOrigin
		switch(graphSpace){
		case graphSpaceEnums.TPV_Space:
			for(int i = 0; i < pathPoints.Length; i++){
				pathPoints[i] = graphOrigin + new Vector3(Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).T)), 
					Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).P)), 
					-Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).V)));
			}
			break;
		case graphSpaceEnums.TUV_Space:
			for(int i = 0; i < pathPoints.Length; i++){
				pathPoints[i] = graphOrigin + new Vector3(Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).T)), 
					Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).U)), 
					-Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).V)));
			}
			break;
		case graphSpaceEnums.THV_Space:
			for(int i = 0; i < pathPoints.Length; i++){
				pathPoints[i] = graphOrigin + new Vector3(Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).T)), 
					Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).H)), 
					-Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).V)));
			}
			break;
		case graphSpaceEnums.TSV_Space:
			for(int i = 0; i < pathPoints.Length; i++){
				pathPoints[i] = graphOrigin + new Vector3(Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).T)), 
					Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).S)), 
					-Mathf.Log((float)((pathInThermoPoints[i] as ThermoPoint).V)));
			}
			break;
		}
		
		return pathPoints;
	}
	
	public static double TOVX(double v, double x, pathType path) {
            return TOVX_recursive(v, x, 0.01, 1200.0, path);
        }
	
    /// <summary>
    /// Use simple binary search to find the best temperature value to go with the given specific volume and
    /// internal energy, entropy, pressure, or enthalpy values.
    /// Note: this function calls itself recursively.
    /// </summary>
    /// <param name="v">Specific volume in m^3 / kg</param>
    /// <param name="x">Specific internal energy, entropy, pressure, or enthalpy</param>
    /// <param name="minTemp">Minimum temperature (degrees C) in the range we are currently considering</param>
    /// <param name="maxTemp">Maximum temperature (degrees C) in the range we are currently considering</param>
    /// <param name="path">ThermoMath.pathType specifying the other property to use in finding V</para>
    /// <returns></returns>
    protected static double TOVX_recursive(double v, double x, double minTemp, double maxTemp, pathType path) {
        double midTemp = (minTemp + maxTemp) / 2;
        double midX = -1;
		
		//if (maxTemp - minTemp < .00001)
		if (maxTemp - minTemp < midTemp / 1000)
            return midTemp;
        
		switch(path){
		case pathType.CONSTANT_S:
			midX = ThermoMath.GetS_TV(midTemp, v, false);
			break;
		case pathType.CONSTANT_P:
			midX = ThermoMath.GetP_TV(midTemp, v, false);
			break;
		case pathType.CONSTANT_H:
			midX = ThermoMath.GetH_TV(midTemp, v, false);
			break;
		case pathType.CONSTANT_U:
			midX = ThermoMath.GetU_TV(midTemp, v, false);
			break;
		}
		
		if(midX == -1)
			throw new MissingReferenceException("Invalid 'path' parameter.");
		
		if (x < midX) {
            return TOVX_recursive(v, x, minTemp, midTemp, path);
        } else {
            return TOVX_recursive(v, x, midTemp, maxTemp, path);
        }
    }
	
	public static double VOTX(double t, double x, pathType path) {
            return VOTX_recursive(t, x, 0.001, 1000.0, path);
        }
	
    /// <summary>
    /// Use simple binary search to find the best temperature value to go with the given specific volume and
    /// internal energy, entropy, pressure, or enthalpy values.
    /// Note: this function calls itself recursively.
    /// </summary>
    /// <param name="v">Specific volume in m^3 / kg</param>
    /// <param name="x">Specific internal energy, entropy, pressure or enthalpy</param>
    /// <param name="minTemp">Minimum temperature (degrees C) in the range we are currently considering</param>
    /// <param name="maxTemp">Maximum temperature (degrees C) in the range we are currently considering</param>
    /// <param name="path">ThermoMath.pathType specifying the other property to use in finding V</para>
    /// <returns></returns>
    protected static double VOTX_recursive(double t, double x, double minVol, double maxVol, pathType path) {
        double midVol = (minVol + maxVol) / 2;
        double midX = -1;
		
		//if (maxVol - minVol < .00001)
		if (maxVol - minVol < midVol / 1000)
            return midVol;
        
		switch(path){
		case pathType.CONSTANT_S:
			midX = ThermoMath.GetS_TV(t, midVol, false);
			break;
		case pathType.CONSTANT_P:
			midX = ThermoMath.GetP_TV(t, midVol, false);
			break;
		case pathType.CONSTANT_H:
			midX = ThermoMath.GetH_TV(t, midVol, false);
			break;
		case pathType.CONSTANT_U:
			midX = ThermoMath.GetU_TV(t, midVol, false);
			break;
		}
		
		if(midX == -1)
			throw new MissingReferenceException("Invalid 'path' parameter.");
			
		if (x < midX) {
            return VOTX_recursive(t, x, minVol, midVol, path);
        } else {
            return VOTX_recursive(t, x, midVol, maxVol, path);
        }
    }
	
	/*public static double TOVU(double v, double u) {
            return TOVU_recursive(v, u, 0.01, 1000.0);
        }
	
    /// <summary>
    /// Use simple binary search to find the best temperature value to go with the given specific volume and internal energy values.
    /// Note: this function calls itself recursively.
    /// </summary>
    /// <param name="v">Specific volume in m^3 / kg</param>
    /// <param name="u">Specific internal energy in J / kg</param>
    /// <param name="minTemp">Minimum temperature (degrees C) in the range we are currently considering</param>
    /// <param name="maxTemp">Maximum temperature (degrees C) in the range we are currently considering</param>
    /// <returns></returns>
    protected static double TOVU_recursive(double v, double u, double minTemp, double maxTemp) {
        double midTemp = (minTemp + maxTemp) / 2;
        if (maxTemp - minTemp < .001) {
            return midTemp;
        }
		
		//Modification to get this method to work with ThermoMath class
        //double minInternalEnergy = ThermoCalc_John.UOTV(minTemp, v);
        //double midInternalEnergy = ThermoCalc_John.UOTV(midTemp, v);
        //double maxInternalEnergy = ThermoCalc_John.UOTV(maxTemp, v);
        
		//double minInternalEnergy = ThermoMath.GetU_TV(minTemp, v, false);
		double midInternalEnergy = ThermoMath.GetU_TV(midTemp, v, false);
		//double maxInternalEnergy = ThermoMath.GetU_TV(maxTemp, v, false);
		
		if (u < midInternalEnergy) {
            return TOVU_recursive(v, u, minTemp, midTemp);
        } else {
            return TOVU_recursive(v, u, midTemp, maxTemp);
        }
    }

    public static double VOTU(double t, double u) {
        return VOTU_recursive(t, u, 0.001, 10000.0);
    }


    /// <summary>
    /// Use binary search to find the best specific volume value to go with the given temperature and specific internal energy values.
    /// Note: this function calls itself recursively.
    /// </summary>
    /// <param name="t">Specific volume in m^3 / kg</param>
    /// <param name="u">Specific internal energy in J / kg</param>
    /// <param name="minVol">Minimum specific volume (m^3/kg) in the range we are currently considering</param>
    /// <param name="maxVol">Maximum specific volume (m^3/kg) in the range we are currently considering</param>
    /// <returns></returns>
    protected static double VOTU_recursive(double t, double u, double minVol, double maxVol) {
        double midVol = (minVol + maxVol) / 2;
        if (maxVol - minVol < .001) {
            return midVol;
        }
	
		//Modification to get this method to work with ThermoMath class
        //double minInternalEnergy = ThermoCalc_John.UOTV(t,minVol);
        //double midInternalEnergy = ThermoCalc_John.UOTV(t,midVol);
        //double maxInternalEnergy = ThermoCalc_John.UOTV(t,maxVol);
        
		//double minInternalEnergy = ThermoMath.GetU_TV(t, minVol, false);
		double midInternalEnergy = ThermoMath.GetU_TV(t, midVol, false);
		//double maxInternalEnergy = ThermoMath.GetU_TV(t, maxVol, false);
	
		if (u < midInternalEnergy) {
            return VOTU_recursive(t, u, minVol, midVol);
        } else {
            return VOTU_recursive(t, u, midVol, maxVol);
        }
    }*/
	
	/*Description - Returns the internal Energy given a temperature
	 * and volume. The densities (and delta) are calculated differently
	 * if the provided values lie in the vapor dome.
	 * 
	 *Params: 	double temperature -> the temperature at this specific point. 
	 *			double volume -> the volume at this specific point.
	 *			bool kelvin -> is the temperature provided in units of Kelvin?
	 * */
	public static double GetU_TV(double temperature, double volume, bool kelvin){
		//Adjust for Kelvin conversion
		if(kelvin){
			T = temperature;
		}else{
			T = temperature + celKelConverter;
		}
		
		bool[] VD_Vacinity = VaporDomeCheck(volume);
		
		//The Critical Point energy, well known
		if(VD_Vacinity[1]){
			return 2126.0;
		
		//If the point lies in the Vapor Dome, use the formula
		//	defined by FinalU() to calculate the internal Energy
		}else if(VD_Vacinity[0]){
			V = volume;
			tao = Tc/T;
			
			SaturatedVapor();
			vg = 1/rho;
			ug = R * T * tao * (PhioTao() + PhirTao());
			
			SaturatedLiquid();
			vf = 1/rho;
			uf = R * T * tao * (PhioTao() + PhirTao());
			
			return FinalU();
		
		//Otherwise, use the formula(s) obtained from Wagner and Prus
		// where temperature and volume are used directly
		}else{
			V = volume;
			rho = 1/V;
		 	delta = rho/rhoc;
			tao = Tc/T;
		}
		
		return R * T * tao * (PhioTao() + PhirTao());
	}
	
	/*Description - Returns the Enthalpy given a temperature and volume.
	 * The densities (and delta) are calculated differently
	 * if the provided values lie in the vapor dome.
	 * 
	 *Params: 	double temperature -> the temperature at this specific point. 
	 *			double volume -> the volume at this specific point.
	 *			bool kelvin -> is the temperature provided in units of Kelvin?
	 * */
	public static double GetH_TV(double temperature, double volume, bool kelvin){
		//Adjust for Kelvin conversion
		if(kelvin){
			T = temperature;
		}else{
			T = temperature + celKelConverter;
		}
		
		bool[] VD_Vacinity = VaporDomeCheck(volume);
		
		//The Critical Point enthalpy, well known
		if(VD_Vacinity[1]){
			return 2084.26;
		
		//If the point lies in the Vapor Dome, use the formula
		//	defined by FinalU() to calculate the Enthalpy
		}else if(VD_Vacinity[0]){
			V = volume;
			tao = Tc/T;
			
			SaturatedVapor();
			vg = 1/rho;
			hg = R * T * (1 + (tao * (PhioTao() + PhirTao())) + (delta * PhirDelta()));
			
			SaturatedLiquid();
			vf = 1/rho;
			hf = R * T * (1 + (tao * (PhioTao() + PhirTao())) + (delta * PhirDelta()));
			
			return FinalH();
		
		//Otherwise, use the formula(s) obtained from Wagner and Prus
		// where temperature and volume are used directly
		}else{
			V = volume;
			rho = 1/V;
		 	delta = rho/rhoc;
			tao = Tc/T;
		}
		
		return R * T * (1 + (tao * (PhioTao() + PhirTao())) + (delta * PhirDelta()));
	}
	
	/*Description - Returns the Entropy given a temperature and volume.
	 * The densities (and delta) are calculated differently
	 * if the provided values lie in the vapor dome.
	 * 
	 *Params: 	double temperature -> the temperature at this specific point. 
	 *			double volume -> the volume at this specific point.
	 *			bool kelvin -> is the temperature provided in units of Kelvin?
	 * */
	public static double GetS_TV(double temperature, double volume, bool kelvin){
		//Adjust for Kelvin conversion
		if(kelvin){
			T = temperature;
		}else{
			T = temperature + celKelConverter;
		}
		
		bool[] VD_Vacinity = VaporDomeCheck(volume);
		
		//The Critical Point entropy, well known
		if(VD_Vacinity[1]){
			return 4.407;
		
		//If the point lies in the Vapor Dome, use the formula
		//	defined by FinalS() to calculate the Entropy
		}else if(VD_Vacinity[0]){
			V = volume;
			tao = Tc/T;
			
			SaturatedVapor();
			vg = 1/rho;
			sg = R*(tao*(PhioTao() + PhirTao()) - Phio() - Phir());
			
			SaturatedLiquid();
			vf = 1/rho;
			sf = R*(tao*(PhioTao() + PhirTao()) - Phio() - Phir());
			
			return FinalS();
		
		//Otherwise, use the formula(s) obtained from Wagner and Prus
		// where temperature and volume are used directly
		}else{
			V = volume;
			rho = 1/V;
		 	delta = rho/rhoc;
			tao = Tc/T;
		}
		
		return R*(tao*(PhioTao() + PhirTao()) - Phio() - Phir());
	}
	
	/*Description - Returns the Pressure given a temperature and volume.
	 * The densities (and delta) are calculated differently
	 * if the provided values lie in the vapor dome.
	 * 
	 *Params: 	double temperature -> the temperature at this specific point. 
	 *			double volume -> the volume at this specific point.
	 *			bool kelvin -> is the temperature provided in units of Kelvin?
	 * */
	public static double GetP_TV(double temperature, double volume, bool kelvin){	
		//Adjust for Kelvin conversion
		if(kelvin){
			T = temperature;
		}else{
			T = temperature + celKelConverter;
		}
		
		bool[] VD_Vacinity = VaporDomeCheck(volume);
		
		//The Critical Point entropy, well known
		if(VD_Vacinity[1]){
			return .022064;
		
		//If the point lies in the Vapor Dome, adjust rho and delta
		//	using SaturatedVapor() then use the W & P equations
		}else if(VD_Vacinity[0]){
			V = volume;
			tao = Tc/T;
			
			SaturatedVapor();
		
		//Otherwise, use temperature and volume directly in the
		//	W & P equations
		}else{
			V = volume;
			rho = 1/V;
		 	delta = rho/rhoc;
			tao = Tc/T;
		}
		
		return rho*R*T * (1 + (delta*PhirDelta()));
	}
	
	/**Description - Check whether the provided volume and previously set
	 * temperature are in the Vapor Dome or at the Critical Point. Depending
	 * on location found, set a combination of booleans stored in an array
	 * and return this array.
	 * */
	static bool[] VaporDomeCheck(double v){
		//inVD - Are the provided values in the Vapor Domme?
		bool inVD = false;
		//atCP - Are the provided values at the Critical Point?
		bool atCP = false;
		
		/*****************************************
		 * May want to use to throw range validity exceptions
		 *	throw new System.ArgumentException("");
		 *
		 *	Can check whether temperature and volume are within
		 *	valid result bound of W & P equations here.
		 ***/
		
		//Check whether provided values describe a point inside the Vapor Dome
		//	or at the Critical Point
		if(T < Tc){
			//If above the Saturated Liquid line and below the Saturated Vapor
			//	line set inVD to true
			SaturatedLiquid();
			
			if(v > 1/rho)
				inVD = true;
			
			SaturatedVapor();
			
			if(inVD && v < 1/rho){
				inVD = true;
			}else{
				inVD = false;
			}
		
		//If the temperature is equal to the Critical Point temperature and
		// the we are at the Critical Point density set atCP to true
		}else if(T == Tc && (1/v) == rhoc){
			atCP = true;
		}
		
		return new bool[]{inVD, atCP};
	}
	
	/** Description - Each of the following functions; Phio, PhioTao,
	 * 	Phir, PhirDelta, Phir2Delta, and PhirTao, are the ideal and residual
	 * 	equations as well as each's respective partial derivative as 
	 *  defined in Wagner and Prus.
	 **/
	static double Phio(){
		double phi = 0;
		
		phi += Mathf.Log((float)delta) + nio[0] + (nio[1]*tao) + (nio[2]*Mathf.Log((float)tao));
		
		for(int i = 3; i < 8; i++){
			phi += nio[i]*Mathf.Log(1-Mathf.Exp(-(float)gammaio[i]*(float)tao));
		}
		
		return phi;
	}
	
	/** Description - Each of the following functions; Phio, PhioTao,
	 * 	Phir, PhirDelta, Phir2Delta, and PhirTao, are the ideal and residual
	 * 	equations as well as each's respective partial derivative as 
	 *  defined in Wagner and Prus.
	 **/
	static double PhioTao(){
		double phi = 0;
		
		phi = nio[1] + (nio[2]/tao);
		
		for(int i = 3; i < 8; i++){
			phi += nio[i]*gammaio[i]*(Mathf.Pow(1-Mathf.Exp(-(float)gammaio[i]*(float)tao), -1.0f) - 1);
		}
		
		return phi;
	}
	
	/** Description - Each of the following functions; Phio, PhioTao,
	 * 	Phir, PhirDelta, Phir2Delta, and PhirTao, are the ideal and residual
	 * 	equations as well as each's respective partial derivative as 
	 *  defined in Wagner and Prus.
	 **/
	static double Phir(){
		double term1 = 0;
		double term2 = 0;
		double term3 = 0;
		double term4 = 0;
				
		int i = 0;
		
		//Term 1 related
		for(i = 0; i < 7; i++){
			term1 += ni[i] * Mathf.Pow ((float)delta, di[i]) * Mathf.Pow ((float)tao, ti[i]) ;
		}
		
		//Term 2 related
		for(i = 7; i < 51; i++){
			term2 += ni[i] * Mathf.Exp(-Mathf.Pow((float)delta, ci[i])) * 
				Mathf.Pow ((float)delta, di[i]) * Mathf.Pow ((float)tao, ti[i]);
		}
				
		//Term 3 related
		for(i = 0; i < 3; i++){
			term3 += ni2[i] * Mathf.Pow ((float)delta, di2[i]) * Mathf.Pow((float)tao, ti2[i]) * 
				Mathf.Exp((float)((-alphai[i]*((delta - epsiloni[i]) * (delta - epsiloni[i]))) - 
					(betai[i] * ((tao - gammai[i]) * (tao - gammai[i])))) );
		}
				
		//Sub-terms of Sub-terms of Term 4 of related
		double cap_theta = 0;
		double cap_psi = 0;
		double cap_delta = 0;
		
		//Term 4 of related
		for(i = 0; i < 2; i++){
			cap_theta = (1f - tao) + Ai[i]*(Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), 1f/(2f*betaia[i])));
			cap_psi = Mathf.Exp((-Ci[i]*Mathf.Pow((float)delta - 1f, 2f)) - (Di[i]*Mathf.Pow((float)tao - 1f, 2f)));
			cap_delta = (cap_theta * cap_theta) + (Bi[i]*Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), ai[i]));
		
			term4 += ni2a[i] * Mathf.Pow((float)cap_delta, bi[i]) * delta * cap_psi;
		}
		
		return term1 + term2 + term3 + term4;
	}
	
	/** Description - Each of the following functions; Phio, PhioTao,
	 * 	Phir, PhirDelta, Phir2Delta, and PhirTao, are the ideal and residual
	 * 	equations as well as each's respective partial derivative as 
	 *  defined in Wagner and Prus.
	 **/
	static double PhirDelta(){
		double term1 = 0;
		double term2 = 0;
		double term3 = 0;
		double term4 = 0;
				
		int i = 0;
		
		//Term 1 related
		for(i = 0; i < 7; i++){
			term1 += ni[i] * di[i] * Mathf.Pow ((float)delta, (di[i] - 1f)) * Mathf.Pow ((float)tao, ti[i]) ;
		}
		
		//Term 2 related
		for(i = 7; i < 51; i++){
			term2 += ni[i] * Mathf.Exp(-Mathf.Pow((float)delta, ci[i])) * 
				(Mathf.Pow ((float)delta, di[i] - 1f) * Mathf.Pow ((float)tao, ti[i]) * 
					( di[i] - (ci[i]*Mathf.Pow((float)delta, ci[i]))) );
		}
				
		//Term 3 related
		for(i = 0; i < 3; i++){
			term3 += ni2[i] * Mathf.Pow ((float)delta, di2[i]) * Mathf.Pow((float)tao, ti2[i]) * 
				Mathf.Exp((float)((-alphai[i]*((delta - epsiloni[i]) * (delta - epsiloni[i]))) - (betai[i] * ((tao - gammai[i]) * (tao - gammai[i])))) ) * 
					((di2[i] / delta) - (2*alphai[i]*(delta - epsiloni[i])));
		}
				
		//Sub-terms within Term 4 of related
		double cap_deltabi_partial = 0;
		double cap_delta_partial = 0;
		double cap_psi_partial = 0;
		
		//Sub-terms of Sub-terms of Term 4 of related
		double cap_theta = 0;
		double cap_psi = 0;
		double cap_delta = 0;
		
		//Term 4 of related
		for(i = 0; i < 2; i++){
			cap_theta = (1f - tao) + Ai[i]*(Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), 1f/(2f*betaia[i])));
			cap_psi = Mathf.Exp((-Ci[i]*Mathf.Pow((float)delta - 1f, 2f)) - (Di[i]*Mathf.Pow((float)tao - 1f, 2f)));
			cap_delta = (cap_theta * cap_theta) + (Bi[i]*Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), ai[i]));
		
			cap_delta_partial = (delta - 1) * (Ai[i] * cap_theta * (2/betaia[i]) * 
				Mathf.Pow((float)((delta - 1)*(delta - 1)), (1/(2*betaia[i])) - 1) + 
				(2*Bi[i]*ai[i]*Mathf.Pow((float)((delta - 1)*(delta - 1)), ai[i] - 1)));
			
			cap_deltabi_partial = bi[i]* Mathf.Pow((float)cap_delta, bi[i]-1)*cap_delta_partial;
			
			cap_psi_partial = -2 * Ci[i] * (delta - 1) * cap_psi;
			
			term4 += ni2a[i] * ((Mathf.Pow((float)cap_delta, bi[i]) * (cap_psi + (delta*cap_psi_partial))) + (cap_deltabi_partial*delta*cap_psi));
		}
		
		return term1 + term2 + term3 + term4;
	}
	
	/** Description - Each of the following functions; Phio, PhioTao,
	 * 	Phir, PhirDelta, Phir2Delta, and PhirTao, are the ideal and residual
	 * 	equations as well as each's respective partial derivative as 
	 *  defined in Wagner and Prus.
	 **/
	static double PhirTao(){
		double term1 = 0;
		double term2 = 0;
		double term3 = 0;
		double term4 = 0;
				
		int i = 0;
		
		//Term 1 related
		for(i = 0; i < 7; i++){
			term1 += ni[i] * ti[i] * Mathf.Pow ((float)delta, di[i]) * Mathf.Pow ((float)tao, ti[i] - 1.0f);
		}
		
		//Term 2 related
		for(i = 7; i < 51; i++){
			term2 += ni[i] * ti[i] * Mathf.Pow((float)delta, di[i]) * 
				Mathf.Pow ((float)tao, ti[i] - 1.0f) * 
					Mathf.Exp (-Mathf.Pow ((float)delta, ci[i]));
		}
				
		//Term 3 related
		for(i = 0; i < 3; i++){
			term3 += ni2[i] * Mathf.Pow ((float)delta, di2[i]) * Mathf.Pow((float)tao, ti2[i]) * 
				Mathf.Exp((float)((-alphai[i]*((delta - epsiloni[i]) * (delta - epsiloni[i]))) - (betai[i] * ((tao - gammai[i]) * (tao - gammai[i])))) ) * 
					((ti2[i] / tao) - (2*betai[i]*(tao - gammai[i])));
		}
				
		//Sub-terms within Term 4 of related
		double cap_deltabi_partial = 0;
		double cap_psi_partial = 0;
		
		//Sub-terms of Sub-terms of Term 4 of related
		double cap_theta = 0;
		double cap_psi = 0;
		double cap_delta = 0;
		
		//Term 4 of related
		for(i = 0; i < 2; i++){
			cap_theta = (1f - tao) + Ai[i]*(Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), 1f/(2f*betaia[i])));
			cap_psi = Mathf.Exp((-Ci[i]*Mathf.Pow((float)delta - 1f, 2f)) - (Di[i]*Mathf.Pow((float)tao - 1f, 2f)));
			cap_delta = (cap_theta * cap_theta) + (Bi[i]*Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), ai[i]));
			
			cap_deltabi_partial = -2 * cap_theta * bi[i] * Mathf.Pow((float)cap_delta, bi[i]-1);
			
			cap_psi_partial = -2 * Di[i] * (tao - 1) * cap_psi;
			
			term4 += ni2a[i] * delta * ((cap_deltabi_partial * cap_psi) + (Mathf.Pow((float)cap_delta, bi[i]) * cap_psi_partial));
		}
		
		return term1 + term2 + term3 + term4;
	}
	
	/** Description - Each of the following functions; Phio, PhioTao,
	 * 	Phir, PhirDelta, Phir2Delta, and PhirTao, are the ideal and residual
	 * 	equations as well as each's respective partial derivative as 
	 *  defined in Wagner and Prus.
	 **/
	static double Phir2Delta(){
		double term1 = 0;
		double term2 = 0;
		double term3 = 0;
		double term4 = 0;
				
		int i = 0;
		
		//Term 1 related
		for(i = 0; i < 7; i++){
			term1 += ni[i] * di[i] * (di[i] - 1f) * Mathf.Pow ((float)delta, (di[i] - 2f)) * Mathf.Pow ((float)tao, ti[i]) ;
		}
		
		//Term 2 related
		for(i = 7; i < 51; i++){
			term2 += ni[i] * Mathf.Exp(-Mathf.Pow((float)delta, ci[i])) * 
				(Mathf.Pow ((float)delta, di[i] - 2f) * Mathf.Pow ((float)tao, ti[i]) * 
					( ((di[i] - (ci[i]*Mathf.Pow((float)delta, ci[i]))) * (di[i] - 1f - (ci[i]*Mathf.Pow((float)delta, ci[i])))) - 
					(ci[i]*ci[i]*Mathf.Pow((float)delta, ci[i])) ) );
		}
				
		//Term 3 related
		for(i = 0; i < 3; i++){
			term3 += ni2[i] * Mathf.Pow((float)tao, ti2[i]) * 
				Mathf.Exp((float)((-alphai[i]*((delta - epsiloni[i]) * (delta - epsiloni[i]))) - (betai[i] * ((tao - gammai[i]) * (tao - gammai[i])))) ) * 
					(((-2*alphai[i]*Mathf.Pow((float)delta, di2[i])) + (4 * alphai[i] * alphai[i] * Mathf.Pow((float)delta, di2[i]) * (delta - epsiloni[i]) * (delta - epsiloni[i])) - 
						(4 * di2[i] * alphai[i] * Mathf.Pow ((float)delta, di2[i] - 1f) * (delta - epsiloni[i])) + 
						(di2[i] * (di2[i] - 1f) * Mathf.Pow ((float)delta, di2[i] - 2f)) ));
		}
				
		//Sub-terms within Term 4 of related
		double cap_deltabi_partial = 0;
		double cap_deltabi_2partial = 0;
		double cap_delta_partial = 0;
		double cap_delta_2partial = 0;
		double cap_psi_partial = 0;
		double cap_psi_2partial = 0;
		
		//Sub-terms of Sub-terms of Term 4 of related
		double cap_theta = 0;
		double cap_psi = 0;
		double cap_delta = 0;
		
		//Term 4 of related
		for(i = 0; i < 2; i++){
			cap_theta = (1f - tao) + Ai[i]*(Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), 1f/(2f*betaia[i])));
			cap_psi = Mathf.Exp((-Ci[i]*Mathf.Pow((float)delta - 1f, 2f)) - (Di[i]*Mathf.Pow((float)tao - 1f, 2f)));
			cap_delta = (cap_theta * cap_theta) + (Bi[i]*Mathf.Pow(Mathf.Pow((float)delta - 1f, 2f), ai[i]));
		
			cap_delta_partial = (delta - 1) * (Ai[i] * cap_theta * (2/betaia[i]) * 
				Mathf.Pow((float)((delta - 1)*(delta - 1)), (1/(2*betaia[i])) - 1) + 
				(2*Bi[i]*ai[i]*Mathf.Pow((float)((delta - 1)*(delta - 1)), ai[i] - 1)));
			
			cap_deltabi_partial = bi[i]* Mathf.Pow((float)cap_delta, bi[i]-1)*cap_delta_partial;
			
			cap_delta_2partial = ((1/(delta - 1)) * cap_delta_partial) + (((delta - 1) * (delta - 1)) * 
				((4*Bi[i]*ai[i]*(ai[i] - 1)*Mathf.Pow( ((float)delta - 1f)*((float)delta - 1f), ai[i] - 2f)) + 
				(2*Ai[i]*Ai[i]*(1/Bi[i])*(1/Bi[i])*Mathf.Pow(Mathf.Pow( ((float)delta - 1f)*((float)delta - 1f), (1f/(2f*Bi[i])) - 1), 2f) ) + 
				(Ai[i]*cap_theta*(4/Bi[i])*((1/(2*Bi[i])) -1) * Mathf.Pow( ((float)delta - 1f)*((float)delta - 1f), (1f/(2f*Bi[i])) - 2))) );
			
			cap_deltabi_2partial = bi[i] * ((Mathf.Pow((float)cap_delta, bi[i] - 1f) * cap_delta_2partial) + (bi[i] - 1)*Mathf.Pow((float)cap_delta, bi[i] - 2f) * 
				cap_delta_partial * cap_delta_partial );
			
			cap_psi_partial = -2 * Ci[i] * (delta - 1) * cap_psi;
			
			cap_psi_2partial = ((2*Ci[i]*(delta - 1)*(delta - 1)) - 1) * 2*Ci[i]*cap_psi;
			
			term4 += ni2a[i] * ((Mathf.Pow((float)cap_delta, bi[i])*((2*cap_psi_partial) + (delta * cap_psi_2partial) ) ) + 
				(2*cap_deltabi_partial*(cap_psi + (delta * cap_psi_partial))) + 
				(cap_deltabi_2partial * delta * cap_psi));
		}
		
		return term1 + term2 + term3 + term4;
		
	}

	/*SaturatedVapor and SaturatedLiquid are used to set rho and delta 
	 * for use in the residual equations when in or around the vapor dome
	 * */
	static void SaturatedVapor(){
		double theta = T/Tc;
		double tao2 = 1 - theta;
		
		double[] csat = new double[]{
			-2.03150240,
			-2.68302940,
			-5.38626492,
			-17.2991605,
			-44.7586581,
			-63.9201063};
		
		rho = Mathf.Exp((float)( 
			(csat[0]*Mathf.Pow((float)tao2, 2f/6f)) +
			(csat[1]*Mathf.Pow((float)tao2, 4f/6f)) +
			(csat[2]*Mathf.Pow((float)tao2, 8f/6f)) +
			(csat[3]*Mathf.Pow((float)tao2, 18f/6f)) +
			(csat[4]*Mathf.Pow((float)tao2, 37f/6f)) +
			(csat[5]*Mathf.Pow((float)tao2, 71f/6f))))
			* rhoc;
		
		delta = rho/rhoc;
	}
	
	/*SaturatedVapor and SaturatedLiquid are used to set rho and delta 
	 * for use in the residual equations when in or around the vapor dome
	 * */
	static void SaturatedLiquid(){
		double theta = T/Tc;
		double tao2 = 1 - theta;
		
		double[] bliq = new double[]{
			1.99274064,
			1.09965342,
			-.510839303,
			-1.75493479,
			-45.5170352,
			-674694.450};
		
		rho = (1f + 
			(bliq[0]*Mathf.Pow((float)tao2, 1f/3f)) +
			(bliq[1]*Mathf.Pow((float)tao2, 2f/3f)) +
			(bliq[2]*Mathf.Pow((float)tao2, 5f/3f)) +
			(bliq[3]*Mathf.Pow((float)tao2, 16f/3f)) +
			(bliq[4]*Mathf.Pow((float)tao2, 43f/3f)) +
			(bliq[5]*Mathf.Pow((float)tao2, 110f/3f)))
			* rhoc;
		
		delta = rho/rhoc;
	}
	
	/*FinalH/S/U are used to calculate H, S, and U when the point lies
	 * inside the Vapor Dome
	 * */
	static double FinalH(){
		return ((V - vf) * (hg - hf) / (vg - vf)) + hf;
	}
	
	/*FinalH/S/U are used to calculate H, S, and U when the point lies
	 * inside the Vapor Dome
	 * */
	static double FinalS(){
		return ((V - vf) * (sg - sf) / (vg - vf)) + sf;
	}
	
	/*FinalH/S/U are used to calculate H, S, and U when the point lies
	 * inside the Vapor Dome
	 * */
	static double FinalU(){
		return ((V - vf) * (ug - uf) / (vg - vf)) + uf;
	}
	
	/*All vert functions are used to set the verteces that will be used
	 * to define the surface
	 * */
	static Vector3[] TPVverts(){
		int numVerts = modelData.Count;
		Vector3[] tvu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tvu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).P), -Mathf.Log((float)(modelData[i] as ThermoPoint).V));	
			mesh_uvs[i] = new Vector2(tvu_temp[i].y, tvu_temp[i].z);
		}
		
		return tvu_temp;
	}
	
	static Vector3[] THVverts(){
		int numVerts = modelData.Count;
		Vector3[] tvu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tvu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).H), -Mathf.Log((float)(modelData[i] as ThermoPoint).V));	
			mesh_uvs[i] = new Vector2(tvu_temp[i].y, tvu_temp[i].z);
		}
		
		return tvu_temp;
	}
	
	static Vector3[] TSVverts(){
		int numVerts = modelData.Count;
		Vector3[] tvu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tvu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).S), -Mathf.Log((float)(modelData[i] as ThermoPoint).V));	
			mesh_uvs[i] = new Vector2(tvu_temp[i].y, tvu_temp[i].z);
		}
		
		return tvu_temp;
	}
	
	static Vector3[] TPHverts(){
		int numVerts = modelData.Count;
		Vector3[] tvu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tvu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).H), -Mathf.Log((float)(modelData[i] as ThermoPoint).P));	
			mesh_uvs[i] = new Vector2(tvu_temp[i].y, tvu_temp[i].z);
		}
		
		return tvu_temp;
	}
	
	static Vector3[] TPSverts(){
		int numVerts = modelData.Count;
		Vector3[] tvu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tvu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).P), -Mathf.Log((float)(modelData[i] as ThermoPoint).S));	
			mesh_uvs[i] = new Vector2(tvu_temp[i].y, tvu_temp[i].z);
		}
		
		return tvu_temp;
	}
	
	static Vector3[] THSverts(){
		int numVerts = modelData.Count;
		Vector3[] tvu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tvu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).H), -Mathf.Log((float)(modelData[i] as ThermoPoint).S));	
			mesh_uvs[i] = new Vector2(tvu_temp[i].y, tvu_temp[i].z);
		}
		
		return tvu_temp;
	}
	
	static Vector3[] TUVverts(){
		int numVerts = modelData.Count;
		Vector3[] tvu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tvu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).U), -Mathf.Log((float)(modelData[i] as ThermoPoint).V));	
			mesh_uvs[i] = new Vector2(tvu_temp[i].y, tvu_temp[i].z);
		}
		
		return tvu_temp;
	}
	
	static Vector3[] TPUverts(){
		int numVerts = modelData.Count;
		Vector3[] tpu_temp = new Vector3[numVerts];
		mesh_uvs = new Vector2[numVerts];
		
		for(int i = 0; i < numVerts; i++){
			tpu_temp[i] = new Vector3(Mathf.Log((float)(modelData[i] as ThermoPoint).T), Mathf.Log((float)(modelData[i] as ThermoPoint).P), -Mathf.Log((float)(modelData[i] as ThermoPoint).U));	
			mesh_uvs[i] = new Vector2(tpu_temp[i].y, tpu_temp[i].z);
		}
		
		return tpu_temp;
	}
	
}

public class Grid {
    public double[] T_Vals;
    public double[] V_Vals;
    protected const int MIN_TEMPERATURE_POWER_TEN = -2;  // 0.01 degrees Celsius
    protected const int MAX_TEMPERATURE_POWER_TEN = 3;   // 1000 degrees Celsius
    protected const int MIN_SPEC_VOLUME_POWER_TEN = -3;  // 0.001 m^3 / kg
    protected const int MAX_SPEC_VOLUME_POWER_TEN = 4;   // 10000 m^3 / kg
	protected double[] divisions = { 1.0, 1.2, 1.4, 1.7, 2.0, 2.4, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };

    public Grid(double[] tVals, double[] vVals) {
        this.T_Vals = tVals = null;
        this.V_Vals = vVals = null;
    }

    public Grid() {
        if (this.T_Vals == null) {
	        this.T_Vals = genValues(MIN_TEMPERATURE_POWER_TEN, MAX_TEMPERATURE_POWER_TEN).ToArray();
	    };
	
	    if (this.V_Vals == null) {
	        List<double> v_list = genValues(MIN_SPEC_VOLUME_POWER_TEN, MAX_SPEC_VOLUME_POWER_TEN);
	        List<double> samllValueList = genValues(-5, -3);
	        for (int i = 0; i < samllValueList.Count; i++) {
	            samllValueList[i] = samllValueList[i] + v_list[0];
	        }
	        //v_list.RemoveAt(1);
			v_list.RemoveRange(1, 3);
	        v_list.InsertRange(1, samllValueList);
	        this.V_Vals = v_list.ToArray();
	    };
        
    }

    protected List<double> genValues(int minPow, int maxPow) {
        List<double> valList = new List<double>();
        double val = 0.0;
		
        for (int power = minPow; power < maxPow; power++) {
            val = Mathf.Pow(10f, (float)power);
            foreach(double div in divisions) {
                valList.Add(val * div);
            }
        }
        valList.Add( val * 10);
        return valList;
    }
}