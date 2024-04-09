using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using TMPro;

public class GetAirportData : MonoBehaviour
{
    private string rawFileText;
    private string[] areaElements;
    private List<string[]> airportData = new List<string[]>();

    static readonly string NAMETAG = "name=";
    static readonly string STARTTAG = "start=";
    static readonly string LATTAG = "lat=";
    static readonly string LONTAG = "lon=";
    static readonly string ALTTAG = "alt=";

    [SerializeField] private TMP_Dropdown dd_start;
    [SerializeField] private TMP_Dropdown dd_dest;

    [SerializeField] private TMP_InputField m_startLatInput;
    [SerializeField] private TMP_InputField m_startLonInput;
    [SerializeField] private TMP_InputField m_destLatInput;
    [SerializeField] private TMP_InputField m_destLonInput;

    public void ReadData()
    {
        string path = EditorUtility.OpenFilePanel("Open AreaData file","","txt");
        StreamReader reader = new StreamReader(path);
        rawFileText = reader.ReadToEnd();
        Debug.Log(rawFileText);
        
        areaElements = rawFileText.Split("Area\n{");

        dd_start.ClearOptions();
        dd_dest.ClearOptions();

        //parse each area element for name, lat, and lon
        foreach (string element in areaElements)
        {
            if (element.Contains(NAMETAG) && element.Contains(STARTTAG))
            {
                int index = element.IndexOf(NAMETAG);
                string areaName = element.Substring(index + NAMETAG.Length, element.IndexOf(STARTTAG) - (2*STARTTAG.Length));   // This is a terrible hack

                index = element.IndexOf(LATTAG);
                string areaLat = element.Substring(index + LATTAG.Length, element.IndexOf(ALTTAG) - (index + ALTTAG.Length + 1) );

                index = element.IndexOf(LONTAG);
                string areaLon = element.Substring(index + LONTAG.Length, element.IndexOf(LATTAG) - (index + LATTAG.Length + 1) );

                string[] item = {areaName, areaLat, areaLon};

                airportData.Add(item);

                addDropdownOption(item[0]);

                Debug.Log(item);
            }
        }

        // Set the current dropdown elements to the first two airfields. TODO: Add a case for the user only having one airfield.
        // If you're reading this and only have one airfield, consider therapy.

        dd_start.value = 0;
        dd_dest.value = 1;

        setStartPoint();
        setDestPoint();
    }

    // There's probably a better way of doing this, but I only have two dropdowns and their contents are to be identical. Just add the options "manually" for both.
    private void addDropdownOption(string airfieldName)
    {
        dd_start.options.Add(new TMP_Dropdown.OptionData(airfieldName,null));
        dd_dest.options.Add(new TMP_Dropdown.OptionData(airfieldName,null));
    }

    // Changes the start point coordinate fields based on start dropdown input.
    public void setStartPoint()
    {
        int airfieldIndex = dd_start.value;

        string airfieldLat = (airportData[airfieldIndex])[1];
        string airfieldLon = (airportData[airfieldIndex])[2];

        convertFileCoordinates(ref airfieldLat);
        convertFileCoordinates(ref airfieldLon);

        m_startLatInput.text = airfieldLat;
        m_startLonInput.text = airfieldLon;
    }

    public void setDestPoint()
    {
        int airfieldIndex = dd_dest.value;

        string airfieldLat = (airportData[airfieldIndex])[1];
        string airfieldLon = (airportData[airfieldIndex])[2];

        convertFileCoordinates(ref airfieldLat);
        convertFileCoordinates(ref airfieldLon);

        m_destLatInput.text = airfieldLat;
        m_destLonInput.text = airfieldLon;
    }

    private void convertFileCoordinates(ref string coordinate_str)
    {
        float coordinate_fl;
        if ( float.TryParse(coordinate_str, out coordinate_fl) )
        {
            coordinate_fl -= 90.0f;
            coordinate_str = coordinate_fl.ToString("0.0000");
        };
    }
}
