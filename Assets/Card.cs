using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour 
{
	private const float elevation = 2.5f, rotSpeed = 2.0f, rotFadeSpeed = 4.0f, timeSinceDrop = 0.0f;
	private int attack, life;
	private bool beingHeld, onBoard;
	
	void Start () 
	{
		MapUVs();

		Physics.IgnoreLayerCollision (LayerMask.NameToLayer ("Card Layer"), LayerMask.NameToLayer ("Card Layer"));
		transform.position = new Vector3(2.5f, elevation, 2.8f);
		beingHeld = onBoard = false;

		attack = 1;
		life = 3;

		UpdateInfo();
	}

	void UpdateInfo()
	{
		Transform[] ts = transform.GetComponentsInChildren<Transform>();
		foreach (Transform t in ts) 
		{
			if (t.gameObject.name == "LifeText") 
				t.gameObject.GetComponent<TextMesh>().text = life.ToString();
			else if(t.gameObject.name == "AttackText") 
				t.gameObject.GetComponent<TextMesh>().text = attack.ToString();
		}
	}

	bool HasBeenClicked()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		LayerMask onlyCardLayer = (1 << LayerMask.NameToLayer("Card Layer")); 
		if(Physics.Raycast(ray, out hit, 99999.0f, onlyCardLayer))
		{
			if(hit.collider.gameObject.GetInstanceID() == gameObject.GetInstanceID())
			{
				return true;
			}
		}
		return false;
	}

	void ReceiveAttack(int attack)
	{
		life -= attack;
	}

	Vector3 GetCollisionCoordinates()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		LayerMask cardExcludingLayer = ~(1 << LayerMask.NameToLayer("Card Layer")); 
		if(Physics.Raycast(ray, out hit, 99999.0f, cardExcludingLayer))
		{
			return hit.point;
		}
		
		return Vector3.zero;
	}
	
	void Update () 
	{
		if(!onBoard)
		{
			if(Input.GetMouseButtonDown(0))
			{
				beingHeld = HasBeenClicked();
			}
			else if(Input.GetMouseButtonUp(0))
			{
				if(beingHeld)
				{
					Vector3 displacement = GetCollisionCoordinates() - transform.position;
					rigidbody.AddForce( new Vector3(displacement.x, 0, displacement.z) * 300.0f );
				}
				
				beingHeld = false;
			}
			
			if(beingHeld)
			{
				Vector3 colCoords = GetCollisionCoordinates();
				rigidbody.MovePosition(new Vector3(colCoords.x, elevation, colCoords.z));

				if(Input.GetMouseButton(1)) rigidbody.MoveRotation(rigidbody.rotation * Quaternion.AngleAxis(rotSpeed, new Vector3(0.0f, 1.0f, 0.0f)) );

				rigidbody.useGravity = false;
			}
			else
			{
				rigidbody.useGravity = true;
			}
		}
		else
		{

		}

		UpdateInfo();
	}

	void OnTriggerEnter(Collider col) 
	{
		Debug.Log("a");

		if(col.gameObject.name == "PlayingZone")
		{
			onBoard = true;
		}
	}

	void MapUVs()
	{
		//Para que solo se vea el top de la carta

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = null;
		if (mf != null) mesh = mf.mesh;
		
		Vector2[] uvs = mesh.uv;
		for(int i = 0; i < 24; ++i)
			if(i < 4 || i > 9) uvs[i]  = new Vector2(0.0f, 0.0f);
		mesh.uv = uvs;
	}
}
