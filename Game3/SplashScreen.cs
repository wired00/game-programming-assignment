using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game3
{
    public class SplashScreen : DrawableGameComponent
    {

        string textToDraw;
        string secondaryTextToDraw;
        SpriteFont spriteFont;
        SpriteFont secondarySpriteFont;
        SpriteBatch spriteBatch;
        Game1.GameState currentGameState;
        
        public SplashScreen(Game game) : base (game) { }

        protected override void LoadContent()
        {
            // Load fonts
            spriteFont = Game.Content.Load<SpriteFont>(@"Fonts/MyFont");
            secondarySpriteFont = Game.Content.Load<SpriteFont>(@"Fonts/MyFont");

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

            //Draw main text
            spriteBatch.DrawString(spriteFont, textToDraw,
                                    new Vector2(Game.Window.ClientBounds.Width / 2
                                                - Game.Window.ClientBounds.Height / 2),
                                                Color.Gold);
            //Draw subtext
            spriteBatch.DrawString(secondarySpriteFont, secondaryTextToDraw,
                                    new Vector2(Game.Window.ClientBounds.Width / 2
                                                - secondarySpriteFont.MeasureString
                                                (secondaryTextToDraw).X / 2,
                                                Game.Window.ClientBounds.Height / 2
                                                + TitleSize.Y + 10), Color.Gold);

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
                    secondaryTextToDraw = "Space bar to boost and damage enemies.\n\n Press ENTER to begin";
                    break;
                case Game1.GameState.END:
                    secondaryTextToDraw = "Press ENTER to quit";
                   
                    break;
            }

        }

        
    }




}

