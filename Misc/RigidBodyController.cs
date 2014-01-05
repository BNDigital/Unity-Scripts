using UnityEngine;
using System.Collections;

public class RigidBodyController : MonoBehaviour 
{
	
	[HideInInspector]
	public Vector3 forward, back, left, right, up, down;
	
	[HideInInspector]
	public bool IsGrounded
	{
		get
		{
			if(collisionState == CollisionState.bottom ||
		   collisionState == CollisionState.topBottom ||
		   collisionState == CollisionState.bottomSides ||
		   collisionState == CollisionState.topBottomSides)
				return true;
			else
				return false;
		}
	}
	
	[HideInInspector]
	public bool debugRay;
	
	[HideInInspector]
	public enum CollisionState
	{
		none,
		top,
		bottom,
		sides,
		topBottom,
		topSides,
		bottomSides,
		topBottomSides
	}
	
	[HideInInspector]
	public CollisionState collisionState;
	
	private float noneCollisionTime = 0f;
	
	public float stepOffset = 0.5f;
	
	public Vector3 normal = Vector3.zero;
	private float surfaceAngle = 0;
	
	// Use this for initialization
	void Start () 
	{
		
	}
	
	/// Update is called once per frame
	void Update () 
	{
		forward = transform.TransformDirection(Vector3.forward);
		back = transform.TransformDirection(Vector3.back);
		left = transform.TransformDirection(Vector3.left);
		right = transform.TransformDirection(Vector3.right);
		up = transform.TransformDirection(Vector3.up);
		down = transform.TransformDirection(Vector3.down);
	}
	
	public void Motion(Vector3 motion)
	{
		float maxVelocityChange = motion.magnitude;
		Vector3 targetVelocity = motion;
				
		Vector3 velocity = rigidbody.velocity;
		
		Vector3 velocityChange = (targetVelocity - velocity);
		
		velocityChange.x = Mathf.Clamp(velocityChange.x, Mathf.Min(-motion.x,motion.x),Mathf.Max(-motion.x,motion.x));
		velocityChange.y = Mathf.Clamp(velocityChange.y, Mathf.Min(-motion.y,motion.y),Mathf.Max(-motion.y,motion.y));
		velocityChange.z = Mathf.Clamp(velocityChange.z, Mathf.Min(-motion.z,motion.z),Mathf.Max(-motion.z,motion.z));
		
		rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
	}

	public Vector3 NormalizeDirection(Vector3 targetDirection)
	{
		float colliderMidHeight = ((collider as CapsuleCollider).height / 2);
		float colliderRadius = ((collider as CapsuleCollider).radius);
		
		Vector3 colliderCenter = (collider as CapsuleCollider).bounds.center;
		Vector3 colliderMidBotton = colliderCenter + down * (colliderMidHeight - colliderRadius);
		
		
		Vector3 stepForward = colliderMidBotton + (forward * (colliderRadius * 1));
		Vector3 stepBack = colliderMidBotton + (back * (colliderRadius * 1));
		
		if(debugRay)
		{
			Debug.DrawRay(colliderMidBotton, down * (colliderRadius + stepOffset), Color.green);
			Debug.DrawRay(stepForward, down * (colliderRadius + stepOffset), Color.green);
			Debug.DrawRay(stepBack, down * (colliderRadius + stepOffset), Color.green);
		}
		
		Vector3 binormal = right;
		RaycastHit hit, hitForward, hitBack;
   		if(Physics.Raycast(colliderMidBotton, down, out hit, colliderRadius + stepOffset))
   		{
   			
   			
   			normal = hit.normal;
			Vector3.OrthoNormalize(ref normal, ref binormal);
   			
			Vector3 crossPoint;
			if(Physics.Raycast(stepForward, down, out hitForward, colliderRadius + stepOffset)
			  && Physics.Raycast(stepBack, down, out hitBack, colliderRadius + stepOffset))
			{
				
				crossPoint = hitForward.point - ((hitForward.point + hitBack.point) / 2);
				normal = Vector3.Cross(crossPoint, binormal).normalized;
				if(debugRay)
				{
					Debug.DrawLine(hitForward.point, hitBack.point, Color.yellow);
					Debug.DrawRay(crossPoint + rigidbody.position,normal,Color.red);
				}
			}
				
	   		else if(Physics.Raycast(stepForward, down, out hitForward, colliderRadius + stepOffset))
			{
				
				crossPoint = hitForward.point - ((hitForward.point + hit.point) / 2);
				normal = Vector3.Cross(crossPoint, binormal).normalized;
				if(debugRay)
				{
					Debug.DrawLine(hitForward.point, hit.point, Color.yellow);
					Debug.DrawRay(crossPoint + rigidbody.position,normal,Color.red);
				}
			}
			else if(Physics.Raycast(stepBack, down, out hitBack, colliderRadius + stepOffset))
			{
				
				crossPoint = hit.point - ((hit.point + hitBack.point) / 2);
				normal = Vector3.Cross(crossPoint, binormal).normalized;
				if(debugRay)
				{
					Debug.DrawLine(hitBack.point, hit.point, Color.yellow);
					Debug.DrawRay(crossPoint + rigidbody.position,normal,Color.red);
				}
			}
			else
			{
				noneCollisionTime = 1;
				surfaceAngle = 0;
			}
			surfaceAngle = Vector3.Angle(normal, forward) - 90;
   		}
   			
   		else
   		{
   			noneCollisionTime = 1;
   			surfaceAngle = 0;
   			normal = Vector3.up;
   		}
//   		Vector3.OrthoNormalize(ref normal, ref targetDirection, ref right); //Alinha o vetor a superficie.
//		Vector3 newRight = Vector3.Cross(targetDirection, normal);
		float factor = new Vector2(targetDirection.x, targetDirection.z).magnitude;
        targetDirection = Vector3.Cross(normal, -binormal);
//   		targetDirection = Vector3.Cross(right, normal);
   		
   		
		return targetDirection * factor;   		
	}	
	
