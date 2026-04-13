using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DestSetAirfield : MonoBehaviour
{
    public TMP_Dropdown dd_dest;
    
    // Start is called before the first frame update
    void Start()
    {
        TMP_Dropdown dropdown = dd_dest.GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(delegate { updateDestinations(dd_dest); });
    }

    // Update is called once per frame
    void updateDestinations(TMP_Dropdown dropdown)
    {
        // Set lat/lon of this destination item's respective input fields
        GetAirportData localGAD = FindObjectOfType<GetAirportData>();
        localGAD.setDestPoint(dropdown);
    }
}
