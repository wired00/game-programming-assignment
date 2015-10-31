using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BatteryDerby {

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
        public enum GameState { START, PLAY, LEVEL_CHANGE, END }
        public GameState currentGameState = GameState.START;

        public bool debugMode { get; set; }

        KeyboardState lastKeyboardState;

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
            this.debugMode = false;
        }

        protected override void Initialize() {
            camera = new Camera(this, new Vector3(770, 1200, 1090), new Vector3(770, -2800, -500), Vector3.Up);
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

            this.IsMouseVisible = true;

            base.Initialize();
        }
        
        protected override void LoadContent() {
            device = graphics.GraphicsDevice;
        }
        
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }
        
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            this.gameTime = gameTime;
            score.survivalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleKeyboardInput();

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
            device.DepthStencilState = DepthStencilState.Default;
        }

        private void HandleKeyboardInput() {

            if (Keyboard.GetState().IsKeyDown(Keys.F1) && Keyboard.GetState() != lastKeyboardState) {
                this.debugMode = !debugMode; // switch debugmode
                
            }
            lastKeyboardState = Keyboard.GetState();
        }

    }
}
