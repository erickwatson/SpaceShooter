using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;

namespace FirstProject
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        int currentFPS = 0;
        float fpsTimer = 0;
        int fpsCounter = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // define constant values for the game states
        const int STATE_RESTART = 0;
        const int STATE_SPLASH = 1;
        const int STATE_GAME = 2;
        const int STATE_GAMEOVER = 3;

        int gameState = STATE_SPLASH;

        //OnScreen
        SpriteFont arialFont;
        Texture2D shipTexture;
        Texture2D asteroidTexture;
        Texture2D spaceBG;
        Texture2D bulletTexture;
        Texture2D startGamePrompt;
        Texture2D gameOverPrompt;

        //Player
        float playerSpeed = 150;
        float playerRotateSpeed = 5;
        float playerSlowdown = 0.9999f;
        Vector2 playerPosition = new Vector2(0, 0);
        Vector2 playerOffset = new Vector2(0, 0);
        bool playerAlive = false;


        float playerAngle = 0;
        Vector2 playerVelocity = new Vector2(0,0);

        //Bullet
        float bulletSpeed = 600;
        //OC- Vector2 bulletPosition = new Vector2(0, 0);
        //OC- Vector2 bulletVelocity = new Vector2(0, 0);
        ArrayList bulletPositions = new ArrayList();
        ArrayList bulletVelocities = new ArrayList();
        int maxBullets = 1;
        float bulletReload = 0.1f;
        //OC- bool bulletAlive = false;

        //Enemy
        float asteroidSpeed = 90;
        Vector2 asteroidOffset = new Vector2();
        ArrayList asteroidPositions = new ArrayList();
        ArrayList asteroidVelocities = new ArrayList();
        ArrayList asteroidSpeeds = new ArrayList();
        //OC- bool asteroidAlive = false;

        //Timer
        float timer = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.IsFixedTimeStep = false;
            this.graphics.SynchronizeWithVerticalRetrace = false;

            //These lines define the Window or "BackBuffer" size
            graphics.PreferredBackBufferWidth = 1152;
            graphics.PreferredBackBufferHeight = 648;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 
        private void UpdateSplashState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;
            }
        }

        private void DrawSplashState(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(startGamePrompt,
                new Vector2(300, 150), Color.White);
        }
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            playerPosition = new Vector2(
                                    graphics.GraphicsDevice.Viewport.Width / 1.2f,
                                    graphics.GraphicsDevice.Viewport.Height / 1.2f);

            //OLDCODE This used to set the asteroid as "Alive" in the game
            //OC- asteroidPosition = new Vector2(0, 0);
            //OC- asteroidAlive = true;
            //
            // We now set this in the ArrayLists in the LoadContent function

            playerAlive = true;

            //gameState = STATE_GAME;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            arialFont = Content.Load<SpriteFont>("Arial");
            shipTexture = Content.Load<Texture2D>("ship");
            spaceBG = Content.Load<Texture2D>("spacebg");
            asteroidTexture = Content.Load<Texture2D>("rock_large");
            bulletTexture = Content.Load<Texture2D>("bullet");
            startGamePrompt = Content.Load<Texture2D>("EntToStart");
            gameOverPrompt = Content.Load<Texture2D>("GameOver");

            // find the width and height of the shop so we can calculate the offset
            // (so that when we draw the ship, the player position refers to the
            // center of the ship)
            playerOffset = new Vector2(shipTexture.Width / 2, shipTexture.Height / 2);

            //FINSH FINDING SCREEN CENTRE
            //screenCenter = new Vector2(Viewport)
            //
            //startGameOffset = new Vector2(startGamePrompt.Width / 2, startGamePrompt.Height / 2);
            //
            //gameOverOffset = new Vector2(gameOverPrompt.Width / 2, gameOverPrompt.Height / 2);

            asteroidOffset = new Vector2(asteroidTexture.Width / 2,
                                            asteroidTexture.Height / 2);

            Random random = new Random();
            for (int i = 0; i < 10; i++)    // create some asteroids
            {
                Vector2 randDirection = new Vector2(
                        random.Next(-100, 100), random.Next(-100, 100));
                randDirection.Normalize();

                Vector2 asteroidPosition =
                    randDirection * GraphicsDevice.Viewport.Height;
                asteroidPositions.Add(asteroidPosition);

                asteroidSpeeds.Add((float)random.Next(10, 200));

                Vector2 velocity = playerPosition - asteroidPosition;
                velocity.Normalize();
                velocity *= ((float)(asteroidSpeeds[i]));

                asteroidVelocities.Add(velocity);
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        private void UpdatePlayer(float deltaTime)
        {
            float currentSpeed = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.Up) == true)
            {
                currentSpeed = playerSpeed * deltaTime;
            }
            else
            {
                playerVelocity = playerVelocity * playerSlowdown;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down) == true)
            {
                currentSpeed = -playerSpeed * deltaTime;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left) == true)
            {
                playerAngle -= playerRotateSpeed * deltaTime;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right) == true)
            {
                playerAngle += playerRotateSpeed * deltaTime;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) == true)
            {
                //STRAFE FUNCTION
                //RADIANS CALCULATE ANGLE RELATIVE TO PLAYER CENTRE
                float s = (float)Math.Sin(playerAngle + 1.5708);
                float c = (float)Math.Cos(playerAngle + 1.5708);
                float xSpeed = 0;
                float ySpeed = playerSpeed * deltaTime;
                playerPosition.X += (xSpeed * c) - (ySpeed * s);
                playerPosition.Y += (xSpeed * s) + (ySpeed * c);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A) == true)
            {
                //STRAFE FUNCTION
                //RADIANS CALCULATE ANGLE RELATIVE TO PLAYER CENTRE
                float s = (float)Math.Sin(playerAngle + -1.5708);
                float c = (float)Math.Cos(playerAngle + -1.5708);
                float xSpeed = 0;
                float ySpeed = playerSpeed * deltaTime;
                playerPosition.X += (xSpeed * c) - (ySpeed * s);
                playerPosition.Y += (xSpeed * s) + (ySpeed * c);
            }

            // figure out the x and y movement based on the current rotation
            // (so that if the ship is moving forward, then it will move
            // forward according to its rotation. Without this, the ship
            // would just move up the screen when we pressed 'up',
            // regardless of which way it was rotated)
            // for an explanation of this formula, see
            // http://en.wikipedia.org/wiki/Rotation_matrix
            // calculate sin and cos for the player's current rotation
            Vector2 playerDirection = new Vector2(-(float)Math.Sin(playerAngle),
                                                   (float)Math.Cos(playerAngle));
            playerDirection.Normalize();

            //Update Timer
            timer += deltaTime;

            //Shoot a bullet 
            if (Keyboard.GetState().IsKeyDown(Keys.Space) == true && bulletPositions.Count < maxBullets && timer > bulletReload)
            {
                // int num = 10;
                // for (int i = 0; i < num; i++)
                // {
                bulletPositions.Add(playerPosition);
                bulletVelocities.Add(playerDirection * bulletSpeed);
                // }
                //OC- bulletPosition = playerPosition;
                //OC- bulletVelocity = playerDirection * bulletSpeed;
                //OC- bulletAlive = true;
            }

            // the Vector2 class also has a normalize function
            Vector2 direction = new Vector2(40, 30);
            direction.Normalize();

            playerVelocity += playerDirection * currentSpeed * deltaTime;
            playerPosition += playerVelocity;



            //Screen Wrap
            if (playerPosition.X < 0)// Left
            {
                // Move to Right
                playerPosition.X = graphics.GraphicsDevice.Viewport.Width;
            }
            if (playerPosition.X > graphics.GraphicsDevice.Viewport.Width)// Right
            {
                // Move to Left
                playerPosition.X = 0;
            }
            if (playerPosition.Y < 0)// Top
            {
                // Move to Bottom
                playerPosition.Y = graphics.GraphicsDevice.Viewport.Height;
            }
            if (playerPosition.Y > graphics.GraphicsDevice.Viewport.Height)// Bottom
            {
                // Move to Top
                playerPosition.Y = 0;
            }

        }
        //Bullet Scripts
        private void UpdateBullets(float deltaTime)
        {
            for (int i = 0; i < bulletPositions.Count; i++)
            {
                bulletPositions[i] = (Vector2)bulletPositions[i] + ((Vector2)bulletVelocities[i] * deltaTime);

                if (((Vector2)bulletPositions[i]).X < 0 ||
                    ((Vector2)bulletPositions[i]).X > graphics.GraphicsDevice.Viewport.Width ||
                    ((Vector2)bulletPositions[i]).Y < 0 ||
                    ((Vector2)bulletPositions[i]).Y > graphics.GraphicsDevice.Viewport.Height)
                {
                    //OC- bulletAlive = false;
                    bulletPositions.RemoveAt(i);
                    bulletVelocities.RemoveAt(i);

                }
            }
        }
        //Collision Scripts - Circle Collision
        private bool IsColliding(Vector2 position1, float radius1, Vector2 position2, float
            radius2)
        {
            Vector2 distance = position2 - position1;

            if (distance.Length() < radius1 + radius2)
            {
                // the distance between these two circles is less
                // than their radii, so they must be colliding
                return true;
            }
            // else, the two circles are not colliding
            return false;
        }

        //Collision Scripts - Rectangle Collision
        private bool IsColliding(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.X + rect1.Width < rect2.X ||
                rect1.X > rect2.X + rect2.Width ||
                rect1.Y + rect1.Height < rect2.Y ||
                rect1.Y > rect2.Y + rect2.Height)

            {
                // these two rectangles are not colliding
                return false;
            }
            // else, the two AABB rectangles overlap, therefore collision...
            return true;
        }

        //Asteroid Scripts
        private void UpdateAsteroids(float deltaTime)
        {
            for (int asteroidIdx = 0; asteroidIdx < asteroidPositions.Count; asteroidIdx++)
            {
                //  the one thing we have to be aware of here is that the Vector2
                // (position) is stored in the ArrayList as an object so we can't
                // modify any of its variables. We need to get a copy of the Vector2,
                // modify the copy, then overwrite the Vector2 in the ArrayList with
                // the updated copy. (Same goes for the velocity)
                Vector2 position = (Vector2)asteroidPositions[asteroidIdx];
                Vector2 velocity = (Vector2)asteroidVelocities[asteroidIdx];



                //Screen Wrap
                if (position.X < 0 && velocity.X < 0)// Left
                {
                    // Move to Right
                    position.X = graphics.GraphicsDevice.Viewport.Width;
                }
                if (position.X > graphics.GraphicsDevice.Viewport.Width && velocity.X > 0)// Right
                {
                    // Move to Left
                    position.X = 0;
                }
                if (position.Y < 0 && velocity.Y < 0)// Top
                {
                    // Move to Bottom
                    position.Y = graphics.GraphicsDevice.Viewport.Height;
                }
                if (position.Y > graphics.GraphicsDevice.Viewport.Height && velocity.X > 0)// Bottom
                {
                    // Move to Top
                    position.Y = 0;
                }

                position += velocity * deltaTime;
                asteroidPositions[asteroidIdx] = position;
                //// if the asteroid goes off the screen, reverse the velocity(does this make it bounce off the "walls" ?)
                //if (position.X < 0 && velocity.X < 0 ||
                //    position.X > graphics.GraphicsDevice.Viewport.Width && velocity.X > 0)
                //{
                //    velocity.Y = -velocity.Y;
                //    asteroidVelocities[asteroidIdx] = velocity;
                //}
                //if (position.Y < 0 && velocity.Y < 0 ||
                //    position.Y > graphics.GraphicsDevice.Viewport.Height && velocity.Y > 0)
                //{
                //    velocity.X = -velocity.X;
                //    asteroidVelocities[asteroidIdx] = velocity;
                //}



            }

            //OC- Vector2 direction = playerPosition - asteroidPosition;
            //OC- direction.Normalize();
            //
            //OC- Vector2 asteroidVelocity = direction * asteroidSpeed * deltaTime;
            //
            //OC- asteroidPosition += asteroidVelocity;
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (gameState)
            {
                case STATE_SPLASH:
                    UpdateSplashState(deltaTime);
                    break;
                case STATE_GAME:
                    UpdateGameState(deltaTime);
                    break;
                case STATE_GAMEOVER:
                    UpdateGameOverState(deltaTime);
                    break;
            }
            base.Update(gameTime);






        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>


        private void UpdateGameState(float deltaTime)
        {


            // calculate the current FPS (frames per second)
            // This is debug code, you can remove it from your final game
            //fpsTimer += deltaTime;
            //if (fpsTimer > 1.0f)
            //{
            //    fpsTimer = 0f;
            //    currentFPS = fpsCounter;
            //    fpsCounter = 0;
            //}
            //fpsCounter++;

            //int counter = 0;
            //for (int i = 0; i < 0; i++)
            //{
            //    counter += i;
            //}

            UpdatePlayer(deltaTime);
            UpdateAsteroids(deltaTime);
            UpdateBullets(deltaTime);


            Rectangle playerRect = new Rectangle(
                            (int)(playerPosition.X - playerOffset.X),
                            (int)(playerPosition.Y - playerOffset.Y),
                            shipTexture.Bounds.Width, shipTexture.Bounds.Height);
            // check for collisions

            for (int asteroidIdx = 0; asteroidIdx < asteroidPositions.Count; asteroidIdx++)
            {


                Vector2 position = (Vector2)asteroidPositions[asteroidIdx];
                Rectangle asteroid = new Rectangle(
                    (int)(position.X - asteroidOffset.X),
                    (int)(position.Y - asteroidOffset.Y),
                    asteroidTexture.Width, asteroidTexture.Height);

                if (IsColliding(playerRect, asteroid) == true)
                {
                    // player and asteroid are colliding, destroy both
                    playerAlive = false;
                    asteroidPositions.RemoveAt(asteroidIdx);
                    asteroidVelocities.RemoveAt(asteroidIdx);
                    gameState = STATE_GAMEOVER;

                }

                for (int bulletIdx = 0; bulletIdx < bulletPositions.Count; bulletIdx++)
                {
                    Rectangle bulletRect = new Rectangle((int)((Vector2)bulletPositions[bulletIdx]).X,
                               (int)((Vector2)bulletPositions[bulletIdx]).Y,
                               bulletTexture.Bounds.Width,
                               bulletTexture.Bounds.Height);


                    if (IsColliding(bulletRect, asteroid) == true)
                    {
                        // bullet and asteroid are colliding, destroy both
                        //OC- bulletAlive = false; //bullets are now in their own array list (bulletIdx)
                        asteroidPositions.RemoveAt(asteroidIdx);
                        asteroidVelocities.RemoveAt(asteroidIdx);

                        bulletPositions.RemoveAt(bulletIdx);
                        bulletVelocities.RemoveAt(bulletIdx);
                        // once we hit the first thing and kill the bullet, there's no
                        // point in checking the rest of the ArrayList
                        break;
                    }

                }
                if (asteroidPositions.Count == 0)
                {
                    //no more asteroids, end the 'game'
                    gameState = STATE_GAMEOVER;
                }
            }
        }

        private void DrawGameState(SpriteBatch spriteBatch)
        {

            if (playerAlive == true)
            {
                spriteBatch.Draw(shipTexture, playerPosition,
                    null, null, playerOffset, playerAngle, null, Color.White);
            }

            //OC- if (asteroidAlive == true) {
            //OC-    spriteBatch.Draw(asteroidTexture, asteroidPosition,
            //OC-         null, null, asteroidOffset, 0, null, Color.White);
            for (int asteroidIdx = 0; asteroidIdx < asteroidPositions.Count; asteroidIdx++)
            {
                Vector2 position = (Vector2)asteroidPositions[asteroidIdx];
                spriteBatch.Draw(asteroidTexture, position, null, null,
                                 asteroidOffset, 0, null, Color.White);
            }


            //OC- spriteBatch.Draw(asteroidTexture, asteroidPosition,
            //OC-    null, null, asteroidOffset, 0, null, Color.White);

            spriteBatch.DrawString(arialFont, currentFPS.ToString(),
                                    new Vector2(20, 20), Color.Yellow);

            spriteBatch.End();

            //base.Draw(gameTime);
        }

        private void UpdateGameOverState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_SPLASH;
            }
        }

        private void DrawGameOverState(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(gameOverPrompt,
                new Vector2(300, 150), Color.White);
        }
        //protected override void Draw(GameTime gameTime)

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(spaceBG, new Vector2(0, 0), Color.White);
            for (int i = 0; i < bulletPositions.Count; i++)
            {
                spriteBatch.Draw(bulletTexture, (Vector2)bulletPositions[i],
                    null, null, null, 0, null, Color.White);
            }

            //OLDCODE
            //OC- spriteBatch.DrawString(arialFont, "This is totally a program!!",
            //OC-     new Vector2(10,50), Color.White);

            //OLDCODE
            //OC- spriteBatch.Draw(shipTexture, new Vector2(100, 100), Color.White);
            //OC- spriteBatch.Draw(shipTexture, playerPosition - playerOffset, Color.White);
            //OC- spriteBatch.Draw(shipTexture, playerPosition,
            //OC-     null, null, playerOffset, playerAngle, null, Color.White);

            switch (gameState)
            {
                case STATE_SPLASH:
                    DrawSplashState(spriteBatch);
                    break;
                case STATE_GAME:
                    DrawGameState(spriteBatch);
                    break;
                case STATE_GAMEOVER:
                    DrawGameOverState(spriteBatch);
                    break;
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }







    }
}
