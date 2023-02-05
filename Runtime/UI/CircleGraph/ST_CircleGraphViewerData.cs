using UnityEngine;

namespace MokomoGamesLib.Runtime.UI.CircleGraph
{
    public class ST_CircleGraphViewData
    {
        public ST_CircleGraphViewData(float rate, string name, Color color)
        {
            Rate = rate;
            Name = name;
            Color = color;
        }

        public float Rate { get; }
        public string Name { get; }
        public Color Color { get; }
    }
}