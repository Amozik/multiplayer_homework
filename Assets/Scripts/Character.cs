using System;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
public abstract class Character : NetworkBehaviour
{
    protected Action OnUpdateAction { get; set; }
    protected abstract FireAction FireAction { get; set; }

    [SyncVar] 
    protected Vector3 _serverPosition;
    [SyncVar] 
    protected Quaternion _serverRotation;

    protected virtual void Initiate()
    {
        OnUpdateAction += Movement;
    }

    private void Update()
    {
        OnUpdate();
    }

    private void OnUpdate()
    {
        OnUpdateAction?.Invoke();
    }

    [Command]
    protected void CmdUpdatePositionAndRotation(Vector3 position, Quaternion rotation)
    {
        _serverPosition = position;
        _serverRotation = rotation;
    }

    public abstract void Movement();
}