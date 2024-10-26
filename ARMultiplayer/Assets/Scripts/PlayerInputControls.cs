using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputControls : NetworkBehaviour
{
    private PlayerControlInputAction _playerControlsInputAction;
    private Vector3 movementVector;

    public event Action<Vector3> OnMoveInput;
    public event Action OnMoveActionCancelled;

    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerControlsInputAction = new PlayerControlInputAction();
            _playerControlsInputAction.Enable();

            _playerControlsInputAction.PlayerControlMap.Move.performed += MoveActionPerformed;
            _playerControlsInputAction.PlayerControlMap.Move.canceled += MoveActionCancelled;
        }
    }

    private void MoveActionCancelled(InputAction.CallbackContext context)
    {
        movementVector = Vector3.zero;
        OnMoveActionCancelled?.Invoke();
    }

    private void MoveActionPerformed(InputAction.CallbackContext context)
    {
        Vector2 v2Movement = context.ReadValue<Vector2>();
        movementVector = new Vector3(v2Movement.x, 0, v2Movement.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (movementVector != Vector3.zero)
        {
            OnMoveInput?.Invoke(movementVector);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerControlsInputAction.PlayerControlMap.Move.performed -= MoveActionPerformed;
            _playerControlsInputAction.PlayerControlMap.Move.canceled -= MoveActionCancelled;
        }
    }
}
