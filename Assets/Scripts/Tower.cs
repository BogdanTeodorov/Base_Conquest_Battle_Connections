using UnityEngine;
using TMPro; // Ensure TextMesh Pro is imported
using System.Collections; // Required for IEnumerator


public class Tower : MonoBehaviour
{
    // Prefab of the soldier to spawn
    public GameObject soldierPrefab;

    // Reference to the enemy tower
    public Transform targetTower;
    public GameObject TowerColored;


    // Time interval between spawns
    public float spawnInterval = 5.0f;

    // Health of the tower
    public int health = 10; // Default to 10 for towers

    [SerializeField] private TMP_Text text; // Reference to the UI text element for displaying health

    private float timeSinceLastSpawn;

    void Start()
    {
        timeSinceLastSpawn = 0;
        AssignTextComponent();
        UpdateUi();
        GameController.Instance.RegisterTower(this); // Register with GameController

        StartCoroutine(FirstSpawnDelay()); // Start with a delay for the first spawn
        if (gameObject.tag != "Player" && gameObject.tag != "NeutralTower")
        {
            StartCoroutine(ChangeTargetRandomly()); // Start the coroutine to change target randomly for non-player towers
        }
        PaintTowerColored();
    }

    private void PaintTowerColored()
    {

        // Assign the tower's material to the TowerColored object
        if (TowerColored != null)
        {
            Renderer towerRenderer = GetComponent<Renderer>();
            Renderer towerColoredRenderer = TowerColored.GetComponent<Renderer>();

            if (towerRenderer != null && towerColoredRenderer != null)
            {
                // Set the TowerColored object's material to the tower's material
                towerColoredRenderer.material = towerRenderer.material;
            }
            else
            {
                Debug.LogError("Renderer component not found on Tower or TowerColored object.");
            }
        }
        else
        {
            Debug.LogError("TowerColored object is not assigned.");
        }
    }

    IEnumerator FirstSpawnDelay()
    {
        // Wait for 3-7 seconds before the first spawn
        yield return new WaitForSeconds(Random.Range(3.0f, 7.0f));

        // Reset timeSinceLastSpawn to 0 to start counting towards the first spawn
        timeSinceLastSpawn = 0;
    }


    IEnumerator ChangeTargetRandomly()
    {
        while (true)
        {
            if (gameObject.tag != "Player" && gameObject.tag != "NeutralTower") // Additional safety check
            {
                yield return new WaitForSeconds(Random.Range(5, 20)); // Wait for 5-20 seconds
                ChooseTarget(); // Change the target
            }
            else
            {
                yield return null; // Just yield if it's a player tower
            }
        }
    }


    private void ChooseTarget()
    {
        Tower enemyTower = GameController.Instance.GetRandomEnemyTower(gameObject.tag);
        if (enemyTower != null)
        {
            SetTarget(enemyTower.transform);
        }
    }


    void Update()
    {
        // Increment the timer
        timeSinceLastSpawn += Time.deltaTime;

        // Check if it's time to spawn a new soldier
        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnSoldier();
            timeSinceLastSpawn = 0;
            spawnInterval = Random.Range(spawnInterval * 0.90f, spawnInterval * 1.20f); // Randomize spawn interval 5-40%

        }
    }


    public void SetTarget(Transform target)
    {
        targetTower = target;

        // Get the PathController component and call StretchPathToTarget
        PathController pathController = GetComponent<PathController>();
        if (pathController != null)
        {
            pathController.StretchPathToTarget(target);
        }
        else
        {
            Debug.LogError("PathController component not found on Tower object.");
        }
    }

    void AssignTextComponent()
    {
        // Find the TMP_Text component in the children of this GameObject
        text = GetComponentInChildren<TMP_Text>();

        // Optional: Add a null check to ensure the component is found
        if (text == null)
        {
            Debug.LogError("TMP_Text component not found on tower's children.");
        }
    }

    void SpawnSoldier()
    {
        if (targetTower == null && gameObject.tag != "Player" && gameObject.tag != "NeutralTower")
        {
            ChooseTarget(); // Choose a target if none is set
        }


        if (targetTower != null)
        {
            // Calculate spawn position
            Vector3 spawnPosition = transform.position;
            spawnPosition.y = transform.position.y; // Keep the Y position unchanged

            // Spawn the soldier as a child of the tower
            GameObject newSoldier = Instantiate(soldierPrefab, spawnPosition, Quaternion.identity, transform);

            // Set createdByTower on the newSoldier
            UnitCollisions soldierCollisions = newSoldier.GetComponent<UnitCollisions>();
            if (soldierCollisions != null)
            {
                soldierCollisions.SetCreatedByTower(this);


                // Change the material of the helmet
                if (soldierCollisions.Helmet != null)
                {
                    Renderer helmetRenderer = soldierCollisions.Helmet.GetComponent<Renderer>();
                    if (helmetRenderer != null)
                    {
                        // Assuming you have a reference to the new material you want to set
                        Material newHelmetMaterial = GetComponent<Renderer>().material; // Assign the new material here
                        helmetRenderer.material = newHelmetMaterial;
                    }
                }
            }


            // Inherit the tag and material from the tower
            newSoldier.tag = gameObject.tag;
            if (GetComponent<Renderer>() != null && newSoldier.GetComponent<Renderer>() != null)
            {
                newSoldier.GetComponent<Renderer>().material = GetComponent<Renderer>().material;

            }

            // Randomize the speed of the soldier
            float randomSpeed = Random.Range(0.8f, 1.2f); // 20% variation

            // Set the enemy tower as the target for the soldier (if the soldier script requires it)
            UnitMovement soldierMovement = newSoldier.GetComponent<UnitMovement>();
            if (soldierMovement != null)
            {
                soldierMovement.target = targetTower;
                soldierMovement.speed *= randomSpeed; // Apply random speed

            }




        }

    }



    public void GetDamage(GameObject attacker, int damage)
    {
        health -= damage;
        UpdateUi();

        if (health <= 0)
        {
            if (attacker != null)
            {
                ChangeOwnership(attacker);
            }
            else
            {
                // Destroy(gameObject); // Optional: Destroy or handle the tower if there's no attacker
                Debug.LogError("Attacker was destroyed before changing ownership.");
            }
        }
    }



    private void ChangeOwnership(GameObject newOwner)
    {

        string oldTag = gameObject.tag;
        gameObject.tag = newOwner.tag;

        targetTower = null;
        // Change the tag
        tag = newOwner.tag;

        // Change the material if both have Renderer components
        Renderer ownRenderer = GetComponent<Renderer>();
        Renderer attackerRenderer = newOwner.GetComponent<Renderer>();
        if (ownRenderer != null && attackerRenderer != null)
        {
            ownRenderer.material = attackerRenderer.material;
            PaintTowerColored();
        }

        // Reset health to a default value (e.g., 10) or as needed
        health = 1;
        UpdateUi();

        // Notify GameController about the change
        GameController.Instance.UpdateTowerCount(oldTag, newOwner.tag);
    }

    private void UpdateUi()
    {
        if (text != null) // Check if text component is set
        {
            text.text = health.ToString();
        }
    }
}
