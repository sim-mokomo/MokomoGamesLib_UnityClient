using System.IO;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localizations.MasterData
{
    [CreateAssetMenu(menuName = "Localize/Config", fileName = "LocalizeConfig")]
    public class Record : ScriptableObject
    {
        [Header("翻訳先言語")] [SerializeField] private AppLanguage language;

        [Header("翻訳後文字列テーブル名")] [SerializeField]
        private string tableName;

        [Header("フォント名")] [SerializeField] private string fontName;

        public AppLanguage Language => language;
        public string TableName => tableName;

        public string GetFontPath(bool isBitMap)
        {
            return isBitMap ? Path.Combine("BitMap", fontName) : Path.Combine("Dynamic", fontName);
        }
    }
}