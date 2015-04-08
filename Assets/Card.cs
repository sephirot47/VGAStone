using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour 
{
	private const float elevation = 2.5f, rotSpeed = 2.0f, rotFadeSpeed = 4.0f, timeSinceDrop = 0.0f;
	private int attack, life;
	private bool beingHeld, onBoard;
	private Vector3 originalScale;

	public static List<Card> cards = new List<Card>();
	
	void Start () 
	{
		MapUVs();
		Physics.IgnoreLayerCollision (LayerMask.NameToLayer ("Card Layer"), LayerMask.NameToLayer ("Card Layer"));
		transform.position = new Vector3(2.5f, elevation, 2.8f);
		beingHeld = onBoard = false;

		cards.Add(this);

		attack = 1;
		life = 3;

		transform.localScale *= 1.4f;
		originalScale = transform.localScale;

		UpdateInfo();
	}
	
	bool IsAttacking()
	{
		return beingHeld && onBoard;
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
		if(Physics.Raycast(ray, out hit, 99999.0f, cardExcludingLayer)) return hit.point;
		return Vector3.zero;
	}
	
	void Update () 
	{
		if(!onBoard)
		{
			if(Input.GetMouseButtonDown(0))
			{
				beingHeld = HasBeenClicked();
				if(beingHeld)
				{
					transform.localScale *= 1.5f;
				}
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
		}
		else //ON BOARD
		{
			if(Input.GetMouseButtonDown(0))
			{
				beingHeld = HasBeenClicked();
				if(beingHeld)
				{
					GameObject arrow = (GameObject) Instantiate(Resources.Load("Arrow"));
					arrow.transform.position = transform.position;
					arrow.name = "Arrow";
				}
			}
			else if(Input.GetMouseButtonUp(0) && !beingHeld)
			{
				foreach(Card c in cards)
				{
					if(c.IsAttacking())
					{
						this.ReceiveAttack(c.GetAttack());
						break;
					}
				}
			}
			else if(Input.GetMouseButtonUp(0) && beingHeld)
			{
				GameObject.Destroy(GameObject.Find("Arrow").gameObject);
			}

			if(beingHeld)
			{
				//move arrow
				GameObject arrow = GameObject.Find("Arrow");
				if(arrow != null)
				{
					Vector3 mouseCoords = GetCollisionCoordinates();
					arrow.transform.rotation = Quaternion.LookRotation( new Vector3(mouseCoords.x, 0.0f, mouseCoords.z), new Vector3(0,1,0) );
					arrow.transform.rotation *= Quaternion.AngleAxis( 90.0f, new Vector3(1, 0, 0) );
					
					Vector3 arrowAux = new Vector3(arrow.transform.position.x, 0.0f, arrow.transform.position.z);
					Vector3 mouseAux = new Vector3(mouseCoords.x, 0.0f, mouseCoords.z);

					Debug.DrawLine(mouseAux, mouseAux * 1.01f, Color.green, 9999.0f, false);

					GameObject tip = GameObject.Find("Tip");
					Vector3 arrowTipPos = new Vector3(tip.transform.position.x, 0.0f, tip.transform.position.z);
					int iter = 0;
					Debug.DrawLine(arrowTipPos, arrowTipPos * 1.01f, Color.red, 9999.0f, false);
					/*
					while((arrowTipPos - mouseAux).magnitude > 0.2f && ++iter < 50)
					{
						arrow.transform.localScale = new Vector3(arrow.transform.localScale.x, 
						                                         arrow.transform.localScale.y * 0.9f,
						                                         arrow.transform.localScale.z);
						arrowTipPos = new Vector3(tip.transform.position.x, 0.0f, tip.transform.position.z);

						Debug.Log ((arrowTipPos - mouseAux).magnitude);
						Debug.DrawLine(arrowTipPos, arrowTipPos * 1.01f, Color.red, 9999.0f, false);
					}*/
				}
			}
		}
		
		if(!beingHeld)
		{
			transform.localScale = Vector3.Lerp(transform.localScale, originalScale / 1.5f, Time.deltaTime * 5.0f); 
			rigidbody.useGravity = true;
		}

		UpdateInfo();
	}

	void OnTriggerEnter(Collider col) 
	{
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

	int GetAttack()
	{
		return attack;
	}
}
