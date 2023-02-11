using UnityEngine;

namespace MokomoGamesLib.Runtime.Store.Restore
{
    public class Process
    {
        private readonly RestorePresenter _restorePresenter;
        private readonly StoreManager _storeManger;

        public Process(RestorePresenter restorePresenter)
        {
            _restorePresenter = restorePresenter;
            _restorePresenter.Show(false);
            _restorePresenter.OnClickedRestoreButton += Restore;

            _storeManger = Object.FindObjectOfType<StoreManager>();
            _storeManger.OnRestored += OnRestored;
        }

        private void Restore()
        {
            _storeManger.Restore();
        }

        private static void OnRestored()
        {
            PlayFab.StoreRepository.LoadInventoryItems();
        }

        ~Process()
        {
            _storeManger.OnRestored -= OnRestored;
            _restorePresenter.OnClickedRestoreButton -= Restore;
        }
    }
}

