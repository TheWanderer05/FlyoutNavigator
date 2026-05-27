using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using TMPro;
using SimpleFileBrowser;
using Unity.VisualScripting;
using UnityEngine.UI;

public class GetAirportData : MonoBehaviour
{
    private string m_defaultPath = "C:\\Users\\%USERNAME%\\AppData\\LocalLow\\Stonext Games\\Flyout\\AreaData";

    static readonly string AREATAG = "Area";
    static readonly string NAMETAG = "name=";
    //static readonly string STARTTAG = "start=";
    static readonly string LATTAG = "lat=";
    static readonly string LONTAG = "lon=";
    //static readonly string ALTTAG = "alt=";
    private string[] m_filePath = null;

    [SerializeField] private TMP_Dropdown dd_start;
    [SerializeField] private TMP_Dropdown dd_dest; // going to need to replace this with a listener

    [SerializeField] private TMP_InputField m_startLatInput;
    [SerializeField] private TMP_InputField m_startLonInput;
    [SerializeField] private TMP_InputField m_destLatInput; // going to need to replace this with a listener
    [SerializeField] private TMP_InputField m_destLonInput;

    public List<AreaElement> m_areas = new List<AreaElement>();
    public List<GameObject> m_destList = new List<GameObject>(); // List of destination items
    public struct AreaElement
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
            m_filePath = FileBrowser.Result;
            ReadData(m_filePath);
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
        //dd_dest.ClearOptions();

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
                addStartDropdownOption(currentArea.name);
                //foreach (GameObject destItem in m_destList)
                //{
                //    addDestDropdownOption(currentArea.name, destItem);
                //}
                // reset current area element properties
                currentArea.lat = "";
                currentArea.lon = "";
                currentArea.name = "";
            }
        }

        // Set the current dropdown elements to the first two airfields. TODO: Add a case for the user only having one airfield.
        // If you're reading this and only have one airfield, consider therapy.

        dd_start.value = 0;
        dd_start.RefreshShownValue();

        setStartPoint();
        // Populate all existing destination items with imported area info and set default to default airfield
        foreach (GameObject destItem in m_destList)
        {
            PopulateNewDestinationDropDown(destItem);
        }

        // create airfield points on the map
        CreatePoints localPointsObject = FindObjectOfType<CreatePoints>();
        localPointsObject.ModifyAirfieldPoints();
        LabelManager localLabelManager = FindObjectOfType<LabelManager>();
        localLabelManager.createAirfieldLabels();
    }

    // Populates a new destination item's dropdown as soon as it's added
    public void PopulateNewDestinationDropDown(GameObject destItem) 
    {
        for (int i = 0; i < m_areas.Count; i++)
        {
            addDestDropdownOption(m_areas[i].name, destItem);
        }

        TMP_Dropdown dd_local = destItem.GetComponentInChildren<TMP_Dropdown>();
        setDestPoint(dd_local); // Just so the fields are filled to avoid confusion. Defaults to default airfield coords
        dd_local.RefreshShownValue();
    }

    private void addStartDropdownOption(string airfieldName)
    {
        dd_start.options.Add(new TMP_Dropdown.OptionData(airfieldName,null));
    }

    private void addDestDropdownOption(string airfieldName, GameObject destItem)
    {
        TMP_Dropdown dd_local = destItem.GetComponentInChildren<TMP_Dropdown>();
        dd_local.options.Add(new TMP_Dropdown.OptionData(airfieldName, null));
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

    // Changes a destination point coordinate fields based on destination dropdown input
    public void setDestPoint(TMP_Dropdown destDropdown)
    {
        int airfieldIndex = destDropdown.value;

        string airfieldLat = (m_areas[airfieldIndex]).lat;
        string airfieldLon = (m_areas[airfieldIndex]).lon;

        convertFileCoordinates(ref airfieldLat);
        convertFileCoordinates(ref airfieldLon);

        // Find input field objects
        Transform latChild = destDropdown.transform.parent.Find("inF_DestLat");
        if (latChild.TryGetComponent<TMP_InputField>(out TMP_InputField destLatField))
        {
            destLatField.text = airfieldLat;
        }

        Transform lonChild = destDropdown.transform.parent.Find("inF_DestLon");
        if (lonChild.TryGetComponent<TMP_InputField>(out TMP_InputField destLonField))
        {
            destLonField.text = airfieldLon;
        }
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

    public void addToRefList(GameObject destItem)
    {
        if (!m_destList.Contains(destItem))
        {
            m_destList.Add(destItem);
        }
    }

    public void removeFromRefList(GameObject destItem)
    {
        if (m_destList.Contains(destItem))
        {
            m_destList.Remove(destItem);
        }
    }

}
