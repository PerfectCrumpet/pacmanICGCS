#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// ICGGS XNA Assignment 2011/12
    /// Assignment programmed by: INSERT YOUR NAME HERE
    /// Student No: INSERT YOUR STUDENT NUMBER HERE
    /// Staffordshire University
    /// </summary>

    class GameplayScreen : GameScreen
    {
        #region Fields

        //Points

        private long ticks = 0;
        private int points = 0;

        // System Variables
        Color backgroundColor = Color.CornflowerBlue;
        ContentManager content;
        float pauseAlpha;

        // Width and Height of the game window, in pixels
        private int gameWidth;
        private int gameHeight;

        // Pacman variables
        private Point frameSize = new Point(32, 32);    // Pacman image size
        private Point currentFrame = new Point(1, 0);   // Start frame
        private Point sheetSize = new Point(2, 4);      // Spritesheet size
        private Vector2 pacmanPos;                      // Pacman position in pixels
        private int pacmanSpeed = 8;                    // Pacman movement speed in pixels
        private int munchieSpeed = 1;                    // Pacman movement speed in pixels

        private Point munchies = new Point(5, 5);
        const int munchySize = 10;

        // Game
        SpriteBatch spriteBatch;
        Texture2D munchie1, munchie2, pacman;

        //Actions
        private int freezeCooldown = 0;

        // Sounds
        SoundEffect collisionSound;

        // Random number generator
        Random rand = new Random();

        // Total number of munchies 
        private int noOfMunchies = 5;

        Vector2[] munchiePos;
        Vector2[] munchieDest;
        private int munchieSize;
        private int[] munchieAnimationCount;

        private int timeSinceLastFrame = 0;
        private int milliSecondsPerFrame = 100; // 500 2 Frames Per Second (fps)

        private bool wallCollided = false;
        private bool hasCollision = false;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // Setup munchies
            munchiePos = new Vector2[noOfMunchies];
            munchieDest = new Vector2[noOfMunchies];
            munchieAnimationCount = new int[noOfMunchies];
        }

        private void resetGame()
        {
            // Setup pacman
            pacmanPos = new Vector2((gameWidth / 2) - (frameSize.X / 2),
                                    (gameHeight / 2) - (frameSize.Y / 2));

            // Generate random munchies
            for (int i = 0; i < noOfMunchies; i++)
            {
                // Random Positions
                munchiePos[i].X = Math.Max(0, rand.Next(gameWidth) - munchieSize);
                munchiePos[i].Y = Math.Max(0, rand.Next(gameHeight) - munchieSize);

                munchieDest[i].X = munchiePos[i].X;
                munchieDest[i].Y = munchiePos[i].Y;

                // Random animation frame
                munchieAnimationCount[i] = rand.Next(2);
            }
        }
        
        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Save local copy of SpriteBatch, which can be used to draw textures.
            spriteBatch = ScreenManager.SpriteBatch;

            // Load textures
            munchie1 = content.Load<Texture2D>("Graphics/Sprites/Munchie1");
            munchie2 = content.Load<Texture2D>("Graphics/Sprites/Munchie2");
            munchieSize = munchie1.Width;

            //Load Text 
            SpriteBatch ForegroundBatch;
            SpriteFont CourirerNew;

        



            pacman = content.Load<Texture2D>("Graphics/Sprites/pacman");

            // Load Sounds
            collisionSound = content.Load<SoundEffect>("Sounds/1");

            // Get screen width and height
            gameWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            gameHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            // Setup game
            resetGame();

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // Once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
            //About 50 ticks/second
            ticks++;

            int ticksPerCalculation = 50;

            if ((ticks % ticksPerCalculation) == 0)
            {
                points++;
            }

            // Munchie Speed Increase & Cap
            if ((ticks % 500) == 0)//10 Seconds
            {
                if (munchieSpeed < 10)
                {
                    munchieSpeed++;
                }
            }



            //Cooldown Negater

            if (freezeCooldown > 0)
            {
                freezeCooldown--;
            }


            //AI Movement

            var x = 0;
            Random rand = new Random();
            foreach(var munchie in munchiePos)
            {
                var dest = munchieDest[x];

                //Check if reached destination
                if (munchie.X == dest.X && munchie.Y == dest.Y)
                {
                    //Destination Reached

                    if (rand.Next(0, 2) == 0)
                    {
                        //X
                        munchieDest[x].X = Math.Max(0, rand.Next(gameWidth) - munchieSize);
                    }
                    else
                    {
                        //Y
                        munchieDest[x].Y = Math.Max(0, rand.Next(gameHeight) - munchieSize);
                    }
                }
                else
                {
                    //Destination Not Reached
                    if (munchie.X != dest.X)
                    {
                        //X
                        var difference = dest.X - munchie.X;

                        if(Math.Abs(difference) <= munchieSpeed)
                        {
                            munchiePos[x].X = dest.X;
                        }
                        else
                        {
                            if(difference > 0)
                            {
                                //Positive Movement
                                munchiePos[x].X += munchieSpeed;
                            }
                            else{
                                //Negative Movement
                                munchiePos[x].X -= munchieSpeed;
                            }
                        }
                    }
                    else
                    {
                        //Y
                        var difference = dest.Y - munchie.Y;

                        if (Math.Abs(difference) <= munchieSpeed)
                        {
                            munchiePos[x].Y = dest.Y;
                        }
                        else
                        {
                            if (difference > 0)
                            {
                                //Positive Movement
                                munchiePos[x].Y += munchieSpeed;
                            }
                            else
                            {
                                //Negative Movement
                                munchiePos[x].Y -= munchieSpeed;
                            }
                        }
                    }
                }


                x++;
            }

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive) 
            {
                // Game logic

                // Check for pacman hitting walls
                if (pacmanPos.X + frameSize.X > gameWidth)
                {
                    // Pacman hit right wall
                    pacmanPos.X = gameWidth - frameSize.X;

                    // Play collision sound effect

                    if (!wallCollided)
                    {
                        wallCollided = true;
                        collisionSound.Play();
                    }
                }
                else if (pacmanPos.X <= 0)
                {
                    //collision detection left wall.
                    pacmanPos.X = 0;

                    //sound

                    if (!wallCollided)
                    {
                        wallCollided = true;
                        collisionSound.Play();
                    }
                }
                else if (pacmanPos.Y + frameSize.Y > gameHeight)
                {
                    //Collision detection floor
                    pacmanPos.Y = gameHeight - frameSize.Y;

                    //sound

                    if (!wallCollided)
                    {
                        wallCollided = true;
                        collisionSound.Play();
                    }
                }
                else if (pacmanPos.Y <= 0)
                {
                    //collision celling
                    pacmanPos.Y = 0;

                    //sound

                    if (!wallCollided)
                    {
                        wallCollided = true;
                        collisionSound.Play();
                    }
                }
                else
                {
                    wallCollided = false;
                }
                
                //Munchie collision

                var thisCollision = false;

                foreach (var munchie in munchiePos)
                {
                    if (Overlap((int)pacmanPos.X, (int)pacmanPos.X + 32, (int)munchie.X, (int)munchie.X + munchieSize) &&
                        Overlap((int)pacmanPos.Y, (int)pacmanPos.Y + 32, (int)munchie.Y, (int)munchie.Y + munchieSize))
                    {
                        thisCollision = true;
                    }
                }

                if (thisCollision)
                {
                    if (!hasCollision)
                    {
                        //Die
                        collisionSound.Play();
                        hasCollision = true;
                    }
                }
                else
                {
                    hasCollision = false;
                }

                // Animations

                //pacman animations
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (timeSinceLastFrame > milliSecondsPerFrame)
                {
                    //reset
                    timeSinceLastFrame -= milliSecondsPerFrame;
                    //next frame
                  
                }
                }
                // Munchie Animations
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (timeSinceLastFrame > milliSecondsPerFrame)
                {
                    // Reset time
                    timeSinceLastFrame -= milliSecondsPerFrame;

                    // Next frame
                    for (int i = 0; i < noOfMunchies; i++)
                    {
                        if (munchieAnimationCount[i] == 0)

                        {
                            munchieAnimationCount[i] = 1;
                        }
                        else
                        {
                            munchieAnimationCount[i] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks overlap between 2 ranges of values
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="s1"></param>
        /// <param name="e1"></param>
        /// <returns></returns>
        private bool Overlap(int s, int e, int s1, int e1)
        {
            if (s > s1 && s < e1)
                return true;
            if (s1 > s && s1 < e)
                return true;
            return false;
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else

            {
                // TODO: Add game keys here!

                // Pacman move keys
                if ((gamePadState.DPad.Right == ButtonState.Pressed) ||
                    (keyboardState.IsKeyDown(Keys.Right)))
                {
                    // Move pacman Right
                    pacmanPos.X += pacmanSpeed;
                }
                else if ((gamePadState.DPad.Left == ButtonState.Pressed) ||
                    (keyboardState.IsKeyDown(Keys.Left)))
                {
                    // Move pacman Left
                    pacmanPos.X -= pacmanSpeed;
                }
                else if ((gamePadState.DPad.Up == ButtonState.Pressed) ||
                    (keyboardState.IsKeyDown(Keys.Up)))
                {
                    // Move pacman Up
                    pacmanPos.Y -= pacmanSpeed;
                }
                else if ((gamePadState.DPad.Down == ButtonState.Pressed) ||
                    (keyboardState.IsKeyDown(Keys.Down)))
                {
                    // Move pacman Down
                    pacmanPos.Y += pacmanSpeed;
                }
                else if ((gamePadState.Buttons.Y == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.A))))
                {   
                    if (points >= 50 && freezeCooldown == 0)
                    {
                        munchieSpeed = munchieSpeed - 2;
                        points = points - 50;

                        freezeCooldown = 1000;
                    }
                }
            }
}
                    /// <summary>
        /// Draws the gameplay screen. 
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!

            var colorMatch = new Dictionary<int, Color>();
            colorMatch.Add(1, Color.LightBlue);
            colorMatch.Add(2, Color.CornflowerBlue);
            colorMatch.Add(3, Color.Blue);
            colorMatch.Add(4, Color.White);
            colorMatch.Add(5, Color.LightGreen);
            colorMatch.Add(6, Color.Green);
            colorMatch.Add(7, Color.Purple);
            colorMatch.Add(8, Color.Chocolate);
            colorMatch.Add(9, Color.Crimson);
            colorMatch.Add(10, Color.IndianRed);

            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               colorMatch[munchieSpeed], 0, 0);

            spriteBatch.Begin();

            // Draw munchies
            for (int i = 0; i < noOfMunchies; i++)
            {
                if (munchieAnimationCount[i] == 0)
                {
                    // Draw frame 1
                    spriteBatch.Draw(munchie1, munchiePos[i], Color.White);
                }
                else
                {
                    // Draw frame 2
                    spriteBatch.Draw(munchie2, munchiePos[i], Color.White);
                }
            }

            // Draw Pacman
            spriteBatch.Draw(pacman, pacmanPos,
                             new Rectangle(currentFrame.X * frameSize.X,
                                           currentFrame.Y * frameSize.Y,
                                           frameSize.X,
                                           frameSize.Y),
                             Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        //Draw Scores
        private void DrawText()
        {
         spriteBatch,DrawString(font, Score, new 
         vector2(20, 45), Color.White);
        }

        #endregion
    }
}
