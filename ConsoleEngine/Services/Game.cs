using ConsoleEngine.Data;
using ConsoleEngine.Properties;

namespace ConsoleEngine.Services
{
  public static class Game
  {
    /// Properties ///

    // Settings
    public static int[] Size = { 64, 16 };
    public static int EntryPoint = 0; // index of the scene the player should start in

    // Game Flags
    public static bool GameRunning;
    public static bool Halt; // pauses the tick loop when set to true

    // World Objects & Scenes
    public static List<Scene> Scenes = new List<Scene>();
    public static Player? Plr;
    public static Scene? CurrentScene;

    /// Methods ///

    // Game Loop

    public static void Start()
    {
      // Move player to spawn location
      if (Plr == null) { throw new Exception("There's no player registered in Game.Plr"); }
      if (Scenes.Count == 0) { throw new Exception("There are no scenes registered in Game.Scenes"); }
      Scene StartScene = Scenes[EntryPoint];
      Game.Plr.SwitchScene(StartScene);

      // Begin game loops
      Game.GameRunning = true;
      Game.Halt = false;
      Render.RenderScene(Scenes[EntryPoint]);
      Game.EachSecondLoop();
      while (GameRunning)
      {
        if (CurrentScene == null) { continue; }
        Game.GameLoop();
      }
    }
    public static void GameLoop()
    {
      while (GameRunning)
      {
        if (Game.Halt) { continue; }
        // get input
        var Input = Console.ReadKey(true).Key;

        // Redraw button
        if (Input == ConsoleKey.F5)
        {
          if (CurrentScene != null) { CurrentScene.Update = true; }
        }

        // Fire input tick events
        if (!(CurrentScene == null))
        {
          try
          {
            foreach (var e in CurrentScene.Entities)
            {
              e.OnInputTick?.Invoke(e, Input);
            }
            foreach (var t in CurrentScene.Matrix)
            {
              t.OnInputTick?.Invoke(t, Input);
            }
          }
          catch (InvalidOperationException)
          {
            return;
          }
        }

        // Scene Input Tick Event
        CurrentScene.OnInputTick?.Invoke(Input);

        // Fire Global Input Tick Event
        Game.OnInputTick?.Invoke(Input);
      }
    }
    public static async void EachSecondLoop()
    {
      while (true)
      {
        if (Game.Halt) { continue; }
        // Delay 1s (1,000 ms)
        await Task.Delay(1000);

        // Invoke Events
        Game.OnEachSecond?.Invoke();

        if (CurrentScene != null)
        {
          // Scene
          CurrentScene.OnEachSecond?.Invoke();

          // Tiles
          foreach (var tile in CurrentScene.Matrix)
          {
            try
            {
              tile.OnEachSecond?.Invoke(tile);
            }
            catch (InvalidOperationException)
            {
              break;
            }
          }

          // Entities
          foreach (var e in CurrentScene.Entities)
          {
            try
            {
              e.OnEachSecond?.Invoke(e);
            }
            catch (InvalidOperationException)
            {
              break;
            }
          }
        }
      }
    }

    // Tile Matrix Manipulation

    public static void UpdateTileAtPos(int x, int y)
    {
      ThrowIfNoCurrentScene();
      ThrowIfInvalidPos(x, y);
      Render.Write(x, y, CurrentScene.Matrix[x, y].Representation, OneTimeWriteFlag: true);
    }

    public static void UpdateTileAtPos(Vector pos)
    {
      UpdateTileAtPos(pos.X, pos.Y);
    }

    public static void UpdateTile(Tile t)
    {
      ThrowIfInvalidPos(t.Pos);
      if (t.ParentScene.Equals(CurrentScene))
      {
        UpdateTileAtPos(t.Pos);
      }
      else
      {
        return;
      }
    }

    // Built-in Game Mechanics

    public static void EnableInventoryUISystem(ConsoleKey InventoryKey)
    {
      Game.OnInputTick = (key) =>
      {
        if (key == InventoryKey)
        {
          // Make sure plr has inventory
          if (Plr.Inventory == null) { return; }

          // Pause game
          Game.Halt = true;

          // Set UI Type
          UI.SetToTwoCols();

          // Inventory loop
          while (true)
          {
            // Get Items & Display information
            var items = Plr.Inventory.GetAllItems();
            var names = new List<string>(Plr.Inventory.Count);
            int choice = -1;

            foreach (var it in items)
            {
              names.Add(it.DisplayName);
            }

            if (Plr.Inventory.Count != 0)
            {
              choice = UI.SetSelection(
                    names.ToArray(),
                    0,
                    StayHaltedFlag: true,
                    OnHover: (choice) =>
                          {
                            UI.SetSecondary(items[choice].Description);
                            UI.Update();
                          },
                    InventoryKey
                );
            }
            else
            {
              // Set message
              UI.SetPrimary("You have no items yet!");
              UI.SetSecondary("Nothing to see here");

              // Display changes & Read input
              UI.Update();
              ConsoleKey Input = Console.ReadKey(true).Key;

              // Interpret input
              if (Input == InventoryKey) { break; }

              // Else keep the player in the inventory screen
              continue;
            }

            // Exit inventory if no choice, else, get chosen item
            if (choice == -1) { break; }
            var item = items[choice];

            // enter second menu (item options)
            choice = UI.SetSelection(
                  new string[2] {
                            "Use",
                            "Discard"
                      },
                  1,
                  StayHaltedFlag: true,
                  EscapeKey: InventoryKey
              );

            // interpret choice
            if (choice == -1) { break; }

            switch (choice)
            {
              case 0: // use
                item.Use(Plr);
                break;
              case 1: // discard
                Plr.Discard(item);
                break;
            }
          }

          // Close inventory
          UI.SetToDescription();
          UI.Update();
          Game.Halt = false;
        }
      };
    }

    // Events
    public static Action<ConsoleKey> OnInputTick { get; set; }
    public static Action OnTick { get; set; }
    public static Action OnEachSecond { get; set; }

    // Validation & Debugging

    public static bool ValidatePos(int x, int y)
    {
      if (x < 0 || x >= Size[0] || y < 0 || y >= Size[1])
      {
        return false;
      }
      return true;
    }
    public static bool ValidatePos(Vector pos)
    {
      return ValidatePos(pos.X, pos.Y);
    }

    public static void ThrowIfInvalidPos(int x, int y)
    {
      if (!ValidatePos(x, y))
      {
        throw new Exception($"Position ({x}, {y}) is out of bounds");
      }
    }

    public static void ThrowIfInvalidPos(Vector pos)
    {
      if (!ValidatePos(pos))
      {
        throw new Exception($"Position ({pos.X}, {pos.Y}) is out of bounds");
      }
    }

    public static void ThrowIfNoCurrentScene()
    {
      if (CurrentScene == null)
      {
        throw new Exception("Expected a scene to be loaded!");
      }
    }

    public static void ThrowIfNotCurrentScene(Scene s)
    {
      ThrowIfNoCurrentScene();
      if (!CurrentScene.Equals(s))
      {
        throw new Exception($"Expected scene '{s.Description}' to be current scene, but current scene is actually '{CurrentScene.Description}'.");
      }
    }
  }
}