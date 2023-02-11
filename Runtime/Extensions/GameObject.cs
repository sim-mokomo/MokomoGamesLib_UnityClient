using UnityEngine;

namespace MokomoGamesLib.Runtime.Extensions
{
    public static class GameObjectExtensions
    {
        public static void SetPositionOnlyXZ(this GameObject self, Vector3 distPos)
        {
            var pos = self.transform.position;
            pos.x = distPos.x;
            pos.z = distPos.z;
            self.transform.position = pos;
        }
    }
}