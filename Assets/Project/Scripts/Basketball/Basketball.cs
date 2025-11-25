using UnityEngine;

public class Basketball : MonoBehaviour
{
    public Transform target;
    public float moveSpeed;

    private float distanceToTargetToDestroyBasketball = 1f;

    private void Update()
    {
        Vector3 moveDirNormalized = (target.position - transform.position).normalized;
        transform.position += moveDirNormalized * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < distanceToTargetToDestroyBasketball)
        {
            Destroy(gameObject);
        }

    }

    public void InitialiseBasketball(Transform target, float moveSpeed)
    {
        this.target = target;
        this.moveSpeed = moveSpeed;
    }

}
