using ConsoleEngine.Properties;
using ConsoleEngine.Data;
using ConsoleEngine.Services;

namespace Red_Panda_Escape
{
  class Program
  {
    public const bool DebugMode = false;
    // Main Menu
    public static void Main(string[] args)
    {
      // Set Screen Size
      Render.SetWindowSize();
      ////////////////////////////////////////////////////////////

      if (args.Contains("debug") || DebugMode) { StartGame(); return; }

      int choice = UI.DisplaySelectionScreen(
          new string[2] {
                "Play Game",
                "Quit"
          },
          "Red Panda Escape!",
          "Welcome to my game! Select one of the options to proceed."
      );

      // Interpret Choice
      switch (choice)
      {
        case 0:
          StartGame();
          break;
        case 1:
          return;
        default:
          return;
      }
    }

    // Game Initializer
    public static void StartGame()
    {
      Console.Clear();
      Console.WriteLine("Loading Game...");
      /////////////////////////////////////////////////

      // Scenes //
      Console.WriteLine("Building Scenes...");

      var PandaPrison = new Scene("CELL 01 - Ailurus Fulgens\nUse the WASD keys to move. Press the E key to open your inventory.");
      var HabitatHall = new Scene("Habitat Hall\nWill you save the other animals or try to sneak past the guard?");
      var ObservationDeck = new Scene("Observation Deck\nWould you sabotage the power to the lab or attempt a more peaceful resolution?");
      var Lab = new Scene("The Science Lab!\nFace the scientist who is plotting your demise once and for all!");
      var BambooField = new Scene("Bamboo Sanctuary\nAn all you can eat buffet of bamboo!");

      Game.Scenes.Add(PandaPrison);
      Game.Scenes.Add(HabitatHall);
      Game.Scenes.Add(ObservationDeck);
      Game.Scenes.Add(Lab);
      Game.Scenes.Add(BambooField);
      Game.EntryPoint = 0;

      /// PandaPrison (EntryPoint Scene) ///

      PandaPrison.OnRender = () =>
      {
        UI.Popup("Scientist: At long last, we've finally created a red panda capable of human intelligence! We're gonna receive so many science awards for this!", true);
        UI.Popup("Scientist: Wait, what's that?", true);
        UI.Popup("Scientist: Our budget's been cut? Well, guess we gotta kill off the test subjects. What a shame...", true);
        UI.Popup("Can you escape the facility with your life?", true);
      };

      PandaPrison.SetAllTiles(Tile.Defaults.Wall); // cell walls
      PandaPrison.SetTileRange(
          new Vector(16, 2),
          new Vector(48, 13),
          Tile.Defaults.Air
      ); // set air pocket
      PandaPrison.SetTileRange(
          new Vector(44, 11),
          new Vector(47, 12),
          Tile.Defaults.Bamboo
      ); // bamboo!

      Door[] PrisonDoors = Door.MakeHorizontalDoor(HabitatHall, keyItem: "Bamboo"); // make door objs
      PandaPrison.SetTile(46, 1, PrisonDoors[0]);
      PandaPrison.SetTile(47, 1, PrisonDoors[1]); // set door tiles
      PandaPrison.SetTileRange(new Vector(46, 0), new Vector(47, 0), Tile.Defaults.Air); // create cell area

      /// Habitat Hall ///

      HabitatHall.PlrStartPos = new Vector(2, 15); // plr spawns here in this room
      HabitatHall.OnRender = () =>
      {
        Game.Plr.Decisions.Add("broke out of cell");
      };

      HabitatHall.SetAllTiles(Tile.Defaults.Wall); // outer walls
      HabitatHall.SetTileRange(
          new Vector(1, 1),
          new Vector(62, 14),
          Tile.Defaults.Air
      ); // open space

      // switch area
      HabitatHall.SetTileRange(
          new Vector(1, 4),
          new Vector(5, 4),
          Tile.Defaults.Wall
      ); // wall
      var CellSwitch = Tile.Defaults.Switch;
      // switch behavior
      CellSwitch.Attributes.Set("OnFlip", (Tile self, Entity flipper) =>
      {
        if (!Game.Plr.Decisions.Contains("freed animals"))
        {
          Game.Plr.Decisions.Add("freed animals");
          UI.Popup("You're friends are finally free thanks to you!", true);
        }

        foreach (var t in HabitatHall.Matrix)
        {
          if (!t.Attributes.Exists("NextDoorPos")) { continue; }
          if (((Scene)t.Attributes.Get("LeadsTo")) == null)
          {
            t.Attributes.Set("Locked", false);
          }
        }
      });

      HabitatHall.SetTile(2, 3, CellSwitch); // set switch tile

      HabitatHall.SetTile(2, 15, Tile.Defaults.Air);
      HabitatHall.SetTile(3, 15, Tile.Defaults.Air); // door area

      var roomDoors = Door.MakeHorizontalDoor();
      var createRoom = (int x, char animalRep) =>
      {

        HabitatHall.SetTileBox(
              new Vector(x, 8),
              new Vector(x + 14, 15),
              Tile.Defaults.Wall
          ); // Room

        HabitatHall.SetTile(x + 12, 8, roomDoors[0].Value);
        HabitatHall.SetTile(x + 13, 8, roomDoors[1].Value); // Room doors

        var myAnimal = new Animal(animalRep, HabitatHall);
        myAnimal.MoveTo(x + 2, 13);
      };

      createRoom(5, '2');
      createRoom(20, '5');
      createRoom(35, '~');
      createRoom(49, 'M');


      var hallDoor = Door.MakeVerticalDoor(ObservationDeck, keyItem: "Key");

      HabitatHall.SetTile(63, 5, hallDoor[0]);
      HabitatHall.SetTile(63, 6, hallDoor[1]); // exit doors

      var guardkey = Tile.Defaults.ItemHolder;
      guardkey.Inventory.Add(Item.Defaults.Key);

      HabitatHall.SetTile(62, 1, guardkey); // key

      var HabitatGuard = new Entity('&', new Vector(32, 6), HabitatHall); // spawn guard 
      HabitatGuard.Attributes.Set("GoingRight", true);

      HabitatGuard.OnEachSecond = (self) =>
      {
        self.MoveTo(self.Pos.X + (((bool)self.Attributes.Get("GoingRight")) ? 2 : -2), self.Pos.Y);
        if (self.Pos.X >= 60) { self.Attributes.Set("GoingRight", false); }
        if (self.Pos.X <= 8) { self.Attributes.Set("GoingRight", true); }

        // catch player if close enough
        var distVec = self.DistanceTo(Game.Plr.Pos);
        if (distVec.X <= 6 && distVec.Y <= 3)
        {
          Game.Halt = true;
          UI.Popup("You were caught", true);
          GameOver();
        }
      }; // guard behavior

      /// Observation Deck ///
      ObservationDeck.PlrStartPos = new Vector(0, 8);

      ObservationDeck.SetAllTiles(Tile.Defaults.Wall);
      ObservationDeck.SetTileRange(
          new Vector(1, 4),
          new Vector(62, 14),
          Tile.Defaults.Air
      );
      ObservationDeck.SetTileRange(
          new Vector(16, 1),
          new Vector(48, 4),
          Tile.Defaults.Air
      );
      ObservationDeck.SetTile(0, 8, Tile.Defaults.Air);
      ObservationDeck.SetTile(0, 9, Tile.Defaults.Air);

      var exitdoors = Door.MakeVerticalDoor(Lab, true);
      ObservationDeck.SetTile(63, 8, exitdoors[0]);
      ObservationDeck.SetTile(63, 9, exitdoors[1]);

      var sabotageSwitch = Tile.Defaults.Switch;
      sabotageSwitch.Attributes.Set("OnFlip", (Tile self, Entity flipper) =>
      {
        if (!Game.Plr.Decisions.Contains("sabotaged"))
        {
          Game.Plr.Decisions.Add("sabotaged");
          UI.Popup("You've blown up the power reactor!");
        }

        foreach (var t in ObservationDeck.Matrix)
        {
          if (t.Representation == '$') { ObservationDeck.Matrix[t.Pos.X, t.Pos.Y] = Tile.Defaults.Air; Game.UpdateTileAtPos(t.Pos); }
          if (!t.Attributes.Exists("NextDoorPos")) { continue; }

          t.Attributes.Set("Locked", false);

        }
      });
      ObservationDeck.SetTile(32, 3, sabotageSwitch);

      var giftholder = Tile.Defaults.ItemHolder;
      giftholder.Representation = '$';

      var thegift = Item.Defaults.Gift;
      thegift.OnPickUp = (self, e) =>
      {
        UI.Popup("I found a gift! I wonder who I'll give it to?", true);
        foreach (var t in ObservationDeck.Matrix)
        {
          if (t.Representation == '\\') { ObservationDeck.Matrix[t.Pos.X, t.Pos.Y] = Tile.Defaults.Air; Game.UpdateTileAtPos(t.Pos); }
          if (!t.Attributes.Exists("NextDoorPos")) { continue; }

          t.Attributes.Set("Locked", false);
        }
      };

      giftholder.Inventory.Add(thegift);


      ObservationDeck.SetTile(32, 13, giftholder);

      /// The Science Lab! ///
      Lab.PlrStartPos = new Vector(0, 13);

      // tables
      Lab.SetTileRange(
          new Vector(1, 2),
          new Vector(8, 8),
          Tile.Defaults.Wall
      );
      Lab.SetTileRange(
          new Vector(55, 2),
          new Vector(62, 8),
          Tile.Defaults.Wall
      );
      Lab.SetTileRange(
          new Vector(20, 12),
          new Vector(44, 14),
          Tile.Defaults.Wall
      );

      // scientist
      var scientist = new Entity('?', Lab);
      scientist.MoveTo(32, 5);
      scientist.Attributes.Set("GoingRight", true);

      // animal spawning
      Action spawnAnimals = () =>
      {
        foreach (var e in Game.CurrentScene.Entities)
        {
          if (e.Attributes.Exists("OldRep"))
          {
            e.Representation = (char)e.Attributes.Get("OldRep");
            Render.Write(e.Pos.X, e.Pos.Y, e.Representation, OneTimeWriteFlag: true);
          }
        }
      };

      var animals = new Animal[4] { new Animal('2', Lab), new Animal('5', Lab), new Animal('~', Lab), new Animal('M', Lab) };
      int x = 0;
      int i = 0;
      do
      {
        animals[i].MoveTo(20 + x, 10);
        animals[i].Attributes.Set("OldRep", animals[i].Representation);
        animals[i].Representation = ' ';
        animals[i].OnEachSecond = null;

        x += 8; i++;
      } while (i < animals.Length);

      // scientist behavior
      scientist.OnEachSecond = (self) =>
      {
        var dir = (bool)self.Attributes.Get("GoingRight");

        if (dir)
        {
          self.MoveTo(self.Pos.X + 1, self.Pos.Y);
        }
        else
        {
          self.MoveTo(self.Pos.X - 1, self.Pos.Y);
        }

        if (self.Pos.X < 16 || self.Pos.X > 48)
        {
          if (dir)
          {
            self.Attributes.Set("GoingRight", false);
          }
          else
          {
            self.Attributes.Set("GoingRight", true);
          }
        }
      };
      scientist.OnInputTick = (self, key) =>
      {
        var distVec = self.DistanceTo(Game.Plr.Pos);
        if (distVec.X < 16 && distVec.Y < 4)
        {
          if (Game.Plr.Decisions.Contains("freed animals"))
          {
            Game.Halt = true;
            spawnAnimals();
            if (Game.Plr.Inventory.HasItemOfName("Gift"))
            {
              UI.Popup("The other animals you saved came back!", true);
              UI.Popup("Scientist: You got me a gift? How kind of you! Maybe killing you is not a good idea after all. Thank you!", true);
              GoodRevoltEnding();
              return;
            }
            UI.Popup("The other animals you saved came back to fight with you! It's a revolution!", true);
            RevoltEnding();

          }
          else if (Game.Plr.Inventory.HasItemOfName("Gift"))
          {
            Game.Halt = true;
            UI.Popup("Scientist: You got me a gift? How kind of you! Maybe killing you is not a good idea after all. Thank you!", true);
            GoodEnding();
          }
          else
          {
            Game.Halt = true;
            UI.Popup("The evil scientist has killed you with their evil science powers :(", true);
            GameOver();
          }
        }
      };

      /// Bamboo Sanctuary ///

      BambooField.SetAllTiles(Tile.Defaults.Bamboo);
      BambooField.SetTile(Vector.Center.X, Vector.Center.Y, Tile.Defaults.Air);


      /// Set up Player ///
      Console.WriteLine("Initializing Player...");

      Game.Plr = new Player("Red Panda");
      Game.Plr.EnableWASDMovement();
      Game.Plr.Inventory = new Inventory(20);
      Game.EnableInventoryUISystem(ConsoleKey.E);

      // Begin Game Loops
      Console.Clear();
      Game.Start();
    }

