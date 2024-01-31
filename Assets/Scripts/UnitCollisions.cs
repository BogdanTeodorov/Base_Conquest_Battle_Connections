using UnityEngine;

public class UnitCollisions : MonoBehaviour
{
    // Health of the unit
    public int health = 1; // Default to 1 for units

    public GameObject Helmet;
    public GameObject smokeEffectPrefab; // Prefab for the smoke effect


    public Tower createdByTower;

    void OnTriggerEnter(Collider other)
    {
        if (health > 0)
        {
            // Check if the collided object is not the home tower
            if ((createdByTower != null)
            && (other.gameObject != createdByTower.gameObject)
            && (gameObject.tag == other.gameObject.tag)
            && (other.gameObject.GetComponent<UnitCollisions>() == null)

            )
            {
                ChangeHealth(other.gameObject, -1);
            }

            // Check if the collided object has an opposing tag

            if (gameObject.tag != other.gameObject.tag)

            {
                ChangeHealth(other.gameObject, 1);
            }
        }
    }

    // Setter method for createdByTower
    public void SetCreatedByTower(Tower tower)
    {
        createdByTower = tower;
    }



    public void ChangeHealth(GameObject target, int value)
    {
        if (target.TryGetComponent<UnitCollisions>(out var targetUnit))
        {
            targetUnit.GetDamage(gameObject, value); // Pass this unit as the attacker
        }
        else if (target.TryGetComponent<Tower>(out var targetTower))
        {
            targetTower.GetDamage(gameObject, value); // Pass this unit as the attacker
        }

        // Self-damage for the attacking unit
        GetDamage(null, 1);
    }

    public void GetDamage(GameObject attacker, int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            // Instantiate the smoke effect at the unit's position and rotation
            if (smokeEffectPrefab != null)
            {
                Instantiate(smokeEffectPrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}
