using System.Windows.Media;

namespace TransQuikConfiguration.Model
{
    public class ConfigurationMenu
    {
        public int ButtonMenuID { get; set; }
        public string ButtonMenuName { get; set; }

        public string ButtonKindIcon { get; set; }

        public ConfigurationMenu(int buttonMenuID, string buttonMenuName, string buttonKindIcon)
        {
            ButtonMenuID = buttonMenuID;
            ButtonMenuName = buttonMenuName;
            ButtonKindIcon = buttonKindIcon;
        }
    }
}
