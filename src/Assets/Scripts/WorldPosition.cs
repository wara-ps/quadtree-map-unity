using Combitech.Geographics;
using QuadTreeMapEngine;
using System.Collections;
using UnityEngine;

public class WorldPosition : MonoBehaviour
{
    public LatLng CurrentPosition { get; private set; } = new LatLng();

    private MapTileManager Map { get; set; }
    private MapProjection Projection { get; set; }
    private UtmCoord Origin { get; set; }
    private bool SetupComplete { get; set; }

    private void Start()
    {
        StartCoroutine(Setup());
    }

    private void Update()
    {
        if (!SetupComplete)
            return;

        CurrentPosition = XYZ2LatLng(transform.position);
    }

    private IEnumerator Setup()
    {
        SetupComplete = false;

        Map = FindObjectOfType<MapTileManager>();
        while (!Map.SetupComplete)
        {
            yield return null;
        }

        var proj = Map.Metadata.UtmProjection;
        Projection = new MapProjection(proj.Zone, proj.North);

        var point = Map.Metadata.Position - Map.Metadata.AnchorOffset;
        Origin = new UtmCoord(point.X, point.Y);

        SetupComplete = true;
    }

    /// <summary>
    /// Takes a WGS84 coordinate and converts it to a Vector3 using the
    /// configured UTM projection.
    /// </summary>
    /// <param name="latlng">A WGS84 latitude/longitude coordinate</param>
    /// <returns>A UTM projected coordinate (offset from Origin)</returns>
    public Vector3 LatLng2XYZ(LatLng latlng)
    {
        var utm = Projection.ToProjection(latlng);
        var x = (float)(utm.Easting - Origin.Easting);
        var z = (float)(utm.Northing - Origin.Northing);
        return Map.transform.TransformPoint(new Vector3(x, latlng.Height, z));
    }

    /// <summary>
    /// Takes a Vector3 and converts it to a WGS84 coordinate using the
    /// configured UTM projection.
    /// </summary>
    /// <param name="pos">A Unity world-space Vector3 coordinate</param>
    /// <returns>A WGS84 latitude/longitude coordinate</returns>
    public LatLng XYZ2LatLng(Vector3 pos)
    {
        pos = Map.transform.InverseTransformPoint(pos);
        var utm = new UtmCoord(pos.x + Origin.Easting, pos.z + Origin.Northing);
        var latlng = Projection.FromProjection(utm);
        latlng.Height = pos.y;
        return latlng;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), $"({CurrentPosition.Lat:0.######} N, {CurrentPosition.Lng:0.######} E)");
    }
}
