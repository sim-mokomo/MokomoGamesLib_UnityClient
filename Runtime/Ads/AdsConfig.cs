using System.Collections.Generic;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads
{
    public class UnitIdRecord
    {
        public AdsType AdsType;
        public string UnitID;
        public string UnitTestId;
    }
    
    public class AdsConfig
    {
        public RuntimePlatform RuntimePlatform;
        public List<UnitIdRecord> UnitIdTable;
    }
}