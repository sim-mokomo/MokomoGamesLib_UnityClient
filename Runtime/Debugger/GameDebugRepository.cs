using UnityEditor;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Debugger
{
    public static class GameDebugRepository
    {
        private const string SaveDataFileName = "Assets/MokomoGames/Debugger/GameDebugSaveData.asset";

        public static void Save(GameDebugSaveData saveData)
        {
            var saveDataAlreadyExists = Load() != null;
            if (!saveDataAlreadyExists) AssetDatabase.CreateAsset(saveData, SaveDataFileName);

            EditorUtility.SetDirty(saveData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!saveDataAlreadyExists)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = saveData;
            }
        }

        public static GameDebugSaveData LoadIfNotExistCreate()
        {
            var saveData = Load();
            if (saveData != null) return saveData;

            Save(ScriptableObject.CreateInstance<GameDebugSaveData>());
            return Load();
        }

        public static GameDebugSaveData Load()
        {
            return AssetDatabase.LoadAssetAtPath<GameDebugSaveData>(SaveDataFileName);
        }
    }
}