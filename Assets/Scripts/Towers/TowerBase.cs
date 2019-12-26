using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all root-level tower scripts.
/// Make sure that all derived logic only happens on the server!
/// </summary>
public abstract class TowerBase : NetworkBehaviour
{
    public Player owner;
}
