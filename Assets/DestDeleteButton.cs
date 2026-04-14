using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements; // No idea why this got added when all it did was make Unity piss itself over what kind of button this is

public class DestDeleteButton : MonoBehaviour
{
    public Button deleteButton;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = deleteButton.GetComponent<Button>();
        btn.onClick.AddListener(DeleteOnClick);
    }

    // Update is called once per frame
    void DeleteOnClick()
    {
        // Get the parent GameObject this button belongs to
        GameObject parentObject = deleteButton.gameObject.transform.parent.gameObject;

        // Remove this destination from list of destinations
        // Go through each one...
        string destItemName = parentObject.name;
        string[] nameParts = destItemName.Split('_');
        // Second half should be a number
        if (int.TryParse(nameParts[1], out int destItemIndex))
        {
            // Remove this destination from CalcStart lists
            CalcStart localCS = FindAnyObjectByType<CalcStart>();
            localCS.removeDestination(destItemIndex);
        }
        

        // Remove this destination item from destination scrollview
        PopulateDestScrollView localPDSV = FindAnyObjectByType<PopulateDestScrollView>();
        localPDSV.removeDestItem(parentObject);

        // Remove this destination item from GetAirportData
        GetAirportData localGAD = FindAnyObjectByType<GetAirportData>();
        localGAD.removeFromRefList(parentObject);
        Destroy(parentObject);
    }
}
