using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]

public class DriPlayerController : MonoBehaviour 
{
	[HideInInspector]
	public Vector3 normal;
	
	public bool debugScreen;
	
	public bool debugRay;
		
	private Vector3 direction = Vector3.zero;
	private Vector3 rootDirection = Vector3.zero;
		
	private Vector3 currentPosition = Vector3.zero, previousPosition = Vector3.zero;
	private Vector3 rootVelocity = Vector3.zero;
	
	private bool stepping;
	
	private float speed;
	private float rootMagnitude = 0f;
	private float offSetJump = 0.0f;
	private float velocityPenalty = 1f;
	private float surfaceAngle;
	
	
	private Animator animator;
	private AnimatorStateInfo stateInfo;
	private RigidBodyController controller;
	
	float oldMotionSpeed;
	float curMotionSpeed;
	/// Use this for initialization
	
	void Start ()
	{		
		animator = GetComponent<Animator>();
		normal = Vector3.up;
		
		gameObject.AddComponent("RigidBodyController");
		controller = GetComponent<RigidBodyController>();
	}
		
	void SmoothMovement()
	{
		Transform cameraTransform = Camera.main.transform;
		
		Vector3 camForward = cameraTransform.TransformDirection(Vector3.forward);
		camForward.y = 0;
		camForward = camForward.normalized;
		
		float v = Input.GetAxis("Vertical");
		float h = Input.GetAxis("Horizontal");
		
		Vector3 camRight = cameraTransform.TransformDirection(Vector3.right);
		Vector3 targetDirection = v * camForward + h * camRight;

		float factor = targetDirection.magnitude;
		float targetSpeed = Mathf.Min(factor, 1.0f);
		
		if(Input.GetKey(KeyCode.LeftShift))
			speed = Mathf.Lerp(speed, 1, 0.1f);
		else
			speed = Mathf.Lerp(speed, 0, 0.1f);		
		
		animator.SetFloat("Motion", targetDirection.normalized.magnitude);
		animator.SetFloat("Speed", speed);
		animator.SetFloat("SurfaceAngle", surfaceAngle);
		direction = targetDirection.normalized;
//		targetDirection = NormalizeDirection(targetDirection);
	}

	void OnAnimatorMove()
	{
		rootDirection = animator.deltaPosition;
		rootMagnitude = animator.deltaPosition.magnitude;
//		
		rootDirection = controller.NormalizeDirection(rootDirection);
//		Vector3 newX = Vector3.Cross(normal, Vector3.forward);
//		Vector3 newZ = Vector3.Cross(newX, normal);
//		rootDirection = (newX * animator.deltaPosition.x) + (normal * animator.deltaPosition.y) + (newZ * animator.deltaPosition.z);
		rootDirection = rootDirection = animator.deltaPosition;
//		rootDirection += normal * animator.deltaPosition.y; 
		rootDirection *= 60;
		Vector3 up = Vector3.up;
		
		if(speed > 0.5f)
		{
   			direction = (Vector3.Cross(controller.normal, Vector3.Cross(direction, controller.normal)) * direction.magnitude);
//   			up = controller.normal;
		}
		if (direction != Vector3.zero)
		{
			rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, Quaternion.LookRotation(direction, up), 0.25f);
		}
		else
		{
			controller.forward = Vector3.Cross(controller.right, up);
			rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, Quaternion.LookRotation(controller.forward, up), 0.5f);
		}
		
	}
	
	void Update () 
	{
		controller.debugRay = debugRay;
		Physics.gravity = normal * -9.81f;
		
		stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		SmoothMovement();
		
		if(Input.GetKeyDown(KeyCode.LeftAlt))
		   animator.SetBool("Climb", true);
		else
			animator.SetBool("Climb", false);
		
		if(Input.GetKeyDown(KeyCode.F))
			animator.SetBool("Fuzzy", true);
		else
			animator.SetBool("Fuzzy", false);
				
		controller.Motion(rootDirection);
//		Motion(rootDirection);
	}

	
	void OnGUI()
	{
		if(debugScreen)
		{
			GUI.color = Color.green;
			GUI.Label(new Rect(10,10,300,20),"Speed: " + (rigidbody.velocity).ToString());
			GUI.Label(new Rect(10,30,300,20),"Speed: " + rigidbody.velocity.magnitude.ToString());
			GUI.Label(new Rect(10,50,300,20),"collisionState: " + controller.collisionState.ToString());
			GUI.Label(new Rect(10,70,400,20),"RootVelocity: " + animator.deltaPosition.magnitude.ToString());
			GUI.Label(new Rect(10,90,300,20),"rootDeltaPosition: " + (animator.deltaPosition * 60).ToString());
			GUI.Label(new Rect(10,110,400,20),"RootDirection: " + (rootDirection * rootMagnitude * 60).ToString());
			GUI.Label(new Rect(10,130,400,20),"IsGrounded: " + controller.IsGrounded.ToString());
		}
	}
}
