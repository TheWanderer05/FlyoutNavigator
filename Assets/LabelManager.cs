using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LabelManager : MonoBehaviour
{
    //GetAirportData m_localAirportsObject;
    Transform m_mainCamera;
    //Transform m_worldCanvas;
    [SerializeField] private GameObject m_focalPoint;
    [SerializeField] private Transform m_anchor;
    private Transform m_localFocalPoint;
    static readonly float m_minRadius = 0.505f;
    static readonly float m_maxRadius = 0.58f;

    static readonly string FIELDTAG = "fieldpoint";
    static readonly string AFLABELTAG = "airfieldLabel";

    // Start is called before the first frame update
    void Start()
    {
        m_mainCamera = Camera.main.transform;
        Vector3 camPos = m_mainCamera.position;
        Vector3 focalPos = new Vector3(camPos.x, camPos.y, camPos.z - 10.0f);
        m_localFocalPoint = Instantiate(m_focalPoint.transform, focalPos, m_mainCamera.rotation, m_mainCamera);
    }

    public void createAirfieldLabels()
    {
        // destroy label objects first?

        // Cycle through all of the field points present on the map, and add a text label to each of them.
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(FIELDTAG))
            {
                var thisChild = m_anchor.transform.GetChild(i).transform;

                // add label by creating a new object with a text mesh, then parent it to the point.

                GameObject namePlate = new GameObject("pointLabel");
                namePlate.tag = AFLABELTAG;
                TextMeshPro textMesh = namePlate.AddComponent<TextMeshPro>();

                if (textMesh != null)
                {
                    textMesh.transform.position = thisChild.position;
                    string airfieldName = m_anchor.transform.GetChild(i).gameObject.name.Substring("fieldPoint_".Length);
                    textMesh.text = airfieldName;

                }
                namePlate.transform.SetParent(thisChild);
                namePlate.transform.position = thisChild.position;
                namePlate.transform.localScale = thisChild.localScale * 25;
                namePlate.transform.rotation = thisChild.rotation;
            }
        }

        // put the labels at a default radius
        UpdateLabelOffset(m_maxRadius - m_minRadius);
    }

    public void UpdateLabelRotation()
    {
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(FIELDTAG))
            {
                // now search for label...
                Transform thisChild = m_anchor.transform.GetChild(i);
                Transform childText = thisChild.transform.Find("pointLabel");
                if (childText != null)
                {
                    childText.rotation = Quaternion.LookRotation(childText.position - m_localFocalPoint.position);
                }
            }
        }
    }

    public void UpdateLabelOffset(float offsetScalar)
    {

        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(FIELDTAG))
            {
                // now search for label...
                Transform thisChild = m_anchor.transform.GetChild(i);
                Transform childText = thisChild.transform.Find("pointLabel");
                if (childText != null)
                {
                    Vector3 newPosition = offsetRadius(childText.position.x, childText.position.y, childText.position.z, offsetScalar);
                    childText.position = newPosition;
                }
            }
        }
    }

    private Vector3 offsetRadius(float xIn, float yIn, float zIn, float radiusOffset)
    {
        // using an input desired radius/delta radius, get spherical coords of old position, add offset, then convert back to cartesian and output.

        float radius = Mathf.Sqrt(xIn*xIn + yIn*yIn + zIn*zIn);
        float az = Mathf.Atan2(zIn, xIn);
        float elev = Mathf.Atan2(Mathf.Sqrt(xIn*xIn + zIn*zIn),yIn);

        float newRadius = radius + radiusOffset;
        
        if (newRadius > m_maxRadius)
        {
            newRadius = m_maxRadius;
        }
        else if (newRadius < m_minRadius)
        {
            newRadius = m_minRadius;
        }

        float xOut = newRadius * Mathf.Cos(az) * Mathf.Sin(elev);
        float zOut = newRadius * Mathf.Sin(az) * Mathf.Sin(elev);
        float yOut = newRadius * Mathf.Cos(elev);

        return new Vector3(xOut, yOut, zOut);
    }
}
