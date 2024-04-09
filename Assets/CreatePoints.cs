using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePoints : MonoBehaviour
{
    private CalcStart calcStart;
    //private List<float[]> coordMat_local = new List<float[]>();
    private int m_ItemCount;
    [SerializeField] private GameObject m_wayPoint;
    [SerializeField] private GameObject m_startPoint;
    [SerializeField] private GameObject m_endPoint;
    [SerializeField] private Transform  m_anchor;

    //static readonly float QUAD = 0.5f * Mathf.PI;
    //static readonly float TAU  = 2.0f * Mathf.PI;

    private float radius = 0.5f;
    static readonly float RADCONV = Mathf.PI / 180.0f;

    public void ModifyPoints()
    {
        List<float[]> coordMat_local = new List<float[]>();
        coordMat_local.Clear();

        calcStart = FindObjectOfType<CalcStart>();
        m_ItemCount = calcStart.coordMat.Count;
        coordMat_local = calcStart.coordMat;

        // get rid of all existing points
        //GameObject[] navObjects = GameObject.FindGameObjectsWithTag("navpoint");
        
        for(var i = m_anchor.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(m_anchor.transform.GetChild(i).gameObject);
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
            }
            else if (i == m_ItemCount - 1)                  // endpoint
            {
                var endPt = Instantiate(m_endPoint);
                endPt.transform.SetParent(m_anchor);
                endPt.transform.position = sph2Cart(matLat, matLon);
            }
            else                                            // waypoint
            {
                var wayPt = Instantiate(m_wayPoint);
                wayPt.transform.SetParent(m_anchor);
                wayPt.transform.position = sph2Cart(matLat, matLon);
            }
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
}
