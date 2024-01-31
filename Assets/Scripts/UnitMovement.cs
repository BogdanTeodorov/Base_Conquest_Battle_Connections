using UnityEngine;
public class UnitMovement : MonoBehaviour
{
    // Target to move towards - the enemy tower
    public Transform target;

    // Movement speed
    public float speed = 5f;

    void Update()
    {
        MoveTowardsTarget();
    }

    void MoveTowardsTarget()
    {
        if (target != null)
        {
            // Move towards the target
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            RotateTowardsTarget();
        }
    }

    void RotateTowardsTarget()
    {
        // Calculate the direction to the target
        Vector3 targetDirection = target.position - transform.position;
        targetDirection.y = 0; // Keep the unit upright, ignore the Y component

        // Check if the direction is not a zero vector
        if (targetDirection != Vector3.zero)
        {
            // Calculate the rotation needed to face the target
            float angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);

            // Apply the rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed);
        }
    }
}

