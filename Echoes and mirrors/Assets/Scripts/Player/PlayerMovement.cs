using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Space(5)]
    public float moveSpeed = 5f;
    public float rotationSpeed = 360f;

    private PlayerControls controls;
    private Vector2 moveInput;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    public void OnEnable()
    {
        controls.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
    }

    public void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;

        controls.Disable();
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

   

    void Update()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);
        moveDir.Normalize();

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        if(moveDir != Vector3.zero)
        {
            Quaternion targetRotation =
               Quaternion.LookRotation(moveDir);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
