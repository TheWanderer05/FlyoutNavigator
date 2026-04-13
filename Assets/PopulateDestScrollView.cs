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
        // Get number of destination items present in the scrollview
        UInt16 numDests = (UInt16)m_destItems.Count;
        itemAdded.transform.name = "listDest_" + numDests; // Starts at zero as opposed to the navpts convention
        m_destItems.Add(itemAdded);
        GetAirportData localGAD = FindAnyObjectByType<GetAirportData>();
        localGAD.addToRefList(itemAdded);
        //Debug.Log("Destination item added.");
    }

    // This will most likely be moved to delete a specific item rather than cutting the tail of the list
    public void removeDestItem(GameObject itemToRemove)
    {
        m_destItems.Remove(itemToRemove);
        GetAirportData localGAD = FindAnyObjectByType<GetAirportData>();
        localGAD.removeFromRefList(itemToRemove);
        Destroy(itemToRemove);
    }

}
