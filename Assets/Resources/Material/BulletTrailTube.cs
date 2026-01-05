using System.Collections.Generic;
using UnityEngine;

public class BulletTrailTube : MonoBehaviour
{
    public float radius = 0.05f;
    public int segments = 12;
    public Material tubeMat;

    private List<Vector3> points = new List<Vector3>();
    private Mesh tubeMesh;
    private GameObject tubeObj;
    private MeshFilter mf;
    private MeshRenderer mr;

    public void BeginTrail(Vector3 startPos)
    {
        points.Clear();
        points.Add(startPos);

        tubeObj = new GameObject("BulletTube");
        mf = tubeObj.AddComponent<MeshFilter>();
        mr = tubeObj.AddComponent<MeshRenderer>();

        mr.material = tubeMat;
        tubeMesh = new Mesh();
        mf.mesh = tubeMesh;
    }

    public void AddSegment(Vector3 p0, Vector3 p1)
    {
        if ((p1 - p0).sqrMagnitude < 0.0001f) return;

        points.Add(p1);
        UpdateMesh();
    }

    public void EndTrail()
    {
        // 遅延で削除して自然に残すのもOK
        GameObject.Destroy(tubeObj, 0.5f);
    }

    private void UpdateMesh()
    {
        if (points.Count < 2) return;

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = points[i];
            Vector3 p1 = points[i + 1];
            Vector3 dir = (p1 - p0).normalized;

            // 任意のUP方向
            Vector3 up = Vector3.up;
            if (Vector3.Dot(dir, up) > 0.9f)
                up = Vector3.right;

            Vector3 right = Vector3.Cross(dir, up).normalized;
            up = Vector3.Cross(right, dir).normalized;

            for (int j = 0; j <= segments; j++)
            {
                float angle = (float)j / segments * Mathf.PI * 2f;
                Vector3 offset = Mathf.Cos(angle) * right * radius +
                                 Mathf.Sin(angle) * up * radius;

                vertices.Add(p0 + offset);
                vertices.Add(p1 + offset);

                normals.Add(offset.normalized);
                normals.Add(offset.normalized);

                int baseIndex = (i * (segments + 1) + j) * 2;

                if (i < points.Count - 2)
                {
                    // 四角形 → 2 triangles
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);

                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 3);
                    triangles.Add(baseIndex + 2);
                }
            }
        }

        tubeMesh.Clear();
        tubeMesh.SetVertices(vertices);
        tubeMesh.SetNormals(normals);
        tubeMesh.SetTriangles(triangles, 0);
        tubeMesh.RecalculateBounds();
    }
}
