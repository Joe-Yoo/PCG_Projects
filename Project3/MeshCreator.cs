using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static MeshEditor;
using static MeshCoordinates;

public class Cube : MonoBehaviour {	

	public int seed = 903602417; // seed: my gtid lol
	
	private float u = 1f;
	void Start() {
		Random.InitState(seed);
		(List<Quad>, List<Edge>) mesh_ = Create(Test_1());
		
		List<Quad> subdivided = CatmullClark(mesh_.Item1, mesh_.Item2);
		Mesh mesh = CreateMesh(subdivided);

		// Mesh mesh = CreateMesh(mesh_.Item1);

		GameObject m = new GameObject("Cube_");
		m.AddComponent<MeshFilter>();
		m.AddComponent<MeshRenderer>();
		m.GetComponent<MeshFilter>().mesh = mesh;
		m.transform.position = new Vector3(0, 0, 0);
		Renderer rend_ = m.GetComponent<Renderer>();
		Color c = new Color(Random.value, Random.value, Random.value, 1f);
		Material mat = new Material(Shader.Find("Standard"));
		mat.color = c;
		mat.SetInt("_CullMode", (int)CullMode.Off);
		rend_.material = mat;
	}

	(List<Quad>, List<Edge>) CreateCube() {

		List<Quad> quads = new List<Quad>();
		List<Edge> edges = new List<Edge>();

		Vector3[] verts_ = new Vector3[24];

		verts_[0] = new Vector3 ( 1, -1, -1);
		verts_[1] = new Vector3 ( 1, -1,  1);
		verts_[2] = new Vector3 (-1, -1,  1);
		verts_[3] = new Vector3 (-1, -1, -1);

		verts_[4] = new Vector3 (-1,  1, -1);
		verts_[5] = new Vector3 (-1,  1,  1);
		verts_[6] = new Vector3 ( 1,  1,  1);
		verts_[7] = new Vector3 ( 1,  1, -1);

		verts_[8]  = new Vector3 (-1,  1,  1);
		verts_[9]  = new Vector3 (-1,  1, -1);
		verts_[10] = new Vector3 (-1, -1, -1);
		verts_[11] = new Vector3 (-1, -1,  1);

		verts_[12] = new Vector3 ( 1,  1,  1);
		verts_[13] = new Vector3 (-1,  1,  1);
		verts_[14] = new Vector3 (-1, -1,  1);
		verts_[15] = new Vector3 ( 1, -1,  1);

		verts_[16] = new Vector3 ( 1,  1, -1);
		verts_[17] = new Vector3 ( 1,  1,  1);
		verts_[18] = new Vector3 ( 1, -1,  1);
		verts_[19] = new Vector3 ( 1, -1, -1);

		verts_[20] = new Vector3 (-1,  1, -1);
		verts_[21] = new Vector3 ( 1,  1, -1);
		verts_[22] = new Vector3 ( 1, -1, -1);
		verts_[23] = new Vector3 (-1, -1, -1);

		Quad q0 = new Quad(verts_[0], verts_[1], verts_[2], verts_[3]);
		Quad q1 = new Quad(verts_[4], verts_[5], verts_[6], verts_[7]);
		Quad q2 = new Quad(verts_[8], verts_[9], verts_[10], verts_[11]);
		Quad q3 = new Quad(verts_[12], verts_[13], verts_[14], verts_[15]);
		Quad q4 = new Quad(verts_[16], verts_[17], verts_[18], verts_[19]);
		Quad q5 = new Quad(verts_[20], verts_[21], verts_[22], verts_[23]);

		quads.Add(q0);
		quads.Add(q1);
		quads.Add(q2);
		quads.Add(q3);
		quads.Add(q4);
		quads.Add(q5);

		for (int i = 0; i < quads.Count; i++)
		{
			for (int j = i + 1; j < quads.Count; j++)
			{
				List<Vector3> shared = new List<Vector3>();
				Vector3[] a = new Vector3[] { quads[i].v1, quads[i].v2, quads[i].v3, quads[i].v4 };
				Vector3[] b = new Vector3[] { quads[j].v1, quads[j].v2, quads[j].v3, quads[j].v4 };
				foreach (Vector3 av in a)
				{
					foreach (Vector3 bv in b)
					{
						if (av == bv && !shared.Contains(av)) shared.Add(av);
					}
				}
				if (shared.Count == 2)
				{
					Edge edge = new Edge(shared[0], shared[1], quads[i], quads[j]);
					edges.Add(edge);
				}
			}
		}

		return (quads, edges);
	}

