using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PineTreeMesh;
using static BudType;

public class BranchMesh {
    // started template used from warmup 2
    private Vector3[] verts;
	private int[] tris; 
    private int ntris = 0;
    private Mesh mesh; 
    private Vector3 origin;
    private float bottom_radius;
    private float top_radius;
    private float height;

    public BranchMesh(float bottom_radius_, float top_radius_, float height_, Vector3 origin){
        bottom_radius = bottom_radius_;
        top_radius = top_radius_;
        height = height_;
        mesh = CreateBranchMesh();
        this.origin = origin;
    }

    public BranchMesh(float bottom_radius_, float top_radius_, float height_, float x, float y, float z)
        : this(bottom_radius_, top_radius_, height_, new Vector3(x, y, z)) { }

    private Mesh CreateBranchMesh() {
        Mesh mesh = new Mesh();

        int num_cylinder = 20;
        int num_end_circle = num_cylinder/2; // 20 in circle, 1 for center

        int num_verts = num_cylinder * 2 + 2;
        int num_tris = num_cylinder * 4;

        // verts = [0-19 = bottom big circle][20-39 = top big circle][40-49 = bottom lil circle][50-59 = top lil circle][60 = bottom center][61 = top center]
        verts = new Vector3[num_verts];
        tris = new int[num_tris * 3];
        Vector3[] normals = new Vector3[verts.Length];
        Vector2[] uvs = new Vector2[verts.Length];

        // cylinder mesh
        for (int i = 0; i < num_cylinder; i++) {
            float angle = 2 * Mathf.PI * i / num_cylinder;
			float x = bottom_radius * Mathf.Cos(angle);
			float z = bottom_radius * Mathf.Sin(angle);
			float x_ = top_radius * Mathf.Cos(angle);
			float z_ = top_radius * Mathf.Sin(angle);
			verts[i] = new Vector3(x, 0, z);
			verts[i + num_cylinder] = new Vector3(x_, height, z_);
        }

        verts[verts.Length - 2] = new Vector3(0, 0, 0);
        verts[verts.Length - 1] = new Vector3(0, height, 0);

        for (int i = 0; i < num_cylinder; i++) {
            if (i == num_cylinder - 1) {
                MakeTri(i, i + num_cylinder, 0);
                MakeTri(i + num_cylinder, num_cylinder, 0);
                MakeTri(verts.Length - 2, i, 0);
                MakeTri(verts.Length - 1, num_cylinder, i + num_cylinder);
            } else {
                MakeTri(i, i + num_cylinder, i + 1);
                MakeTri(i + num_cylinder, i + num_cylinder + 1, i + 1);
                MakeTri(verts.Length - 2, i, i + 1);
                MakeTri(verts.Length - 1, i + num_cylinder + 1, i + num_cylinder);
            }
        }

        // with a lot of help copilot
        for (int i = 0; i < num_cylinder; i++) {
            // bottom vertex
            Vector3 vBot = verts[i];
            Vector3 nBot = new Vector3(vBot.x, 0f, vBot.z).normalized;
            normals[i] = nBot;
            uvs[i] = new Vector2((float)i / num_cylinder, 0f);

            // top vertex
            Vector3 vTop = verts[i + num_cylinder];
            Vector3 nTop = new Vector3(vTop.x, 0f, vTop.z).normalized;
            normals[i + num_cylinder] = nTop;
            uvs[i + num_cylinder] = new Vector2((float)i / num_cylinder, 1f);
        }

        int bottomCenter = verts.Length - 2;
        int topCenter = verts.Length - 1;
        normals[bottomCenter] = Vector3.down;
        uvs[bottomCenter] = new Vector2(0.5f, 0.5f);
        normals[topCenter] = Vector3.up;
        uvs[topCenter] = new Vector2(0.5f, 0.5f);


        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = normals;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
        return mesh;
    }

    void MakeTri(int i1, int i2, int i3) {
		int index = ntris * 3;  // figure out the base index for storing triangle indices
		ntris++;

		tris[index]     = i1;
		tris[index + 1] = i2;
		tris[index + 2] = i3;
	}
    
    public Mesh getMesh() {
        return mesh;
    }

    public Vector3 getOrigin() {
        return origin;
    }

    public float getHeight() {
        return height;
    }

    public float getBottomRadius() {
        return bottom_radius;
    }   

    public float getTopRadius() {
        return top_radius;
    }

    // method here should calculate direction/angle
    void calculateAngle() {

    }
}