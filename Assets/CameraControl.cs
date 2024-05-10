using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Camera cameraObj;
    private GameObject planetSphere;
    private float camSensitivity = 200.0f;
    private float zoomSensitivity = 1000.0f;

    [SerializeField] private Transform m_anchor;

    // Start is called before the first frame update
    void Start()
    {
        cameraObj = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main.pixelRect.Contains(Input.mousePosition))
        {
            if (Input.GetMouseButton(0))
            {
                CameraOrbit();
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                CameraZoom();
            }
        }
    }

    // Rotate the focal point, which the camera is a child of
    private void CameraOrbit()
    {
        if (Input.GetAxis("Mouse Y") != 0 || Input.GetAxis("Mouse X") != 0)  // God I hope that's LMB
        {
            float verticalInput = -Input.GetAxis("Mouse Y") * camSensitivity * Time.deltaTime;
            float horizontalInput = Input.GetAxis("Mouse X") * camSensitivity * Time.deltaTime;
                                                                                             
            transform.Rotate(Vector3.right, verticalInput); // rotate about the lateral axis 
            transform.Rotate(Vector3.up, horizontalInput, Space.World);  // rotate about the vertical axis
        }
    }

    private void CameraZoom()
    {
        float zoomInput = Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity * Time.deltaTime;
        cameraObj.fieldOfView -= zoomInput;

        float objScaleScalar = (cameraObj.fieldOfView/60.0f) * 0.015f;  // 0.015 is the default waypoint scale
        Vector3 scaleChange = new Vector3(objScaleScalar, objScaleScalar, objScaleScalar);

        // scale waypoint size with fov
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            var child = m_anchor.GetChild(i);
            if (child != null)
            {
                if (child.gameObject.CompareTag("navpoint"))
                {
                    child.localScale = scaleChange;
                }
                else if (child.gameObject.CompareTag("startpoint")
                    || child.gameObject.CompareTag("endpoint"))
                {
                    child.localScale = scaleChange*1.15f;
                }
                else if (child.gameObject.CompareTag("fieldpoint"))
                {
                    child.localScale = scaleChange;

                    // Move text labels based on zoom.
                }
            }
        }
    }
}
