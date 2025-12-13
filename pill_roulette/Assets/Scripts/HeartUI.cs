using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [Header("Hearts")]
    public Image[] hearts;          // size = 3 in Inspector
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private int maxHearts;

    private void Awake()
    {
        if (hearts == null)
        {
            maxHearts = hearts.Length;
        }

        
    }

    public void SetHearts(int currentHearts)
    {
        if (hearts == null || hearts.Length == 0) return;

        if (maxHearts == 0)
        {
            maxHearts = hearts.Length;
        }

        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHearts)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }
}
