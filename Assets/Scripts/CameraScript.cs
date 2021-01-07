using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
    // Smooth camera taken from here
    // https://gamedev.stackexchange.com/questions/114742/how-can-i-make-camera-to-follow-smoothly

    // The target to follow
    public Transform target;

    // How far away in the x-z plane should the camera be from the target
    public float distance;

    // How high should the camera be above the target
    public float height;

    // How quickly should changes in height or direction be applied
    public float heightDamping;
    // public float rotationDamping;

    // float wantedRotationAngle;
    float wantedHeight;

    // float currentRotationAngle;
    float currentHeight;

    Quaternion currentRotation;

    void LateUpdate()
    {
        if (target)
        {
            // Calculate the current rotation angles
            // wantedRotationAngle = target.eulerAngles.y;
            wantedHeight = target.position.y + height;
            // currentRotationAngle = transform.eulerAngles.y;
            currentHeight = transform.position.y;
            // Damp the rotation around the y-axis
            // currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
            // Damp the height
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
            // Convert the angle into a rotation
            // currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera to distance units in the -z direction relative to the player
            transform.position = target.position + Vector3.back * distance;
            // Set the height of the camera to be currentHeight units above the player
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            // Always look at the target
            transform.LookAt(target);
        }
    }
}