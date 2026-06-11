using ConsoleEngine.Properties;
using ConsoleEngine.Services;

namespace ConsoleEngine.Data
{
  public class Scene
  {
    // Properties
    public Tile[,] Matrix;
    public bool LoadedBefore = false;
    public List<Entity> Entities;
    public string Description = "";
    public bool Update = true;
    public bool Visible = false;
    public Vector PlrStartPos;

    // Constructor
    public Scene(string description = "")
    {
      Matrix = new Tile[Game.Size[0], Game.Size[1]];
      Entities = new List<Entity>();
      Description = description;
      PlrStartPos = Vector.Center;

      // Fill Matrix with Air tiles
      SetAllTiles(Tile.Defaults.Air);
    }

    // Tile Manipulation
    public void SetTile(int x, int y, Tile MyTile)
    {
      var WorkingTile = MyTile.Value;
      Matrix[x, y] = WorkingTile;
      WorkingTile.ParentScene = this;
      WorkingTile.Pos = new Vector(x, y);
    }

    public void SetTileRange(Vector StartPos, Vector EndPos, Tile MyTile)
    {
      for (int x = StartPos.X; x <= EndPos.X; x++)
      {
        for (int y = StartPos.Y; y <= EndPos.Y; y++)
        {
          SetTile(x, y, MyTile);
        }
      }
    }

    public void SetAllTiles(Tile MyTile)
    {
      SetTileRange(
          new Vector(),
          new Vector(Game.Size[0] - 1, Game.Size[1] - 1),
          MyTile
      );
    }

    public void SetTileBox(Vector StartPos, Vector EndPos, Tile MyTile)
    {
      this.SetTileRange(
      new Vector(StartPos.X, StartPos.Y),
      new Vector(EndPos.X, StartPos.Y),
      Tile.Defaults.Wall
      );

      this.SetTileRange(
          new Vector(EndPos.X, StartPos.Y),
          new Vector(EndPos.X, EndPos.Y),
          Tile.Defaults.Wall
      );

      this.SetTileRange(
          new Vector(StartPos.X, EndPos.Y),
          new Vector(EndPos.X, EndPos.Y),
          Tile.Defaults.Wall
      );

      this.SetTileRange(
          new Vector(StartPos.X, StartPos.Y),
          new Vector(StartPos.X, EndPos.Y),
          Tile.Defaults.Wall
      );
    }

    public Tile? GetTileAt(int x, int y)
    {
      if (Game.ValidatePos(x, y))
      {
        return Matrix[x, y];
      }
      else
      {
        return null;
      }
    }

    // Events
    public Action OnRender { get; set; }
    public Action OnTick { get; set; }
    public Action<ConsoleKey> OnInputTick { get; set; }
    public Action OnEachSecond { get; set; }
  }
}