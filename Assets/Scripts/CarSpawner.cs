using UnityEngine;
using System.Collections.Generic; // Required for using Lists

public class CarSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public List<GameObject> carPrefabs; // Assign your car prefabs in the Inspector
    public float spawnInterval = 3f;    // Time between each spawn
    public float minSpawnDelay = 1f;    // Minimum random delay added to spawnInterval
    public float maxSpawnDelay = 3f;    // Maximum random delay added to spawnInterval

    [Header("Car Movement Settings")]
    public float carSpeed = 10f;
    public float despawnTime = 10f;     // Time in seconds before the car despawns
    public Vector3 movementDirection = Vector3.forward; // Default to forward, can be customized

    private float nextSpawnTime;

    void Start()
    {
        // Initialize the next spawn time
        SetNextSpawnTime();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnCar();
            SetNextSpawnTime();
        }
    }

    void SetNextSpawnTime()
    {
        // Set the next spawn time with a base interval and a random additional delay
        nextSpawnTime = Time.time + spawnInterval + Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    void SpawnCar()
    {
        if (carPrefabs == null || carPrefabs.Count == 0)
        {
           // Debug.LogWarning("No car prefabs assigned to the spawner!");
            return;
        }

        // 1. Select a random car prefab from the list
        int randomIndex = Random.Range(0, carPrefabs.Count);
        GameObject selectedCarPrefab = carPrefabs[randomIndex];

        // 2. Instantiate the car at the spawner's position and rotation
        // The car will move based on its own forward direction relative to the spawner's orientation.
        GameObject newCar = Instantiate(selectedCarPrefab, transform.position, transform.rotation);

        // 3. Add a script to the car to handle its movement and despawning
        CarMovement carMovement = newCar.AddComponent<CarMovement>();
        carMovement.Initialize(carSpeed, despawnTime, movementDirection);

      //  Debug.Log($"Spawned {newCar.name} at {transform.position}");
    }

    // Optional: Visualize the spawn point and direction in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f); // Show spawn point

        // Draw an arrow indicating the spawn direction based on the spawner's rotation and movementDirection
        Vector3 worldMovementDirection = transform.TransformDirection(movementDirection.normalized);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + worldMovementDirection * 2f); // Show spawn direction
    }
}