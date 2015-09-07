using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game3 {
    class DebugPrint {
        private SpriteFont spriteFont;
        private SpriteBatch batch;
        private ContentManager contentManager;

        public DebugPrint(SpriteBatch batch, ContentManager contentManager) {
            this.batch = batch;
            this.contentManager = contentManager;
            this.contentManager.RootDirectory = "Content";
            this.spriteFont = contentManager.Load<SpriteFont>("MyFont");
        }

        public void LoadContent(ContentManager content) {
            this.spriteFont = contentManager.Load<SpriteFont>("MyFont");
        }
        public void DrawIt(String blah, SpriteBatch batch) {
            batch.DrawString(this.spriteFont, "HELLO WORLD BABY " + blah, new Vector2(10f, 10f), Color.White);
        }
    }
}
