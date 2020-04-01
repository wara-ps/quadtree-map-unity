using UnityEngine;

public class WorldAxes : MonoBehaviour
{
    [SerializeField]
    private Material AxisMaterial;

    protected void Start()
    {
        CreateCube(new Vector3(1000, 1, 1), Color.red);
        CreateCube(new Vector3(1, 1000, 1), Color.green);
        CreateCube(new Vector3(1, 1, 1000), Color.blue);
    }

    private void CreateCube(Vector3 size, Color color)
    {
        var x = GameObject.CreatePrimitive(PrimitiveType.Cube);
        x.transform.localScale = size;

        var renderer = x.GetComponent<Renderer>();
        renderer.material = new Material(AxisMaterial);
        renderer.material.color = color;
    }
}
