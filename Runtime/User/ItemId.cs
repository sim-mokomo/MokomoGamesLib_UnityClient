using UnityEngine;

namespace MokomoGamesLib.Runtime.User
{
    public class ItemId
    {
        private readonly string _name;

        public ItemId(string name)
        {
            _name = name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ItemId);
        }

        public override int GetHashCode()
        {
            return IdByPlatform().GetHashCode();
        }

        private bool Equals(ItemId obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return false;

            if (GetType() != obj.GetType()) return false;

            return IdByPlatform() == obj.IdByPlatform();
        }

        public string IdByPlatform()
        {
            var itemBaseId = $"com.{Application.companyName}.{Application.productName}.{_name}";
#if UNITY_IOS || UNITY_EDITOR_OSX
            return $"{itemBaseId}.iOS";
#else
            // NOTE: デフォルトでAndroid
            return $"{itemBaseId}.Android";
#endif
        }
    }
}