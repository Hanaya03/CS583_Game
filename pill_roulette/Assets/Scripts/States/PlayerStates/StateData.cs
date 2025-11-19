using UnityEngine;

public class StateData
{
    private int _health = 1;
    public int Health{get{return _health;} set{_health = value;}}
    private Ray ray;
    public Ray RAY{set{ray = value;} get{return ray;}}
    private GameObject _selectedGO;
    public GameObject SelectedGO{set{_selectedGO = value;} get{return _selectedGO;}}
    private Item _selectedItm;
    public Item SelectedItm{set{_selectedItm = value;} get{return _selectedItm;}}
 
    private LayerMask _itemLayer;
    public LayerMask ItemLayer => _itemLayer;
    private SushiBoard _sboard;
    public SushiBoard Sboard => _sboard;

    public StateData(LayerMask m, SushiBoard b)
    {
        _itemLayer = m;
        _sboard = b;
    }
}