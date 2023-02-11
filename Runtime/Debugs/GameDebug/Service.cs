namespace MokomoGamesLib.Runtime.Debugs.GameDebug
{
    public static class Service
    {
        public static GameDebugSaveData LoadIfNotExistCreate()
        {
            var saveData = GameDebugRepository.Load();
            return saveData != null ? saveData : GameDebugRepository.CreateSaveData();
        }
    }
}