//// Subclasses.cs
/// Classes in this file are meant to be used for game development specifically,
/// they are not necessary for the core functions of the engine.

using ConsoleEngine.Services;
using ConsoleEngine.Properties;

namespace ConsoleEngine.Data
{
  public class Door : Tile
  {
    // Constructor
    public Door(char rep, Scene? LeadsTo = null, bool IsLocked = true, string? keyItem = null, Vector? NextDoorPos = null) : base(rep, !IsLocked)
    {
      this.Attributes.Set("Locked", IsLocked);
      this.Attributes.Set("LeadsTo", LeadsTo);
      this.Attributes.Set("NextDoorPos", NextDoorPos);
      this.Attributes.Set("KeyItemName", keyItem);
      this.Attributes.Set("preventEntrance", false);
      this.Attributes.Set("originalRep", rep);
      this.Attributes.Set("OnUnlock", new Action<Tile>((_) =>
      {
      }));

      this.Attributes.ListenTo("Locked", (objSelf, obj) =>
      {
        var self = objSelf as Tile;
        if (self == null) return;
        var value = (bool)obj;
        self.Attributes.Set("preventEntrance", true);
        self.Representation = value ? (char)self.Attributes.Get("originalRep") : ' ';
        self.Steppable = !value;
        if (self.ParentScene == Game.CurrentScene) { self.Update(); }
        foreach (var ThisTile in self.GetSurroundingTiles())
        {
          if (ThisTile == null) { continue; }
          if (!ThisTile.Attributes.Exists("NextDoorPos")) { continue; }
          if ((bool)ThisTile.Attributes.Get("Locked") == value) { continue; }

          ThisTile.Attributes.Set("Locked", value);
          ThisTile.Representation = value ? (char)ThisTile.Attributes.Get("originalRep") : ' ';
          ThisTile.Steppable = !value;
          if (self.ParentScene == Game.CurrentScene) { ThisTile.Update(); }


        }
        if (!value && self.Attributes.Get("OnUnlock") != null)
        {
          ((Action<Tile>)self.Attributes.Get("OnUnlock")).Invoke(self);
        }
        self.Attributes.Set("preventEntrance", false);
      });
      this.OnStep = (self, e) =>
      {
        if (!e.IsPlayer) { return; }
        if ((bool)self.Attributes.Get("Locked") == true)
        {
          if (self.Attributes.Get("KeyItemName") == null) { return; }
          if (e.Inventory == null) { return; }
          ///////////////////////////////////////////

          if (e.Inventory.HasItemOfName((string)self.Attributes.Get("KeyItemName")))
          {
            UI.Popup("The door made a clicking sound...", true);
            self.Attributes.Set("Locked", false);
          }
          else
          {
            UI.Popup("The door won't budge!", true);
          }
          return;
        }

        if ((bool)self.Attributes.Get("preventEntrance")) { return; }
        if (self.Attributes.Get("LeadsTo") == null) { return; }
        e.MoveToScene((Scene)self.Attributes.Get("LeadsTo"), self.Attributes.Get("NextDoorPos") == null ? ((Scene)self.Attributes.Get("LeadsTo")).PlrStartPos : (Vector)self.Attributes.Get("NextDoorPos"));
        Render.RenderScene((Scene)self.Attributes.Get("LeadsTo"));
      };
    }

    // Static Methods
    public static Door[] MakeHorizontalDoor(Scene? NextScene = null, bool Locked = true, string? keyItem = null, Vector? NextPos = null)
    {
      Door[] NewDoor = new Door[2]
      {
                new Door('-',NextScene,Locked,keyItem,NextPos),
                new Door('-',NextScene,Locked,keyItem,NextPos)
      };
      return NewDoor;
    }
    public static Door[] MakeVerticalDoor(Scene? NextScene = null, bool Locked = true, string? keyItem = null, Vector? NextPos = null)
    {
      Door[] NewDoor = new Door[2]
      {
                new Door('|',NextScene,Locked,keyItem,NextPos),
                new Door('|',NextScene,Locked,keyItem,NextPos)
      };
      return NewDoor;
    }
  }

  public class Player : Entity
  {
    // Properties
    public string Name;
    public List<string> Decisions;
    public bool CanMove;

    // Constructor
    public Player(string name) : base('@', 32, 8)
    {
      Name = name;
      Decisions = new List<string>();
      CanMove = true;
    }

    // Methods
    public void EnableWASDMovement()
    {
      var Plr = Game.Plr;
      OnInputTick = (self, Key) =>
      {
        if (!CanMove) { return; }
        switch (Key)
        {
          case ConsoleKey.W: // up
            Plr.MoveTo(Plr.Pos.X, Plr.Pos.Y - 1);
            break;
          case ConsoleKey.S: // down
            Plr.MoveTo(Plr.Pos.X, Plr.Pos.Y + 1);
            break;
          case ConsoleKey.A: // left
            Plr.MoveTo(Plr.Pos.X - 1, Plr.Pos.Y);
            break;
          case ConsoleKey.D: // right
            Plr.MoveTo(Plr.Pos.X + 1, Plr.Pos.Y);
            break;
          default:
            return;
        }
      };
    }

  }

  public class Animal : Entity
  {
    // Constructor
    public Animal(char representation, Scene? parentScene = null) : base(representation, parentScene)
    {
      this.Attributes.Set("Mode", "Idle");
      this.Attributes.Set("WalkAgain", true);
      this.Attributes.Set("Direction", 0);
      this.OnSceneLoad = (self) =>
      {
        this.Attributes.Set("Mode", "Idle");
      };
      this.OnEachSecond = (self) =>
      {
        var dir = (int)self.Attributes.Get("Direction");
        Vector nextPos;
        switch (dir)
        {
          case 0:
            nextPos = new Vector(self.Pos.X, self.Pos.Y - 1);
            break;
          case 1:
            nextPos = new Vector(self.Pos.X + 1, self.Pos.Y);
            break;
          case 2:
            nextPos = new Vector(self.Pos.X, self.Pos.Y + 1);
            break;
          case 3:
            nextPos = new Vector(self.Pos.X - 1, self.Pos.Y);
            break;
          default:
            return;
        }
        bool changedir = false;

        if (!Game.ValidatePos(nextPos)) { changedir = true; }
        else if (Game.CurrentScene.GetTileAt(nextPos.X, nextPos.Y).Steppable == false) { changedir = true; }

        if (changedir)
        {
          if (dir == 3) { self.Attributes.Set("Direction", 0); return; }
          self.Attributes.Set("Direction", dir + 1);
        }
        else
        {
          self.MoveTo(nextPos.X, nextPos.Y);
        }
      };
    }
  }
}