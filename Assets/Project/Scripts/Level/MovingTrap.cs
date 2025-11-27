using UnityEngine;

public class MovingTrap : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 moveOffset = new Vector3(5f, 0f, 0f);
    public float speed = 2f;
    public float waitTime = 0.5f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
    }

    void Update()
    {
        Vector3 destination = movingToTarget ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            if (waitTime > 0)
            {
                // Simple wait logic could go here, but for now let's just flip immediately or use a coroutine if needed.
                // For simplicity in this script, we just flip direction.
            }
            movingToTarget = !movingToTarget;
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPos, targetPos);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + moveOffset);
        }
    }
}
