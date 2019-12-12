using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerEx : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject netPlayer = Instantiate(playerPrefab);
        // This call spawns the NetPlayer game object on all clients,
        // and sets it up as the local player-controlled object on the connecting client.
        // The "Player" (in the game sense) will be assigned once CmdSetReady is invoked.
        NetworkServer.AddPlayerForConnection(conn, netPlayer);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity == null)
            return;
        
        Player player = conn.identity.GetComponent<PlayerNetController>().player;
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
    
    // Probably obsolete since the scene is unloaded anyway ...
    public override void OnStopHost()
    {
        GameManager.Instance.UnassignPlayer(GameManager.Instance.player1);
    }

    public override void OnStopServer()
    {
        GameManager.Instance.StopGame();
    }
}
