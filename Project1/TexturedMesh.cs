using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturedMesh : MonoBehaviour {
	public GameObject treePrefab;
	public GameObject pebblePrefab;
	public GameObject grassPrefab;

	private int offset = 1000;
	public int texture_width = 64;
	public int texture_height = 64;
	public float scale = 10;
	private static float amp0 = 0.05f;
	private static float amp1 = 0.55f;
	private static float amp2 = 10f;
	private static float amp3 = 45f;
	public float treeSpawnChance = 0.035f;
	public float grassSpawnChance = 0.15f;
	private static bool spawned = false;
	public int seed = 903602417; // my gtid lol

	private static float maxHeight = amp0 + amp1 + amp2 + amp3;
	
	// create a quad that is textured
	void Start() {
		if (spawned) {
			// Unity likes to call start() multiple times for some reason. This ensures only 1 tile is being made per chunk.
			return;
		}
		spawned = true;

		int count = 5;
		for (int i = 0; i < count; i++) {
			for (int j = 0; j < count; j++) {
				spawnTile(i, j);
			}
		}
		// testing purposes
		// despawnTile(0,0);
		// spawnTile(0,3);
	}

	public void spawnTile(int x, int z) {
		Random.InitState(seed); // consistent spawning of trees grass and pebbles
		int currx = x * texture_width;
		int currz = z * texture_height;

		GameObject tile = new GameObject($"Tile_{x}_{z}");

		// create a new GameObject
		GameObject m = new GameObject($"Mountain_{x}_{z}");
		m.transform.parent = tile.transform;
		m.AddComponent<MeshFilter>();
		m.AddComponent<MeshRenderer>();

		// assign the mesh
		Mesh mesh = CreateMountainMesh(currx, currz);
		m.GetComponent<MeshFilter>().mesh = mesh;
		
		MeshCollider collider = m.AddComponent<MeshCollider>();
		collider.sharedMesh = mesh;

		// give it a material and color
		Renderer rend = m.GetComponent<Renderer>();
		rend.material = new Material(Shader.Find("Standard"));
		rend.material.color = Color.white;

		// optional: apply texture
		Texture2D texture = TextureMountain(currx, currz);
		rend.material.mainTexture = texture;

		SpawnTreesPebbles(currx, currz, texture, tile.transform);
	}

	public void despawnTile(int x, int z)
	{
		string tileName = $"Tile_{x}_{z}";
		GameObject tile = GameObject.Find(tileName);

		if (tile != null)
		{
			Destroy(tile);
		}
	}

	// using Perlin noise here!
	public float ComputeHeight(int x, int z) {
		float y0 = Mathf.PerlinNoise((x + offset) * 0.95f, (z + offset) * 0.95f) * amp0;
		float y1 = Mathf.PerlinNoise((x + offset) * 0.5f, (z + offset) * 0.5f) * amp1;
		float y2 = Mathf.PerlinNoise((x + offset) * 0.04f, (z + offset) * 0.06f) * amp2;
		float y3 = Mathf.PerlinNoise((x + offset) * 0.01f, (z + offset) * 0.02f) * amp3;
		float y_water = Mathf.PerlinNoise((x + offset) * 0.4f, (z + offset) * 0.25f) * amp1;
		float height = y0 + y1 + y2 + y3;
		// calculation to make sure water is at a uniform level (with some waves using perlin noise!)
		return height / maxHeight < 0.24f ? (0.239f * maxHeight + y_water) : height;
	}

	Vector3 ComputeNormal(int x, int z) {
		float hL = ComputeHeight(x - 1, z);
		float hR = ComputeHeight(x + 1, z);
		float hD = ComputeHeight(x, z - 1);
		float hU = ComputeHeight(x, z + 1);

		// Use cross products to approximate slope
		Vector3 normal = new Vector3(hL - hR, 2f, hD - hU);
		return normal.normalized;
	}

	Mesh CreateMountainMesh(int currx, int currz) {
		Mesh mesh = new Mesh();
		int tile_width = texture_width + 1;
		int tile_height = texture_height + 1;

		Vector3[] verts = new Vector3[tile_width * tile_height];
		Vector3[] normals = new Vector3[tile_width * tile_height];
		Vector2[] uv = new Vector2[tile_width * tile_height];
		int[] tris = new int[(tile_width - 1) * (tile_height - 1) * 6];


		for (int z = 0; z < tile_width; z++) {
			for (int x = 0; x < tile_height; x++) {
				int i = z * tile_width + x;
				float height = ComputeHeight(x + currx, z + currz); // water cutoff
				verts[i] = new Vector3(x + currx, height, z + currz);
				uv[i] = new Vector2(x / (float)texture_width, z / (float)texture_height);
				normals[i] = ComputeNormal(x + currx, z + currz);
			}
		}

		int t = 0;
		for (int z = 0; z < tile_width - 1; z++) {
			for (int x = 0; x < tile_height - 1; x++) {
				int i = z * tile_width + x;

				tris[t++] = i;	
				tris[t++] = i + tile_height;
				tris[t++] = i + 1;

				tris[t++] = i + 1;
				tris[t++] = i + tile_height;
				tris[t++] = i + tile_height + 1;
			}
		}

		mesh.vertices = verts;
		mesh.uv = uv;
		mesh.triangles = tris;
		mesh.normals = normals;

		return mesh;
	}

	Texture2D TextureMountain(int currx, int currz) {		
		// create the texture and an array of colors that will be copied into the texture
		int tile_width = texture_width + 1;
		int tile_height = texture_height + 1;
		Texture2D texture = new Texture2D (tile_width, tile_height);
		Color32[] colors = new Color32[tile_width * tile_height];

		for (int z = 0; z < tile_width; z++) {
			for (int x = 0; x < tile_height; x++) {
				int i = z * tile_width + x;
				float height = ComputeHeight(x + currx, z + currz) / maxHeight;

				Color32 c;
				if (height > 0.7f) { // snowcap white
					c = new Color32(230, 230, 230, 255);
				} else if (height > 0.5f) { // mountain gray
					c = new Color32(128, 128, 128, 255);
				} else if (height > 0.3f) { // grass green
					c = new Color32(59, 91, 47, 255);
				} else if (height > 0.25f) { // sand brown
					c = new Color32(185, 153, 118, 255); 
				} else { // water blue
					c = new Color32(45, 74, 156, 255);
                }
				colors[i] = c;
			}
		}


		// copy the colors into the texture
		texture.SetPixels32(colors);

		// do texture specific stuff, probably including making the mipmap levels
		texture.wrapMode = TextureWrapMode.Clamp; // needed to fix interchunk colors 
		texture.Apply();

		// return the texture
		return (texture);
	}

	// update is called once per frame
	void Update () {

	}

	public static float getMountainHeight() {
		return maxHeight;
	}

	void SpawnTreesPebbles(int tileX, int tileZ, Texture2D texture, Transform parent) {
		for (int x = 0; x < texture_width; x += 4)
		{
			for (int z = 0; z < texture_height; z += 4)
			{
				SpawnTrees(tileX + x, tileZ + z, texture, x, z, parent);
				SpawnGrass(tileX + x, tileZ + z, texture, x, z, parent);
			}
		}

		SpawnPebbles(tileX, tileZ, parent);
	}

	void SpawnTrees(int globalx, int globalz, Texture2D texture, int currx, int currz, Transform parent) {
		Color32 pixel = texture.GetPixel(currx, currz);
		Color32 grassColor = new Color32(59, 91, 47, 255);
		if (pixel.Equals(grassColor)) {
			if (Random.value < treeSpawnChance) {
				float y = ComputeHeight(globalx, globalz);
				Vector3 pos = new Vector3(globalx, y, globalz);

				GameObject tree = Instantiate(treePrefab, pos, Quaternion.identity, parent);
			}
		}
	}


	void SpawnGrass(int globalx, int globalz, Texture2D texture, int currx, int currz, Transform parent) {
		Color32 pixel = texture.GetPixel(currx, currz);
		Color32 grassColor = new Color32(59, 91, 47, 255);
		if (pixel.Equals(grassColor)) {
			if (Random.value < grassSpawnChance) {
				float y = ComputeHeight(globalx, globalz);
				Vector3 pos = new Vector3(globalx, y, globalz);

				GameObject grass = Instantiate(grassPrefab, pos, Quaternion.identity, parent);
			}
		}
	}

	void SpawnPebbles(int tileX, int tileZ, Transform parent) {
		float x = tileX + Random.Range(0, texture_width);
		float z = tileZ + Random.Range(0, texture_height);
		float y = ComputeHeight((int)x, (int)z);

		Vector3 center = new Vector3(x, y, z);
		int count = Random.Range(5, 9);

		for (int i = 0; i < count; i++) {
			Vector2 circle = Random.insideUnitCircle * 1.5f;
			Vector3 pos = new Vector3(center.x + circle.x, center.y + 2, center.z + circle.y);

			GameObject pebble = Instantiate(pebblePrefab, pos, Random.rotation, parent);

			// add gravity!
			Rigidbody rb = pebble.GetComponent<Rigidbody>();
			if (rb == null) {
				rb = pebble.AddComponent<Rigidbody>();
			}
			rb.mass = 0.1f;
			rb.useGravity = true;
		}
	}		
}
