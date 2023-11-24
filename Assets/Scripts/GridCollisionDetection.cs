using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GridCollisionDetection : MonoBehaviour
{
    [System.Serializable]
    private struct GridObject
    {
        public GameObject gObject;
        public List<Vector3> gCollisions;
    }

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
    private List<GridObject> objectsToChekcCollision;

    [Header("Gizmo Settings")]
    [SerializeField]
    private Color vertexColor = Color.green;
    [SerializeField]
    private float vertexSize = 0.1f;
    [SerializeField]
    private Color vertexConnectionColor = Color.gray;

    private bool CheckCollisionPointsInCommon(GridObject obj1, GridObject obj2)
    {
        foreach (Vector3 collisionPoint in obj1.gCollisions)
        {
            if (obj2.gCollisions.Contains(collisionPoint))
            {
                return true;
            }
        }
        return false;
    }

    bool Vector3ToSimpleConvexModelCollision(Vector3 point, GameObject gameObject)
    {
        bool isInside = true;

        Transform objectTransform = gameObject.transform;
        Mesh objectMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        if (!objectMesh)
            return false;

        // Recorremos los triangulos (o poligonos) de la mesh.
        for (int i = 0; i < objectMesh.triangles.Length; i += 3)
        {
            // Obtenemos los 3 vertices del triangulo/poligono actual.
            Vector3 v1 = objectMesh.vertices[objectMesh.triangles[i]];
            Vector3 v2 = objectMesh.vertices[objectMesh.triangles[i + 1]];
            Vector3 v3 = objectMesh.vertices[objectMesh.triangles[i + 2]];

            // Transformamos los vertices de el triangulo de la mesh en relacion a la posicion/rotacion/escala del gameobject.
            v1 = objectTransform.TransformPoint(v1);
            v2 = objectTransform.TransformPoint(v2);
            v3 = objectTransform.TransformPoint(v3);

            // Calculamos las aristas del triangulo
            Vector3 e1 = v2 - v1;
            Vector3 e2 = v3 - v1;

            // Calculamos la normal del triangulo mediante el producto cruz de sus aristas.
            Vector3 normal = Vector3.Cross(e1, e2);

            // Calculamos el vector desde cualquier vertice del triangulo hasta el punto.
            Vector3 vp = point - v1;

            // Calcula el producto punto entre la normal y el vector.
            float dot = Vector3.Dot(normal, vp);

            // Si el resultado es negativo, el punto esta fuera de la malla. (Recordar que solo funciona con una malla convexa)
            if (dot < 0)
            {
                isInside = false;
                break;
            }
        }
        return isInside;
    }

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

    private void Start()
    {
        Debug.LogWarning("Para comprobar colisiones mover manualmente las entidades desde el editor.");
    }

    /*
    private void Update()
    {
        if(objectsToChekcCollision.Count > 1)
        {
            foreach(GridObject gObj in objectsToChekcCollision)
            {
                GameObject obj = gObj.gObject;
                for (int i = 0; i < grid.Length; i++)
                {
                    if (Vector3ToSimpleConvexModelCollision(grid[i], obj))
                    {
                        if (!gObj.gCollisions.Contains(grid[i]))
                        {
                            gObj.gCollisions.Add(grid[i]);
                        }
                    }
                    else 
                    {
                        if (gObj.gCollisions.Contains(grid[i]))
                        {
                            gObj.gCollisions.Remove(grid[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < objectsToChekcCollision.Count; i++)
            {
                for (int j = i + 1; j < objectsToChekcCollision.Count; j++)
                {
                    GridObject obj1 = objectsToChekcCollision[i];
                    GridObject obj2 = objectsToChekcCollision[j];

                    if (CheckCollisionPointsInCommon(obj1, obj2))
                    {
                        Debug.Log(obj1.gObject.name + " is colliding with " + obj2.gObject.name + "!!!");
                    }
                }
            }
        }
    }
    */

    private void Update()
    {
        if (objectsToChekcCollision.Count > 1)
        {
            for (int i = 0; i < grid.Length; i++)
            {
                foreach (GridObject gObj in objectsToChekcCollision)
                {
                    GameObject obj = gObj.gObject;
                    if (Vector3ToSimpleConvexModelCollision(grid[i], obj))
                    {
                        if (!gObj.gCollisions.Contains(grid[i]))
                        {
                            gObj.gCollisions.Add(grid[i]);
                        }
                    }
                    else
                    {
                        if (gObj.gCollisions.Contains(grid[i]))
                        {
                            gObj.gCollisions.Remove(grid[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < objectsToChekcCollision.Count; i++)
            {
                for (int j = i + 1; j < objectsToChekcCollision.Count; j++)
                {
                    GridObject obj1 = objectsToChekcCollision[i];
                    GridObject obj2 = objectsToChekcCollision[j];

                    if (CheckCollisionPointsInCommon(obj1, obj2))
                    {
                        Debug.Log(obj1.gObject.name + " is colliding with " + obj2.gObject.name + "!!!");
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
