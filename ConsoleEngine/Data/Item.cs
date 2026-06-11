using ConsoleEngine.Services;
using ConsoleEngine.Properties;

namespace ConsoleEngine.Data
{
  public class Item
  {
    // Properties
    public readonly string Name;
    public string DisplayName;
    public Inventory? ParentInventory;
    public string Description;

    // Constructors
    public Item(string name, string description = "Nothing to see here", Inventory? parent = null)
    {
      Name = name;
      DisplayName = name;
      Description = description;

      // Add to parent if specified
      if (parent != null)
      {
        bool success = parent.Add(this);
        if (!success)
        {
          throw new Exception("Item can't be constructed because target inventory is full!");
        }
      }
    }

    // Methods
    public bool MoveTo(Inventory NewInv)
    {
      Inventory? OldInv = ParentInventory;
      bool success = NewInv.Add(this);
      if (success && OldInv != null) { OldInv.Remove(this); }
      return success;
    }
    public void RemoveFromInventory()
    {
      if (ParentInventory == null) { throw new Exception("Can't remove item from empty inventory"); }
      ParentInventory.Remove(this);
    }
    public bool AddTo(Inventory inv) => MoveTo(inv);

    public void Use(Entity e) => OnUse?.Invoke(this, e);

    // Events
    public Action<Item, Entity> OnUse { get; set; }
    public Action<Item, Entity> OnPickUp { get; set; }
    public Action<Item, Entity> OnDiscard { get; set; }

    // Predefined Items
    public static class Defaults
    {
      public static Item Bamboo
      {
        get
        {
          var ToReturn = new Item("Bamboo", "Delicious crunchy snack");
          return ToReturn;
        }
      }
      public static Item Key { get { return new Item("Key", "The Guard's key! He must've dropped it!"); } }
      public static Item Gift { get { return new Item("Gift", "Giving it someone would surely make them very happy!"); } }
    }
  }

}