	void CheckCollisionState(Collision collision)
	{
		// A colisão é detectada em um ciclo independente do frame rate, por isso deve ser revalidada.
		up = transform.TransformDirection(Vector3.up);
		down = transform.TransformDirection(Vector3.down);
		forward = transform.TransformDirection(Vector3.forward);
		
		float colliderMidHeight = ((collider as CapsuleCollider).height / 2);
		float colliderRadius = ((collider as CapsuleCollider).radius);
		
		Vector3 colliderCenter = (collider as CapsuleCollider).bounds.center;
		Vector3 colliderMidBotton = colliderCenter + down * (colliderMidHeight - colliderRadius);
		Vector3 colliderMidTop = colliderCenter + up * (colliderMidHeight - colliderRadius);
		
		CollisionState curCollisionState = CollisionState.none;
		foreach(ContactPoint contact in collision.contacts)
		{
			float topAngle = Vector3.Angle(contact.point - colliderMidTop, up);
			float bottomAngle = Vector3.Angle(contact.point - colliderMidBotton, down);
								
			if(topAngle <= 90.0f)
			{
				curCollisionState = CollisionState.top;
				if(debugRay)
					Debug.DrawLine(colliderMidTop, contact.point, Color.magenta);
			}
			else if(bottomAngle <= 90.0f)
			{
				curCollisionState = CollisionState.bottom;
				if(debugRay)
					Debug.DrawLine(colliderMidBotton, contact.point, Color.magenta);
			}
			else
			{
				curCollisionState = CollisionState.sides;
				if(debugRay)
					Debug.DrawLine(colliderCenter, contact.point, Color.yellow);
			}
			
			switch(collisionState)
			{
				case CollisionState.none:
					switch(curCollisionState)
					{
						case CollisionState.none:
							collisionState = CollisionState.none;
							break;
						case CollisionState.top:
							collisionState = CollisionState.top;
							break;
						case CollisionState.bottom:
							collisionState = CollisionState.bottom;
							break;
						case CollisionState.sides:
							collisionState = CollisionState.sides;
							break;
						case CollisionState.topBottom:
							collisionState = CollisionState.topBottom;
							break;
						case CollisionState.bottomSides:
							collisionState = CollisionState.bottomSides;
							break;
						case CollisionState.topBottomSides:
							collisionState = CollisionState.topBottomSides;
							break;
					}
					break;
				case CollisionState.top:
					switch(curCollisionState)
					{
						case CollisionState.none:
							print("Caiu");
							collisionState = CollisionState.top;
							break;
						case CollisionState.bottom:
							collisionState = CollisionState.topBottom;
							break;
						case CollisionState.sides:
							collisionState = CollisionState.topSides;
							break;
						case CollisionState.bottomSides:
							collisionState = CollisionState.topBottomSides;
							break;
						default:
							break;
					}
					break;
				case CollisionState.bottom:
					switch(curCollisionState)
					{
						case CollisionState.none:
							print("Caiu");
							collisionState = CollisionState.bottom;
							break;
						case CollisionState.top:
							collisionState = CollisionState.topBottom;
							break;
						case CollisionState.sides:
							collisionState = CollisionState.bottomSides;
							break;
						case CollisionState.topSides:
							collisionState = CollisionState.topBottomSides;
							break;
						default:
							break;
					}
					break;
				case CollisionState.sides:
					switch(curCollisionState)
					{
						case CollisionState.none:
							print("Caiu");
							collisionState = CollisionState.sides;
							break;
						case CollisionState.top:
							collisionState = CollisionState.topSides;
							break;
						case CollisionState.bottom:
							collisionState = CollisionState.bottomSides;
							break;
						case CollisionState.topBottom:
							collisionState = CollisionState.topBottomSides;
							break;
						default:
							break;
					}
					break;
				case CollisionState.topBottom:
					switch(curCollisionState)
					{
						case CollisionState.none:
							print("Caiu");
							collisionState = CollisionState.topBottom;
							break;
						case CollisionState.sides:
							collisionState = CollisionState.topBottomSides;
							break;
						case CollisionState.bottomSides:
							collisionState = CollisionState.topBottomSides;
							break;
						default:
							break;
					}
					break;
				case CollisionState.topSides:
					switch(curCollisionState)
					{
						case CollisionState.none:
							collisionState = CollisionState.topSides;
							break;
						case CollisionState.bottom:
							collisionState = CollisionState.topBottomSides;
							break;
						case CollisionState.topBottom:
							collisionState = CollisionState.topBottomSides;
							break;
						case CollisionState.bottomSides:
							collisionState = CollisionState.topBottomSides;
							break;
					}
					break;
				case CollisionState.bottomSides:
					switch(curCollisionState)
					{
						case CollisionState.none:
							print("Caiu");
							collisionState = CollisionState.bottomSides;
							break;
						case CollisionState.top:
							collisionState = CollisionState.topBottomSides;
							break;						
						case CollisionState.topBottom:
							collisionState = CollisionState.topBottomSides;
							break;
						case CollisionState.topSides:
							collisionState = CollisionState.topBottomSides;
							break;
						default:
							break;
					}
					break;
				case CollisionState.topBottomSides:
					switch(curCollisionState)
					{
						case CollisionState.none:
							collisionState = CollisionState.topBottomSides;
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}
		}
	}
	
	void OnCollisionStay(Collision collision)
	{
		CheckCollisionState(collision);
	}
	
	void OnCollisionExit(Collision collision)
	{
		collisionState = CollisionState.none;
	}
}
