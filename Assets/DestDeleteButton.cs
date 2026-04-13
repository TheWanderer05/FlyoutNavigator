using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
        // Remove this destination from list of destinations
        // Go through each one...
        
        // Remove this destination item from destination scrollview
        PopulateDestScrollView localPDSV = FindAnyObjectByType<PopulateDestScrollView>();
        localPDSV.removeDestItem(deleteButton.gameObject.transform.parent.gameObject);
    }
}
