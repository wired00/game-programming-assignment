using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

/// <summary>
/// Audio manager handles all audio
/// </summary>
namespace BatteryDerby
{
    public class AudioManager
    {
        Game game;
        private SoundEffect soundFX;
        public SoundEffectInstance music;
        public SoundEffectInstance accelerate;
        public SoundEffectInstance boost;
        public SoundEffectInstance boostEnd;
        public SoundEffectInstance boostStart;
        public SoundEffectInstance crash;
        public SoundEffectInstance enemyDeath;
        public SoundEffectInstance idleLoop;
        public SoundEffectInstance revving;
        public SoundEffectInstance charge;

        public AudioManager(Game game)
        {
            this.game = game;
            LoadContent();
        }

        public  void Initialize()
        {
        }

        protected void LoadContent()
        {
            soundFX = game.Content.Load<SoundEffect>(@"Audio/bgm");
            music = soundFX.CreateInstance();
            music.Volume = 0.2f;
            music.IsLooped = true;
            music.Play();

            soundFX = game.Content.Load<SoundEffect>(@"Audio/accelerate");
            accelerate = soundFX.CreateInstance();
            accelerate.Volume = 0.3f;

            soundFX = game.Content.Load<SoundEffect>(@"Audio/boost");
            boost = soundFX.CreateInstance();
            boost.Volume = 0.9f;

            soundFX = game.Content.Load<SoundEffect>(@"Audio/crash");
            crash = soundFX.CreateInstance();
            crash.Volume = 0.2f;

            soundFX = game.Content.Load<SoundEffect>(@"Audio/enemydeath");
            enemyDeath = soundFX.CreateInstance();
            enemyDeath.Volume = 0.4f;

            soundFX = game.Content.Load<SoundEffect>(@"Audio/idleloop");
            idleLoop = soundFX.CreateInstance();
            idleLoop.Volume = 0.4f;

            soundFX = game.Content.Load<SoundEffect>(@"Audio/revving");
            revving = soundFX.CreateInstance();
            revving.Volume = 0.5f;
            revving.IsLooped = true;
            revving.Play();

            soundFX = game.Content.Load<SoundEffect>(@"Audio/charge");
            charge = soundFX.CreateInstance();
            charge.Volume = 0.5f;
        }

        public void Update(GameTime gametime)
        {
        }

    }
}
