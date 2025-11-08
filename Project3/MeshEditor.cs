using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEditor
{
    public static List<Quad> CatmullClark(List<Quad> quads, List<Edge> edges)
    {
        int iterations = 5;

        for (int i = 0; i < iterations; i++)
        {
            List<Quad> new_quads = new List<Quad>();
            List<Edge> new_edges = new List<Edge>();

            Dictionary<Vector3, List<Vector3>> vert_valences = new Dictionary<Vector3, List<Vector3>>();
            Dictionary<Vector3, List<Vector3>> edge_valences = new Dictionary<Vector3, List<Vector3>>();
            Dictionary<Vector3, List<Vector3>> face_valences = new Dictionary<Vector3, List<Vector3>>();

            Dictionary<(Vector3, Vector3), List<Vector3>> quad_helper = new Dictionary<(Vector3, Vector3), List<Vector3>>();
            Dictionary<(Vector3, Vector3), Edge> edge_helper = new Dictionary<(Vector3, Vector3), Edge>();

            HashSet<Vector3> visited = new HashSet<Vector3>();

            foreach (Edge e in edges)
            {
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

            foreach (Vector3 v in vert_valences.Keys)
            {
                if (!visited.Contains(v))
                {
                    visited.Add(v);
                }
                else
                {
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

                foreach (Vector3 centroid in face_val_list)
                {
                    if (quad_helper.ContainsKey((v, centroid)) && quad_helper[(v, centroid)].Count >= 2)
                    {
                        List<Vector3> edge_vals_centroids = quad_helper[(v, centroid)];
                        Vector3 e1 = edge_vals_centroids[0];
                        Vector3 e2 = edge_vals_centroids[1];

                        Quad new_quad = new Quad(new_v, e1, centroid, e2);
                        Quad new_quad_2 = new Quad(new_v, e2, centroid, e1);
                        new_quads.Add(new_quad);
                        new_quads.Add(new_quad_2);

                        // register edges for the first new quad
                        if (!edge_helper.ContainsKey((new_v, e1)))
                        {
                            edge_helper[(new_v, e1)] = new Edge(new_v, e1, new_quad, null);
                        }
                        else
                        {
                            edge_helper[(new_v, e1)].q2 = new_quad;
                        }

                        if (!edge_helper.ContainsKey((new_v, e2)))
                        {
                            edge_helper[(new_v, e2)] = new Edge(new_v, e2, new_quad, null);
                        }
                        else
                        {
                            edge_helper[(new_v, e2)].q2 = new_quad;
                        }

                        if (!edge_helper.ContainsKey((centroid, e1)))
                        {
                            edge_helper[(centroid, e1)] = new Edge(centroid, e1, new_quad, null);
                        }
                        else
                        {
                            edge_helper[(centroid, e1)].q2 = new_quad;
                        }

                        if (!edge_helper.ContainsKey((centroid, e2)))
                        {
                            edge_helper[(centroid, e2)] = new Edge(centroid, e2, new_quad, null);
                        }
                        else
                        {
                            edge_helper[(centroid, e2)].q2 = new_quad;
                        }

                        // register edges for the second new quad (opposite winding)
                        if (!edge_helper.ContainsKey((new_v, e1)))
                        {
                            edge_helper[(new_v, e1)] = new Edge(new_v, e1, new_quad_2, null);
                        }
                        else
                        {
                            // if q1 already set, fill q2 only if null
                            if (edge_helper[(new_v, e1)].q2 == null) edge_helper[(new_v, e1)].q2 = new_quad_2;
                        }

                        if (!edge_helper.ContainsKey((new_v, e2)))
                        {
                            edge_helper[(new_v, e2)] = new Edge(new_v, e2, new_quad_2, null);
                        }
                        else
                        {
                            if (edge_helper[(new_v, e2)].q2 == null) edge_helper[(new_v, e2)].q2 = new_quad_2;
                        }

                        if (!edge_helper.ContainsKey((centroid, e1)))
                        {
                            edge_helper[(centroid, e1)] = new Edge(centroid, e1, new_quad_2, null);
                        }
                        else
                        {
                            if (edge_helper[(centroid, e1)].q2 == null) edge_helper[(centroid, e1)].q2 = new_quad_2;
                        }

                        if (!edge_helper.ContainsKey((centroid, e2)))
                        {
                            edge_helper[(centroid, e2)] = new Edge(centroid, e2, new_quad_2, null);
                        }
                        else
                        {
                            if (edge_helper[(centroid, e2)].q2 == null) edge_helper[(centroid, e2)].q2 = new_quad_2;
                        }
                    }
                }
            }

            foreach (KeyValuePair<(Vector3, Vector3), Edge> kvp in edge_helper)
            {
                new_edges.Add(kvp.Value);
            }

            quads = new_quads;
            edges = new_edges;
        }

        return quads;
    }
    
	static Vector3 calc_centroid(Quad quad) {
		return (quad.v1 + quad.v2 + quad.v3 + quad.v4) * 0.25f;
	}

	static Vector3 calc_edgeValence(Edge edge, Vector3 centroid1, Vector3 centroid2) {
		Vector3 u = edge.u;
		Vector3 v = edge.v;
		return (u + v + centroid1 + centroid2) * 0.25f;
	}
}