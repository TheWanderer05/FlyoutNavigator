using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateDestScrollView : MonoBehaviour
{
    [SerializeField] private Transform m_ContentContainer;
    [SerializeField] private GameObject m_destItem;
    [SerializeField] private int m_ItemCount;

    public List<GameObject> m_destItems = new List<GameObject>();
    private int grossItems = 0; // Tracks total number of destination items that have existed. DO NOT DECREASE OR RESET. This is used in the item name, which is critical for bookkeeping!


    // Start is called before the first frame update
    void Start()
    {
        addDestItem();
    }

    public void addDestItem()
    {
        var itemAdded = Instantiate(m_destItem);
        // Set item's parent to container
        itemAdded.transform.SetParent(m_ContentContainer);
        // Reset item's scale to one
        itemAdded.transform.localScale = Vector2.one;
        // Get total number of destination items that have existed in the scrollview
        itemAdded.transform.name = "listDest_" + grossItems; // Starts at zero as opposed to the navpts convention
        m_destItems.Add(itemAdded);
        GetAirportData localGAD = FindAnyObjectByType<GetAirportData>();
        localGAD.addToRefList(itemAdded);

        // NEED TO ADD CALCSTART DESTINATION ADDITION WITH DEFAULTED COORDINATE ENTRIES
        CalcStart localCS = FindAnyObjectByType<CalcStart>();
        localCS.addDestCoordsItem(grossItems);


        grossItems += 1; // Increase number of items that have existed.
        //Debug.Log("Destination item added.");
    }

    // This will most likely be moved to delete a specific item rather than cutting the tail of the list
    public void removeDestItem(GameObject itemToRemove)
    {
        m_destItems.Remove(itemToRemove);
        // Why was this even in here instead of the delete button
        //GetAirportData localGAD = FindAnyObjectByType<GetAirportData>();
        //localGAD.removeFromRefList(itemToRemove);
        //Destroy(itemToRemove);
    }

}
