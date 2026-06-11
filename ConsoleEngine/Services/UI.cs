using System;

using ConsoleEngine.Data;
using ConsoleEngine.Properties;

namespace ConsoleEngine.Services
{
  public static class UI
  {
    /// Properties ///

    // Calculate dimensions
    public static int UIHeight = 4; // adjust how many rows the bottom UI takes up
    private static int PrimaryColWidth { get { return Game.Size[0] / 2 - 1; } }
    private static int SecondaryColWidth { get { return Game.Size[0] - PrimaryColWidth - 2; } }

    // State properties
    private static string ToWrite = "";
    private static string ToWriteSecondary = "";
    private static bool IsDescription = true;
    private static bool WaitForKeyFlag = false;
    private static bool WaitForEnterFlag = false;

    // Stored state for layering menus & popups?
    public static class StoredState
    {
      private static bool StoredStateWritten;
      ////////////////////////////////////////////////

      private static string ToWrite;
      private static string ToWriteSecondary;
      private static bool IsDescription;
      private static bool WaitForKeyFlag;
      private static bool WaitForEnterFlag;

      ////////////////////////////////////////////////

      public static void Construct()
      {
        ToWrite = "";
        ToWriteSecondary = "";
        IsDescription = true;
        WaitForKeyFlag = false;
        WaitForEnterFlag = false;
        StoredStateWritten = false;
      }
      public static void Save(bool overwrite = false)
      {
        if (StoredStateWritten && !overwrite)
        {
          throw new Exception("Stored state is already set! If you mean to overwrite, pass true to my overwrite argument!");
        }
        StoredStateWritten = true;
        ToWrite = UI.ToWrite;
        ToWriteSecondary = UI.ToWriteSecondary;
        IsDescription = UI.IsDescription;
        WaitForEnterFlag = UI.WaitForEnterFlag;
        WaitForKeyFlag = UI.WaitForKeyFlag;
      }
      public static void Load()
      {
        UI.ToWrite = ToWrite;
        UI.ToWriteSecondary = ToWriteSecondary;
        UI.IsDescription = IsDescription;
        UI.WaitForEnterFlag = WaitForEnterFlag;
        UI.WaitForKeyFlag = WaitForKeyFlag;
        StoredState.StoredStateWritten = false;
      }
    }

    /// Methods ///


    // Underlying private functions

    private static void CLR()
    {
      // Build string with just spaces
      string ClrStr = "";
      for (int i = 0; i < Game.Size[0]; i++)
      {
        ClrStr += " ";
      }

      // Write clearing string for every line
      for (int i = 0; i < UIHeight; i++)
      {
        Console.SetCursorPosition(0, Game.Size[1] + i);
        Console.Write(ClrStr);
      }
    }

    // Handles SetSelection loop behavior (duh)
    // renderSelectionState = void (int top, int k, int dispHeight) -- for rendering the selection box itself. k is the offset from top to where the user has selected
    // onHover = void (int choice) -- for rendering secondary content based on the choice identified by its index (int choice)
    private static int SETSELECTION_LOOP(int n_choices, Action<int, int, int> renderSelectionState, Action<int>? onHover = null, bool StayHaltedFlag = false, ConsoleKey EscapeKey = ConsoleKey.Escape)
    {
      if (n_choices == 0) throw new Exception("Cannot start selection loop from empty array.");

      Game.Halt = true; // pause game first

      // loop vars
      bool chosen = false;
      int choice = 0;
      int top = 0;
      int dispHeight = UIHeight - 1;

      // prevent dispHeight from going over array length (prevents out-of-bounds errors)
      if (dispHeight > n_choices) { dispHeight = n_choices; }

      while (!chosen)
      {
        // render selection
        renderSelectionState.Invoke(top, choice, dispHeight);

        // render secondary info
        onHover?.Invoke(top + choice);

        // get input
        ConsoleKey Input = Console.ReadKey(true).Key;

        // interp
        if (Input == EscapeKey)
        {
          choice = -1; // -1 is returned if user doesn't make a selection
          chosen = true;
          break;
        }

        switch (Input)
        {
          case ConsoleKey.W:
          case ConsoleKey.UpArrow:
            choice--;
            break;
          case ConsoleKey.S:
          case ConsoleKey.DownArrow:
            choice++;
            break;
          case ConsoleKey.D:
          case ConsoleKey.Enter:
            chosen = true;
            break;
          default:
            break;
        }

        // Keep choice between 0 & max index
        if (choice < 0 && top == 0) { choice = 0; continue; }
        if ((choice + top) >= n_choices) { choice = dispHeight - 1; top = n_choices - dispHeight; continue; }

        if (choice >= dispHeight)
        {
          choice = dispHeight - 1;
          top++;
        }

        if (choice < 0)
        {
          choice = 0;
          top--;
        }
      }

      // Calc choice
      choice = top + choice;

      // Unpause & return
      if (!StayHaltedFlag) { Game.Halt = false; }
      return choice;
    }

