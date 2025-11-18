using UnityEngine;

public class Coin : MonoBehaviour, IItem
{

    public void Collect()
    {
        Destroy(gameObject);
    }

}
