using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bud {
    public Vector3 dir;

    public int age;
    public int order;
    public BudType type;

    public bool is_alive;
    public bool has_branch;

    public Bud(Vector3 dir, int order, BudType type)
    {
        this.dir = dir.normalized;
        this.order = order;
        this.age = 0;
        this.type = type;
        this.is_alive = true;
        this.has_branch = false;
    }

    public Bud(float dx, float dy, float dz, int order, BudType type)
        : this(new Vector3(dx, dy, dz), order, type)
    { }
}