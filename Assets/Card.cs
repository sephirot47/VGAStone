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

		GetComponent<NetworkView>().observed = this;
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

	bool ReceiveAttack(int attack)
	{
		life -= attack;
		if (life <= 0)
		{
			Die();
		}

		return life <= 0;
	}

	Vector3 GetCollisionCoordinates()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		LayerMask cardExcludingLayer = ~(1 << LayerMask.NameToLayer("Card Layer")); 
		if(Physics.Raycast(ray, out hit, 99999.0f, cardExcludingLayer)) return hit.point;
		return Vector3.zero;
	}
	
	void Update()
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
			float arrowElevation = 5.0f;
			if(Input.GetMouseButtonDown(0))
			{
				beingHeld = HasBeenClicked();
				if(beingHeld)
				{
					GameObject arrow = (GameObject) Instantiate(Resources.Load("Arrow"));
					arrow.name = "Arrow";
					arrow.transform.position = new Vector3(transform.position.x, arrowElevation, transform.position.z);
				}
			}
			else if(Input.GetMouseButtonUp(0) && !beingHeld)
			{
				foreach(Card c in cards)
				{
					if(c.IsAttacking())
					{
						if(HasBeenClicked()) this.ReceiveAttack(c.GetAttack());
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
					Vector3 arrowAux = new Vector3(arrow.transform.position.x, arrowElevation, arrow.transform.position.z);
					Vector3 mouseAux = new Vector3(mouseCoords.x, arrowElevation, mouseCoords.z);

					arrow.transform.rotation = Quaternion.LookRotation( new Vector3(mouseCoords.x, arrowElevation, mouseCoords.z) - arrowAux, 
					                                                    new Vector3(0,1,0) );
					arrow.transform.rotation *= Quaternion.AngleAxis( 90.0f, new Vector3(1, 0, 0) );

					GameObject tip = GameObject.Find("Tip");
					Vector3 arrowTipPos = new Vector3(tip.transform.position.x, arrowElevation, tip.transform.position.z);
					int iter = 0;
					while((arrowTipPos - mouseAux).magnitude > 0.05f && ++iter < 1000)
					{
						float mult = 0.01f;
						arrow.transform.localScale = new Vector3(arrow.transform.localScale.x, 
						                                         arrow.transform.localScale.y * (1.0f + mult * ((arrowTipPos - arrowAux).magnitude < (mouseAux - arrowAux).magnitude ? 1.0f : -1.0f)),
						                                         arrow.transform.localScale.z);
						arrowTipPos = new Vector3(tip.transform.position.x, arrowElevation, tip.transform.position.z);
					}

					//Para que se vea solo cuando esta bien colocado, si no hace flicker al crear la arrow a veces
					//esta desactivada de inicio desde unity
					GameObject.Find("ArrowMesh").GetComponent<MeshRenderer>().enabled = true; 
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

	void Die()
	{
		Destroy(gameObject);
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

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		int numberOfCards = Card.cards.Count;
		if (stream.isWriting)
		{
			stream.Serialize(ref numberOfCards);
		}
		else
		{
			stream.Serialize(ref numberOfCards);
			Debug.Log("Number of cards on the other host: " + numberOfCards);
		}
	}
	
	int GetAttack()
	{
		return attack;
	}
}
