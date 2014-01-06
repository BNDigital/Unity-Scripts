using System.Linq.Expressions;
using UnityEngine;
using System.Collections;

//Comment commited via BNDigital!!!
///[AddComponentMenu("Third Person Camera/Mouse Orbit")]
public class CameraOrbit : MonoBehaviour
{
	public Transform target;
	public GameObject gameObject;
	
	public bool surfaceAlignment;
	public bool debugScreen;
	public float distance = 3.0f;
	public float minDistance = 1;
	public float maxDistance = 4;
	public float xSpeed = 5;
	public float ySpeed = 5;
	public float maxYAngle = 60;
	public float waitToFollow = 3;
	
	public Vector3 targetOffset = Vector3.zero;
	public LayerMask layerMask = 0;
	
	public float closerRadius = 0.2f;
	public float closerSnapLag = 0.2f;
	
	[HideInInspector]
	public float x = 0, y = 0;
	
	private DriPlayerController playerController;
	private float currentDistance = 10;
	private float lastCameraMovement = 0;
	private float distanceVelocity = 0;
	private Vector3 targetOffset_ = Vector3.zero;
	private Vector3 targetX, targetY, targetZ;
	private Vector3 direction;
	private Vector3 lastDirection;

	void Start ()
	{
		playerController = gameObject.GetComponent<DriPlayerController>();
		lastDirection = target.forward;
	}
	
	void OnDrawGizmos()
	{
		if(debugScreen)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(target.position + targetOffset_, 0.2f);
		}
		
	}
	
	void Update()
	{
	 	
	}
	
	void LateUpdate()
	{
	   if (target) 
	    {
	   		targetX = target.transform.right * targetOffset.x;
		 	targetY = target.transform.up * targetOffset.y;
		 	targetZ = target.transform.forward * targetOffset.z;
		 		
		 	targetOffset_ = targetX + targetY + targetZ; 
		 	Vector3 up;
		 	if(surfaceAlignment)
		 		up = playerController.transform.up;
		 	else
		 		up = Vector3.up;
	    	Vector3 forward = (target.position + targetOffset_) - transform.position;
	    	forward = forward.normalized;
	    	Vector3 right = Vector3.Cross(forward, up);
	    	
	    	x = Input.GetAxis("Mouse X") * xSpeed * 0.005f;
			y = Input.GetAxis("Mouse Y") * ySpeed * 0.005f;
			distance += Input.GetAxis("Mouse ScrollWheel");
			distance = Mathf.Clamp(distance, minDistance, maxDistance);
			
			direction = forward + (up * y) + (right * -x);
			direction = direction.normalized;
			
			lastCameraMovement += Time.deltaTime;
			
			Vector3 camUp = Vector3.Cross(direction, transform.right);
			float upAngle = Vector3.Angle(camUp, target.up);
						
			if((x + y == 0 || upAngle > maxYAngle))
//			if(x + y == 0)
				direction = lastDirection;
//			else if(x + y != 0)
//				lastCameraMovement = 0;
			
			Quaternion rotation = Quaternion.LookRotation(direction, up);
			Vector3 position = target.position + targetOffset_;
			
			float targetDistance = AdjustSight(position, -direction);
			currentDistance = Mathf.SmoothDamp(distance, currentDistance, ref distanceVelocity, 
			                                   closerSnapLag * 0.3f);
			currentDistance = Mathf.Clamp(distance, 1, 4);
			transform.position = position + (-direction * targetDistance);
			transform.rotation = rotation;
 			
//			float forwardAngle = Vector3.Angle(transform.forward, target.forward);
//			if( forwardAngle < 30)
//				targetOffset.x = ((30 - forwardAngle) / 30) * ((maxDistance - targetDistance) * 0.2f);
//			else
//				targetOffset.x = 0;
			targetOffset.x = (maxDistance - targetDistance) * 0.2f;
			lastDirection = direction;
		 }
    		
    	
	}
	
	float AdjustSight(Vector3 position, Vector3 direction)
	{
		RaycastHit hit;
		if(Physics.Raycast(position, direction, out hit, distance, layerMask))
		{
			return hit.distance - closerRadius;
		}
		else
			return distance;
	}
	
	void OnGUI()
	{
		if(debugScreen)
		{
			GUI.Box(new Rect(5, 5, 200, 200), "");
			GUI.Box(new Rect(5, 5, 200, 200), "");
			GUI.Box(new Rect(5, 5, 200, 200), "");
			
			GUI.color = Color.green;
			GUI.Label(new Rect(10,130,400,20),"CameraDirection: " + direction.ToString());
			GUI.Label(new Rect(10,150,400,20),"X: " + x.ToString() + ", Y: " + y.ToString());
			GUI.Label(new Rect(10,170,400,20),"lastCameraMovemnt: " + lastCameraMovement.ToString());
		}
	}
}
