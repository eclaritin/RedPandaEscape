using ConsoleEngine.Data;
using ConsoleEngine.Properties;

namespace ConsoleEngine.Services
{
    public static class Render
    {
        /// Properties ///
        private static Scene? NextScene; // scene queue i think

        /// Methods ///
        // Basic Rendering

        public static void SetWindowSize()
        {
            Console.SetWindowSize(Game.Size[0], Game.Size[1] + UI.UIHeight);
            Console.SetBufferSize(Game.Size[0], Game.Size[1] + UI.UIHeight);
        }

        public static void Write(int x, int y, char rep, bool OneTimeWriteFlag = false)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(rep);
            if (OneTimeWriteFlag)
            {
                Console.SetCursorPosition(0, 16);
            }
        }

        public static void WriteLine(int y, string text, bool NoNewLineFlag = false)
        {
            Console.SetCursorPosition(0, y);
            Console.Write(text + (NoNewLineFlag ? "" : "\n"));
        }

        // Complex Rendering

        public static async void RenderScene(Scene ToRender)
        {
            if (Game.CurrentScene != null)
            {
                NextScene = ToRender;
                return;
            }
            UI.StoredState.Construct();
            Game.CurrentScene = ToRender;
            Game.CurrentScene.Visible = true;
            UI.SetToDescription();
            Game.CurrentScene.OnRender?.Invoke();
            foreach (var t in Game.CurrentScene.Matrix)
            {
                t.OnSceneLoad?.Invoke(t);
            }
            foreach (var e in Game.CurrentScene.Entities)
            {
                e.OnSceneLoad?.Invoke(e);
            }
            Game.CurrentScene.LoadedBefore = true;

            // Renderloop
            while (Game.CurrentScene.Visible)
            {
                if (Game.Halt)
                {
                    continue;
                }
                if (Game.CurrentScene.Update)
                {
                    await Task.Run(Render.Draw);
                }

                // Fire Entity Tick Events
                foreach (var e in Game.CurrentScene.Entities)
                {
                    e.OnTick?.Invoke(e);
                }
                foreach (var t in Game.CurrentScene.Matrix)
                {
                    t.OnTick?.Invoke(t);
                }

                // Fire Global Tick Event & Current scene
                Game.OnTick?.Invoke();
                Game.CurrentScene.OnTick?.Invoke();

                // Exit loop if next scene must be rendered
                if (NextScene != null)
                {
                    break;
                }
            }

            // Call RenderScene on NextScene
            if (NextScene == null)
            {
                return;
            }
            var SceneToLoad = NextScene;
            Game.CurrentScene = null;
            NextScene = null;
            RenderScene(SceneToLoad);
        }

        // Render Loop

        public static void Draw()
        {
            // Disable update flag
            Game.CurrentScene.Update = false;

            // Reset Screen
            Console.Clear();

            // Draw Tiles
            for (int x = 0; x < Game.CurrentScene.Matrix.GetLength(0); x++)
            {
                for (int y = 0; y < Game.CurrentScene.Matrix.GetLength(1); y++)
                {
                    Render.Write(x, y, Game.CurrentScene.Matrix[x, y].Representation);
                }
            }

            // Draw Entities
            foreach (Entity ThisEntity in Game.CurrentScene.Entities)
            {
                Render.Write(ThisEntity.Pos.X, ThisEntity.Pos.Y, ThisEntity.Representation);
            }

            // Draw UI
            UI.Update();
        }
    }
}
