using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour 
{
	private const float elevation = 2.5f, rotSpeed = 3.0f, rotFadeSpeed = 4.0f, timeSinceDrop = 0.0f;
	private int attack, life;
	private bool beingHeld;
	
	void Start () 
	{
		transform.position = new Vector3(0, elevation, 0);
		beingHeld = false;
		MapUVs();

		attack = life = 3;

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
			rigidbody.AddTorque(new Vector3(100.0f, 100.0f, 100.0f));
			rigidbody.useGravity = false;
		}
		else
		{
			rigidbody.useGravity = true;
		}
	}
	
	void MapUVs()
	{
		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = null;
		if (mf != null)
			mesh = mf.mesh;
		
		if (mesh == null || mesh.uv.Length != 24) {
			Debug.Log("Script needs to be attached to built-in cube");
			return;
		}
		
		Vector2[] uvs = mesh.uv;
		
		// Front
		uvs[0]  = new Vector2(0.0f, 0.0f);
		uvs[1]  = new Vector2(0.0f, 0.0f);
		uvs[2]  = new Vector2(0.0f, 0.0f);
		uvs[3]  = new Vector2(0.0f, 0.0f);
		
		// Top
		uvs[8]  = new Vector2(0.334f, 0.0f);
		uvs[9]  = new Vector2(0.666f, 0.0f);
		uvs[4]  = new Vector2(0.334f, 0.333f);
		uvs[5]  = new Vector2(0.666f, 0.333f);
		
		// Back
		uvs[10]  = new Vector2(0.0f, 0.0f);
		uvs[11]  = new Vector2(0.0f, 0.0f);
		uvs[6]  = new Vector2(0.0f, 0.0f);
		uvs[7]  = new Vector2(0.0f, 0.0f);
		
		// Bottom
		uvs[12] = new Vector2(0.0f, 0.334f);
		uvs[14] = new Vector2(0.333f, 0.334f);
		uvs[15] = new Vector2(0.0f, 0.666f);
		uvs[13] = new Vector2(0.333f, 0.666f);                
		
		// Left
		uvs[16]  = new Vector2(0.0f, 0.0f);
		uvs[18]  = new Vector2(0.0f, 0.0f);
		uvs[19]  = new Vector2(0.0f, 0.0f);
		uvs[17]  = new Vector2(0.0f, 0.0f);    
		
		// Right        
		uvs[20]  = new Vector2(0.0f, 0.0f);
		uvs[22]  = new Vector2(0.0f, 0.0f);
		uvs[23]  = new Vector2(0.0f, 0.0f);
		uvs[21]  = new Vector2(0.0f, 0.0f);    
		
		mesh.uv = uvs;
	}
}
