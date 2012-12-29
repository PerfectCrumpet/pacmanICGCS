using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;

namespace ICGGSAssignment.Screens.System
{
    public class GameOverScreen : MainMenuScreen

    {
        MenuEntry playagain, quit;

        public GameOverScreen()
        {
            playagain = new MenuEntry(this, "Game Over - Play Again?");

        }


    }
}
