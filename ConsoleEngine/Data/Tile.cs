using ConsoleEngine.Services;
using ConsoleEngine.Properties;

namespace ConsoleEngine.Data
{
  public class Tile
  {
    // Properties
    public char Representation;
    public bool Steppable;
    public Scene? ParentScene;
    public Vector Pos = Vector.Null;
    public Inventory? Inventory;
    public Attributes Attributes;
    public Tile Value
    {
      get
      {
        Tile ToReturn = new Tile(this.Representation, this.Steppable);
        ToReturn.Inventory = this.Inventory;

        // copy events
        if (this.OnStep != null) { ToReturn.OnStep = new Action<Tile, Entity>(this.OnStep); }
        if (this.OnStepOff != null) { ToReturn.OnStepOff = new Action<Tile, Entity>(this.OnStepOff); }
        if (this.OnTick != null) { ToReturn.OnTick = new Action<Tile>(this.OnTick); }
        if (this.OnInputTick != null) { ToReturn.OnInputTick = new Action<Tile, ConsoleKey>(this.OnInputTick); }

        // copy attr
        ToReturn.ConnectAttributes(this.Attributes.Value);

        // Return
        return ToReturn;
      }
    }

    // Constructor
    public Tile(char rep, bool steppable = false)
    {
      Representation = rep;
      Steppable = steppable;
      this.Attributes = new Attributes(this);
    }

    // Methods

    private void ThrowIfNotInScene()
    {
      if (!IsSceneMember) { throw new Exception($"Expected Tile {Representation} to be in a Scene!"); }
    }
    public void ResetAttributes()
    {
      var oldattr = this.Attributes;
      this.Attributes = new Attributes(this);
      if ((Tile)oldattr.Parent == this) { oldattr.DisconnectParent(); }
    }
    public void ConnectAttributes(Attributes attr)
    {
      ResetAttributes();
      this.Attributes = attr;
      if ((Tile)attr.Parent != this) { attr.ConnectParent(this); }
    }

    public Tile?[,] GetSurroundingTiles()
    {
      ThrowIfNotInScene();
      Tile?[,] SurroundingTiles = new Tile?[3, 3];

      for (int oy = -1; oy <= 1; oy++)
      {
        for (int ox = -1; ox <= 1; ox++)
        {
          int ix = ox + 1;
          int iy = oy + 1;
          int tx = Pos.X + ox;
          int ty = Pos.Y + oy;
          if (Game.ValidatePos(tx, ty))
          {
            SurroundingTiles[ix, iy] = ParentScene.Matrix[tx, ty];
          }
          else
          {
            SurroundingTiles[ix, iy] = null;
          }
        }
      }
      return SurroundingTiles;
    }

    public void Update()
    {
      Game.UpdateTile(this);
    }

    // More properties
    public bool IsSceneMember { get { return ParentScene != null; } }
    public Tile? Above
    {
      get
      {
        ThrowIfNotInScene();
        if (Game.ValidatePos(Pos.X, Pos.Y - 1))
        {
          return ParentScene.Matrix[Pos.X, Pos.Y - 1];
        }
        else
        {
          return null;
        }
      }
    }
    public Tile? Below
    {
      get
      {
        ThrowIfNotInScene();
        if (Game.ValidatePos(Pos.X, Pos.Y + 1))
        {
          return ParentScene.Matrix[Pos.X, Pos.Y + 1];
        }
        else
        {
          return null;
        }
      }
    }
    public Tile? Left
    {
      get
      {
        ThrowIfNotInScene();
        if (Game.ValidatePos(Pos.X - 1, Pos.Y))
        {
          return ParentScene.Matrix[Pos.X - 1, Pos.Y];
        }
        else
        {
          return null;
        }
      }
    }
    public Tile? Right
    {
      get
      {
        ThrowIfNotInScene();
        if (Game.ValidatePos(Pos.X + 1, Pos.Y))
        {
          return ParentScene.Matrix[Pos.X + 1, Pos.Y];
        }
        else
        {
          return null;
        }
      }
    }

    // Events
    public Action<Tile> OnTick { get; set; }
    public Action<Tile, ConsoleKey> OnInputTick { get; set; }
    public Action<Tile, Entity> OnStep { get; set; }
    public Action<Tile, Entity> OnStepOff { get; set; }
    public Action<Tile> OnEachSecond { get; set; }
    public Action<Tile> OnSceneLoad { get; set; }

    // Predefined Tiles
    public static class Defaults
    {
      // Static Tiles
      public static Tile Air { get { return new Tile(' ', true); } }
      public static Tile Wall { get { return new Tile('#', false); } }

      // Interactive Tiles
      public static Tile Bamboo
      {
        get
        {
          Tile ToReturn = new Tile('|', true);
          ToReturn.OnStep = (Tile self, Entity e) =>
          {
            if (!e.IsPlayer) { return; }
            if (self.Representation == '|' && e.Inventory != null)
            {
              if (!e.Inventory.HasItemOfName("Bamboo"))
              {
                UI.Popup("I got a bamboo shoot! I wonder if it can be used for something?", true);
              }
              e.GiveItem(Item.Defaults.Bamboo);
            }
            self.Representation = '.';
          };
          ToReturn.OnStepOff = (Tile self, Entity e) =>
          {
            if (!(e.IsPlayer)) { return; }
            self.Update();
          };
          return ToReturn;
        }
      }
      public static Tile Switch
      {
        get
        {
          var ToReturn = new Tile('\\');
          ToReturn.Attributes.Set("Flipped", false);
          ToReturn.Attributes.Set("OnFlip", null);
          ToReturn.OnStep = (self, stepper) =>
          {
            if (!stepper.IsPlayer) { return; }
            var flipped = (bool)self.Attributes.Get("Flipped");
            if (flipped)
            {
              self.Attributes.Set("Flipped", false);
              self.Representation = '\\';
            }
            else
            {
              self.Attributes.Set("Flipped", true);
              self.Representation = '/';
            }
            self.Update();
            ((Action<Tile, Entity>)self.Attributes.Get("OnFlip"))?.Invoke(self, stepper);
          };
          return ToReturn;
        }
      }
      public static Tile ItemHolder
      {
        get
        {
          var ToReturn = new Tile('*', true);
          ToReturn.Inventory = new Inventory(1);

          ToReturn.OnStep = (self, stepper) =>
          {
            if (self.Inventory.Count == 0 || self.Inventory.Capacity == 0) { return; }
            if (stepper.Inventory == null) { return; }
            if (stepper.Inventory.Count == stepper.Inventory.Capacity) { return; }

            foreach (var item in self.Inventory.GetAllItems())
            {
              stepper.GiveItem(item);
            }

            self.ParentScene.Matrix[self.Pos.X, self.Pos.Y] = Tile.Defaults.Air;
          };

          return ToReturn;
        }
      }
    }
  }

}