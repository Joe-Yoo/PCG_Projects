using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour {

	private Vector3[] verts;  // the vertices of the mesh
	private int[] tris;       // the triangles of the mesh (triplets of integer references to vertices)
	private int ntris = 0;    // the number of triangles that have been created so far
	public int seed = 903602417; // seed: my gtid lol

	private List<Quad> quads = new List<Quad>();
	private List<Edge> edges = new List<Edge>();


	void Start() {
		Random.InitState(seed);
		List<Quad> quads = new List<Quad>();
		List<Edge> edges = new List<Edge>();
		Mesh my_mesh = CreateCube(quads, edges);

		int iterations = 5;

		for (int i = 0; i < iterations; i++) {
			Dictionary<Vector3, List<Vector3>> valences = new Dictionary<Vector3, Dictionary<Vector3, List<Vector3>>>();
			List<Quad> new_quads = new List<Quad>();
			List<Vector3> new_verts = new List<Vector3>();

			foreach (Edge e in edges) {
				Vector3 centroid1 = calc_centroid(e.q1);
				Vector3 centroid2 = calc_centroid(e.q2);
				Vector3 u = verts[e.u];
				Vector3 v = verts[e.v];
				Vector3 edge_val = calc_edgeValence(e, centroid1, centroid2);

				int newUIndex = new_verts.Count; new_verts.Add(u);
				int newVIndex = new_verts.Count; new_verts.Add(v);
				int edgePtIndex = new_verts.Count; new_verts.Add(edge_val);
				int cen1Index = new_verts.Count; new_verts.Add(centroid1);
				int cen2Index = new_verts.Count; new_verts.Add(centroid2);

				if (!valences.TryGetValue(u, out var dictU)) {
					dictU = new Dictionary<Vector3, List<Vector3>>();
					valences[u] = dictU;
				}
				if (!dictU.TryGetValue(centroid1, out var edge_list_u1)) {
					edge_list_u1 = new List<Vector3>();
					dictU[centroid1] = edge_list_u1;
				}
				if (!dictU.TryGetValue(centroid2, out var edge_list_u2)) {
					edge_list_u2 = new List<Vector3>();
					dictU[centroid2] = edge_list_u2;
				}
				edge_list_u1.Add(edge_val);
				edge_list_u2.Add(edge_val);

				if (!valences.TryGetValue(v, out var dictV)) {
					dictV = new Dictionary<Vector3, List<Vector3>>();
					valences[v] = dictV;
				}
				if (!dictV.TryGetValue(centroid1, out var edge_list_v1)) {
					edge_list_v1 = new List<Vector3>();
					dictV[centroid1] = edge_list_v1;
				}
				if (!dictV.TryGetValue(centroid2, out var edge_list_v2)) {
					edge_list_v2 = new List<Vector3>();
					dictV[centroid2] = edge_list_v2;
				}
				edge_list_v1.Add(edge_val);
				edge_list_v2.Add(edge_val);
				
			}

			foreach (Vector3 v in verts) {
				Dictionary<Vector3, List<Vector3>> dict = valences[v];
				int K = valence_list.Count;
				Vector3 E = new Vector3(0,0,0);
				foreach (Vector3 v in new_edge_valences) E += v;

				Vector3 V = new Vector3(0,0,0);
				Vector3 F = new Vector3(0,0,0);
				foreach (KeyValuePair<Vector3, List<Vector3>> kvp in dict) {
					Vector3 key = kvp.Key;
					List<Vector3> value = kvp.Value;

					V += key;
				}

				E /= (float) K;
				F /= (float) K;
				V /= (float) K;
			}
		}

		GameObject cube = new GameObject("Cube");
		cube.AddComponent<MeshFilter>();
		cube.AddComponent<MeshRenderer>();
		cube.GetComponent<MeshFilter>().mesh = my_mesh;
		cube.transform.position = new Vector3(0, 1, 0);
		Renderer rend = cube.GetComponent<Renderer>();
		rend.material.color = new Color(Random.value, Random.value, Random.value, 1f);
	}

	Mesh CreateCube(List<Quad> quads, List<Edge> edges) {
		Mesh mesh = new Mesh();
		// reset triangle counter before we start creating triangles
		ntris = 0;

		int num_verts = 24;
		verts = new Vector3[num_verts];

		// Create a cube centered at origin with side length 2 (coordinates -1..1)
		verts[0] = new Vector3 ( 1, -1, -1);
		verts[1] = new Vector3 ( 1, -1,  1);
		verts[2] = new Vector3 (-1, -1,  1);
		verts[3] = new Vector3 (-1, -1, -1);

		verts[4] = new Vector3 (-1,  1, -1);
		verts[5] = new Vector3 (-1,  1,  1);
		verts[6] = new Vector3 ( 1,  1,  1);
		verts[7] = new Vector3 ( 1,  1, -1);

		verts[8]  = new Vector3 (-1,  1,  1);
		verts[9]  = new Vector3 (-1,  1, -1);
		verts[10] = new Vector3 (-1, -1, -1);
		verts[11] = new Vector3 (-1, -1,  1);

		verts[12] = new Vector3 ( 1,  1,  1);
		verts[13] = new Vector3 (-1,  1,  1);
		verts[14] = new Vector3 (-1, -1,  1);
		verts[15] = new Vector3 ( 1, -1,  1);

		verts[16] = new Vector3 ( 1,  1, -1);
		verts[17] = new Vector3 ( 1,  1,  1);
		verts[18] = new Vector3 ( 1, -1,  1);
		verts[19] = new Vector3 ( 1, -1, -1);

		verts[20] = new Vector3 (-1,  1, -1);
		verts[21] = new Vector3 ( 1,  1, -1);
		verts[22] = new Vector3 ( 1, -1, -1);
		verts[23] = new Vector3 (-1, -1, -1);

		int num_tris = 12;  // 2 triangles per face * 6 faces
		tris = new int[num_tris * 3];

		// create quads for each face (using duplicated vertices so edges can be hard)
		Quad q0 = new Quad(0, 1, 2, 3);    // bottom
		Quad q1 = new Quad(4, 5, 6, 7);    // top
		Quad q2 = new Quad(8, 9, 10, 11);  // left
		Quad q3 = new Quad(12, 13, 14, 15); // front
		Quad q4 = new Quad(16, 17, 18, 19); // right
		Quad q5 = new Quad(20, 21, 22, 23); // back

		quads.Add(q0);
		quads.Add(q1);
		quads.Add(q2);
		quads.Add(q3);
		quads.Add(q4);
		quads.Add(q5);

		// create triangles for each quad
		foreach (Quad q in quads) MakeQuad(q);

		for (int i = 0; i < quads.Count; i++) {
			for (int j = i + 1; j < quads.Count; j++) {
				List<int> shared = new List<int>();
				int[] a = new int[] { quads[i].v1, quads[i].v2, quads[i].v3, quads[i].v4 };
				int[] b = new int[] { quads[j].v1, quads[j].v2, quads[j].v3, quads[j].v4 };
				foreach (int ai in a) {
					foreach (int bi in b) {
						if (verts[ai] == verts[bi] && !shared.Contains(ai)) shared.Add(ai);
					}
				}
				// if exactly two shared vertex positions => they share an edge
				if (shared.Count == 2) {
					Edge edge = new Edge(shared[0], shared[1], quads[i], quads[j]);
					edges.Add(edge);
				}
			}
		}

		// assign geometry to mesh and compute normals
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.RecalculateNormals();

		// (optional) you may want to keep the quads and edges for later processing

		return mesh;
	}


	// make a triangle from three vertex indices (clockwise order)
	void MakeTri(int i1, int i2, int i3) {
		int index = ntris * 3;  // figure out the base index for storing triangle indices
		ntris++;

		tris[index]     = i1;
		tris[index + 1] = i2;
		tris[index + 2] = i3;
	}

	// make a quadrilateral from four vertex indices (clockwise order)
	void MakeQuad(Quad q) {
		MakeTri (q.v1, q.v2, q.v3);
		MakeTri (q.v1, q.v3, q.v4);
	}

	Vector3 calc_centroid(Quad quad) {
		Vector3 a = verts[quad.v1];
		Vector3 b = verts[quad.v2];
		Vector3 c = verts[quad.v3];
		Vector3 d = verts[quad.v4];
		return (a + b + c + d) * 0.25f;
	}

	Vector3 calc_edgeValence(Edge edge, Vector3 centroid1, Vector3 centroid2) {
		Vector3 u = verts[edge.u];
		Vector3 v = verts[edge.v];
		return (u + v + centroid1 + centroid2) * 0.25f;
	}

	// Update is called once per frame (in this case we don't need to do anything)
	void Update () {
	}
}
