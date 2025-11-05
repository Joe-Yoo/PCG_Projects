using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TexturedMesh;

public class CameraMotion : MonoBehaviour {
    public float moveSpeed = 10.0f;
    public float lookSpeed = 2.0f;

    float yaw = 0.0f;
    float pitch = 0.0f;
	Rigidbody rb;

	bool fly_mode = true;
	public int currTileX = 2;
	public int currTileZ = 2;
	public TexturedMesh terrainManager; 

    void Start () {
		rb = Camera.main.gameObject.AddComponent<Rigidbody>();
		rb.useGravity = false;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

		CapsuleCollider col = Camera.main.gameObject.AddComponent<CapsuleCollider>();
		col.height = 3f;
		col.radius = 0.5f;
		col.center = new Vector3(0f, -1f, 0f);

		Camera.main.transform.position = new Vector3(160f, getMountainHeight() + 20f, 160f);
        Camera.main.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
    }
    
    void Update () {
        // WASD movement
       	float dx = Input.GetAxis("Horizontal");
        float dz = Input.GetAxis("Vertical");
		Vector3 pos = transform.position;

        // Toggle flight mode on spacebar press
        if (Input.GetKeyDown(KeyCode.Space)) {
            fly_mode = !fly_mode;
            rb.useGravity = !fly_mode; 
            rb.velocity = Vector3.zero;

			if (fly_mode) {
				transform.position = new Vector3(pos.x, getMountainHeight() + 20f, pos.z);
			} else {
				transform.position = new Vector3(pos.x, terrainManager.ComputeHeight((int)pos.x, (int)pos.z) + 2f, pos.z);
			}
        }

		// mouse camera movement help from chatgpt for this
        yaw   += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

		
        if (fly_mode) {
            Vector3 move = new Vector3(dx, 0, dz);
			move = transform.TransformDirection(move);
			move.y = 0f;
			move.Normalize();
			transform.position += move * moveSpeed * Time.deltaTime;
        } else {
            Vector3 move = new Vector3(dx, 0, dz).normalized;
            Vector3 moveDir = transform.TransformDirection(move);
            moveDir.y = 0f;
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);
        }

		// code to check current position
		int offsetx = 0;
		int offsetz = 0;
		
		if (pos.x < 0) {
			offsetx = 1;
		}
		if (pos.z < 0) {
			offsetz = 1;
		}

		int nextTileX = ((int) (pos.x / terrainManager.texture_width)) - offsetx;
		int nextTileZ = ((int) (pos.z / terrainManager.texture_height)) - offsetz;

		/**
			x -> x + 1 ==> if x goes to the next tile
			despawn x - 2, spawn x + 3

			x -> x - 1 ==> if x goes to the previous tile
			despawn x + 2, spawn x - 3
		*/
		if (nextTileX != currTileX) {
			int diff = nextTileX - currTileX; // either 1 or -1
			int prev = currTileX - 2 * diff;
			int next = currTileX + 3 * diff;
			terrainManager.despawnTile(prev, currTileZ - 2);
			terrainManager.despawnTile(prev, currTileZ - 1);
			terrainManager.despawnTile(prev, currTileZ);
			terrainManager.despawnTile(prev, currTileZ + 1);
			terrainManager.despawnTile(prev, currTileZ + 2);

			terrainManager.spawnTile(next, currTileZ - 2);
			terrainManager.spawnTile(next, currTileZ - 1);
			terrainManager.spawnTile(next, currTileZ);
			terrainManager.spawnTile(next, currTileZ + 1);
			terrainManager.spawnTile(next, currTileZ + 2);

			currTileX = nextTileX;
		}

		if (nextTileZ != currTileZ) {
			int diff = nextTileZ - currTileZ; // either 1 or -1
			int prev = currTileZ - 2 * diff;
			int next = currTileZ + 3 * diff;
			terrainManager.despawnTile(currTileX - 2, 	prev);
			terrainManager.despawnTile(currTileX - 1, 	prev);
			terrainManager.despawnTile(currTileX, 		prev);
			terrainManager.despawnTile(currTileX + 1, 	prev);
			terrainManager.despawnTile(currTileX + 2, 	prev);

			terrainManager.spawnTile(currTileX - 2, 	next);
			terrainManager.spawnTile(currTileX - 1, 	next);
			terrainManager.spawnTile(currTileX, 		next);
			terrainManager.spawnTile(currTileX + 1, 	next);
			terrainManager.spawnTile(currTileX + 2, 	next);

			currTileZ = nextTileZ;
		}
	}
}
