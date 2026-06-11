using ConsoleEngine.Services;
using ConsoleEngine.Properties;

namespace ConsoleEngine.Data
{
  public class Entity
  {
    // Properties
    public Vector Pos;
    public char Representation;
    public Scene? ParentScene;
    public Attributes Attributes;
    public bool IsPlayer { get { return this.GetType() == typeof(Player); } }
    public Inventory? Inventory;
    public Tile StandingOn
    {
      get
      {
        Game.ThrowIfInvalidPos(Pos);
        if (ParentScene == null) { throw new Exception("Cannot get tile entity is standing on if it isn't added to a scene!"); }
        return ParentScene.Matrix[Pos.X, Pos.Y];
      }
    }


    // Constructor
    public Entity(char rep, int x, int y, Scene? parentScene = null)
    {
      Representation = rep;
      Pos = new Vector(x, y);
      this.Attributes = new Attributes(this);
      if (parentScene != null) { this.SwitchScene(parentScene); }
    }
    public Entity(char rep, Vector v, Scene? parentScene = null) : this(rep, v.X, v.Y, parentScene) { }

    public Entity(char rep, Scene? parentScene = null) : this(rep, Vector.Center, parentScene) { }

    // Methods
    public Vector DistanceTo(Vector v)
    {
      return (v - this.Pos).Abs();
    }
    public Vector DistanceTo(int x, int y)
    {
      return DistanceTo(new Vector(x, y));
    }

    // Movement Code (Reduces redraws)
    public void MoveTo(int x, int y)
    {
      if (!Game.ValidatePos(x, y)) { return; }
      if (!(ParentScene == null))
      {
        var OldTile = ParentScene.Matrix[Pos.X, Pos.Y];
        OldTile.OnStepOff?.Invoke(OldTile, this);
        var NewTile = ParentScene.Matrix[x, y];
        if (!(NewTile.Steppable))
        {
          NewTile.OnStep?.Invoke(NewTile, this);
          return;
        }
        if (ParentScene.Visible)
        {
          Render.Write(Pos.X, Pos.Y, OldTile.Representation, true);
          Render.Write(x, y, Representation, true);
        }
        Pos = new Vector(x, y);
        NewTile.OnStep?.Invoke(NewTile, this);
      }
      else
      {
        Pos = new Vector(x, y);
      }
    }
    public async void WalkToXValue(int x, int speed = 3, Action<Entity>? OnReach = null)
    {
      while (this.Pos.X != x)
      {
        MoveTo(this.Pos.X + (this.Pos.X > x ? -1 : 1), this.Pos.Y);
        await Task.Delay(1000 / speed);
      }
      OnReach?.Invoke(this);
    }
    public async Task WalkToYValue(int y, int speed = 3, Action<Entity>? OnReach = null)
    {
      while (this.Pos.Y != y)
      {
        MoveTo(this.Pos.X, this.Pos.Y + (this.Pos.Y > y ? -1 : 1));
        await Task.Delay(1000 / speed);
      }
      OnReach?.Invoke(this);
    }
    public void WalkTo(int x, int y, int speed = 3, Action<Entity>? OnReach = null)
    {
      Task.Run(() => { WalkToXValue(x, speed); });
      Task.Run(() => { WalkToYValue(y, speed); });
      OnReach?.Invoke(this);
    }

    // Scene adding code ???
    public void SwitchScene(Scene NewScene)
    {
      if (!(ParentScene == null))
      {
        // Unrenders the entity if old scene is still visible
        if (ParentScene.Visible)
        {
          Tile OldTile = ParentScene.Matrix[Pos.X, Pos.Y];
          Render.Write(Pos.X, Pos.Y, OldTile.Representation, true);
        }

        ParentScene.Entities.Remove(this);
      }

      // Move Plr to Entry Point
      if (this.IsPlayer)
      {
        Pos = NewScene.PlrStartPos.Value;
      }

      // Render entity on new scene if it's visible
      if (NewScene.Visible)
      {
        Render.Write(Pos.X, Pos.Y, Representation, true);
      }

      NewScene.Entities.Add(this);
      ParentScene = NewScene;
    }

    public void MoveToScene(Scene NewScene, Vector NewPos)
    {
      SwitchScene(NewScene);
      MoveTo(NewPos.X, NewPos.Y);
    }

    // Inventory Management
    public bool GiveItem(Item it)
    {
      ThrowIfNoInventory();
      it.AddTo(this.Inventory);
      it.OnPickUp?.Invoke(it, this);
      return true;
    }
    public bool Discard(Item it)
    {
      ThrowIfNoInventory();
      bool success = this.Inventory.Remove(it);
      if (success) { it.OnDiscard?.Invoke(it, this); }
      return success;
    }
    public void ClearInventory(bool TriggerDiscardEvents = false)
    {
      ThrowIfNoInventory();
      if (TriggerDiscardEvents)
      {
        foreach (Item it in this.Inventory.GetAllItems())
        {
          it.OnDiscard?.Invoke(it, this);
        }
      }
      this.Inventory.Clear();
    }
    private void ThrowIfNoInventory()
    {
      if (this.Inventory == null)
      {
        throw new Exception("Cannot perform inventory operations on entity with no inventory.");
      }
    }

    // Attribute Management
    public void ResetAttributes()
    {
      var oldattr = this.Attributes;
      this.Attributes = new Attributes(this);
      if ((Entity)oldattr.Parent == this) { oldattr.DisconnectParent(); }
    }
    public void ConnectAttributes(Attributes attr)
    {
      ResetAttributes();
      this.Attributes = attr;
      if ((Entity)attr.Parent != this) { attr.ConnectParent(this); }
    }

    // Events

    public Action<Entity, ConsoleKey> OnInputTick { get; set; }
    public Action<Entity> OnTick { get; set; }
    public Action<Entity> OnEachSecond { get; set; }
    public Action<Entity> BehaviorLoop { get; set; }
    public Action<Entity> OnSceneLoad { get; set; }
  }

}