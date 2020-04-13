using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkManager
{

    public string ROOM = "Room";

    public List<GameObject> playerPrefabList = new List<GameObject>();
    public int index = 0;
    protected bool isHost;
    protected long networkId;
    protected long nodeId;

    List<string> ignoredMatchList;
    string lastMatch;

    public void StartMultiplayer()
    {
        StopClient();

        if (ignoredMatchList == null)
            ignoredMatchList = new List<string>();

        if(lastMatch != "" && ignoredMatchList.IndexOf(lastMatch) == -1)
        {
            ignoredMatchList.Add(lastMatch);
        }

        StartMatchMaker();
        matchMaker.ListMatches(0, 10, ROOM, true, 0, 0, OnMatchList);
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        if (matchMaker == null)
            return;

        if (success && matches != null && matches.Count > 0)
        {
            for(int i = 0; i < matches.Count;i++)
            {
                Debug.LogError("name " + ignoredMatchList.IndexOf(matches[i].name) + "  " + matches[i].name + "count " + ignoredMatchList.Count);
                if(matches[i].name.StartsWith(ROOM) && matches[i].currentSize > 0 && ignoredMatchList.IndexOf(matches[i].name) == -1)
                {
                    isHost = false;
                    lastMatch = matches[i].name;
                    matchMaker.JoinMatch(matches[i].networkId, "", "", "", 0, 0, OnMatchJoined);
                    return;
                }
            }
        }

        isHost = true;
        useWebSockets = true;
        lastMatch = ROOM + "_" + Random.Range(0, 1000000);
        matchMaker.CreateMatch(lastMatch, 4, true, "", "", "", 0, 0, OnMatchCreate);
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {

        if (success)
        {
            base.OnMatchCreate(success, extendedInfo, matchInfo);
            Debug.Log("Create match succeeded");
            matchMaker.JoinMatch(matchInfo.networkId, "", "", "", 0, 0, OnMatchJoined);
        }
        else
        {
            Debug.LogError("Create match failed");
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        networkId = (long)matchInfo.networkId;
        nodeId = (long)matchInfo.nodeId;
    }
    
    //// Client
    public override void OnClientConnect(NetworkConnection conn)
    {
        lastMatch = "";
        if (clientLoadedScene)
        {
            ClientScene.AddPlayer(conn, 0, new IntegerMessage(index));
            
            //foreach (var item in  ClientScene.localPlayers)
            //{
            //    item.unetView.GetComponent<CameraController>().number = ClientScene.localPlayers.Count; 
            //}
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        SceneManager.LoadScene("ChoosePlayer");
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        //base.OnClientSceneChanged(conn);
        //    ClientScene.AddPlayer(conn, 0, new IntegerMessage(index));
    }

    // Server
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {

        // Variable to store the identifier
        int id = 0;

        // Read client message and receive identifier
        if (extraMessageReader != null)
        {
            var i = extraMessageReader.ReadMessage<IntegerMessage>();
            id = i.value;
        }

        GameObject _playerPrefab = playerPrefabList[id];

        // Create player object with prefab
        GameObject player = (GameObject)Instantiate(_playerPrefab, NetworkManager.singleton.GetStartPosition().position, Quaternion.identity);
        player.name = _playerPrefab.name + "_" + conn.connectionId;
        player.GetComponent<Player>().playerName = player.name;
        // Add player object for connection
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public void Disconnect()
    {
        if (isHost)
        {
            Debug.LogError("one");
            matchMaker.DestroyMatch((NetworkID)networkId, 0, (bool success, string extendedInfo) =>
            {
                StopHost();
                StopServer();
                StopMatchMaker();
                SceneManager.LoadScene("ChoosePlayer");
            });
        }
        else
        {
            Debug.LogError("two");
            matchMaker.DropConnection((NetworkID)networkId, (NodeID)nodeId, 0, (bool success, string extendedInfo) =>
            {
                Debug.Log("Shutdown client");
                StopClient();
                StopMatchMaker();
                SceneManager.LoadScene("ChoosePlayer");
            });
        }
    }

    void OnApplicationQuit()
    {
        //SceneManager.LoadScene("ChoosePlayer");
        AudioManager.Instance.StopAll();
        AudioManager.Instance.Play("ui_swish");
        Disconnect();
    }
}