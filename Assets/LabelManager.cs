using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LabelManager : MonoBehaviour
{
    GetAirportData m_localAirportsObject;
    Transform m_mainCamera;
    Transform m_worldCanvas;
    [SerializeField] private Transform m_anchor;

    static readonly string FIELDTAG = "fieldpoint";
    static readonly string AFLABELTAG = "airfieldLabel";

    // Start is called before the first frame update
    void Start()
    {
        m_localAirportsObject = GetComponent<GetAirportData>();
        m_mainCamera = Camera.main.transform;
        m_worldCanvas = GameObject.FindObjectOfType<Canvas>().transform;
    }

    // Update is called once per frame
    void Update()
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
                    childText.rotation = Quaternion.LookRotation(childText.position - m_mainCamera.position);
                }
            }
        }
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
    }
}
