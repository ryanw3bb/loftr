using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Loft : MonoBehaviour 
{
	public enum AXIS { X, Y, Z };

    public float Distance { get; private set; }
    public int VertCount { get { return mesh.vertexCount; } } 

	public AXIS TextureAxis;
	public Texture2D Texture;
	public int Segments;
	public List<LoftPointBezier> PathVerts;
	public List<Vector3> CrossVerts;

	private Mesh mesh;
	private new Renderer renderer;
	private new MeshCollider collider;
	private List<LoftPoint> segPoints;
	private Vector3 p0, p1, p2, p3;
    private bool update;

	private void Init()
	{
		if(mesh == null)
		{
			mesh = gameObject.GetOrAddComponent<MeshFilter>().sharedMesh = new Mesh();
		}

		if(renderer == null)
		{
			renderer = gameObject.GetOrAddComponent<MeshRenderer>();

            if(renderer.sharedMaterial.mainTexture == null)
            {
                renderer.sharedMaterial.mainTexture = Texture;
            }
		}

		if(collider == null)
		{
			collider = gameObject.GetOrAddComponent<MeshCollider>();
		}
	}

	private void LateUpdate()
	{
        if(update)
        {
            GenerateMesh();
        }
	}

	public void GenerateMesh()
	{
		if(PathVerts.IsNullOrEmpty()) { return; }

		Init();

		// estimation pass - get straight line distances between path points
		for(int i = 1; i < PathVerts.Count; i++)
		{
			float dist = Vector3.Distance(PathVerts[i].Position, PathVerts[i - 1].Position);
			PathVerts[i].Distance = PathVerts[i - 1].Distance + dist;
		}

		// set each segments position
		segPoints = new List<LoftPoint>();

		for(int s = 0; s < Segments; s++)
		{
			float segPos = PathVerts[PathVerts.Count - 1].Distance / (Segments - 1f) * s;

			for(int p = 1; p < PathVerts.Count; p++)
			{
				p0 = PathVerts[p - 1].Position;
				p1 = PathVerts[p - 1].P2;
				p2 = PathVerts[p].P0;
				p3 = PathVerts[p].Position;

                if((PathVerts[p].Distance >= segPos || Mathf.Approximately(PathVerts[p].Distance, segPos)) && 
                   (PathVerts[p - 1].Distance <= segPos || Mathf.Approximately(PathVerts[p - 1].Distance, segPos)))
				{
					float t = (segPos - (PathVerts[p - 1].Distance)) / (PathVerts[p].Distance - PathVerts[p - 1].Distance);

					Vector3 worldPos = Mathf.Pow(1f - t, 3) * p0 + 3 * Mathf.Pow(1f - t, 2) * t * p1 + 3 * (1f - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;

					float dist = segPoints.IsNullOrEmpty() ? 0 : segPoints.Last().Distance + Vector3.Distance(segPoints.Last().Position, worldPos);

					segPoints.Add(new LoftPoint(s, dist, worldPos));
				}
			}
		}

        Distance = segPoints.Last().Distance;

		// create verts for each segment
		List<Vector3> verts = new List<Vector3>();

		for(int i = 0; i < segPoints.Count; i++)
		{
			float yrot = 0f;

			if(i > 0 && i < segPoints.Count - 1)
			{
				yrot = Mathf.Atan2(segPoints[i + 1].Position.z - segPoints[i - 1].Position.z, segPoints[i + 1].Position.x - segPoints[i - 1].Position.x) * Mathf.Rad2Deg;
			}

			segPoints[i].Angle = new Vector3(0, -yrot, 0);

			for(int v = 0; v < CrossVerts.Count; v++)
			{
				Vector3 vertpos = Quaternion.Euler(0, -yrot, 0) * CrossVerts[v] + segPoints[i].Position;
				verts.Add(vertpos);
			}
		}

		// tris
		List<int> tris = new List<int>();

		for(int v = 0; v < CrossVerts.Count * (Segments - 1); v++)
		{
			// add triangles for new vert, 1 for first and last vert in segment
			// or 2 tris for the middle verts
			if(v % CrossVerts.Count == 0)
			{
				// first tri in row // v = 0 [ 3, 1, 0 ]
				tris.AddRange(new List<int>() { v + CrossVerts.Count, v + 1, v });
			}
			else if((v + 1) % CrossVerts.Count == 0) 
			{
				// last tri in row // v = 2 [ 5, 2, 4 ]
				tris.AddRange(new List<int>() { v + CrossVerts.Count, v, v + CrossVerts.Count - 1 }); 
			}
			else
			{
				// middle tris // v = 1 [ 4, 1, 3 ] // v = 1 [ 4, 2, 1 ]
				tris.AddRange(new List<int>() { v + CrossVerts.Count, v, v + CrossVerts.Count - 1 });
				tris.AddRange(new List<int>() { v + CrossVerts.Count, v + 1, v }); 
			}
		}

		// uvs
		List<Vector2> uvs = new List<Vector2>();
		float uvx, uvy;

		for(int v = 0; v < verts.Count; v++)
		{
			int seg = Mathf.FloorToInt(v / CrossVerts.Count);
			uvx = segPoints[seg].Distance / segPoints[segPoints.Count - 1].Distance;
			uvy = v % CrossVerts.Count;
			uvy = uvy / (CrossVerts.Count - 1);

			if(TextureAxis == AXIS.Z)
			{
				uvs.Add(new Vector2(uvx, uvy));
			}
			else if(TextureAxis == AXIS.X)
			{
				uvs.Add(new Vector2(uvy, uvx));
			}
		}

		mesh.Clear();
		mesh.vertices = verts.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.RecalculateNormals();

		collider.sharedMesh = null;
		collider.sharedMesh = mesh;

        update = false;
    }
		
	public Vector3 GetPositionOnPath(float alpha)
	{
		if(segPoints.IsNullOrEmpty()) { return Vector3.zero; }

		float distance = alpha * Distance;

		for(int p = 1; p < segPoints.Count; p++)
		{
			if((segPoints[p].Distance >= distance) && (segPoints[p - 1].Distance <= distance))
			{
				float t = (distance - segPoints[p - 1].Distance) / (segPoints[p].Distance - segPoints[p - 1].Distance);

				return Vector3.Lerp(segPoints[p - 1].Position, segPoints[p].Position, t);
			}
		}

		return Vector3.zero;
	}

	public Vector3 GetRotationOnPath(float alpha)
	{
		if(segPoints.IsNullOrEmpty()) { return Vector3.zero; }

		float distance = alpha * Distance;

		for(int p = 1; p < segPoints.Count - 1; p++)
		{
			if((segPoints[p + 1].Distance >= distance) && (segPoints[p - 1].Distance <= distance))
			{
				return segPoints[p].Angle;
			}
		}

		return Vector3.zero;
	}

	public Vector3 GetPositionOnPathBetween(int idx0, int idx1, float t = 0.5f)
	{
		Vector3 p0 = PathVerts[idx0].Position;
		Vector3 p1 = PathVerts[idx0].P2;
		Vector3 p2 = PathVerts[idx1].P0;
		Vector3 p3 = PathVerts[idx1].Position;

		Vector3 worldPos = Mathf.Pow(1f - t, 3) * p0 + 3 * Mathf.Pow(1f - t, 2) * t * p1 + 3 * (1f - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;

		return worldPos;
	}

	public void SetDirty()
	{
        update = true;
	}
}