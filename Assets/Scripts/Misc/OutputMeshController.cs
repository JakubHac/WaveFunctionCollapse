using System;
using UnityEngine;

public class OutputMeshController : MonoBehaviour
{
    [SerializeField] private Material Material;
    [SerializeField] private MeshRenderer Renderer;
    [SerializeField] private MeshFilter Filter;
    private static int HeightMapID = 0;

    [SerializeField] private Texture2D TestTexture;

    private ComputeBuffer buffer = null;
    private int count;


    private void Awake()
    {
        HeightMapID = Shader.PropertyToID("HeightMap");
    }

    private void Start()
    {
        TestWithTexture(TestTexture);
    }

    private void TestWithSin()
    {
        int vertexCount = Filter.sharedMesh.vertexCount;
        if (buffer != null)
        {
            if (count != vertexCount)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        if (buffer == null)
        {
            buffer = new(vertexCount, sizeof(float));
            count = vertexCount;
        }
        Array array = new float[Filter.sharedMesh.vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            array.SetValue(Mathf.Sin(i + Time.realtimeSinceStartup),i);
        }
        buffer.SetData(array);
        Material.SetBuffer(HeightMapID, buffer);
        //buffer.Dispose();
    }

    private void TestWithTexture(Texture2D testTexture)
    {
        
    }

    private void OnDestroy()
    {
        if (buffer != null)
        {
            buffer.Dispose();
            buffer = null;
        }
    }
}
