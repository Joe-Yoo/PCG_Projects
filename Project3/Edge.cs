using UnityEngine;

public class Edge {
    public Vector3 u, v;
    public Quad q1, q2;

    public Edge(Vector3 u, Vector3 v, Quad q1, Quad q2) {
        this.u = u;
        this.v = v;
        this.q1 = q1;
        this.q2 = q2;
    }
}