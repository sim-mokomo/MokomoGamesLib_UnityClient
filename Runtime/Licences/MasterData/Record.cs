using UnityEngine;

namespace MokomoGamesLib.Runtime.Licences.MasterData
{
    [CreateAssetMenu(fileName = "LicenceMasterDataRecord", menuName = "Licence/CreateRecord")]
    public class Record : ScriptableObject
    {
        [SerializeField] private string title;
        [SerializeField] private TextAsset licenceText;

        public string Title => title;

        public TextAsset LicenceText => licenceText;
    }
}