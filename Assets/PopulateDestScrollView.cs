using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateDestScrollView : MonoBehaviour
{
    [SerializeField] private Transform m_ContentContainer;
    [SerializeField] private GameObject m_destItem;
    [SerializeField] private int m_ItemCount;

    public List<GameObject> destItems = new List<GameObject>();

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
    }

    // This will most likely be moved to to delete a specific item rather than cutting the tail of the list
    public void removeDestItem()
    {

    }

}
