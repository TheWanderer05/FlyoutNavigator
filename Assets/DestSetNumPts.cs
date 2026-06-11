using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DestSetNumPts : MonoBehaviour
{
    public TMP_InputField numField;
    // Start is called before the first frame update
    void Start()
    {
        TMP_InputField field = numField.GetComponent<TMP_InputField>();
        field.onEndEdit.AddListener(delegate { setNumMidPts(numField); });
    }

    void setNumMidPts(TMP_InputField numField)
    {
        if (int.TryParse(numField.text, out int numDestPts))
        {
            string destItemName = numField.transform.parent.name;
            string[] nameParts = destItemName.Split('_');
            // Second half should be a number
            if (int.TryParse(nameParts[1], out int destItemIndex))
            {
                CalcStart localCS = FindAnyObjectByType<CalcStart>();
                localCS.updateNumPtsList(numDestPts, destItemIndex);
                
                //Debug.Log("Dest item " + destItemIndex + " updated numPts: " + numDestPts);
            }
            
        }

        
    }
}
