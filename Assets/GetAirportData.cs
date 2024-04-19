using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using TMPro;
using SimpleFileBrowser;

public class GetAirportData : MonoBehaviour
{
    private string m_defaultPath = "C:\\Users\\%USERNAME%\\AppData\\LocalLow\\Stonext Games\\Flyout\\AreaData";

    static readonly string AREATAG = "Area";
    static readonly string NAMETAG = "name=";
    //static readonly string STARTTAG = "start=";
    static readonly string LATTAG = "lat=";
    static readonly string LONTAG = "lon=";
    //static readonly string ALTTAG = "alt=";

    [SerializeField] private TMP_Dropdown dd_start;
    [SerializeField] private TMP_Dropdown dd_dest;

    [SerializeField] private TMP_InputField m_startLatInput;
    [SerializeField] private TMP_InputField m_startLonInput;
    [SerializeField] private TMP_InputField m_destLatInput;
    [SerializeField] private TMP_InputField m_destLonInput;

    private List<AreaElement> m_areas = new List<AreaElement>();
    struct AreaElement
    {
        public string name;
        public string lat;
        public string lon;
        public int index;
    }

    public void StartDialog()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Files",".txt"));
        FileBrowser.AddQuickLink("AreaData", m_defaultPath, null);

        StartCoroutine(ShowDialogCoroutine());
    }

    private IEnumerator ShowDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, m_defaultPath, null, "Load", "Select");
        
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            ReadData(FileBrowser.Result);
        }
    }

    private void ReadData(string[] filePath)
    {
        // Multiselect was disabled in the dialog options, so we should only take from the first array element

        // Need to refactor this to use this method here on line 55. Turns out some areadata files have \r\n, which breaks the parser I made
        string[] fileLines = File.ReadAllLines(filePath[0]);
        //StreamReader reader = new StreamReader(filePath[0]);
        //rawFileText = reader.ReadToEnd();
        //Debug.Log(rawFileText);

        //areaElements = rawFileText.Split("Area\n{");

        m_areas.Clear();
        dd_start.ClearOptions();
        dd_dest.ClearOptions();

        int idx = 0;

        char[] delimiters = { '=', '\n' };

        AreaElement currentArea;
        currentArea.lat = "";
        currentArea.lon = "";
        currentArea.name = "";
        currentArea.index = 0;

        // count the number of area elements
        foreach (string line in fileLines)
        {
            string[] lineContents;

            if (line.StartsWith(AREATAG))
            {
                currentArea.index = idx;
                idx++;
            }
            else if (line.StartsWith(NAMETAG))
            {
                lineContents = line.Split(delimiters);
                currentArea.name = lineContents[1];
            }
            else if(line.StartsWith(LATTAG))
            {
                lineContents = line.Split(delimiters);
                currentArea.lat = lineContents[1];
            }
            else if (line.StartsWith(LONTAG))
            {
                lineContents = line.Split(delimiters);
                currentArea.lon = lineContents[1];
            }
            else // Nothing relevant to grab, move on to next line
            {
                continue;
            }
            if (currentArea.lat != "" && currentArea.lon != "" && currentArea.name != "")
            {
                // when all fields are populated, add struct to the list
                m_areas.Add(currentArea);
                addDropdownOption(currentArea.name);
                // reset current area element properties
                currentArea.lat = "";
                currentArea.lon = "";
                currentArea.name = "";
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

        string airfieldLat = (m_areas[airfieldIndex]).lat;
        string airfieldLon = (m_areas[airfieldIndex]).lon;

        convertFileCoordinates(ref airfieldLat);
        convertFileCoordinates(ref airfieldLon);

        m_startLatInput.text = airfieldLat;
        m_startLonInput.text = airfieldLon;
    }

    public void setDestPoint()
    {
        int airfieldIndex = dd_dest.value;

        string airfieldLat = (m_areas[airfieldIndex]).lat;
        string airfieldLon = (m_areas[airfieldIndex]).lon;

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
