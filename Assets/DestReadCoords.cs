using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DestReadCoords : MonoBehaviour
{
    public TMP_InputField latField;
    public TMP_InputField lonField;

    // Start is called before the first frame update
    void Start()
    {
        TMP_InputField localLatField = latField.GetComponent<TMP_InputField>();
        localLatField.onEndEdit.AddListener(delegate { setDestLat(latField); });

        TMP_InputField localLonField = lonField.GetComponent<TMP_InputField>();
        localLonField.onEndEdit.AddListener(delegate { setDestLon(lonField); });
    }

    // Note: when selecting an airfield, these don't get called.

    void setDestLat(TMP_InputField field)
    {
        if (float.TryParse(field.text, out float latCoords))
        {
            string destItemName = field.transform.parent.name;
            string[] nameParts = destItemName.Split('_');
            // Second half should be a number
            if (int.TryParse(nameParts[1], out int destItemIndex))
            {
                CalcStart localCS = FindAnyObjectByType<CalcStart>();
                // Update stuff in CalcStart
                localCS.updateDestItemLat(destItemIndex, latCoords);
                //Debug.Log("Dest item " + destItemIndex + " updated Lat coords : " + latCoords);
            }
        }

    }

    void setDestLon(TMP_InputField field)
    {
        if (float.TryParse(field.text, out float lonCoords))
        {
            string destItemName = field.transform.parent.name;
            string[] nameParts = destItemName.Split('_');
            // Second half should be a number
            if (int.TryParse(nameParts[1], out int destItemIndex))
            {
                CalcStart localCS = FindAnyObjectByType<CalcStart>();
                // Update stuff in CalcStart
                localCS.updateDestItemLon(destItemIndex, lonCoords);
                //Debug.Log("Dest item " + destItemIndex + " updated Lon coords: " + lonCoords);
            }
        }
    }

    // Intended to be called by DestSetAirfield so that the coordinates are actually 
    // read in when a selection is made and the input fields populate.
    public void forceSetCoords()
    {
        setDestLat(latField);
        setDestLon(lonField);
    }
}
