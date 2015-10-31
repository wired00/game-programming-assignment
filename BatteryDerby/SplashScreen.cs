using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


/// <summary>
/// Handles everything regarding splash screen either at startup or death screen
/// </summary>
namespace BatteryDerby
{
    public class SplashScreen : DrawableGameComponent
    {

        string textToDraw;
        string secondaryTextToDraw;
        SpriteFont spriteFont;
        SpriteFont secondarySpriteFont;
        SpriteBatch spriteBatch;
        Game1.GameState currentGameState;
        Texture2D backgroundTexture;
        
        public SplashScreen(Game game) : base (game) { }

        protected override void LoadContent()
        {
            // Load fonts
            spriteFont = Game.Content.Load<SpriteFont>(@"Fonts/MyFont");
            secondarySpriteFont = Game.Content.Load<SpriteFont>(@"Fonts/MyFont");
            backgroundTexture = Game.Content.Load<Texture2D>(@"Models/Splashscreen/StartBackGround");

            // Create sprite batch
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            //Did player hit enter?
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                
                //If not in end game, then move to play state
                if (currentGameState == Game1.GameState.LEVEL_CHANGE ||
                    currentGameState == Game1.GameState.START)
                    ((Game1)Game).ChangeGameState(Game1.GameState.PLAY, 0);

                // If at end game, exit
                else if (currentGameState == Game1.GameState.END)
                    Game.Exit();
            }
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            //Get size of string
            Vector2 TitleSize = spriteFont.MeasureString(textToDraw);
            base.Draw(gameTime);


            
            if (currentGameState == Game1.GameState.START)
            {
                spriteBatch.Draw(backgroundTexture,
                                  new Rectangle(0, 0, Game.Window.ClientBounds.Width,
                                                Game.Window.ClientBounds.Height), null,
                                                Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);

                //Draw main text
                spriteBatch.DrawString(spriteFont, textToDraw,
                                        new Vector2(Game.Window.ClientBounds.Width / 2
                                                    - Game.Window.ClientBounds.Height / 2),
                                                    Color.Black);

                //Draw subtext
                spriteBatch.DrawString(secondarySpriteFont, secondaryTextToDraw,
                                        new Vector2(Game.Window.ClientBounds.Width / 2
                                                    - secondarySpriteFont.MeasureString
                                                    (secondaryTextToDraw).X / 2,
                                                    Game.Window.ClientBounds.Height / 2
                                                    + TitleSize.Y - 20), Color.Black);


            }

            if (currentGameState == Game1.GameState.END)
            {
                spriteBatch.Draw(backgroundTexture,
                                  new Rectangle(0, 0, Game.Window.ClientBounds.Width,
                                                Game.Window.ClientBounds.Height), null,
                                                Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);

                //Draw main text
                spriteBatch.DrawString(spriteFont, textToDraw,
                                        new Vector2(Game.Window.ClientBounds.Width / 2
                                                    - Game.Window.ClientBounds.Height / 2 ),
                                                    Color.Black);

                //Draw subtext
                spriteBatch.DrawString(secondarySpriteFont, secondaryTextToDraw,
                                        new Vector2(Game.Window.ClientBounds.Width / 2
                                                    - secondarySpriteFont.MeasureString
                                                    (secondaryTextToDraw).X / 2,
                                                    Game.Window.ClientBounds.Height / 2
                                                    + TitleSize.Y - 10), Color.Black);
            }  

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void SetData(string textToDraw, Game1.GameState currGameState)
        {
            this.textToDraw = textToDraw;
            this.currentGameState = currGameState;

            switch (currentGameState)
            {
                case Game1.GameState.START:
                case Game1.GameState.LEVEL_CHANGE:
                    secondaryTextToDraw = "WASD to move, Space bar to boost.\n\n Press ENTER to begin";
                    break;
                case Game1.GameState.END:
                    secondaryTextToDraw = "Press ENTER to quit";
                   
                    break;
            }

        }

        
    }




}

