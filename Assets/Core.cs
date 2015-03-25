using UnityEngine;
using System.Collections;

public class Core : MonoBehaviour 
{
	void Start () 
	{
		GameObject.Find("OpponentsNameText").GetComponent<TextMesh>().text = "Opponent";
		GameObject.Find("YourNameText").GetComponent<TextMesh>().text = "You";
	}
	
	void Update () 
	{
	
	}
}
