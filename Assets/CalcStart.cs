using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CalcStart : MonoBehaviour
{
    // Start Lat/Lon coordinates in DEGREES
    private float startLocLat = 0.0f;
    private float startLocLon = 0.0f;

    // End Lat/Lon coordinates in DEGREES, soon to be deprecated
    private float destLocLat = 0.0f;
    private float destLocLon = 0.0f;
    
    private class DestCoordsItem // Used to store lat/lon pairs. The ID is to track which destination item it belongs to.
    {
        public float destLat;
        public float destLon;
        public int destID;
    }

    // Destination coordinate list
    private List<DestCoordsItem> destLocCoordList = new List<DestCoordsItem>();
    
    // Number of midpoints
    public int numPts = 0; // Soon to be deprecated
    public List<int[]> numPtsList = new List<int[]>(); // [0]: Destination item identifier, [1]: Destination number of points

    // Planet radius in km
    private float planetRadius = 6371.955f;

    // Conversion factor for degrees to radians
    private float radConv = Mathf.PI / 180.0f;

    // Distance between start and end points
    public float haversine_distance = 0.0f; // Consider making private list

    // Distance between midpoints
    public float haversine_dSplit = 0.0f; // Consider making private list

    // Initial bearing
    public float bearingStart = 0.0f;

    public List<float[]> coordMat = new List<float[]>();

    /* 
     * I have come to the terrifying realization that I might need to give each destination its own number of midpoints.
     * Otherwise, how would I split them up? What if I have two destinations and one of them is very close to the start or first destination?
     * If the number of midpoints for both are equal, there would be too many to be useful for short distances.
     */
    public void OnCalculateBtnClick()
    {
        coordMat.Clear();

        float localStartLat = startLocLat;
        float localStartLon = startLocLon;

        for (int j = 0; j < destLocCoordList.Count; j++)
        {
            float localDestLat = destLocCoordList[j].destLat;
            float localDestLon = destLocCoordList[j].destLon;
            int localNumPts = numPtsList[j][1];

            haversine_distance = CalcDistance(localDestLat, localDestLon, localStartLat, localStartLon);
            bearingStart = CalcBearing(localDestLat, localDestLon, localStartLat, localStartLon);
            haversine_dSplit = haversine_distance / (localNumPts + 1);

            /*
                For each checkpoint, project a point haversine_dSplit km in front of
                the vehicle from initial heading. From there, calculate heading to the 
                next point towards the destination.
            */

            float[] startRow = { localStartLat, localStartLon, bearingStart };
            coordMat.Add(startRow);

            float fraction = 1.0f / ((float)localNumPts + 1.0f);

            // Calculate midpoint coordinates and add them to the matrix. Calculate bearing in the next loop...
            for (int i = 1; i <= localNumPts; i++)
            {
                float[] interPtCoords = CalcInterPt(localStartLat, localStartLon, localDestLat, localDestLon, haversine_distance, fraction * i);

                // Bearing has not been added yet
                coordMat.Add(interPtCoords);
            }

            float[] destRow = { localDestLat, localDestLon, 0 };
            coordMat.Add(destRow);

            // Calculate bearing to midpoints and add them to the matrix.
            for (int i = 1; i < coordMat.Count - 1; i++)
            {
                float bearingToNext = CalcBearing((coordMat[i + 1])[0], (coordMat[i + 1])[1], (coordMat[i])[0], (coordMat[i])[1]);
                (coordMat[i])[2] = bearingToNext;
            }

            foreach (var item in coordMat)
            {
                Debug.Log(item[0].ToString() + ", " + item[1].ToString() + ", " + item[2].ToString());
            }

            // Update "start" location to current destination for next loop to use as a starting point
            localStartLat = localDestLat;
            localStartLon = localDestLon;
        }

        // We have the list of coordinates, now go through and delete any duplicates
        for (int k = 1; k < coordMat.Count; k++)
        {
            if ( (coordMat[k])[0] == (coordMat[k-1])[0] && (coordMat[k])[1] == (coordMat[k - 1])[1])
            {
                coordMat.RemoveAt(k-1);
            }
        }
    }


    // Calculate haversine distance between two points.
    private float CalcDistance(float destLat, float destLon, float currLat, float currLon)
    {
        float deltaLon = (destLon - currLon) * radConv;
        float deltaLat = (destLat - currLat) * radConv;

        float haversine_a = Mathf.Pow(Mathf.Sin(deltaLat / 2.0f), 2) + Mathf.Cos(currLat * radConv)*Mathf.Cos(destLat * radConv) * Mathf.Pow(Mathf.Sin(deltaLon / 2.0f), 2);
        float haversine_c = 2.0f * Mathf.Atan2(Mathf.Sqrt(haversine_a), Mathf.Sqrt(1.0f - haversine_a));

        float haversine_d = planetRadius * haversine_c;

        return haversine_d;
    }

    // Calculate bearing from one point to another on a sphere.
    private float CalcBearing(float destLat, float destLon, float currLat, float currLon)
    {
        float deltaLon = (destLon - currLon) * radConv;

        float x_val = Mathf.Cos(destLat * radConv) * Mathf.Sin(deltaLon);
        float y_val = Mathf.Cos(currLat * radConv) * Mathf.Sin(destLat * radConv) - Mathf.Sin(currLat * radConv) * Mathf.Cos(destLat * radConv) * Mathf.Cos(deltaLon);

        float bearing_out = Mathf.Atan2(x_val, y_val) * 180.0f / Mathf.PI; // convert back to degrees

        if (bearing_out < 0.0f)
            bearing_out = bearing_out + 360.0f;

        return bearing_out; 
    }

    // Project a midpoint a specified distance from the current point.
    private float[] CalcInterPt(float startLat, float startLon, float endLat, float endLon, float distance, float fraction)
    {
        // Convert lat/lon to radians.
        float latRad_st = startLat * radConv;
        float lonRad_st = startLon * radConv;
        float latRad_end = endLat * radConv;
        float lonRad_end = endLon * radConv;

        float angDist = distance / planetRadius;

        float a_inter = Mathf.Sin((1 - fraction) * angDist) / Mathf.Sin(angDist);
        float b_inter = Mathf.Sin(fraction * angDist) / Mathf.Sin(angDist);

        float x_inter = a_inter * Mathf.Cos(latRad_st) * Mathf.Cos(lonRad_st) + b_inter * Mathf.Cos(latRad_end) * Mathf.Cos(lonRad_end);
                                  
        float y_inter = a_inter * Mathf.Cos(latRad_st) * Mathf.Sin(lonRad_st)+b_inter * Mathf.Cos(latRad_end) * Mathf.Sin(lonRad_end);
                                  
        float z_inter = a_inter * Mathf.Sin(latRad_st) + b_inter * Mathf.Sin(latRad_end);

        float lat_inter = Mathf.Atan2( z_inter, Mathf.Sqrt( Mathf.Pow(x_inter, 2) + Mathf.Pow(y_inter,2) ) );
        float lon_inter = Mathf.Atan2( y_inter, x_inter );

        //convert back to degrees
        float lat_out = lat_inter / radConv;
        float lon_out = lon_inter / radConv;

        float[] coordsOut = { lat_out, lon_out, 0.0f }; // Haven't calculated bearing yet, leave it zero for now
        //coordsOut[0] = lat_out;
        //coordsOut[1] = lon_out;

        return coordsOut;
    }

    public void readLatStartInput(string inLatStart)
    {
        startLocLat = float.Parse(inLatStart);
        Debug.Log(startLocLat);
    }

    public void readLonStartInput(string inLonStart)
    {
        startLocLon = float.Parse(inLonStart);
        Debug.Log(startLocLon);
    }

    //public void readLatEndInput(string inLatEnd)
    //{
    //    destLocLat = float.Parse(inLatEnd);
    //    Debug.Log(destLocLat);
    //}

    //public void readLonEndInput(string inLonEnd)
    //{
    //    destLocLon = float.Parse(inLonEnd);
    //    Debug.Log(destLocLon);
    //}

    public void addDestCoordsItem(int idNum)
    {
        DestCoordsItem newCoordsItem = new DestCoordsItem();
        newCoordsItem.destID = idNum;
        newCoordsItem.destLon = 0.0f;
        newCoordsItem.destLat = 0.0f;
        destLocCoordList.Add(newCoordsItem);

        // Create a numPtsList entry defaulted to zero points
        updateNumPtsList(0, idNum);

        Debug.Log("Added new DestCoordsItem in CalcStart with id "+idNum);
    }

    public void updateDestItemLat(int idNum, float updatedLat)
    {
        for(int i=0;i<destLocCoordList.Count;i++)
        {
            if (idNum == destLocCoordList[i].destID)
            {
                destLocCoordList[i].destLat = updatedLat;
            }
        }
    }

    public void updateDestItemLon(int idNum, float updatedLon)
    {
        for (int i = 0; i < destLocCoordList.Count; i++)
        {
            if (idNum == destLocCoordList[i].destID)
            {
                destLocCoordList[i].destLon = updatedLon;
            }
        }
    }


    public void readNumPtsInput(string pointCount)
    {
        numPts = int.Parse(pointCount);
        Debug.Log(numPts);
    }

    public void updateNumPtsList(int numDestPts, int idNum)
    {
        // Somehow need to keep track of which entry belongs to which destination
        // Check if this entry already exists
        bool foundFlag = false;

        for(int i=0;i<numPtsList.Count;i++)
        {
            if (idNum == numPtsList[i][0]) // This entry already exists, just update its number of points
            {
                numPtsList[i][1] = numDestPts;
                foundFlag = true;
                //Debug.Log("CalcStart NumPts list item updated at index " + i + " (Item " + idNum + ")");
            }
        }
        // If this entry doesn't already exist, make one
        if (!foundFlag)
        {
            int[] arrayToAdd =new int[]{ idNum,numDestPts};
            numPtsList.Add(arrayToAdd);
            //Debug.Log("CalcStart NumPts list item created at index " + (numPtsList.Count-1) + "(Item " + idNum + ")");
        }
    }

    public void removeDestination(int idNum)
    {
        for (int i = 0; i < numPtsList.Count; i++)
        {
            if (idNum == numPtsList[i][0]) // Remove the entry when found. if it isn't found, do nothing because it doesn't exist
            {
                numPtsList.RemoveAt(i);
                Debug.Log("CalcStart NumPts list item removed at index " + i + " (Item " + idNum +")");
            }
        }

        // Check the coords item list, too
        for (int i = 0; i < destLocCoordList.Count; i++)
        {
            if (idNum == destLocCoordList[i].destID) // Remove the entry when found. if it isn't found, do nothing because it doesn't exist
            {
                destLocCoordList.RemoveAt(i);
                Debug.Log("CalcStart LocCoordList item removed at index " + i + " (Item " + idNum + ")");
            }
        }
    }
}
