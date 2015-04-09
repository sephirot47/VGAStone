using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour 
{
	private List<HostData> hostList;

	private const string typeName = "VGAStone";
	private const string gameName = "Game";

	void Start () 
	{
		hostList = new List<HostData>();
	}
	
	void Update () 
	{
	}

	void StartServer()
	{
		Network.InitializeServer(2, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	void OnServerInitialized()
	{
		Debug.Log("Server Initializied");
	}

	void OnGUI()
	{
		if (!Network.isClient && !Network.isServer)
		{
			if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server")) StartServer();
			if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts")) RefreshHostList();
			
			Debug.Log(hostList.Count);
			for (int i = 0; i < hostList.Count; i++)
			{
				if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName)) JoinServer(hostList[i]);
			}
		}
	}
	
	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		Debug.Log("*************");
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			Debug.Log("asdasdasdsdads   " + MasterServer.PollHostList().Length);
			hostList.AddRange(MasterServer.PollHostList());
		}
	}

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}
	
	void OnConnectedToServer()
	{
		Debug.Log("Server Joined");
	}
}
