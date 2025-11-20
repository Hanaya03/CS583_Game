using UnityEngine;

public class Item : MonoBehaviour
{
    private bool _poisoned = false;
    public bool IsPoisoned => _poisoned;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    * public function that sets the poisoned boolean of this gameobject.
    */
    public void PoisonItem()
    {
        _poisoned = true;
    }

    /*
    * public function that destroys this GameObject.
    */
    public void EatItem()
    {
        Object.Destroy(this.gameObject);
    }
}