    // Endings
    public static void GoodEnding()
    {
      Game.Halt = false;
      Game.Plr.SwitchScene(Game.Scenes[4]);
      Render.RenderScene(Game.Scenes[4]);
    }
    public static void RevoltEnding()
    {
      var animals = new Animal[4] { new Animal('2', Game.Scenes[4]), new Animal('5', Game.Scenes[4]), new Animal('~', Game.Scenes[4]), new Animal('M', Game.Scenes[4]) };
      animals[0].MoveTo(16, 4);
      animals[1].MoveTo(48, 4);
      animals[2].MoveTo(16, 12);
      animals[3].MoveTo(48, 12);
      Game.Halt = false;
      Game.Plr.SwitchScene(Game.Scenes[4]);
      Render.RenderScene(Game.Scenes[4]);

    }
    public static void GoodRevoltEnding()
    {
      var animals = new Animal[5] { new Animal('2', Game.Scenes[4]), new Animal('5', Game.Scenes[4]), new Animal('~', Game.Scenes[4]), new Animal('M', Game.Scenes[4]), new Animal('?', Game.Scenes[4]) };
      animals[0].MoveTo(16, 4);
      animals[1].MoveTo(48, 4);
      animals[2].MoveTo(16, 12);
      animals[3].MoveTo(48, 12);
      animals[4].MoveTo(32, 4);
      Game.Halt = false;
      Game.Plr.SwitchScene(Game.Scenes[4]);
      Render.RenderScene(Game.Scenes[4]);
    }
    public static void GameOver()
    {
      Game.GameRunning = false;
      UI.DisplaySelectionScreen(new string[] { "Exit Game" }, "You died!!", "Nice try, but it appears you've been caught! Better luck next time!");
      Environment.Exit(0);
    }
  }
}