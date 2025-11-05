using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BranchMesh;
using static Bud;
using static BudType;
using static Node;

public class PineTreeMesh : MonoBehaviour {
    public GameObject leafPrefab;
    public GameObject flowerPrefab;
    public GameObject sproutPrefab;

    public int seed = 903602417;
    public int id = 0;
    private List<Node> node_list;

    private static float height = 2f;
    private static float width = 1f;
    private static float delta_width = 14f;
    private static float threshold = 0.01f;
    private static float delta_stem_angle = 0.02f;
    private static float delta_angle = 0.1f;
    private static float die_prob = 0.01f;
    private static float pause_prob = 0.1f;
    private static float branch_prob = 0.2f;
    private static float floraSpawnChance = 0.1f;
    private static float leafSpawnChance = 0.5f;
    private static float flowerSpawnChance = 0.2f;

    private static Vector3 flower_offset = new Vector3(0f, 2f, 0f);   

    void Start()
    {
        Random.InitState(seed);
        node_list = new();
        // float y = Random.value/2;
        
        createTrunk();
        createBranches();
        
        this.transform.position = new Vector3(transform.position.x -20f * id, transform.position.y, transform.position.z);
        
    } 

    private void createTrunk() {
        float y = 0f;
        float orix = Random.value * 2f - 1f;
        float oriz = Random.value * 2f - 1f;
        Vector3 otherDir = (new Vector3(orix, 5, oriz)).normalized;
        Vector3 currDir = new Vector3(0,1,0);

        // 4 starter branches
        Vector3 currOri = new Vector3(0, 0, 0);
        for (int i = 0; i < 10; i++) {
            GameObject s = new GameObject("Branch_" + i);
            s.AddComponent<MeshFilter>();
            s.AddComponent<MeshRenderer>();
            Renderer rend = s.GetComponent<Renderer>();
            rend.material.color = new Color(0.4118f, 0.2863f, 0.0392f, 1f);
            

            float curr_width = width - (i / delta_width);
            curr_width = curr_width > threshold ? curr_width : threshold;
            float next_width = width - ((i + 1) / delta_width);
            next_width = next_width > threshold ? next_width : threshold;
            
            BranchMesh branch = new BranchMesh(curr_width, next_width, height, currOri);
            s.GetComponent<MeshFilter>().mesh = branch.getMesh();
            s.transform.position = currOri;  // Use exact currOri position

            currDir = (new Vector3(currDir.x + otherDir.x * i * delta_stem_angle, currDir.y + otherDir.y * i * delta_stem_angle, currDir.z + otherDir.z * i * delta_stem_angle)).normalized;
            s.transform.rotation = Quaternion.FromToRotation(Vector3.up, currDir);

            Node newNode = new Node(currOri);

            if (i >= 3) {
                spawnSprouts(s.transform, branch);
            }

            currOri = s.transform.position + s.transform.up * height;
                
            float startAngle = Random.Range(0f, 90f);

            // make 4 buds, 90° apart
            for (int k = 0; k < 4; k++) {
                float angle = (startAngle + k * 90f) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);

                Bud newBud = new Bud(x, y, z, i, BudType.Side);
                newNode.bud_list.Add(newBud);
                if (i < 4) {
                    newBud.is_alive = false;
                }
            }
            
            node_list.Add(newNode);
            s.transform.SetParent(this.transform);
        }
        Node tipNode = node_list[node_list.Count - 1];
        Bud apicalBud = new Bud(currDir, node_list.Count, BudType.Stem);
        tipNode.bud_list.Add(apicalBud);

    }

    private void createBranches() {
        float y = 0f;

        for (int i = 0; i < 5; i++) {
            List<Node> newNodeList = new();

            foreach (Node node in node_list) {
                List<Bud> newBudList = new();
                foreach (Bud bud in node.bud_list) {
                    if (bud.is_alive && !bud.has_branch) {
                        if (Random.value < die_prob) {
                            bud.is_alive = false;
                        } else if (Random.value > pause_prob) {
                            Vector3 pos = node.pos;
                            bud.has_branch = true;

                            GameObject s = new GameObject("Branch_" + node_list.Count + "_" + i);
                            s.AddComponent<MeshFilter>();
                            s.AddComponent<MeshRenderer>();
                            Renderer rend = s.GetComponent<Renderer>();
                            rend.material.color = new Color(0.4118f, 0.2863f, 0.0392f, 1f);

                            int order = bud.order + (bud.type == BudType.Stem ? 0 : 2);
                            float calc_width = width - (order / delta_width);
                            if (calc_width < threshold) {
                                bud.is_alive = false;
                                GameObject.Destroy(s);
                                continue;
                            }

                            calc_width = calc_width < threshold ? threshold : calc_width;
                            float new_calc_width = width - ((order + 1) / delta_width);
                            new_calc_width = new_calc_width < threshold ? threshold : new_calc_width;
                            
                            BranchMesh branch = new BranchMesh(calc_width, new_calc_width, height, pos);
                            s.GetComponent<MeshFilter>().mesh = branch.getMesh();
                            s.transform.position = branch.getOrigin();

                            // orthotropic angle calculation
                            Vector3 orthoDir = (new Vector3(bud.dir.x, bud.dir.y + 1 * delta_angle, bud.dir.z)).normalized;

                            s.transform.rotation = Quaternion.FromToRotation(Vector3.up, orthoDir);

                            // new node creation logic here
                            Vector3 end = s.transform.position + s.transform.up * height;
                            Bud newBud = new Bud(orthoDir, bud.order + 1, bud.type);
                            Node newNode = new Node(end);
                            newNode.bud_list.Add(newBud);
                            newNodeList.Add(newNode);

                            if (bud.type == BudType.Stem) {
                                float x = Random.value * 2f - 1f;
                                float z = Random.value * 2f - 1f;
                                Bud newSideBud_1 = new Bud(x, y, z, bud.order, BudType.Side);
                                Bud newSideBud_2 = new Bud( -x, y, -z, bud.order, BudType.Side);
                                Bud newSideBud_3 = new Bud(z, y, -x, bud.order, BudType.Side);
                                Bud newSideBud_4 = new Bud( -z, y, x, bud.order, BudType.Side);
                                newNode.bud_list.Add(newSideBud_1);
                                newNode.bud_list.Add(newSideBud_2);
                                newNode.bud_list.Add(newSideBud_3);
                                newNode.bud_list.Add(newSideBud_4);
                            }
                        
                            if (bud.type == BudType.Side && Random.value < branch_prob) {
                                // spawn new bud
                                float dx = Random.value / 3f + 0.05f + bud.dir.x;
                                float dy = -0.5f + bud.dir.y;
                                float dz = Random.value / 3f + 0.05f + bud.dir.z;
                                Vector3 newDir = (new Vector3(dx, dy, dz)).normalized;
                                Bud newBranchBud = new Bud(newDir, bud.order + 1, bud.type);
                                newBudList.Add(newBranchBud);
                            }

                            spawnFlora(s.transform, branch);

                            s.transform.SetParent(this.transform);
                        }
                    }
                }
                node.bud_list.AddRange(newBudList);
            }
            node_list.AddRange(newNodeList);
        }
    }

    private void spawnSprouts(Transform curr, BranchMesh branch) {
        // 8 spots on the way up, check for sprout spawn
        int rows = 8;
        float bottomRadius = branch.getBottomRadius();
        float topRadius = branch.getTopRadius();
        for (int i = 0 ; i < rows; i++) {
            if (Random.value < floraSpawnChance) {
                continue;
            }

            float radius = bottomRadius - (bottomRadius - topRadius) / rows * i;
            float theta = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // with some help from chatgpt
            Vector3 localPos = new Vector3(Mathf.Cos(theta) * radius, branch.getHeight() / rows * i, Mathf.Sin(theta) * radius);
            Vector3 localNormal = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)); 

            Vector3 worldPos = curr.TransformPoint(localPos);
            Vector3 worldDir = curr.TransformDirection(localNormal).normalized;

            Quaternion spin = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, worldDir);

            GameObject s = Instantiate(sproutPrefab, worldPos, rot * spin, this.transform);
        }
    }

    private void spawnFlora(Transform curr, BranchMesh branch) {
        int rows = 8;
        int end = 8;
        
        float bottomRadius = branch.getBottomRadius();
        float topRadius = branch.getTopRadius();

        for (int i = 0; i < end; i++) {
            if (Random.value < floraSpawnChance) {
                continue;
            }

            float radius = bottomRadius - (bottomRadius - topRadius) / rows * i;

            float theta = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // with some help from chatgpt
            Vector3 localPos = new Vector3(Mathf.Cos(theta) * radius, branch.getHeight() / rows * i, Mathf.Sin(theta) * radius);
            Vector3 localNormal = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)); 

            Vector3 worldPos = curr.TransformPoint(localPos);
            Vector3 worldDir = curr.TransformDirection(localNormal).normalized;

            Quaternion spin = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, worldDir);
            
            GameObject prefab = sproutPrefab;
            if (Random.value < leafSpawnChance) {
                prefab = leafPrefab;

                Vector3 prefabRight = (rot * spin) * Vector3.right;
                Vector3 prefabUp = (rot * spin) * Vector3.up;
                float rightPushDistance = 0.149f;
                float pushDistance = 0.22f;
                worldPos = worldPos + prefabRight * rightPushDistance + prefabUp * pushDistance;
            
            } else if (Random.value < flowerSpawnChance) {
                prefab = flowerPrefab;
                Vector3 prefabUp = (rot * spin) * Vector3.up;
                float pushDistance = 0.22f;
                worldPos = worldPos + prefabUp * pushDistance;
            } else {
                // sprout prefab
                Vector3 prefabUp = (rot * spin) * Vector3.up;
                float pushDistance = 0.09f;
                worldPos = worldPos + prefabUp * pushDistance;
            }

            Instantiate(prefab, worldPos, rot * spin, this.transform);
        }
    }
}