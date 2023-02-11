using System.Collections.Generic;
using System.Linq;
using MokomoGamesLib.Runtime.Licences.MasterData;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Licences
{
    public class LicenceManager : MonoBehaviour
    {
        [SerializeField] private List<Record> masterData;

        public List<Licence> Load()
        {
            return
                masterData
                    .Select(x => new Licence(x.Title, x.LicenceText.text))
                    .ToList();
        }
    }
}