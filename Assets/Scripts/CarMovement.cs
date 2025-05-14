using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private float speed;
    private float lifetime;
    private Vector3 moveDirection; // Direction relative to the car's local space

    private float timeAlive = 0f;

    // Initialize is called by the Spawner
    public void Initialize(float carSpeed, float carLifetime, Vector3 direction)
    {
        speed = carSpeed;
        lifetime = carLifetime;
        moveDirection = direction.normalized; // Ensure it's a unit vector
    }

    void Update()
    {
        // 1. Move the car forward (relative to its own orientation)
        // transform.Translate(Vector3.forward * speed * Time.deltaTime); // Simple forward movement
        // Or, move in the specified direction relative to its initial spawn orientation
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.Self);


        // 2. Track lifetime and despawn
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
            Debug.Log($"{gameObject.name} despawned after {lifetime} seconds.");
        }
    }

    // Optional: If your cars have Rigidbody components and you want physics-based movement
    // void FixedUpdate()
    // {
    //     if (GetComponent<Rigidbody>())
    //     {
    //         GetComponent<Rigidbody>().MovePosition(transform.position + transform.TransformDirection(moveDirection) * speed * Time.fixedDeltaTime);
    //     }
    // }
}