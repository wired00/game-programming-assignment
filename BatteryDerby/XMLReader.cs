using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml;

namespace BatteryDerby {
    /// <summary>
    /// XML Reader helper class to read in XML files
    /// </summary>
    class XMLReader {
        Game1 game;

        /// <summary>
        /// On construct, read all xml files.
        /// </summary>
        /// <param name="game"></param>
        public XMLReader(Game1 game) {
            this.game = game;

            ReadXMLBehaviour(); // READ XML BEHAVIOUR
            ReadXMLConfig(); // READ XML CONFIG
        }

        private void ReadXMLBehaviour() {
            bool bReadingEnemyDamagedBehaviour = false;

            XmlTextReader reader = new XmlTextReader("Content/behaviour.xml");

            while (reader.Read()) {

                switch (reader.NodeType) {

                    case XmlNodeType.Element:
                        // check for start of xml element
                        if (reader.Name == "EnemyDamagedBehaviour") { bReadingEnemyDamagedBehaviour = true; }
                        break;

                    case XmlNodeType.Text:
                        // read xml element's value, transpose to game enemyDamagedBehaviour enum value
                        if (bReadingEnemyDamagedBehaviour) {
                            if (reader.Value == "aggressive") {
                                this.game.enemyDamagedBehaviour = Game1.EnemyDamagedBehaviour.aggressive;
                            } else if (reader.Value == "normal") {
                                this.game.enemyDamagedBehaviour = Game1.EnemyDamagedBehaviour.normal;
                            } else if (reader.Value == "thief") {
                                this.game.enemyDamagedBehaviour = Game1.EnemyDamagedBehaviour.thief;
                            }
                        }
                        break;

                    case XmlNodeType.EndElement:
                        // check for end of xml element
                        if (reader.Name == "EnemyDamagedBehaviour") { bReadingEnemyDamagedBehaviour = false; }
                        break;
                }
            }
        }

        private void ReadXMLConfig() {
            bool bReadingEnemyMoveSpeed = false;
            bool bReadingPlayerMoveSpeed = false;
            bool bReadingTruckMoveSpeed = false;
            bool bReadingEnemyHealth = false;
            bool bReadingPlayerHealth = false;
            bool bReadingTruckHealth = false;

            XmlTextReader reader = new XmlTextReader("Content/config.xml");

            while (reader.Read()) {

                switch (reader.NodeType) {

                    case XmlNodeType.Element:
                        // check for start of xml element
                        if (reader.Name == "EnemyMoveSpeed") { bReadingEnemyMoveSpeed = true; }
                        if (reader.Name == "PlayerMoveSpeed") { bReadingPlayerMoveSpeed = true; }
                        if (reader.Name == "TruckMoveSpeed") { bReadingTruckMoveSpeed = true; }
                        if (reader.Name == "EnemyHealth") { bReadingEnemyHealth = true; }
                        if (reader.Name == "PlayerHealth") { bReadingPlayerHealth = true; }
                        if (reader.Name == "TruckHealth") { bReadingTruckHealth = true; }
                        break;

                    case XmlNodeType.Text:
                        // read xml element's value, transpose to game enemyDamagedBehaviour enum value
                        if (bReadingEnemyMoveSpeed) { this.game.enemyMoveSpeed = float.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat); }
                        if (bReadingPlayerMoveSpeed) { this.game.playerMoveSpeed = float.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat); }
                        if (bReadingTruckMoveSpeed) { this.game.truckMoveSpeed = float.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat); }
                        if (bReadingEnemyHealth) { this.game.enemyHealth = float.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat); }
                        if (bReadingPlayerHealth) { this.game.playerHealth = float.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat); }
                        if (bReadingTruckHealth) { this.game.truckHealth = float.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat); }
                        break;

                    case XmlNodeType.EndElement:
                        // check for end of xml element
                        if (reader.Name == "EnemyMoveSpeed") { bReadingEnemyMoveSpeed = false; }
                        if (reader.Name == "PlayerMoveSpeed") { bReadingPlayerMoveSpeed = false; }
                        if (reader.Name == "TruckMoveSpeed") { bReadingTruckMoveSpeed = false; }
                        if (reader.Name == "EnemyHealth") { bReadingEnemyHealth = false; }
                        if (reader.Name == "PlayerHealth") { bReadingPlayerHealth = false; }
                        if (reader.Name == "TruckHealth") { bReadingTruckHealth = false; }
                        break;

                }
            }
        }
    }
}
