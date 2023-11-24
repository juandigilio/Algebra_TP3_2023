using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDebug : MonoBehaviour
{
    [Header("Debug")]

    [SerializeField]
    private bool debugVertex;
    [SerializeField]
    private bool debugVertexConnections;
    [SerializeField]
    private bool debugNormals;

    [Header("Variables")]
    [SerializeField]
    private Mesh modelMesh;

    [Header("Gizmo Settings")]
    [SerializeField]
    private Color vertexColor = Color.red;
    [SerializeField]
    private float vertexSize = 0.1f;
    [SerializeField]
    private Color normalColor = Color.cyan;
    [SerializeField]
    private float normalLength = 0.2f;
    [SerializeField]
    private Color vertexConnectionColor = Color.white;

    private void OnValidate()
    {
        modelMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
    }

    private void OnDrawGizmos()
    {
        if (!modelMesh) 
            return;

        Matrix4x4 currentMatrix = Gizmos.matrix;

        Gizmos.matrix = transform.localToWorldMatrix;

        if (debugVertex)
        {
            Gizmos.color = vertexColor;

            foreach (Vector3 vert in modelMesh.vertices)
            {
                Gizmos.DrawSphere(vert, vertexSize);
            }
        }


        if (debugVertexConnections)
        {
            Gizmos.color = vertexConnectionColor;

            for (int i = 0; i < modelMesh.vertices.Length - 1; i++)
            {
                Gizmos.DrawLine(modelMesh.vertices[i], modelMesh.vertices[i + 1]);
            }

            Gizmos.DrawLine(modelMesh.vertices[modelMesh.vertices.Length - 1], modelMesh.vertices[0]);
        }

        if (debugNormals)
        {
            Gizmos.color = normalColor;

            for (int i = 0; i < modelMesh.triangles.Length; i += 3)
            {
                Vector3 faceCenter = (modelMesh.vertices[modelMesh.triangles[i]] +
                                      modelMesh.vertices[modelMesh.triangles[i + 1]] +
                                      modelMesh.vertices[modelMesh.triangles[i + 2]]) / 3f;

                Gizmos.DrawLine(faceCenter, faceCenter + modelMesh.normals[modelMesh.triangles[i]] * normalLength);
            }
        }

        Gizmos.matrix = currentMatrix;
    }

}
