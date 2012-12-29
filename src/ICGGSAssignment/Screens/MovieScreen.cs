#region File Description
//-----------------------------------------------------------------------------
// MovieScreen.cs
//
// Staffordshire University
// Steve Foster 12/7/11
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
#endregion

namespace GameStateManagement
{
    public class MovieScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        float pauseAlpha;

        // Width and Height of the game window, in pixels
        private int gameWidth;
        private int gameHeight;

        // Movie
        private int noOfMovieFrames = 24;            // Number of frames in your movie
        Texture2D[] movie;
        private int currentMovieFrame = 0;
        private int timeSinceLastMovieFrame = 0;
        private int milliSecondsPerMovieFrame = 100; // 10 Frames Per Second (fps)
        private int movieWidth = 0;
        private int movieHeight = 0;

        // Game
        SpriteBatch spriteBatch;

        #endregion

        #region Initialization

        public MovieScreen()
        {
            // Setup on and off transition times
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // Movie frames array
            movie = new Texture2D[noOfMovieFrames];
        }
 
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Save local copy of SpriteBatch, which can be used to draw textures.
            spriteBatch = ScreenManager.SpriteBatch;

            // Movie Setup
            for (int i = 0; i < noOfMovieFrames; i++)
            {
                // Load Movie frames
                movie[i] = content.Load<Texture2D>("Movie/movie" + (i + 1));
            }

            // Get screen width and height
            gameWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            gameHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            // Get frame width and height
            movieWidth = movie[0].Width;
            movieHeight = movie[0].Height;

            // A real game would probably have more content than this, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
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
        /// Update movie frame
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                // Animate Movie
                timeSinceLastMovieFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (timeSinceLastMovieFrame > milliSecondsPerMovieFrame)
                {
                    // Reset time
                    timeSinceLastMovieFrame -= milliSecondsPerMovieFrame;

                    // Next frame
                    currentMovieFrame++;
                    if (currentMovieFrame >= noOfMovieFrames)
                    {
                        currentMovieFrame = 0;
                    }
                }
            }
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
                // Movie Controls

                // Reset movie
                if ((gamePadState.Buttons.A == ButtonState.Pressed) || keyboardState.IsKeyDown(Keys.Enter))
                {
                    currentMovieFrame = 0;
                }
            }
        }

        /// <summary>
        /// This is called when the movie should draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Clear background
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            spriteBatch.Begin();

            // Draw current frame
            spriteBatch.Draw(movie[currentMovieFrame], new Vector2((gameWidth / 2) - (movieWidth / 2), (gameHeight / 2) - (movieHeight / 2)), Color.White);
         
            spriteBatch.End();

            // If the movie is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        #endregion
    }
}
