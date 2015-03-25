using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour 
{
	private const float elevation = 2.5f, rotSpeed = 3.0f, rotFadeSpeed = 4.0f, timeSinceDrop = 0.0f;
	private bool beingHeld;

	void Start () 
	{
		transform.position = new Vector3(0, elevation, 0);
		beingHeld = false;
	}

	bool HasBeenClicked()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Ray rayLargo = new Ray(ray.origin, ray.direction);

		if(Physics.Raycast(ray, out hit, 99999.0f))
		{
			if(hit.collider.gameObject.GetInstanceID() == gameObject.GetInstanceID())
			{
				return true;
			}
		}
		return false;
	}

	Vector3 GetCollisionCoordinates()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		LayerMask cardExcludingLayer = ~(1 << LayerMask.NameToLayer("Card Layer")); 
		Ray rayLargo = new Ray(ray.origin, ray.direction);
		
		if(Physics.Raycast(ray, out hit, 99999.0f, cardExcludingLayer))
		{
			return hit.point;
		}

		return Vector3.zero;
	}

	void Update () 
	{
		if(Input.GetMouseButtonDown(0))
		{
			beingHeld = HasBeenClicked();
		}
		else if(Input.GetMouseButtonUp(0))
		{
			beingHeld = false;

			Vector3 displacement = GetCollisionCoordinates() - transform.position;
			Debug.Log(displacement.x);
			rigidbody.AddForce( new Vector3(displacement.x, 0, displacement.z) * 300.0f );
		}

		if(beingHeld)
		{
			Vector3 colCoords = GetCollisionCoordinates();
			Vector3 displacement = colCoords - transform.position;

			rigidbody.MovePosition(new Vector3(colCoords.x, elevation, colCoords.z));
			rigidbody.useGravity = false;
			//rigidbody.rotation = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotSpeed, new Vector3(0.0f, 0.0f, 1.0f)) * transform.rotation;
			//rigidbody.rotation = Quaternion.AngleAxis(-Input.GetAxis("Mouse Y") * rotSpeed, new Vector3(1.0f, 0.0f, 0.0f)) * transform.rotation;

			//rigidbody.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * rotFadeSpeed);  
		}
		else
		{
			Vector3 colCoords = GetCollisionCoordinates();
			Vector3 displacement = colCoords - transform.position;

			//rigidbody.rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, 0), transform.rotation, Time.deltaTime); 
			rigidbody.useGravity = true;
		}
	}
}
