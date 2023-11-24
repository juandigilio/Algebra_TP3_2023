using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCollisionDetection : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    private bool debugVertex;
    [SerializeField]
    private bool debugVertexConnections;

    [Header("Grid Settings")]
    [SerializeField]
    private int gridWidth = 10;
    [SerializeField]
    private int gridHeight = 10;
    [SerializeField]
    private int gridDepth = 10;
    [SerializeField]
    private float gridSpacing = 1.0f;

    [Header("Variables")]
    [SerializeField]
    private Vector3[] grid;
    [SerializeField]
    private List<GameObject> objectsToChekcCollision;

    [Header("Gizmo Settings")]
    [SerializeField]
    private Color vertexColor = Color.green;
    [SerializeField]
    private float vertexSize = 0.1f;
    [SerializeField]
    private Color vertexConnectionColor = Color.gray;

    bool Vector3ToSimpleConvexModelCollision(Vector3 point, GameObject gameObject)
    {
        bool isInside = true;

        Transform objectTransform = gameObject.transform;
        Mesh objectMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        if (!objectMesh)
            return false;

        // Vamos a sacar las normales nosotros manualmente.
        for (int i = 0; i < objectMesh.triangles.Length; i += 3) // Recorremos los triangulos (o poligonos) de la mesh.
        {
            // Sacamos los 3 vertices del triangulo actual.
            Vector3 v1 = objectMesh.vertices[objectMesh.triangles[i]];
            Vector3 v2 = objectMesh.vertices[objectMesh.triangles[i + 1]];
            Vector3 v3 = objectMesh.vertices[objectMesh.triangles[i + 2]];

            // Transformamos los vertices de el triangulo de la mesh en relacion a la posicion/rotacion/escala del gameobject.
            v1 = objectTransform.TransformPoint(v1);
            v2 = objectTransform.TransformPoint(v2);
            v3 = objectTransform.TransformPoint(v3);

            // Calculamos las esquinas del triangulo
            Vector3 e1 = v2 - v1;
            Vector3 e2 = v3 - v1;

            // Sacamos la normal haciendo un producto cruz de las esquinas del triangulo.
            Vector3 normal = Vector3.Cross(e1, e2);

            // Sacamos un vector desde cualquier vertice del triangulo hasta el punto.
            Vector3 vp = point - v1;

            // Calculamos el producto punto de la normal y el vector.
            float dot = Vector3.Dot(normal, vp);

            // Si el resultado es negativo, significa que el punto esta fuera de la mesh.
            if (dot < 0)
            {
                isInside = false;
                break;
            }
        }
        return isInside;
    }

    /*
    bool Vector3ToSimpleConvexMeshCollision(Vector3 point, Mesh mesh, Transform objectTransform)
    {
        bool isInside = true;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 faceCenter = (mesh.vertices[mesh.triangles[i]] +
                                  mesh.vertices[mesh.triangles[i + 1]] +
                                  mesh.vertices[mesh.triangles[i + 2]]) / 3f;

            Vector3 currentNormal = objectTransform.TransformDirection(faceCenter + mesh.normals[mesh.triangles[i]]);

            float dot = Vector3.Dot(currentNormal, point);

            if (dot < 0)
            {
                isInside = false;
                break;
            }
        }
        return isInside;
    }
    */

    private void CreateGrid()
    {
        float xOffset = (gridWidth - 1) * gridSpacing / 2.0f;
        float yOffset = (gridHeight - 1) * gridSpacing / 2.0f;
        float zOffset = (gridDepth - 1) * gridSpacing / 2.0f;

        grid = new Vector3[gridWidth * gridHeight * gridDepth];

        Vector3 objectPosition = transform.position;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    float xPos = x * gridSpacing - xOffset + objectPosition.x;
                    float yPos = y * gridSpacing - yOffset + objectPosition.y;
                    float zPos = z * gridSpacing - zOffset + objectPosition.z;
                    Vector3 point = new Vector3(xPos, yPos, zPos);
                    grid[x * gridHeight * gridDepth + y * gridDepth + z] = point;
                }
            }
        }
    }

    private Vector3 GetGridPoint(int x, int y, int z)
    {
        float xOffset = (gridWidth - 1) * gridSpacing / 2.0f;
        float yOffset = (gridHeight - 1) * gridSpacing / 2.0f;
        float zOffset = (gridDepth - 1) * gridSpacing / 2.0f;

        float xPos = x * gridSpacing - xOffset + transform.position.x;
        float yPos = y * gridSpacing - yOffset + transform.position.y;
        float zPos = z * gridSpacing - zOffset + transform.position.z;

        return new Vector3(xPos, yPos, zPos);
    }

    private void DrawConnectionsAlongAxis(Vector3 axis)
    {
        for (int x = 0; x < gridWidth - (int)axis.x; x++)
        {
            for (int y = 0; y < gridHeight - (int)axis.y; y++)
            {
                for (int z = 0; z < gridDepth - (int)axis.z; z++)
                {
                    Vector3 start = GetGridPoint(x, y, z);
                    Vector3 end = GetGridPoint(x + (int)axis.x, y + (int)axis.y, z + (int)axis.z);

                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }

    private void DrawGizmos()
    {
        Matrix4x4 currentMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (debugVertex)
        {
            Gizmos.color = vertexColor;

            foreach (Vector3 vert in grid)
            {
                Gizmos.DrawSphere(vert, vertexSize);
            }
        }

        if (debugVertexConnections)
        {
            Gizmos.color = vertexConnectionColor;

            DrawConnectionsAlongAxis(Vector3.right);
            DrawConnectionsAlongAxis(Vector3.up);
            DrawConnectionsAlongAxis(Vector3.forward);
        }

        Gizmos.matrix = currentMatrix;
    }

    private void OnValidate()
    {
        CreateGrid();
    }

    private void Update()
    {
        if(objectsToChekcCollision.Count > 0)
        {
            foreach(GameObject obj in objectsToChekcCollision)
            {
                for (int i = 0; i < grid.Length; i++)
                {
                    if (Vector3ToSimpleConvexModelCollision(grid[i], obj))
                    {
                        Debug.Log(obj.name + " colliding with Vertex[" + i + "] from the grid.");
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (grid == null || grid.Length < 1)
            return;

        DrawGizmos();
    }

}
