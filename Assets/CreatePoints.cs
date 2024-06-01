using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePoints : MonoBehaviour
{
    private CalcStart m_calcStart;
    private GetAirportData m_getAirportData;
    //private List<float[]> coordMat_local = new List<float[]>();
    private int m_ItemCount;
    [SerializeField] private GameObject m_wayPoint;
    [SerializeField] private GameObject m_startPoint;
    [SerializeField] private GameObject m_endPoint;
    [SerializeField] private GameObject m_fieldPoint;
    [SerializeField] private Transform  m_anchor;

    private Camera cameraObj;   // used to scale points on creation

    //static readonly float QUAD = 0.5f * Mathf.PI;
    //static readonly float TAU  = 2.0f * Mathf.PI;

    private float radius = 0.5f;
    static readonly float RADCONV = Mathf.PI / 180.0f;

    void Start()
    {
        cameraObj = FindObjectOfType<Camera>();
    }

    public void ModifyPoints()
    {
        List<float[]> coordMat_local = new List<float[]>();
        coordMat_local.Clear();

        m_calcStart = FindObjectOfType<CalcStart>();
        m_ItemCount = m_calcStart.coordMat.Count;
        coordMat_local = m_calcStart.coordMat;

        // get camera FOV for initial zoom scaling (literally yoinked from CameraControl)
        float objScaleScalar = (cameraObj.fieldOfView / 60.0f) * 0.015f;  // 0.015 is the default waypoint scale
        Vector3 scaleChange = new Vector3(objScaleScalar, objScaleScalar, objScaleScalar);

        // get rid of all existing points
        //GameObject[] navObjects = GameObject.FindGameObjectsWithTag("navpoint");

        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag("navpoint")
                || m_anchor.transform.GetChild(i).gameObject.CompareTag("startpoint")
                || m_anchor.transform.GetChild(i).gameObject.CompareTag("endpoint"))
            { 
                Object.Destroy(m_anchor.transform.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < m_ItemCount; i++)   // number of rows in the coordmat
        {
            float matLat = (coordMat_local[i])[0];
            //matLat = 90.0f - matLat;   // need to transform to turn to cartesian

            float matLon = (coordMat_local[i])[1];
            if (matLon < 0.0f)
                matLon += 360.0f;
            matLon += 270;

            //convert transformed angles from deg to rad
            matLat = matLat * RADCONV;
            matLon = matLon * RADCONV;

            //Vector3 cartCoords = latLon2Cart(matLat, matLon);
            
            if (i == 0)                                     // startpoint
            {
                var startPt = Instantiate(m_startPoint);
                startPt.transform.SetParent(m_anchor);
                startPt.transform.position = sph2Cart(matLat, matLon);
                startPt.transform.localScale = scaleChange*1.15f;
            }
            else if (i == m_ItemCount - 1)                  // endpoint
            {
                var endPt = Instantiate(m_endPoint);
                endPt.transform.SetParent(m_anchor);
                endPt.transform.position = sph2Cart(matLat, matLon);
                endPt.transform.localScale = scaleChange*1.15f;
            }
            else                                            // waypoint
            {
                var wayPt = Instantiate(m_wayPoint);
                wayPt.transform.SetParent(m_anchor);
                wayPt.transform.position = sph2Cart(matLat, matLon);
                wayPt.transform.localScale = scaleChange;
                wayPt.name = "navPoint_" + i.ToString() + " ";  // the space is a stopgap for placing the number slightly to the left
            }
        }

        LabelManager localLabelManager = FindObjectOfType<LabelManager>();
        localLabelManager.createNavPointLabels();
    }

    public void ModifyAirfieldPoints()
    {
        List<float[]> airfieldCoords_Local = new List<float[]>();
        airfieldCoords_Local.Clear();
        
        List<GetAirportData.AreaElement> areaElements_Local = new List<GetAirportData.AreaElement>();
        areaElements_Local.Clear();

        // get rid of all existing points
        for (var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            if (m_anchor.transform.GetChild(i).gameObject.CompareTag("fieldpoint"))
            {
                Object.Destroy(m_anchor.transform.GetChild(i).gameObject);
            }
        }

        // get camera FOV for initial zoom scaling (literally yoinked from CameraControl)
        float objScaleScalar = (cameraObj.fieldOfView / 60.0f) * 0.015f;  // 0.015 is the default waypoint scale
        Vector3 scaleChange = new Vector3(objScaleScalar, objScaleScalar, objScaleScalar);

        m_getAirportData = FindObjectOfType<GetAirportData>();
        areaElements_Local = m_getAirportData.m_areas;
        
        foreach (var areaElement in areaElements_Local)
        {
            string latLocal = areaElement.lat;
            string lonLocal = areaElement.lon;
            // test if airfield coords are already correct from file for placement
            // they're not
            float latLocal_fl = convertFileCoordinates(latLocal);
            float lonLocal_fl = convertFileCoordinates(lonLocal);

            if (lonLocal_fl < 0.0f)
                lonLocal_fl += 360.0f;
            lonLocal_fl += 270;

            //convert transformed angles from deg to rad
            latLocal_fl = latLocal_fl * RADCONV;
            lonLocal_fl = lonLocal_fl * RADCONV;

            var fieldPt = Instantiate(m_fieldPoint);
            fieldPt.transform.SetParent(m_anchor);
            fieldPt.transform.position = sph2Cart(latLocal_fl, lonLocal_fl);
            fieldPt.transform.localScale = scaleChange;
            fieldPt.name = "fieldPoint_" + areaElement.name;    // kind of a stopgap solution for getting airfield names to the labels in LabelManager
        }
    }

    private Vector3 sph2Cart(float elev, float az)
    {
        float y = radius * Mathf.Sin(elev);
        float rcosElev = radius * Mathf.Cos(elev);
        float x = rcosElev * Mathf.Cos(az);
        float z = rcosElev * Mathf.Sin(az);

        return new Vector3(x, y, z);
    }

    private float convertFileCoordinates(string coordinate_str)
    {
        float coordinate_fl = 0.0f;
        if (float.TryParse(coordinate_str, out coordinate_fl))
        {
            coordinate_fl -= 90.0f;
        };

        return coordinate_fl;
    }
}
