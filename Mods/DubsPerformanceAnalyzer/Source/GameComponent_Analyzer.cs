using Analyzer.Performance;
using Analyzer.Profiling;
using Analyzer.Fixes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Analyzer
{
    public class GameComponent_Analyzer : GameComponent
    {
        private Game game = null;
        public float TimeTillCleanup = -1;

        public GameComponent_Analyzer(Game game)
        {
            this.game = game;
            // On game load, initialise the currently active performance patches
            PerformancePatches.ActivateEnabledPatches();

            FixPatches.OnGameInit(game);
        }

        public override void LoadedGame()
        {
            FixPatches.OnGameLoad(game);
        }


        public override void GameComponentUpdate()
        {
            // Display our logged messages that we may have recieved from other threads.
            ThreadSafeLogger.DisplayLogs();

            if (TimeTillCleanup == -1) return;

            if (Profiling.Analyzer.CurrentlyProfiling)
            {
                TimeTillCleanup = -1;
                return;
            }

            TimeTillCleanup -= Time.deltaTime;
            if (TimeTillCleanup <= 0)
            {
                Profiling.Analyzer.Cleanup();
                TimeTillCleanup = -1;
            }
            
        }

    }
}