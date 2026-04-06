using UnityEngine;

public class PositionController : MonoBehaviour
{
    public Transform cameraTransform;
    //Set it to whatever value you think is best
    public float zDistanceFromCamera;
    public float yDistanceFromCamera;
    public float xDistanceFromCamera;

    void Update()
    {
        //Vector3 resultingPosition = cameraTransform.position + (cameraTransform.forward * xDistanceFromCamera) + (-cameraTransform.up * yDistanceFromCamera);
        //transform.position = resultingPosition;

        Vector3 resultingPosition = new Vector3(cameraTransform.position.x, cameraTransform.position.y - yDistanceFromCamera, cameraTransform.position.z + (1 * yDistanceFromCamera));



    }
}
