using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerScript : MonoBehaviour
{
    // Reference to main game camera
    [FormerlySerializedAs("MainCamera")] public Camera mainCamera;

    // Which movement type to use
    public bool moveWithMouseClick = true;
    public LayerMask movementLayerMask;

    // How fast the player can move in cardinal directions using arrow keys
    private const float CardinalSpeed = 6f;

    // How far can the player move in any direction using the arrow keys
    private const float OmnidirectionalSpeed = 8f;

    // How quickly the player model can turn
    private const float RotationSpeed = 1000f;

    // Reference to the RigidBody for this player
    private Rigidbody _rigidbody;

    // Where the player is trying to go
    private Vector3 _targetDestination;

    // Variables for arrow-key based movement
    private float _horizontal, _vertical;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize RigidBody object
        _rigidbody = GetComponent<Rigidbody>();

        // Set position to center, not inside the ground
        transform.position = new Vector3(0, 1, 0);

        // Set target destination to player's own position initially
        _targetDestination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player is holding left-click down this frame
        if (Input.GetMouseButton(0))
        {
            // Raycast to the onscreen location -> get global coordinates of that click on the ground
            RaycastHit hit;
            if (Physics.Raycast(
                mainCamera.ScreenPointToRay(Input.mousePosition),
                out hit,
                Mathf.Infinity,
                movementLayerMask))
            {
                // Compute new target destination of player
                _targetDestination = new Vector3(hit.point.x, _rigidbody.position.y, hit.point.z);
            }
        }

        // Check arrow key status
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
    }

    // FixedUpdate is called by the Physics System
    private void FixedUpdate()
    {
        // Process movement based on mouse input
        if (moveWithMouseClick)
        {
            // Compute how the player would move to get there in one step
            Vector3 movement = _targetDestination - _rigidbody.position;
            movement = new Vector3(movement.x, 0, movement.z);

            // Only bother if actually moving somewhere
            if (movement != Vector3.zero)
            {
                // If destination is farther than the player can move since the last frame...
                if (movement.magnitude > OmnidirectionalSpeed * Time.deltaTime)
                {
                    // Turn towards the intended destination
                    Quaternion intendedLookDir = Quaternion.LookRotation(movement);
                    _rigidbody.rotation = Quaternion.RotateTowards(
                        _rigidbody.rotation,
                        intendedLookDir,
                        RotationSpeed * Time.deltaTime);
                    // Move the maximum possible distance in the needed direction
                    _rigidbody.MovePosition(_rigidbody.position +
                                            OmnidirectionalSpeed * Time.deltaTime * movement.normalized);
                }
                else
                {
                    // Will arrive at destination, so instant turn towards destination
                    _rigidbody.rotation = Quaternion.LookRotation(movement);
                    // Arrive at the destination.
                    _rigidbody.position = _targetDestination;
                }
            }
        }

        // Process movement based on keyboard input
        else
        {
            // Movement based on arrow keys - smoothing enabled by default
            _rigidbody.MovePosition(_rigidbody.position + new Vector3(
                CardinalSpeed * _horizontal * Time.deltaTime,
                0,
                CardinalSpeed * _vertical * Time.deltaTime
            ));
        }
    }
}