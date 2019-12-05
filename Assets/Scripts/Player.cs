using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public string playerName;
    public Transform homeAreaAnchor;

    [SyncVar]
    public int score = 0;
}
