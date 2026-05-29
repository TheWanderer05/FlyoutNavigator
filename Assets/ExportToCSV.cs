using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;


public class ExportToCSV : MonoBehaviour
{
    static readonly string exportPath = Application.dataPath;
    
    public void ExportPointsToCSV()
    {
        CalcStart localCS=FindObjectOfType<CalcStart>();
        if (localCS != null )
        {
            var localCoords = localCS.coordMat;
            //File.Create(exportPath+"/SavedPoints.csv"); // Should clear and overwrite the file every time it's "opened"
            string latOut = localCoords[0][0].ToString();
            string lonOut = localCoords[0][1].ToString();
            File.WriteAllText(exportPath + "/SavedPoints.csv", latOut + "," + lonOut + Environment.NewLine);

            for (int i = 1; i < localCoords.Count; i++)
            {
                latOut = localCoords[i][0].ToString();
                lonOut = localCoords[i][1].ToString();
                File.AppendAllText(exportPath + "/SavedPoints.csv",latOut + "," + lonOut + Environment.NewLine);
            }
            
        }
    }
}
