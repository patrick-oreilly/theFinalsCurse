using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OggerEnter2D(Collider2D collision)
    {
        IItem item = collision.GetComponent<IItem>();
        if (item != null)
        {
            item.Collect();
        }


    }
}
