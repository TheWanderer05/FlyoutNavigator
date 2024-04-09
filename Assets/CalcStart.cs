using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CalcStart : MonoBehaviour
{
    // Start Lat/Lon coordinates in DEGREES
    public float startLocLat = 0.0f;
    public float startLocLon = 0.0f;
    
    // End Lat/Lon coordinates in DEGREES
    public float destLocLat = 0.0f;
    public float destLocLon = 0.0f;
    
    // Number of midpoints
    public int numPts = 0;

    // Planet radius in km
    private float planetRadius = 6371.955f;

    // Conversion factor for degrees to radians
    private float radConv = Mathf.PI / 180.0f;

    // Distance between start and end points
    public float haversine_distance = 0.0f;

    // Distance between midpoints
    public float haversine_dSplit = 0.0f;

    // Initial bearing
    public float bearingStart = 0.0f;

    public List<float[]> coordMat = new List<float[]>();

    public void OnCalculateBtnClick()
    {
        haversine_distance = CalcDistance( destLocLat, destLocLon, startLocLat, startLocLon );
        bearingStart = CalcBearing( destLocLat, destLocLon, startLocLat, startLocLon );
        haversine_dSplit = haversine_distance / (numPts + 1);

        /*
            For each checkpoint, project a point haversine_dSplit km in front of
            the vehicle from initial heading. From there, calculate heading to the 
            next point towards the destination.
        */

        // Establish matrix dimensions (Lat/Lon/Dist/Bearing = 3 wide, Height = Start + Midpoints + end, so midpoints + 2)
        //int coordMatHeight = numPts + 2;
        //int coordMatWidth = 4;

        coordMat.Clear();

        //float[,] coordMat = new float[coordMatWidth, coordMatHeight];
        float[] startRow = { startLocLat, startLocLon, bearingStart};
        coordMat.Add(startRow);

        float fraction = 1.0f / ((float)numPts + 1.0f);

        // Calculate midpoint coordinates and add them to the matrix. Calculate bearing in the next loop...
        for (int i = 1; i <= numPts; i++)
        {
            float[] interPtCoords = CalcInterPt(startLocLat, startLocLon, destLocLat, destLocLon, haversine_distance, fraction*i);

            // Bearing has not been added yet
            coordMat.Add(interPtCoords);
        }

        float[] destRow = { destLocLat, destLocLon, 0 };
        coordMat.Add(destRow);

        // Calculate bearing to midpoints and add them to the matrix.
        for (int i = 1; i < coordMat.Count - 1; i++)
        {
            float bearingToNext = CalcBearing((coordMat[i + 1])[0], (coordMat[i+1])[1], (coordMat[i])[0], (coordMat[i])[1]);
            (coordMat[i])[2] = bearingToNext;
        }

        foreach (var item in coordMat)
        {
            Debug.Log(item[0].ToString() + ", " + item[1].ToString() + ", " + item[2].ToString());
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

    public void readLatEndInput(string inLatEnd)
    {
        destLocLat = float.Parse(inLatEnd);
        Debug.Log(destLocLat);
    }

    public void readLonEndInput(string inLonEnd)
    {
        destLocLon = float.Parse(inLonEnd);
        Debug.Log(destLocLon);
    }

    public void readNumPtsInput(string pointCount)
    {
        numPts = int.Parse(pointCount);
        Debug.Log(numPts);
    }
}
