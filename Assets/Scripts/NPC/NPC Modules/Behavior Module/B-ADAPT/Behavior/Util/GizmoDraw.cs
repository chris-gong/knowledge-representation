using UnityEngine;
using System.Collections;

public class GizmoDraw : MonoBehaviour 
{
    private static Vector3[] cylVerts = null;
    private static int[] cylTris = null;

    /// <summary>
    /// Draws a gizmo cylinder with the given TRS matrix and color
    /// </summary>
    /// <param name="trs"></param>
    /// <param name="color"></param>
    public static void DrawCylinder(Matrix4x4 trs, Color color)
    {
        if (cylVerts == null || cylTris == null)
        {
            GameObject cyl = GameObject.CreatePrimitive(
                PrimitiveType.Cylinder);
            MeshFilter filter = cyl.GetComponent<MeshFilter>();
            cylVerts = filter.sharedMesh.vertices;
            cylTris = filter.sharedMesh.triangles;
            GameObject.DestroyImmediate(cyl);
        }

        Vector3[] verts = new Vector3[cylVerts.Length];
        for (int i = 0; i < cylVerts.Length; i++)
            verts[i] = trs.MultiplyPoint(cylVerts[i]);

        Gizmos.color = color;
        for (int i = 0; i < cylTris.Length / 3; i++)
        {
            int j = i * 3;
            Gizmos.DrawLine(verts[cylTris[j]],
                verts[cylTris[j + 1]]);
            Gizmos.DrawLine(verts[cylTris[j + 1]],
                verts[cylTris[j + 2]]);
            Gizmos.DrawLine(verts[cylTris[j + 2]],
                verts[cylTris[j]]);
        }
    }

    /// <summary>
    /// Draws out the hierarchy of an object with its children
    /// </summary>
    /// <param name="root"></param>
    /// <param name="color"></param>
    public static void DrawHierarchy(Transform root, Color color)
    {
        foreach (Transform child in root)
        {
            Debug.DrawLine(root.position, child.position, color);
            DrawHierarchy(child, color);
        }
    }
}
