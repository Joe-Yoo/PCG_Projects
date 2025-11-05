using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node {
    public List<Bud> bud_list;
    public Vector3 pos;

    public Node(float x_, float y_, float z_) {
        bud_list = new();
        pos = new Vector3(x_, y_, z_);
    }

    public Node(Vector3 v) {
        bud_list = new();
        pos = v;
    }
}   