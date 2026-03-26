
[System.Serializable]
public class ActiveUpgrade
{
    public UpgradeData Data;
    public int CurrentLevel;

    public ActiveUpgrade(UpgradeData data)
    {
        Data = data;
        CurrentLevel = 0;
    }
}
