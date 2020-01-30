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

        if (!conn.identity.isLocalPlayer)
        {
            // Initially send the current scale, in case it was already changed before the player connected.
            Scale.Instance.TargetRpcChangeScale(conn, Scale.gameScale);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Player player = conn.identity?.GetComponent<PlayerNetController>()?.player;
        base.OnServerDisconnect(conn);

        if (player != null)
        {
            Debug.Log("Client disconnected! Unassigned player: " + player.playerName, player);
            GameManager.Instance.UnassignPlayer(player);
        }
        else
        {
            Debug.Log("Client disconnected (was not assigned to player)!");
        }
    }
}
