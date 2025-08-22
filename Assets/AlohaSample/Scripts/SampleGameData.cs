using Aloha.Coconut;

public class SampleGameData
{
    public int LocalCounter
    {
        get => _localSaveData.counter;
        private set => _localSaveData.counter = value;
    }
    
    public int RemoteCounter
    {
        get => _remoteSaveData.counter;
        private set => _remoteSaveData.counter = value;
    }
    
    public int RemoteCounter2
    {
        get => _remoteSaveData2.counter;
        private set => _remoteSaveData2.counter = value;
    }
    
    private LocalSaveData _localSaveData;
    private RemoteSaveData _remoteSaveData;
    private RemoteSaveData _remoteSaveData2;

    public SampleGameData(SaveDataManager saveDataManager)
    {
        _localSaveData = saveDataManager.Get<LocalSaveData>("sample_local");
        _remoteSaveData = saveDataManager.Get<RemoteSaveData>("sample_remote");
        _remoteSaveData2 = saveDataManager.Get<RemoteSaveData>("sample_remote2");
    }
    
    public void AddLocalCounter(int value)
    {
        LocalCounter += value;
    }
    
    public void AddRemoteCounter(int value)
    {
        RemoteCounter += value;
    }
    
    public void AddRemoteCounter2(int value)
    {
        RemoteCounter2 += value;
    }
    
    [LocalSave]
    public class LocalSaveData
    {
        public int counter;
    }

    public class RemoteSaveData
    {
        public int counter;
    }
}
