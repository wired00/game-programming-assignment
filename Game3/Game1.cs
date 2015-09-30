using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TexturedQuad;

namespace Game3 {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        public GraphicsDevice device { get; protected set; }
        public Camera camera { get; protected set; }

        public GraphicsDeviceManager graphics { get; set; }

        ModelManager modelManager;
        public UIManager uiManager;
        public AudioManager audioManager;
        public Score score;
        public SplashScreen splashScreen;
        public GameTime gameTime; 
     

        Matrix world = Matrix.CreateTranslation(0, 0, 0);     
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 2, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);

        // Splash Screen
        public enum GameState { START, PLAY, LEVEL_CHANGE, END}
        public GameState currentGameState = GameState.START;

        public void ChangeGameState (GameState state, int level)
        {
            currentGameState = state;

            switch (currentGameState)
            {
                case GameState.LEVEL_CHANGE:
                    splashScreen.SetData("Level " + (level + 1),
                    GameState.LEVEL_CHANGE);
                    modelManager.Enabled = false;
                    modelManager.Visible = false;
                    splashScreen.Enabled = true;
                    splashScreen.Visible = true;
                    break;

                case GameState.PLAY:
                    modelManager.Enabled = true;
                    modelManager.Visible = true;
                    splashScreen.Enabled = false;
                    splashScreen.Visible = false;
                    break;

                case GameState.END:
                    splashScreen.SetData("Game Over.\n" +
                        "\n Enemies Defeated: " + score.enemiesDefeatedCount +
                        "\n You picked up " + score.playerBatteryCount + " Batteries" +
                        "\n The enemies picked up " + score.enemyBatteryCount + " Batteries" +
                        "\n You survived " + Math.Round(score.survivalTime, 2) + " seconds", GameState.END);
                    modelManager.Enabled = false;
                    modelManager.Visible = false;
                    splashScreen.Enabled = true;
                    splashScreen.Visible = true;
                    
                    break;

            }
        }

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            camera = new Camera(this, new Vector3(0, 700, 400), new Vector3(0, 100, 0), Vector3.Up);
            Components.Add(camera);

            // Splash screen component
            splashScreen = new SplashScreen(this);
            Components.Add(splashScreen);
            splashScreen.SetData("Welcome to Battery Derby!", currentGameState);

            modelManager = new ModelManager(this, splashScreen);
            modelManager.Enabled = false; // for when splash screen is enabled
            modelManager.Visible = false; // for when splash screen is enabled
            Components.Add(modelManager);


            uiManager = new UIManager(this);
            
            audioManager = new AudioManager(this);

            score = new Score();

            //world.Translation = new Vector3(world)

            this.IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            device = graphics.GraphicsDevice;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            this.gameTime = gameTime;
            score.survivalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // fix issue with spriteBatch above changing render states which messex
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
            device.DepthStencilState = DepthStencilState.Default;
        }

    }
}
