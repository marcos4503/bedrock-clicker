using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bedrock_Clicker
{
    public class ApplicationPreferences
    {
        //Constants
        private const string fileName = "settings.xml";

        //Variables
        public int windowPositionX = 80;
        public int windowPositionY = 80;
        public int clicksPerSecond = 0;
        public int toggleHotkey = 0;
        public int toggleHotkey2 = 0;
        public int sprintHotkey = 0;
        public int autoToggleOff = 0;
        public int worksOnlyInMinecraft = 0;
        public int playSound = 0;
        public int aimComplementPhysic = 0;

        //Core methods

        public void LoadPreferences()
        {
            //If the save file not exists, create one and return
            if (File.Exists(fileName) == false)
            {
                ApplyPreferences();
                return;
            }

            //Deserealize settings
            using (StreamReader sw = new StreamReader(fileName))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ApplicationPreferences));
                ApplicationPreferences loadedPreferences = xmls.Deserialize(sw) as ApplicationPreferences;

                //Get the loaded preferences
                windowPositionX = loadedPreferences.windowPositionX;
                windowPositionY = loadedPreferences.windowPositionY;
                clicksPerSecond = loadedPreferences.clicksPerSecond;
                toggleHotkey = loadedPreferences.toggleHotkey;
                toggleHotkey2 = loadedPreferences.toggleHotkey2;
                sprintHotkey = loadedPreferences.sprintHotkey;
                autoToggleOff = loadedPreferences.autoToggleOff;
                worksOnlyInMinecraft = loadedPreferences.worksOnlyInMinecraft;
                playSound = loadedPreferences.playSound;
                aimComplementPhysic = loadedPreferences.aimComplementPhysic;
            }
        }

        public void ApplyPreferences()
        {
            //Save all preferences to file (serialization)
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ApplicationPreferences));
                xmls.Serialize(sw, this);
            }
        }
    }
}
