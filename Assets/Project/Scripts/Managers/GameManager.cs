using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    int progress;
    public Slider progressBar;
    void Start()
    {
        progress = 0;
        progressBar.value = 0;
        Coin.OnCoinCollect += IncreaseProgress;

    }

    // Update is called once per frame
    void Update()
    {

    }
    void IncreaseProgress(int amount)
    {
        progress += amount;
        progressBar.value = progress;
        if (progress >= 100)
        {
            Debug.Log("Level Complete!");
        }
    }
}
