using System;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Aloha.Coconut.Launcher
{
    public class SaveDataManagerLaunchingProcess : ILaunchingProcess
    {
        public int Order => -1;
        public string Message => "Initializing Save Data Manager";
        public bool IsBlocker => true;
        
        private readonly SaveDataManager _saveDataManager;
        private readonly ISaveDataSaver _saveDataSaver;

        public SaveDataManagerLaunchingProcess(SaveDataManager saveDataManager, [InjectOptional] ISaveDataSaver saveDataSaver)
        {
            _saveDataManager = saveDataManager;
            _saveDataSaver = saveDataSaver;
        }
        
        public async UniTask Run(ITitleScreen titleScreen)
        {
            titleScreen.Report(0);
            if(_saveDataSaver != null)
            {
                await _saveDataManager.LinkAsync(_saveDataSaver);
            }
            else
            {
                _saveDataManager.LinkFileDataSaver();   
            }
            titleScreen.Report(1);
        }
    }
}
