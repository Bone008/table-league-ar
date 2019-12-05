using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerEx : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Player player = GameManager.Instance.AssignPlayer();
        if (player == null)
        {
            Debug.LogError("Tried to connect more players than available!");
            conn.Disconnect();
            return;
        }

        GameObject netPlayer = Instantiate(playerPrefab, player.transform);
        netPlayer.GetComponent<PlayerNetController>().gamePlayer = player;

        // This call spawns the Player game object on all clients,
        // and sets it up as the local player-controlled object on the connecting client.
        NetworkServer.AddPlayerForConnection(conn, netPlayer);
        if(GameManager.Instance.isReadyToStart)
        {
            GameManager.Instance.StartGame();
        }
    }
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity == null)
            return;
        
        Player player = conn.identity.GetComponent<PlayerNetController>().gamePlayer;
        if (player != null)
        {
            Debug.Log("unassigning player " + player.playerName, player);
            GameManager.Instance.UnassignPlayer(player);
        }
        else
        {
            Debug.LogError("Could not find Player script to unassign!");
        }
    }
    
    public override void OnStopHost()
    {
        GameManager.Instance.UnassignPlayer(GameManager.Instance.player1);
    }
}
