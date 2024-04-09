using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateScrollView : MonoBehaviour
{
    [SerializeField] private Transform m_ContentContainer;
    [SerializeField] private GameObject m_ItemPrefab;
    [SerializeField] private int m_ItemCount;
    [SerializeField] private GameObject m_distData;

    private CalcStart calcStart;
    //private List<float[]> coordMat_local = new List<float[]>();

    public void PopulateTable()
    {
        ClearTable();
        List<float[]> coordMat_local = new List<float[]>();
        coordMat_local.Clear();
        
        calcStart = FindObjectOfType<CalcStart>();
        m_ItemCount = calcStart.coordMat.Count;
        coordMat_local = calcStart.coordMat;
        
        for (int i = 0; i < m_ItemCount; i++)   // number of rows
        {
            var item_go = Instantiate(m_ItemPrefab);

            // Replace the item text with relevant fields
            var latText = item_go.transform.Find("LatText");
            latText.GetComponent<TMPro.TextMeshProUGUI>().text = ((coordMat_local[i])[0]).ToString();

            var lonText = item_go.transform.Find("LonText");
            lonText.GetComponent<TMPro.TextMeshProUGUI>().text = ((coordMat_local[i])[1]).ToString();

            var brgText = item_go.transform.Find("BrgText");
            brgText.GetComponent<TMPro.TextMeshProUGUI>().text = ((coordMat_local[i])[2]).ToString();


            // set item's parent to content container
            item_go.transform.SetParent(m_ContentContainer);
            // reset item's scale
            item_go.transform.localScale = Vector2.one;
        }

        // Populate distance table
        var item = m_distData;
        var totalDistText = item.transform.Find("totalDist");
        totalDistText.GetComponent<Text>().text = calcStart.haversine_distance.ToString();

        var pointDistText = item.transform.Find("pointDist");
        pointDistText.GetComponent<Text>().text = calcStart.haversine_dSplit.ToString();
    }

    private void ClearTable()
    {
        for (var i = m_ContentContainer.childCount - 1; i > 0; i--)
        {
            Object.Destroy(m_ContentContainer.transform.GetChild(i).gameObject);
        }
    }
}
