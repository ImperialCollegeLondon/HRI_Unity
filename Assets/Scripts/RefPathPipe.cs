using UnityEngine;
using System.Collections;

public class RefPathPipe : MonoBehaviour
{

    public float curveRadius, pipeRadius;
    public int curveSegmentCount, pipeSegmentCount;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3 point, TranslatePoint;
    private Vector3 translate_p;

    private Vector3 GetPointOnTorus(float u, float v)
    {
        Vector3 p;
        float r = (curveRadius + pipeRadius * Mathf.Cos(v));
        p.x = r * Mathf.Sin(u);
        p.y = r * Mathf.Cos(u);
        p.z = pipeRadius * Mathf.Sin(v);
        return p+ translate_p;
    }

    public void setTransalte(Vector3 in_)
    {
        translate_p = in_;
    }

    private void OnDrawGizmos()
    {

        float uStep = (2f * Mathf.PI) / curveSegmentCount;
        float vStep = (2f * Mathf.PI) / pipeSegmentCount;

        for (int u = 0; u < curveSegmentCount; u++)
        {
            for (int v = 0; v < pipeSegmentCount; v++)
            {
                point = GetPointOnTorus(u * uStep, v * vStep);
               
				Gizmos.color = new Color(0.5f, (float)v / pipeSegmentCount, (float)u / curveSegmentCount);
                Gizmos.DrawSphere(point, 0.1f);
                
            }
        }
    }

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "RefPathPipe";

        SetVertices();
        SetTriangles();
        mesh.RecalculateNormals();
    }

    private void SetVertices()
    {
        vertices = new Vector3[pipeSegmentCount * curveSegmentCount * 4];
        float uStep = (2f * Mathf.PI) / curveSegmentCount;
        CreateFirstQuadRing(uStep);
        int iDelta = pipeSegmentCount * 4;
        for (int u = 2, i = iDelta; u <= curveSegmentCount; u++, i += iDelta)
        {
            CreateQuadRing(u * uStep, i);
        }
        mesh.vertices = vertices;
    }

    private void SetTriangles()
    {
        triangles = new int[pipeSegmentCount * curveSegmentCount * 6];
        for (int t = 0, i = 0; t < triangles.Length; t += 6, i += 4)
        {
            triangles[t] = i;
            triangles[t + 1] = triangles[t + 4] = i + 1;
            triangles[t + 2] = triangles[t + 3] = i + 2;
            triangles[t + 5] = i + 3;
        }
        mesh.triangles = triangles;
    }

    private void CreateFirstQuadRing(float u)
    {
        float vStep = (2f * Mathf.PI) / pipeSegmentCount;

        Vector3 vertexA = GetPointOnTorus(0f, 0f);
        Vector3 vertexB = GetPointOnTorus(u, 0f);
        for (int v = 1, i = 0; v <= pipeSegmentCount; v++, i += 4)
        {
            vertices[i] = vertexA;
            vertices[i + 1] = vertexA = GetPointOnTorus(0f, v * vStep);
            vertices[i + 2] = vertexB;
            vertices[i + 3] = vertexB = GetPointOnTorus(u, v * vStep);
        }
    }

    private void CreateQuadRing(float u, int i)
    {
        float vStep = (2f * Mathf.PI) / pipeSegmentCount;
        int ringOffset = pipeSegmentCount * 4;

        Vector3 vertex = GetPointOnTorus(u, 0f);
        for (int v = 1; v <= pipeSegmentCount; v++, i += 4)
        {
            vertices[i] = vertices[i - ringOffset + 2];
            vertices[i + 1] = vertices[i - ringOffset + 3];
            vertices[i + 2] = vertex;
            vertices[i + 3] = vertex = GetPointOnTorus(u, v * vStep);
        }
    }

    public void set6DPose(Vector3 Pos, Vector4 Quat)
    {
        setTransalte(Pos);
        transform.Translate(Pos.x, Pos.y, Pos.z);
        transform.rotation.Set(Quat.x, Quat.y, Quat.z, Quat.w);
    }

    public void setPosition(Vector3 Pos)
    {
       setTransalte(Pos);
       transform.Translate(Pos.x, Pos.y, Pos.z);
    }
    // Use this for initialization
    void Start()
    {
        setTransalte(this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        mesh.RecalculateNormals();
    }
}