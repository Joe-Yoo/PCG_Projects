using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Quad;

public class Edge {
    public int u, v;
    public Quad q1, q2;

    public Edge(int u, int v, Quad q1, Quad q2) {
        this.u = u;
        this.v = v;
        this.q1 = q1;
        this.q2 = q2;
    }
}