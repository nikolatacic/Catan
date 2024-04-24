using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float panBorderThickness = 10f;
    [SerializeField] private Vector2 panLimit;
    [Header("Perspective")]
    [SerializeField] private float scrollSpeed = 20f;
    [SerializeField] private float minZ = -10f;
    [SerializeField] private float maxZ = 20f;
    [Header("Ortographic")]
    [SerializeField] private float minCameraSize = 1.5f;
    [SerializeField] private float maxCameraSize = 5f;
    [SerializeField] private float cameraZoomSpeed = 5f;

    private Camera mainCamera;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        mainCamera = transform.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsMouseOverGameWindow() || mainCamera == null)
        {
            return;
        }

        //Debug.Log(Input.mousePosition);
        
        Vector3 pos = transform.position;
        
        if (/*Input.GetKey("w") ||*/ Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.y += panSpeed * Time.deltaTime;
        }

        if (/*Input.GetKey("s") ||*/ Input.mousePosition.y <= panBorderThickness)
        {
            pos.y -= panSpeed * Time.deltaTime;
        }
        if (/*Input.GetKey("a") ||*/ Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }
        if (/*Input.GetKey("d") ||*/ Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * Time.deltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        

        // Scroll
        if (mainCamera.orthographic)
        {
            float cameraSize = mainCamera.orthographicSize - scroll * cameraZoomSpeed;
            cameraSize = Mathf.Clamp(cameraSize, minCameraSize, maxCameraSize);
            mainCamera.orthographicSize = cameraSize;
        }
        else
        {
            pos.z += scroll * scrollSpeed * 100f * Time.deltaTime;
        }
        
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        
        transform.position = pos;
    }
    
    bool IsMouseOverGameWindow()
    {
        Rect windowRect = new Rect(0, 0, Screen.width + 5, Screen.height + 5);
        return windowRect.Contains(Input.mousePosition);
    }
}