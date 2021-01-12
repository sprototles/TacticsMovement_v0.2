using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject cameraPlaceholder;

    /// <summary>
    /// GameObject for Position
    /// </summary>
    public GameObject XYZ;

    /// <summary>
    /// GameObject for Rotation
    /// </summary>
    public GameObject RPY;

    public Camera mainCamera;

    /// <summary>
    /// TRUE when zooming in/out on city tile
    /// </summary>
    public bool isLocked;

    /// <summary>
    /// position of object XYZ
    /// </summary>
    public Vector3 posXYZ;

    /// <summary>
    /// position of camera
    /// </summary>
    public Vector3 posMainCamera;

    public Vector2 panLimitX;

    public Vector2 panLimitZ;

    public Vector2 panScrollLimit;  // Y limit

    [Range(0f, 1000f)]
    public float camTransitionSpeed;

    /// <summary>
    /// degree
    /// </summary>
    public float targetAngle;

    /// <summary>
    /// degree
    /// </summary>
    public float camAngle;

    public float panSpeed = 20f;

    public float scrollSpeed = 5f;

    public float movementMultiplier = 0.5f;

    private void OnGUI()
    {
        if (mainCamera.isActiveAndEnabled)
        {
            GUI.Box(new Rect(0, 0, 150, 25), "Camera: " + cameraPlaceholder.name);
        }
    }

    private void Awake()
    {
        if (cameraPlaceholder == null)
        {
            Debug.LogError("cameraPlaceholder\n" + this, gameObject);
        }

        if (XYZ == null)
        {
            Debug.LogError("XYZ\n" + this,gameObject);
        }

        if (RPY == null)
        {
            Debug.LogError("RPY\n" + this, gameObject);
        }

        if (mainCamera == null)
        {
            Debug.LogError("mainCamera\n" + this, gameObject);
        }


    }

    private void Start()
    {

        panLimitX = new Vector2(-10,10);
        panLimitZ = new Vector2(-10, 10);
        panScrollLimit = new Vector2(-20f,-5f);
        camTransitionSpeed = 100.0f;
        targetAngle = 0;

    }

    // Update is called once per frame
    void Update()
    {
        posXYZ = XYZ.transform.localPosition;
        posMainCamera = mainCamera.transform.localPosition;

        if (Input.GetKey(KeyCode.W)) MoveForward();     // forward
        if (Input.GetKey(KeyCode.S)) MoveBackward();    // backward
        if (Input.GetKey(KeyCode.D)) MoveRigth();       // right
        if (Input.GetKey(KeyCode.A)) MoveLeft();        // left
        if (Input.GetKeyDown(KeyCode.E)) RotateRight(); // rotate
        if (Input.GetKeyDown(KeyCode.Q)) RotateLeft();  // rotate
        if (Input.GetKey(KeyCode.Z)) ZoomOut(); // down
        if (Input.GetKey(KeyCode.X)) ZoomIn(); // up

        // mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        posMainCamera.z -= scroll * scrollSpeed * 100f * Time.deltaTime;
        posMainCamera.z = Mathf.Clamp(posMainCamera.z, panScrollLimit.x, panScrollLimit.y);
        // panSpeed = posMainCamera.y * 2;


        mainCamera.fieldOfView = 60f;
        camAngle = Mathf.MoveTowardsAngle(RPY.transform.localEulerAngles.y, targetAngle, camTransitionSpeed * Time.deltaTime);
        RPY.transform.localEulerAngles = new Vector3(RPY.transform.localEulerAngles.x, camAngle, 0);

        // map limits
        posXYZ.x = Mathf.Clamp(posXYZ.x, panLimitX.x, panLimitX.y);
        posXYZ.z = Mathf.Clamp(posXYZ.z, panLimitZ.x, panLimitZ.y);


        XYZ.transform.localPosition = posXYZ;
        mainCamera.transform.localPosition = posMainCamera;
    }

    private void MoveForward()
    {
        posXYZ.z += Mathf.Cos(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
        posXYZ.x += Mathf.Sin(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
    }

    private void MoveBackward()
    {
        posXYZ.z -= Mathf.Cos(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
        posXYZ.x -= Mathf.Sin(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
    }
    private void MoveRigth()
    {
        posXYZ.z -= Mathf.Sin(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
        posXYZ.x += Mathf.Cos(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
    }
    private void MoveLeft()
    {
        posXYZ.z += Mathf.Sin(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
        posXYZ.x -= Mathf.Cos(targetAngle * Mathf.PI / 180) * panSpeed * movementMultiplier * Time.deltaTime; 
    }

    private void RotateLeft()
    {
        targetAngle += 45.0f;

        if(Mathf.Abs(targetAngle) == 360)
        {
            targetAngle = 0;
        }
    }

    private void RotateRight()
    {
        targetAngle -= 45.0f;

        if (Mathf.Abs(targetAngle) == 360)
        {
            targetAngle = 0;
        }
    }

    private void ZoomIn()
    {
        posMainCamera.z -= scrollSpeed * movementMultiplier * Time.deltaTime * 5;
    }

    private void ZoomOut()
    {
        posMainCamera.z += scrollSpeed * movementMultiplier * Time.deltaTime * 5;
    }
}
