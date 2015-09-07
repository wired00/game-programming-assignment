using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Game3 {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        public GraphicsDevice device { get; protected set; }
        public Camera camera { get; protected set; }

        public GraphicsDeviceManager graphics { get; }

        ModelManager modelManager;
        public UIManager uiManager;

        Matrix world = Matrix.CreateTranslation(0, 0, 0);     
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 2, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);

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

            modelManager = new ModelManager(this);
            Components.Add(modelManager);
            uiManager = new UIManager(this);

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
        }

    }
}
