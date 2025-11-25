using UnityEngine;
using UnityEngine.Rendering;

public class Shooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform target;

    public float shootRate;
    public float basketballMoveSpeed;
    private float shootTimer;

    private void Update()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0)
        {
            shootTimer = shootRate;
            Basketball basketball = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Basketball>();
            basketball.InitialiseBasketball(target, basketballMoveSpeed);
        }
    }
}
