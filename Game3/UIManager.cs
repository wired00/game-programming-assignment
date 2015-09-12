using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Game3
{
    public class UIManager
    {
        Game game;
        SpriteBatch spriteBatch;
        SpriteFont font;
        public float playerHealth = 100;
        public float playerEnergy = 100;
        public string debug1 = "1";
        public string debug2 = "2";

        public UIManager(Game game)
        {
            this.game = game;
            LoadContent();
        }

        public  void Initialize()
        {
            //base.Initialize();
        }

        protected void LoadContent()
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            font = game.Content.Load<SpriteFont>("Fonts/myFont");
            //base.LoadContent();
        }

        public void Update(GameTime gametime)
        {

            //base.Update(gametime);
        }

        public void Draw(GameTime gametime) {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, "Health " + playerHealth, new Vector2(0, 0), Color.Cyan);
            spriteBatch.DrawString(font, "Energy " + Math.Round(playerEnergy, 2), new Vector2(0, 40), Color.Cyan);
            spriteBatch.End();

            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            //base.Draw(gametime);
        }

    }
}
