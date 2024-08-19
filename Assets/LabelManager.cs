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
    [SerializeField] private GameObject m_labelPrefab;
    [SerializeField] private GameObject m_subLabelPrefab;

    private CalcStart m_calcStart;

    private Transform m_localFocalPoint;
    static readonly float m_minRadius = 0.505f;
    static readonly float m_maxRadius = 0.58f;

    static readonly string FIELDTAG = "fieldpoint";
    static readonly string NAVTAG = "navpoint";
    static readonly string AFLABELTAG = "airfieldLabel";
    static readonly string NAVLABELTAG = "navpointLabel";
    static readonly string NAVSUBLABELTAG = "navSubLabel";
    static readonly string STARTTAG = "startpoint";
    static readonly string ENDTAG = "endpoint";

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

                var namePlate = Instantiate(m_labelPrefab);
                if (namePlate != null)
                {
                    namePlate.tag = AFLABELTAG;

                    string airfieldName = m_anchor.transform.GetChild(i).gameObject.name.Substring("fieldPoint_".Length);
                    var labelText = namePlate.transform.Find("LabelText");
                    labelText.GetComponent<TMPro.TextMeshPro>().text = airfieldName;

                    namePlate.transform.SetParent(thisChild);
                    namePlate.transform.position = thisChild.position;
                    namePlate.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f) * 25;
                    namePlate.transform.rotation = thisChild.rotation;
                }
            }
        }

        // put the labels at a default radius
        UpdateLabelOffset(m_maxRadius - (m_maxRadius - m_minRadius));
        UpdateLabelRotation();
        UpdateLabelOpacity(m_mainCamera.position);
    }

    public void createNavPointLabels()
    {
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVTAG))
            {
                var thisChild = m_anchor.transform.GetChild(i).transform;

                var namePlate = Instantiate(m_labelPrefab);
                if (namePlate != null)
                {
                    namePlate.tag = NAVLABELTAG;

                    string pointName = m_anchor.transform.GetChild(i).gameObject.name.Substring("navPoint_".Length);
                    var labelText = namePlate.transform.Find("LabelText");
                    labelText.GetComponent<TMPro.TextMeshPro>().text = pointName;

                    namePlate.transform.SetParent(thisChild);
                    namePlate.transform.position = thisChild.position;
                    namePlate.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f) * 25;
                    namePlate.transform.rotation = thisChild.rotation;
                }
            }
        }

        // put the labels at a default radius and rotation
        UpdateLabelOffset(m_maxRadius - (m_maxRadius - m_minRadius));
        UpdateLabelRotation();
        UpdateLabelOpacity(m_mainCamera.position);
    }

    public void clearNavPointLabels()
    {
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            // Find navpoint object
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVTAG))
            {
                var thisChild = m_anchor.transform.GetChild(i).gameObject;
                // Find any nav labels attached and delete them
                for (var j = thisChild.transform.childCount - 1; j >= 0; j--)
                {
                    if (thisChild.transform.GetChild(j).gameObject.CompareTag(NAVLABELTAG))
                    {
                        // Delete the label.
                        
                        Object.Destroy(thisChild.transform.GetChild(j).gameObject);
                    }
                }
            }
        }
    }

    public void createSubNavLabels()
    {
        // Get waypoint data from list
        List<float[]> coordMat_local = new List<float[]>();
        coordMat_local.Clear();
        int coordListCount = 0;

        m_calcStart = FindObjectOfType<CalcStart>();
        coordListCount = m_calcStart.coordMat.Count;
        coordMat_local = m_calcStart.coordMat;

        for (var i = m_anchor.childCount - 1; i >= 0; i--) // Will need to scale this code to work for multiple destinations, specifically finding the point index for intermediate destinations
        {
            // Search for points that are navpoints
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVTAG))
            {
                var thisPoint = m_anchor.transform.GetChild(i).transform;

                // Find the number at the end of the point name and use it as an index
                string pointName = m_anchor.transform.GetChild(i).gameObject.name.Substring("navPoint_".Length);

                int pointIndex = 0;
                if (!int.TryParse(pointName, out pointIndex))
                {
                    continue; // bad index, try next
                }

                //Find data struct info that corresponds to this navpoint
                float lat = (coordMat_local[pointIndex])[0]; // Can't use i here, as the child index does not correspond to the coordinate matrix index
                float lon = (coordMat_local[pointIndex])[1];
                float brg = (coordMat_local[pointIndex])[2];

                instantiateSubLabel(thisPoint, lat, lon, brg);
                
            }
            else if (m_anchor.transform.GetChild(i).gameObject.CompareTag(STARTTAG))    
            {
                var thisPoint = m_anchor.transform.GetChild(i).transform;

                //Find data struct info that corresponds to this point (0 for start point)
                float lat = (coordMat_local[0])[0]; // Can't use i here, as the child index does not correspond to the coordinate matrix index
                float lon = (coordMat_local[0])[1];
                float brg = (coordMat_local[0])[2];

                instantiateSubLabel(thisPoint, lat, lon, brg);
            }
            else if (m_anchor.transform.GetChild(i).gameObject.CompareTag(ENDTAG))  
            {
                var thisPoint = m_anchor.transform.GetChild(i).transform;

                //Find data struct info that corresponds to this point (last one for end point)
                float lat = (coordMat_local[coordListCount - 1])[0]; // Can't use i here, as the child index does not correspond to the coordinate matrix index
                float lon = (coordMat_local[coordListCount - 1])[1];
                float brg = (coordMat_local[coordListCount - 1])[2];

                instantiateSubLabel(thisPoint, lat, lon, brg);
            }
        }

        // put the labels at a default radius and rotation
        UpdateLabelOffset(m_maxRadius - (m_maxRadius - m_minRadius));
        UpdateLabelRotation();
        UpdateLabelOpacity(m_mainCamera.position);
    }

    public void clearSubNavLabels()
    {

    }

    public void UpdateLabelRotation()
    {
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            // Search for navpoints or airfield points
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(FIELDTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(STARTTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(ENDTAG)
                )
            {

                Transform thisChild = m_anchor.transform.GetChild(i);
                
                // now search for labels
                for (var j = thisChild.transform.childCount - 1; j >= 0; j--)
                {
                    if (thisChild.transform.GetChild(j).gameObject.CompareTag(NAVLABELTAG)
                        || thisChild.transform.GetChild(j).gameObject.CompareTag(NAVSUBLABELTAG)
                        || thisChild.transform.GetChild(j).gameObject.CompareTag(AFLABELTAG)
                        )
                    {
                        Transform childText = thisChild.transform.GetChild(j);

                        if (childText != null)
                        {
                            childText.rotation = Quaternion.LookRotation(childText.position - m_localFocalPoint.position);
                        }
                    }
                }
            }
        }
    }

    // Compare distance between camera and label to determine opacity
    public void UpdateLabelOpacity(Vector3 cameraPos)
    {
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(FIELDTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(STARTTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(ENDTAG)
                )
            {

                Transform thisChild = m_anchor.transform.GetChild(i);

                // now search for labels
                for (var j = thisChild.transform.childCount - 1; j >= 0; j--)
                {

                    if (thisChild.transform.GetChild(j).gameObject.CompareTag(NAVLABELTAG)
                        || thisChild.transform.GetChild(j).gameObject.CompareTag(NAVSUBLABELTAG)
                        || thisChild.transform.GetChild(j).gameObject.CompareTag(AFLABELTAG)
                        )
                    {
                        float distToLabel = (thisChild.transform.position - cameraPos).magnitude;

                        Transform childText = thisChild.transform.GetChild(j);

                        if (childText != null)
                        {
                            TextMeshPro labelText = null;

                            Transform textObject = childText.transform.Find("LabelText");
                            if (textObject)
                            {
                                labelText = textObject.GetComponent<TextMeshPro>();
                            }
                            else    
                            {
                                textObject = childText.transform.Find("LabelLatText");
                                if (textObject)
                                {
                                    labelText = textObject.GetComponent<TextMeshPro>(); // If the latitude text exists, then so should the other two. Find them.
                                }
                                
                                Transform lonTextObject = childText.transform.Find("LabelLonText");
                                TextMeshPro lonLabelText = null;
                                if (lonTextObject)
                                {
                                    lonLabelText = lonTextObject.GetComponent<TextMeshPro>(); // longitude text
                                }

                                Transform brgTextObject = childText.transform.Find("LabelBearingText");
                                TextMeshPro brgLabelText = null;
                                if (brgTextObject)
                                {
                                   brgLabelText = brgTextObject.GetComponent<TextMeshPro>(); // bearing text
                                }

                                fadeLabel(labelText, distToLabel);
                                
                                if (lonLabelText != null)
                                {
                                    fadeLabel(lonLabelText, distToLabel);
                                }

                                if (brgLabelText != null)
                                {
                                    fadeLabel(brgLabelText, distToLabel);
                                }
                            }

                            if (labelText != null)
                            {
                                fadeLabel(labelText, distToLabel);
                            }
                        }
                    }
                }
            }
        }
    }

    public void UpdateLabelOffset(float offsetScalar)
    {
        float localOffsetScalar = offsetScalar;
        bool isNavPt = false;
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            localOffsetScalar = offsetScalar;
            
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag(FIELDTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(STARTTAG)
                || m_anchor.transform.GetChild(i).gameObject.CompareTag(ENDTAG)
                )
            {
                isNavPt = false;

                if (m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVTAG)
                    || m_anchor.transform.GetChild(i).gameObject.CompareTag(NAVSUBLABELTAG)
                    )
                {
                    isNavPt = true;
                    localOffsetScalar = offsetScalar / 2;
                }
                
                
                Transform thisChild = m_anchor.transform.GetChild(i);

                // now search for labels
                for (var j = thisChild.transform.childCount - 1; j >= 0; j--)
                {
                    if (thisChild.transform.GetChild(j).gameObject.CompareTag(NAVLABELTAG)
                        || thisChild.transform.GetChild(j).gameObject.CompareTag(NAVSUBLABELTAG)
                        || thisChild.transform.GetChild(j).gameObject.CompareTag(AFLABELTAG)
                        )
                    {
                        Transform childText = thisChild.transform.GetChild(j);

                        if (childText != null)
                        {
                            Vector3 newPosition = offsetRadius(childText.position.x, childText.position.y, childText.position.z, localOffsetScalar, isNavPt);
                            childText.position = newPosition;
                        }
                    }
                }
            }
        }
    }

    private Vector3 offsetRadius(float xIn, float yIn, float zIn, float radiusOffset, bool isNavPt)
    {
        // using an input desired radius/delta radius, get spherical coords of old position, add offset, then convert back to cartesian and output.

        float radius = Mathf.Sqrt(xIn*xIn + yIn*yIn + zIn*zIn);
        float az = Mathf.Atan2(zIn, xIn);
        float elev = Mathf.Atan2(Mathf.Sqrt(xIn*xIn + zIn*zIn),yIn);

        float newRadius = radius + radiusOffset;
        float navMaxRadius = m_maxRadius - (m_maxRadius - m_minRadius)/2;

        if (!isNavPt && newRadius >= m_maxRadius)
        {   
            newRadius = m_maxRadius;
        }
        else if (isNavPt && newRadius >= navMaxRadius)
        {
            newRadius = navMaxRadius;
        }
        else if (newRadius <= m_minRadius)
        {
            newRadius = m_minRadius;
        }

        float xOut = newRadius * Mathf.Cos(az) * Mathf.Sin(elev);
        float zOut = newRadius * Mathf.Sin(az) * Mathf.Sin(elev);
        float yOut = newRadius * Mathf.Cos(elev);

        return new Vector3(xOut, yOut, zOut);
    }

    private void fadeLabel(TextMeshPro tmp, float distance)
    {
        if (distance > 1.10f) // Camera rotation is around 0,0,0 so its radius from planet center is just the position magnitude
        {
            tmp.alpha = 8.33f - 6.67f * distance;   // Should be opaque until 1.1 units or more away, completely transparent at 1.25 units
        }
        else
        {
            tmp.alpha = 1.0f;
        }
    }

    private void instantiateSubLabel(Transform point, float lat, float lon, float brg)
    {
        var infoLabel = Instantiate(m_subLabelPrefab);
        if (infoLabel != null)
        {
            infoLabel.tag = NAVSUBLABELTAG;

            // Replace sublabel prefab texts with relevant info
            var latText = infoLabel.transform.Find("LabelLatText");
            latText.GetComponent<TextMeshPro>().text += string.Format("{0:0.0000}", lat);

            var lonText = infoLabel.transform.Find("LabelLonText");
            lonText.GetComponent<TextMeshPro>().text += string.Format("{0:0.0000}", lon);

            var brgText = infoLabel.transform.Find("LabelBearingText");
            brgText.GetComponent<TextMeshPro>().text += string.Format("{0:0.0}", brg);

            infoLabel.transform.SetParent(point);
            infoLabel.transform.position = point.position;
            infoLabel.transform.localScale = new Vector3(0.02f, 0.02f , 0.02f) * 25;
            infoLabel.transform.rotation = point.rotation;

        }
    }
}
