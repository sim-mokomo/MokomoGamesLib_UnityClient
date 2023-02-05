using UnityEditor;

namespace MokomoGamesLib.Editor
{
    public class Repository
    {
        public static void Refresh()
        {
            AssetDatabase.Refresh();
        }
    }
}