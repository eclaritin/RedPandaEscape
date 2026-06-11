using ConsoleEngine.Data;
using ConsoleEngine.Services;

namespace ConsoleEngine.Properties
{
  public class Inventory
  {
    // Properties
    private List<Item> _inv;
    public int Capacity
    {
      get { return _inv.Capacity; }
      set
      {
        if (value < 0) { throw new Exception($"Inventory capacity cannot be negative. Got {value}"); }
        if (value < _inv.Count) { throw new Exception("Inventory capacity cannot be smaller than amount of items."); }

        _inv.Capacity = value;
      }
    }
    public int Count { get { return _inv.Count; } }

    // Constructors
    public Inventory(int Capacity = 1)
    {
      if (Capacity < 0) { throw new Exception($"Inventory cannot have negative capacity, {Capacity}."); }
      _inv = new List<Item>(Capacity);
    }
    public Inventory(Item[] items)
    {
      _inv = new List<Item>(items.Length);

      foreach (Item i in items)
      {
        if (i == null) { continue; }
        this.Add(i);
      }
    }
    public Inventory(Item[] items, int Capacity)
    {
      if (Capacity < 0) { throw new Exception($"Inventory cannot have negative capacity, {Capacity}."); }
      _inv = new List<Item>(Capacity);

      foreach (Item i in items)
      {
        if (i == null) { continue; }
        this.Add(i);
      }
    }

    // Methods
    public bool Add(Item it)
    {
      if (Count == Capacity) { return false; }
      _inv.Add(it);
      it.ParentInventory = this;
      return true;
    }
    public bool Remove(Item it)
    {
      it.ParentInventory = null;
      return _inv.Remove(it);
    }
    public bool Has(Item it)
    {
      return _inv.Contains(it);
    }
    public void Clear()
    {
      _inv.Clear();
    }
    public List<Item> GetAllItems()
    {
      return new List<Item>(_inv);
    }
    public Item? GetItemByName(string name)
    {
      foreach (var it in _inv)
      {
        if (it.Name == name)
        {
          return it;
        }
      }
      return null;
    }
    public bool HasItemOfName(string name)
    {
      foreach (var it in _inv)
      {
        if (it.Name == name)
        {
          return true;
        }
      }
      return false;
    }
  }

}