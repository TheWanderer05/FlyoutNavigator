using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PopulateCoordScrollView : MonoBehaviour
{
    private RectTransform m_ContentContainerCoords;
    private RectTransform m_ContentContainerDists;
    [SerializeField] private GameObject m_coordItem;
    [SerializeField] private GameObject m_distItem;

    private CalcStart calcStart;
    //private List<float[]> coordMat_local = new List<float[]>();

    void Start()
    {
        // There's probably a better way of finding a specific object in a given scene, but I only have one scene and these objects are persistent
        ScrollRect coordsTable = GameObject.Find("Scroll View_Coords").GetComponent<ScrollRect>();
        m_ContentContainerCoords = coordsTable.content;

        ScrollRect distsTable = GameObject.Find("Scroll View_Dists").GetComponent<ScrollRect>();
        m_ContentContainerDists = distsTable.content;
    }

    public void PopulateTable()
    {
        ClearTable();
        List<float[]> coordMat_local = new List<float[]>();
        coordMat_local.Clear();
        
        calcStart = FindObjectOfType<CalcStart>();
        int coordItemCount = calcStart.coordMat.Count;
        coordMat_local = calcStart.coordMat;
        
        for (int i = 0; i < coordItemCount; i++)   // number of rows
        {
            var item_go = Instantiate(m_coordItem);

            // Replace the item text with relevant fields
            var latText = item_go.transform.Find("LatText");
            latText.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0:0.0000}", ((coordMat_local[i])[0]));

            var lonText = item_go.transform.Find("LonText");
            lonText.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0:0.0000}", ((coordMat_local[i])[1]));

            var brgText = item_go.transform.Find("BrgText");
            brgText.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0:0.0}", ((coordMat_local[i])[2]));

            var idText = item_go.transform.Find("IDText");
            idText.GetComponent<TMPro.TextMeshProUGUI>().text = i.ToString();

            // set item's parent to content container
            item_go.transform.SetParent(m_ContentContainerCoords);
            // reset item's scale
            item_go.transform.localScale = Vector2.one;
        }

        // Populate distance table
        int distItemCount = calcStart.locationCount;

        for (int j=0; j<distItemCount;j++)
        {
            var item_dist = Instantiate(m_distItem);

            var routeIDText = item_dist.transform.Find("RouteIDText");
            routeIDText.GetComponent<TMPro.TextMeshProUGUI>().text = j.ToString();

            var totalDistText = item_dist.transform.Find("TotalDistText");
            if (calcStart.haversineDistList.Count > 0)
            {
                totalDistText.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0:0.0}", ((calcStart.haversineDistList[j])));
            }
            else // If there are no destinations in the list, say so.
            {
                totalDistText.GetComponent<TMPro.TextMeshProUGUI>().text = "Invalid: No destinations";
            }

            var splitDistText = item_dist.transform.Find("SplitDistText");
            if (calcStart.haversineDistList.Count > 0)
            {
                splitDistText.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0:0.0}", ((calcStart.distSplitList[j])));
            }
            else // If there are no destinations in the list, say so.
            {
                splitDistText.GetComponent<TMPro.TextMeshProUGUI>().text = "Invalid: No destinations";
            }

            // set item's parent to content container
            item_dist.transform.SetParent(m_ContentContainerDists);
            // reset item's scale
            item_dist.transform.localScale = Vector2.one;
        }
    }

    private void ClearTable()
    {
        for (var i = m_ContentContainerCoords.childCount - 1; i > 0; i--)
        {
            Object.Destroy(m_ContentContainerCoords.transform.GetChild(i).gameObject);
        }

        for (var j=m_ContentContainerDists.childCount - 1; j > 0; j--)
        {
            Object.Destroy(m_ContentContainerDists.transform.GetChild(j).gameObject);
        }
    }
}
