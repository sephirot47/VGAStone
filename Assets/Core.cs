using UnityEngine;
using System.Collections;

public class Core : MonoBehaviour 
{
	private static int initialLife = 100;

	void Start () 
	{
	}
	
	void Update () 
	{
		GameObject.Find("OpponentsNameText").GetComponent<TextMesh>().text = "Opponent";
		GameObject.Find("YourNameText").GetComponent<TextMesh>().text = "You";
		
		GameObject.Find("OpponentsLifeText").GetComponent<TextMesh>().text = "100";
		GameObject.Find("YourLifeText").GetComponent<TextMesh>().text = initialLife.ToString();
		
		GameObject.Find("OpponentsManaText").GetComponent<TextMesh>().text = "1/1";
		GameObject.Find("YourManaText").GetComponent<TextMesh>().text = "10/10";
	}
}