    // Whole Screen Methods

    public static int DisplaySelectionScreen(string[] Choices, string Title = "", string Body = "")
    {
      Game.Halt = true;

      int choice = 0;
      bool chosen = false;

      while (!chosen)
      {
        // Header
        Console.Clear();
        Console.WriteLine(Title);
        WriteBar(1);
        Console.WriteLine($"\n{Body}");
        Console.WriteLine("\nW or ↑: Up | S or ↓: Down | Enter: Select\n");

        // Display choices with selector on which choice is selected
        for (int i = 0; i < Choices.Length; i++)
        {
          if (choice == i)
          {
            Console.WriteLine($"> {Choices[i]} <");
          }
          else
          {
            Console.WriteLine($"  {Choices[i]}  ");
          }
        }



      }

      Game.Halt = false;
      return choice;
    }

    // Methods for the box below the main game

    // clears everything
    public static void Clear()
    {
      if (!IsDescription) { throw new Exception("Please specify column to clear (0: Primary, 1: Secondary, 2: Both)"); }
      ToWrite = "";
      Update();
    }

    // clears a specific column
    public static void Clear(int col)
    {
      if (IsDescription && col != 2) { throw new Exception("Cannot clear specific col when UI is in Description mode"); }
      if (IsDescription) { Clear(); return; }
      switch (col)
      {
        case 0: // Primary
          ToWrite = "";
          Update();
          break;
        case 1: // Secondary
          ToWriteSecondary = "";
          Update();
          break;
        case 2: // Both
          ToWrite = "";
          ToWriteSecondary = "";
          Update();
          break;
        default:
          throw new Exception($"Expected int between 0 & 2, got {col} instead.");
      }
    }

    // draws a horizontal line at a specific row across the whole screen
    public static void WriteBar(int y)
    {
      string BarStr = "";
      for (int i = 0; i < Game.Size[0]; i++)
      {
        BarStr += "─";
      }

      Render.WriteLine(y, BarStr + "\n", true);
    }

    // draws the default game bars
    public static void WriteBar()
    {
      WriteBar(Game.Size[1]);
      if (!IsDescription)
      {
        for (int line = Game.Size[1] + 1; line < Game.Size[1] + UIHeight; line++)
        {
          Render.Write(PrimaryColWidth + 1, line, '|');
        }
      }
    }

    public static void SetToDescription()
    {
      IsDescription = true;
      ToWriteSecondary = "";
      if (Game.CurrentScene == null) { ToWrite = ""; return; }
      ToWrite = Game.CurrentScene.Description;
    }

    public static void SetDescription(string desc, bool WaitForKeyFlag = false)
    {
      // make sure UI in description mode
      if (!IsDescription) { SetToDescription(); }

      // set desc
      ToWrite = desc;

      // make sure waitforkeyflag only changes if the method flag is true
      if (WaitForKeyFlag) { UI.WaitForKeyFlag = true; }
    }

    public static void SetToTwoCols()
    {
      IsDescription = false;
      ToWrite = "";
      ToWriteSecondary = "";
    }

