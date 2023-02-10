using UnityEditor;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Debugs.GameDebug
{
    public static class GameDebugRepository
    {
        private const string SaveDataFileName = "Assets/MokomoGames/Debugger/GameDebugSaveData.asset";

        // TODO: 実機でも動作チェックをしたいタイプのデバッグなのでEditorに依存しない形で実装を行う。
        public static void Save(GameDebugSaveData saveData)
        {
            EditorUtility.SetDirty(saveData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static GameDebugSaveData CreateSaveData()
        {
            var saveData = ScriptableObject.CreateInstance<GameDebugSaveData>();
            AssetDatabase.CreateAsset(saveData, SaveDataFileName);
            return saveData;
        }

        public static GameDebugSaveData Load()
        {
            return AssetDatabase.LoadAssetAtPath<GameDebugSaveData>(SaveDataFileName);
        }
    }
}