﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Game3
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
            //base.Initialize();
        }

        protected void LoadContent()
        {
            soundFX = game.Content.Load<SoundEffect>(@"Audio/bgm");
            music = soundFX.CreateInstance();
            music.Volume = 0.4f;
            music.IsLooped = true;
            music.Play();
            soundFX = game.Content.Load<SoundEffect>(@"Audio/accelerate");
            accelerate = soundFX.CreateInstance();
            accelerate.Volume = 0.5f;
            soundFX = game.Content.Load<SoundEffect>(@"Audio/boost");
            boost = soundFX.CreateInstance();
            boost.Volume = 1f;
            soundFX = game.Content.Load<SoundEffect>(@"Audio/boostend");
            boostEnd = soundFX.CreateInstance();
            soundFX = game.Content.Load<SoundEffect>(@"Audio/booststart");
            boostStart = soundFX.CreateInstance();
            soundFX = game.Content.Load<SoundEffect>(@"Audio/crash");
            crash = soundFX.CreateInstance();
            crash.Volume = 0.5f;
            soundFX = game.Content.Load<SoundEffect>(@"Audio/enemydeath");
            enemyDeath = soundFX.CreateInstance();
            enemyDeath.Volume = 0.5f;
            soundFX = game.Content.Load<SoundEffect>(@"Audio/idleloop");
            idleLoop = soundFX.CreateInstance();
            idleLoop.Volume = 0.5f;
            soundFX = game.Content.Load<SoundEffect>(@"Audio/revving");
            revving = soundFX.CreateInstance();
            soundFX = game.Content.Load<SoundEffect>(@"Audio/charge");
            charge = soundFX.CreateInstance();
            charge.Volume = 1f;
            //base.LoadContent();
        }

        public void Update(GameTime gametime)
        {

            //base.Update(gametime);
        }

    }
}