	Mesh CreateMesh(List<Quad> quads)
	{
		Mesh mesh = new Mesh();
		mesh.indexFormat = IndexFormat.UInt32;

		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Vector3> normals = new List<Vector3>();
		List<int> tris = new List<int>();

		for (int qi = 0; qi < quads.Count; qi++)
		{
			Quad q = quads[qi];
			int baseIndex = verts.Count;

			Vector3 normal = Vector3.Cross(q.v2 - q.v1, q.v3 - q.v1).normalized;

			verts.Add(q.v1);
			verts.Add(q.v2);
			verts.Add(q.v3);
			verts.Add(q.v4);

			uvs.Add(new Vector2(0f, 0f));
			uvs.Add(new Vector2(1f, 0f));
			uvs.Add(new Vector2(1f, 1f));
			uvs.Add(new Vector2(0f, 1f));

			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);

			tris.Add(baseIndex + 0);
			tris.Add(baseIndex + 1);
			tris.Add(baseIndex + 2);

			tris.Add(baseIndex + 0);
			tris.Add(baseIndex + 2);
			tris.Add(baseIndex + 3);
		}

		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs);
		mesh.SetNormals(normals);
		mesh.SetTriangles(tris, 0);

		mesh.RecalculateBounds();

		return mesh;
	}

	(List<Quad>, List<Edge>) Create(List<(int, int, int)> coords)
	{

		// vertex pool so adjacent cubes reuse the same Vector3 instances
		var vertexPool = new Dictionary<(int, int, int), Vector3>();
		List<Quad> quads = GenerateQuads(coords, vertexPool);
		List<Edge> edges = new List<Edge>();

		/*
			copilot helped with this: Vertex Pooling: especially with making an xyz coordinate grid
			the code makes sure that internal quads are erased to make the catmull clark algorithm work correctly
		*/
		Dictionary<string, int> faceIndex = new Dictionary<string, int>();
		List<Quad> temp = new List<Quad>();

		string Key(Vector3 v) => v.x + "," + v.y + "," + v.z;

		for (int i = 0; i < quads.Count; i++)
		{
			Quad q = quads[i];
			string[] ks = new string[] { Key(q.v1), Key(q.v2), Key(q.v3), Key(q.v4) };
			System.Array.Sort(ks);
			string fk = string.Join("|", ks);

			if (!faceIndex.ContainsKey(fk))
			{
				faceIndex[fk] = temp.Count;
				temp.Add(q);
			}
			else
			{
				int prev = faceIndex[fk];
				if (prev >= 0 && prev < temp.Count)
				{
					temp[prev] = null;
				}
				faceIndex.Remove(fk);
			}
		}

		List<Quad> deduped = new List<Quad>();
		foreach (Quad q in temp) if (q != null) deduped.Add(q);
		quads = deduped;

		for (int i = 0; i < quads.Count; i++)
		{
			for (int j = i + 1; j < quads.Count; j++)
			{
				List<Vector3> shared = new List<Vector3>();
				Vector3[] a = new Vector3[] { quads[i].v1, quads[i].v2, quads[i].v3, quads[i].v4 };
				Vector3[] b = new Vector3[] { quads[j].v1, quads[j].v2, quads[j].v3, quads[j].v4 };
				foreach (Vector3 av in a)
				{
					foreach (Vector3 bv in b)
					{
						if (av == bv && !shared.Contains(av)) shared.Add(av);
					}
				}
				if (shared.Count == 2)
				{
					Edge edge = new Edge(shared[0], shared[1], quads[i], quads[j]);
					edges.Add(edge);
				}
			}
		}

		return (quads, edges);
	}

	// The idea is to make cubes to help with 3d object creation, so make it so that it inputs an xyz coordinate.
	List<Quad> GridQuad(int x_, int y_, int z_, Dictionary<(int, int, int), Vector3> pool)
	{
		List<Quad> quads = new List<Quad>();

		// helper to get or create a pooled vertex at integer grid coords
		Vector3 GetP(int vx, int vy, int vz)
		{
			var key = (vx, vy, vz);
			if (pool.TryGetValue(key, out Vector3 v)) return v;
			Vector3 nv = new Vector3(vx * u, vy * u, vz * u);
			pool[key] = nv;
			return nv;
		}

		// cube corner indices in grid coordinates
		int x = x_, y = y_, z = z_;

		// 8 cube corners (using pooled vertices)
		Vector3 b0 = GetP(x, y, z);           // bottom-back-left
		Vector3 b1 = GetP(x + 1, y, z);       // bottom-back-right
		Vector3 b2 = GetP(x + 1, y, z + 1);   // bottom-front-right
		Vector3 b3 = GetP(x, y, z + 1);       // bottom-front-left
		Vector3 t0 = GetP(x, y + 1, z);       // top-back-left
		Vector3 t1 = GetP(x + 1, y + 1, z);   // top-back-right
		Vector3 t2 = GetP(x + 1, y + 1, z + 1); // top-front-right
		Vector3 t3 = GetP(x, y + 1, z + 1);     // top-front-left

		quads.Add(new Quad(b0, b1, b2, b3));
		quads.Add(new Quad(t0, t3, t2, t1));
		quads.Add(new Quad(b1, b0, t0, t1));
		quads.Add(new Quad(b3, b2, t2, t3));
		quads.Add(new Quad(b0, b3, t3, t0));
		quads.Add(new Quad(b2, b1, t1, t2));

		return quads;
	}
	
	List<Quad> GenerateQuads(List<(int, int, int)> xyz_list, Dictionary<(int, int, int), Vector3> pool) {
		List<Quad> q = new List<Quad>();
		foreach ((int, int, int) xyz in xyz_list)
		{			
			q.AddRange(GridQuad(xyz.Item1, xyz.Item2, xyz.Item3, pool));
		}
		return q;
	}
	
	void Update () {
	}
}
