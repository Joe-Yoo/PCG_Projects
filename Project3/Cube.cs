using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

	public int seed = 903602417; // seed: my gtid lol

	void Start() {
		Random.InitState(seed);
		Color c = new Color(Random.value, Random.value, Random.value, 1f);
		List<Quad> quads = new List<Quad>();
		List<Edge> edges = new List<Edge>();
		CreateCube(quads, edges);
		Mesh mesh = CreateMesh(quads);

		GameObject cube_ = new GameObject("Cube_");
		cube_.AddComponent<MeshFilter>();
		cube_.AddComponent<MeshRenderer>();
		cube_.GetComponent<MeshFilter>().mesh = mesh;
		cube_.transform.position = new Vector3(0, 1, 0);
		Renderer rend_ = cube_.GetComponent<Renderer>();
		rend_.material.color = c;

		int iterations = 5;

		for (int i = 0; i < iterations; i++) {
			// Debug.Log("Iteration " + i);
			List<Quad> new_quads = new List<Quad>();
			List<Edge> new_edges = new List<Edge>();

			Dictionary<Vector3, List<Vector3>> vert_valences = new Dictionary<Vector3, List<Vector3>>();
			Dictionary<Vector3, List<Vector3>> edge_valences = new Dictionary<Vector3, List<Vector3>>();
			Dictionary<Vector3, List<Vector3>> face_valences = new Dictionary<Vector3, List<Vector3>>();

			Dictionary<(Vector3, Vector3), List<Vector3>> quad_helper = new Dictionary<(Vector3, Vector3), List<Vector3>>();
			Dictionary<(Vector3, Vector3), Edge> edge_helper = new Dictionary<(Vector3, Vector3), Edge>();

			HashSet<Vector3> visited = new HashSet<Vector3>();

			foreach (Edge e in edges) {
				//Debug.Log(e == null);
				//Debug.Log((e.u == null) + " " + (e.v == null) + " " + (e.q1 == null) + " " + (e.q2 == null));
				Vector3 centroid1 = calc_centroid(e.q1);
				Vector3 centroid2 = calc_centroid(e.q2);
				Vector3 u = e.u;
				Vector3 v = e.v;
				Vector3 edge_val = calc_edgeValence(e, centroid1, centroid2);

				if (!quad_helper.ContainsKey((u, centroid1))) quad_helper[(u, centroid1)] = new List<Vector3>();
				if (!quad_helper.ContainsKey((u, centroid2))) quad_helper[(u, centroid2)] = new List<Vector3>();
				if (!quad_helper.ContainsKey((v, centroid1))) quad_helper[(v, centroid1)] = new List<Vector3>();
				if (!quad_helper.ContainsKey((v, centroid2))) quad_helper[(v, centroid2)] = new List<Vector3>();
				quad_helper[(u, centroid1)].Add(edge_val);
				quad_helper[(u, centroid2)].Add(edge_val);
				quad_helper[(v, centroid1)].Add(edge_val);
				quad_helper[(v, centroid2)].Add(edge_val);

				if (!vert_valences.ContainsKey(u)) vert_valences[u] = new List<Vector3>();
				if (!vert_valences.ContainsKey(v)) vert_valences[v] = new List<Vector3>();
				if (!vert_valences[u].Contains(v)) vert_valences[u].Add(v);
				if (!vert_valences[v].Contains(u)) vert_valences[v].Add(u);

				if (!edge_valences.ContainsKey(u)) edge_valences[u] = new List<Vector3>();
				if (!edge_valences.ContainsKey(v)) edge_valences[v] = new List<Vector3>();
				if (!edge_valences[u].Contains(edge_val)) edge_valences[u].Add(edge_val);
				if (!edge_valences[v].Contains(edge_val)) edge_valences[v].Add(edge_val);

				if (!face_valences.ContainsKey(u)) face_valences[u] = new List<Vector3>();
				if (!face_valences.ContainsKey(v)) face_valences[v] = new List<Vector3>();
				if (!face_valences[u].Contains(centroid1)) face_valences[u].Add(centroid1);
				if (!face_valences[u].Contains(centroid2)) face_valences[u].Add(centroid2);
				if (!face_valences[v].Contains(centroid1)) face_valences[v].Add(centroid1);
				if (!face_valences[v].Contains(centroid2)) face_valences[v].Add(centroid2);
			}

			foreach (Vector3 v in mesh.vertices) {
				if (!visited.Contains(v)) {
					visited.Add(v);
				} else {
					continue;
				}
				
				Vector3 V = new Vector3(0, 0, 0);
				Vector3 E = new Vector3(0, 0, 0);
				Vector3 F = new Vector3(0, 0, 0);

				List<Vector3> vert_val_list = vert_valences[v];
				List<Vector3> edge_val_list = edge_valences[v];
				List<Vector3> face_val_list = face_valences[v];

				foreach (Vector3 vee in vert_val_list) V += vee;
				foreach (Vector3 vee in edge_val_list) E += vee;
				foreach (Vector3 vee in face_val_list) F += vee;

				int K = vert_val_list.Count;

				E /= K;
				F /= K;
				V /= K;
				Vector3 new_v = (2 * E + F + (K - 3) * V) / K;

				foreach (Vector3 centroid in face_val_list) {
					List<Vector3> edge_vals_centroids = quad_helper[(v, centroid)];
					Vector3 e1 = edge_vals_centroids[0];
					Vector3 e2 = edge_vals_centroids[1];

					Quad new_quad = new Quad(new_v, e1, centroid, e2);
					new_quads.Add(new_quad);

					if (!edge_helper.ContainsKey((new_v, e1))) {
						edge_helper[(new_v, e1)] = new Edge(new_v, e1, new_quad, null);
					} else {
						edge_helper[(new_v, e1)].q2 = new_quad;
					}

					if (!edge_helper.ContainsKey((new_v, e2))) {
						edge_helper[(new_v, e2)] = new Edge(new_v, e2, new_quad, null);
					} else {
						edge_helper[(new_v, e2)].q2 = new_quad;
					}


					if (!edge_helper.ContainsKey((centroid, e1))) {
						edge_helper[(centroid, e1)] = new Edge(centroid, e1, new_quad, null);
					} else {
						edge_helper[(centroid, e1)].q2 = new_quad;
					}
					
					
					if (!edge_helper.ContainsKey((centroid, e2))) {
						edge_helper[(centroid, e2)] = new Edge(centroid, e2, new_quad, null);
					} else {
						edge_helper[(centroid, e2)].q2 = new_quad;
					}

				}
			}

			foreach (KeyValuePair<(Vector3, Vector3), Edge> kvp in edge_helper) {
				new_edges.Add(kvp.Value);
			}

			quads = new_quads;
			edges = new_edges;
			mesh = CreateMesh(quads);
			
			GameObject cube = new GameObject("Cube_" + i);
			cube.AddComponent<MeshFilter>();
			cube.AddComponent<MeshRenderer>();
			cube.GetComponent<MeshFilter>().mesh = mesh;
			cube.transform.position = new Vector3(3 + i * 3, 1, 0);
			Renderer rend = cube.GetComponent<Renderer>();
			rend.material.color = c;
		}
	}

	void CreateCube(List<Quad> quads, List<Edge> edges) {

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

		for (int i = 0; i < quads.Count; i++) {
			for (int j = i + 1; j < quads.Count; j++) {
				List<Vector3> shared = new List<Vector3>();
				Vector3[] a = new Vector3[] { quads[i].v1, quads[i].v2, quads[i].v3, quads[i].v4 };
				Vector3[] b = new Vector3[] { quads[j].v1, quads[j].v2, quads[j].v3, quads[j].v4 };
				foreach (Vector3 av in a) {
					foreach (Vector3 bv in b) {
						if (av == bv && !shared.Contains(av)) shared.Add(av);
					}
				}
				if (shared.Count == 2) {
					Edge edge = new Edge(shared[0], shared[1], quads[i], quads[j]);
					edges.Add(edge);
				}
			}
		}
	}

	Mesh CreateMesh(List<Quad> quads) {
		Mesh mesh = new Mesh();

		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> normals = new List<Vector3>();

		foreach (Quad quad in quads) {
			int startIndex = vertices.Count;

			vertices.Add(quad.v1);
			vertices.Add(quad.v2);
			vertices.Add(quad.v3);
			vertices.Add(quad.v4);

			Vector3 normal = Vector3.Cross(quad.v2 - quad.v1, quad.v3 - quad.v1).normalized;

			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);

			triangles.Add(startIndex);
			triangles.Add(startIndex + 1);
			triangles.Add(startIndex + 2);

			triangles.Add(startIndex);
			triangles.Add(startIndex + 2);
			triangles.Add(startIndex + 3);

			vertices.Add(quad.v1);
			vertices.Add(quad.v2);
			vertices.Add(quad.v3);
			vertices.Add(quad.v4);

			Vector3 invertedNormal = -normal;

			normals.Add(invertedNormal);
			normals.Add(invertedNormal);
			normals.Add(invertedNormal);
			normals.Add(invertedNormal);

			triangles.Add(startIndex + 6);
			triangles.Add(startIndex + 5);
			triangles.Add(startIndex + 4);

			triangles.Add(startIndex + 7);
			triangles.Add(startIndex + 6);
			triangles.Add(startIndex + 4);
		}

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.normals = normals.ToArray();

		return mesh;
	}
	
	Vector3 calc_centroid(Quad quad) {
		return (quad.v1 + quad.v2 + quad.v3 + quad.v4) * 0.25f;
	}

	Vector3 calc_edgeValence(Edge edge, Vector3 centroid1, Vector3 centroid2) {
		Vector3 u = edge.u;
		Vector3 v = edge.v;
		return (u + v + centroid1 + centroid2) * 0.25f;
	}

	void Update () {
	}
}
