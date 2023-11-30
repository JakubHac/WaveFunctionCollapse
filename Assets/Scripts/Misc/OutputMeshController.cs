using System;
using System.Threading.Tasks;
using UnityEngine;

public class OutputMeshController : MonoBehaviour
{
    [SerializeField] private Material Material;
    [SerializeField] private MeshRenderer Renderer;
    [SerializeField] private MeshFilter Filter;
    [SerializeField] private float MaxDistanceFromCenter;
    [SerializeField] private float MaxHeight;

    private int[,] triangleScheme = new int[12, 3]
    {
        //bottom
        {1,2,0},
        {1,3,2},
        //sides
        {1,0,4},
        {1,4,5},
        
        {3,1,5},
        {3,5,7},
        
        {2,3,7},
        {2,7,6},
        
        {0,2,6},
        {0,6,4},
        //top
        {4,6,7},
        {4,7,5}
    };
    
    
    private static int HeightMapID = Shader.PropertyToID("HeightMap");

    private ComputeBuffer buffer = null;
    private int count;

    private void SetBufferData(Array array)
    {
        buffer.SetData(array);
        Material.SetBuffer(HeightMapID, buffer);
    }

    private int PrepareBuffer()
    {
        int vertexCount = Filter.mesh.vertexCount;
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

        return vertexCount;
    }
    
    public void RefreshFromMaterialTexture()
    {
        Texture2D texture = Material.mainTexture as Texture2D;

        if (texture == null)
        {
            return;
        }

        var vertexCount = PrepareBuffer();

        Array array = new float[Filter.mesh.vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            if (i % 8 < 4)
            {
                array.SetValue(0f, i);
            }
            else
            {
                int place = Mathf.FloorToInt(i / 8f);
                int x = place % texture.width;
                int y = place / texture.width;
                float value = texture.GetPixel(x,y).a;
                array.SetValue(value * MaxHeight, i);
            }
            //array.SetValue(texture.GetPixel(x,y).a,i);
        }
        
        SetBufferData(array);
    }

    public void SetupMeshFromTexture()
    {
        Texture2D texture = Material.mainTexture as Texture2D;

        if (texture == null)
        {
            Debug.LogError("Texture is null");
            return;
        }
        
        int textureWidth = texture.width;
        int textureHeight = texture.height;
        float textureWidthAsFloat = textureWidth;
        float textureHeightAsFloat = textureHeight;
        
        var biggerDimension = Mathf.Max(textureWidth, textureHeight);
        var smallerDimension = Mathf.Min(textureWidth, textureHeight);
        var biggerDistance = MaxDistanceFromCenter;
        var smallerDistance = smallerDimension / (float)biggerDimension * biggerDistance;
        bool widthIsBigger = textureWidth >= textureHeight;
        var meshWidth = widthIsBigger ? biggerDistance : smallerDistance * 2f;
        var meshHeight = widthIsBigger ? smallerDistance : biggerDistance;
        var doubleMeshWidthStep = meshWidth / textureWidth;
        var doubleMeshHeightStep = meshHeight / textureHeight;
        float halfMeshWidth = meshWidth / 2f;
        float halfMeshHeight = meshHeight / 2f;

        if (Filter.mesh == null)
        {
            Filter.mesh = new Mesh();
        }
        var mesh = Filter.mesh;

        var vertices = new Vector3[texture.width * texture.height * 8];
        var UVs = new Vector2[vertices.Length];
        var triangles = new int[texture.width * texture.height * 36];
        
        Parallel.For(0, textureWidth, (x) =>
        {
            for (int y = 0; y < textureHeight; y++)
            {
                int vertexIndex = (x + y * textureWidth) * 8;
                int triangleIndex = (x + y * textureWidth) * 36;
                Vector2 UV = new Vector2(x / textureWidthAsFloat, y / textureHeightAsFloat);
                for (int i = 0; i < 2; i++)
                {
                    UVs[vertexIndex + 0] = UV;
                    UVs[vertexIndex + 1] = UV;
                    UVs[vertexIndex + 2] = UV;
                    UVs[vertexIndex + 3] = UV;
                    vertices[vertexIndex + 0] = new Vector3(x * doubleMeshWidthStep - halfMeshWidth, 0, y * doubleMeshHeightStep - halfMeshHeight);
                    vertices[vertexIndex + 1] = new Vector3((x + 1) * doubleMeshWidthStep - halfMeshWidth, 0, y * doubleMeshHeightStep - halfMeshHeight);
                    vertices[vertexIndex + 2] = new Vector3(x * doubleMeshWidthStep - halfMeshWidth, 0, (y + 1) * doubleMeshHeightStep - halfMeshHeight);
                    vertices[vertexIndex + 3] = new Vector3((x + 1) * doubleMeshWidthStep - halfMeshWidth, 0, (y + 1) * doubleMeshHeightStep - halfMeshHeight);
                    vertexIndex += 4;
                }

                for (int i = 0; i < triangleScheme.GetLength(0); i++)
                {
                    triangles[triangleIndex + 0] = vertexIndex - 8 + triangleScheme[i, 0];
                    triangles[triangleIndex + 1] = vertexIndex - 8 + triangleScheme[i, 1];
                    triangles[triangleIndex + 2] = vertexIndex - 8 + triangleScheme[i, 2];
                    triangleIndex += 3;
                }
            }
        });
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, UVs);
        Filter.mesh = mesh;

        RefreshFromMaterialTexture();
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
