using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Class to keep track of the players score
/// </summary>
namespace BatteryDerby {
    public class Score {
        public int enemyBatteryCount;
        public int playerBatteryCount;
        public int enemiesDefeatedCount;
        public int monsterTruckDefeatedCount;
        public float survivalTime;

        public Score() {
            enemyBatteryCount = 0;
            playerBatteryCount = 0;
            enemiesDefeatedCount = 0;
            monsterTruckDefeatedCount = 0;
            survivalTime = 0;

        }

    }
}