    public static void SetPrimary(string str)
    {
      int col = 0;
      int line = 1;
      string ToSet = "";
      for (int i = 0; i < str.Length; i++)
      {
        if (col == PrimaryColWidth || str[i] == '\n' || str[i] == '\r')
        {
          if (line == UIHeight)
          {
            throw new Exception($"String '{str}' is too large for container!");
          }
          ToSet += "\n";
          col = 0;
          line++;
          continue;
        }
        ToSet += str[i];
        col++;
      }
      ToWrite = ToSet;
    }
    public static void SetSecondary(string str)
    {
      int col = 0;
      int line = 1;
      string ToSet = "";
      for (int i = 0; i < str.Length; i++)
      {
        ToSet += str[i];
        col++;
        if (col == SecondaryColWidth)
        {
          if (line == UIHeight)
          {
            throw new Exception($"String '{str}' is too large for container!");
          }
          ToSet += "\n";
          col = 0;
          line++;
        }
      }
      ToWriteSecondary = ToSet;
    }

    public static void WritePrimaryCol()
    {
      string[] Lines = ToWrite.Split("\n");
      for (int i = 0; i < Lines.Length; i++)
      {
        if (i >= UIHeight) { break; }
        Console.SetCursorPosition(0, Game.Size[1] + 1 + i);
        Console.Write(Lines[i]);
      }
    }
    public static void WriteSecondaryCol()
    {
      string[] Lines = ToWriteSecondary.Split("\n");
      for (int i = 0; i < Lines.Length; i++)
      {
        if (i >= UIHeight) { break; }
        Console.SetCursorPosition(PrimaryColWidth + 2, Game.Size[1] + 1 + i);
        Console.Write(Lines[i]);
      }
    }

    // Halts game & prompts user to make a selection
    // OnHover is useful for when you wanna display stuff on one column while the user makes a selection on the other column
    public static int SetSelection(string[] choices, bool StayHaltedFlag = false, Action<int>? OnHover = null, ConsoleKey EscapeKey = ConsoleKey.Escape) // Description mode
    {
      // Guard conditions
      if (!IsDescription) { throw new Exception("Please specify which column to set selection."); }

      return SETSELECTION_LOOP(choices.Length, (int top, int choice, int dispHeight) =>
      {
        // vars
        string printable = "";

        // display each choice
        for (int i = 0; i < dispHeight; i++)
        {
          if (top + i > choices.Length - 1) { continue; }
          printable += $"{choices[top + i]} {(choice == i ? " <" : "")}\n";
        }
        printable = printable.Substring(0, printable.Length - 1);

        UI.SetDescription(printable);
        UI.Update();
      }, OnHover, StayHaltedFlag, EscapeKey);
    }

    public static int SetSelection(string[] choices, int col, bool StayHaltedFlag = false, Action<int>? OnHover = null, ConsoleKey EscapeKey = ConsoleKey.Escape) // TwoCols mode
    {
      // Guard conditions
      if (IsDescription && col != 2) { throw new Exception("Cannot write to column while UI is set to description mode."); }
      if (IsDescription) { SetSelection(choices); }

      return SETSELECTION_LOOP(choices.Length, (int top, int choice, int dispHeight) =>
      {
        // vars
        string printable = "";

        // display each choice
        for (int i = 0; i < dispHeight; i++)
        {
          if (top + i > choices.Length - 1) { continue; }
          printable += $"{choices[top + i]} {(choice == i ? " <" : "")}\n";
        }
        printable = printable.Substring(0, printable.Length - 1);

        if (col == 0)
        {
          SetPrimary(printable);
        }
        if (col == 1)
        {
          SetSecondary(printable);
        }

        Update();
      }, OnHover, StayHaltedFlag, EscapeKey);
    }

    // Displays a text popup below the game
    public static void Popup(string message, bool RequireEnterKey = false)
    {
      StoredState.Save(true);
      SetDescription(message, WaitForKeyFlag: true);
      UI.WaitForEnterFlag = RequireEnterKey;
      Update();
      StoredState.Load();
      Update();
    }

    // Main update method (executes whatever changes you've set since the last update)
    public static void Update()
    {
      CLR();
      WriteBar();
      if (IsDescription)
      {
        Render.WriteLine(Game.Size[1] + 1, ToWrite, true);
      }
      else
      {
        WritePrimaryCol();
        WriteSecondaryCol();
      }
      if (WaitForKeyFlag)
      {
        while (true)
        {
          var key = Console.ReadKey(true).Key;
          if (!WaitForEnterFlag) { break; }
          if (key == ConsoleKey.Enter) { break; }
        }
      }
      WaitForKeyFlag = false;
      WaitForEnterFlag = false;
    }
  }
}