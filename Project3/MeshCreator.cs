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
		GameObject creature = new GameObject("Pig_With_Wings");

		// ====== Textures ======
		Material light_pink = CreateMat(new Color(0.99215686f, 0.84313726f, 0.89411765f, 1f));
		Material dark_pink = CreateMat(new Color(0.99215686f, 0.70313726f, 0.70411765f, 1f));
		Material white = CreateMat(new Color(1f, 1f, 1f, 1f));
		Material black = CreateMat(new Color(0f, 0f, 0f, 1f));

		// ======= Body =======
		GameObject body = CreateGameObject("Body", Body(), creature, light_pink, 0, 0, 0);

		// ======= Wings =======
		GameObject left_wing = CreateGameObject("Left_Wing", Wings(), creature, white, 6, 5, 0);
		GameObject right_wing = CreateGameObject("Right_Wing", Wings(), creature, white, 6, 5, 6);

		// ======= Nose =======
		GameObject nostril = CreateGameObject("Nostril", Nostril_1(), creature, dark_pink, -5, 5, 2);

		// ====== Eyes ======
		GameObject eyes = CreateGameObject("Eyes", Eyes(), creature, white, -4.5f, 7, 2);

		GameObject left_pupil = CreateGameObject("Left_Pupil", Single(), eyes, black, -4.75f, 7, 2);
		GameObject left_pivot = new GameObject("Left_Pivot");
		left_pivot.transform.position = left_pupil.transform.position + new Vector3(u / 2, u / 2, u / 2);
		left_pupil.transform.parent = left_pivot.transform;
		left_pivot.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		left_pivot.transform.parent = creature.transform;

		GameObject right_pupil = CreateGameObject("Right_Pupil", Single(), eyes, black, -4.75f, 7, 4);
		GameObject right_pivot = new GameObject("Right_Pivot");
		right_pivot.transform.position = right_pupil.transform.position + new Vector3(u / 2, u / 2, u / 2);
		right_pupil.transform.parent = right_pivot.transform;
		right_pivot.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		right_pivot.transform.parent = creature.transform;

		// ====== Tail ======
		GameObject tail = CreateGameObject("Tail", Tail_1(), creature, light_pink, 11.5f, 1.5f, 3);
	}

	GameObject CreateGameObject(string name, List<(int, int, int)> co, GameObject parent, Material mat, float x, float y, float z) {
		(List<Quad>, List<Edge>) mesh_qe = Create(co);
		List<Quad> subdiv_mesh = CatmullClark(mesh_qe.Item1, mesh_qe.Item2);

		GameObject go = new GameObject(name);
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>();
		go.GetComponent<MeshFilter>().mesh = CreateMesh(subdiv_mesh);
		go.transform.position = ScaleVector(x, y, z);
		go.transform.parent = parent.transform;

		Renderer rend = go.GetComponent<Renderer>();
		rend.material = mat;
		return go;
	}

	Material CreateMat(Color c) {
		Material mat = new Material(Shader.Find("Standard"));
		mat.color = c;
		mat.SetInt("_CullMode", (int)CullMode.Off);
		return mat;
	}

	Vector3 ScaleVector(float x, float y, float z) {
		return new Vector3(x * u, y * u, z * u);
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
			Vector3 nv = ScaleVector(vx, vy, vz);
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
