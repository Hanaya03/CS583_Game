using UnityEngine;

public class Sushi : MonoBehaviour
{
    public bool IsPoisoned { get; private set; }

    public void Poison()
    {
        IsPoisoned = true;
        var r = GetComponent<Renderer>();
        if (r) r.material.color = Color.red;  
    }

    public void ClearPoison()
    {
        IsPoisoned = false;
        var r = GetComponent<Renderer>();
        if (r) r.material.color = Color.white;
    }

    public void Eat()
    {
        gameObject.SetActive(false); // removes from play for the current round
    }
}
