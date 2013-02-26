using UnityEngine;
using System.Collections;

public class EasyTouch_FreeCam : MonoBehaviour
{
	/*** ADDED w/orbit ***/
    public Transform target;
    private float distance = 1.0f;

    private float xSpeed = 250.0f;
    private float ySpeed = 120.0f;

    private float yMinLimit = -20;
    private float yMaxLimit = 80;

    private float x = 0.0f;
    private float y = 0.0f;
	
	private float min_distance = 0.5f;
	private float max_distance = 7.0f;
	
	Camera cam;
	/*** END ADDED ***/
	
	private float rotationX;
	private float rotationY;
	private bool firstRun = true;
	
	void Start()
	{
		cam = Camera.mainCamera;

		/*** ADDED w/orbit ***/
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

        // Make the rigid body not change rotation
        //if (rigidbody) 
        //	rigidbody.freezeRotation = true;	

		/*** END ADDED ***/
	}
	
	void Update()
	{
		// handle zoom here
		if (Input.GetAxis("Mouse ScrollWheel") != 0 || firstRun)
		{
			// minus rather than plus below to flip the wheel action
			distance -= Input.GetAxis("Mouse ScrollWheel");
			
			// lock move if too close or too far
			if (distance < min_distance)
				distance = min_distance;
			else if (distance > max_distance)
				distance = max_distance;
			
			cam.transform.position = (Quaternion.Euler(y, x, 0)) * new Vector3(0.0f, 0.0f, -distance) + target.position;
//Debug.Log(distance.ToString());
			if (firstRun)
				firstRun = false;
		}
	}
	
	// Subscribe to events
	void OnEnable()
	{
		EasyTouch.On_TouchDown += On_TouchDown;
		EasyTouch.On_Swipe += On_Swipe;
	}
	
	void OnDisable()
	{
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent()
	{
		EasyTouch.On_TouchDown -= On_TouchDown;
		EasyTouch.On_Swipe -= On_Swipe;	
	}
		
	void On_TouchDown(Gesture gesture)
	{
		if (gesture.touchCount==2){
			cam.transform.Translate(new  Vector3(0,0,1f) * Time.deltaTime);
		}
		
		 if (gesture.touchCount==3){
			cam.transform.Translate( new Vector3(0,0,-1f) * Time.deltaTime);
		}	
	}
	
	
	void On_Swipe( Gesture gesture)
	{
		/*** ADDED w/orbit ***/
		if (target)
		{
			x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
			y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
			
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			
			cam.transform.rotation = Quaternion.Euler(y, x, 0);
			cam.transform.position = (Quaternion.Euler(y, x, 0)) * new Vector3(0.0f, 0.0f, -distance) + target.position;
		}
		
		/*** REMOVED w/orbit
		rotationX += gesture.deltaPosition.x;
		rotationY += gesture.deltaPosition.y;

		cam.transform.localRotation = Quaternion.AngleAxis (rotationX, Vector3.up); 

		cam.transform.localRotation *= Quaternion.AngleAxis (rotationY, Vector3.left);
		*** END REMOVED ***/
	}
	
	/*** ADDED w/orbit ***/
	float ClampAngle(float angle, float min, float max) 
    {
		if (angle < -360)
        	angle += 360;
		
        if (angle > 360)
			angle -= 360;
		
		return Mathf.Clamp(angle, min, max);
	}
	/*** END ADDED ***/
}
