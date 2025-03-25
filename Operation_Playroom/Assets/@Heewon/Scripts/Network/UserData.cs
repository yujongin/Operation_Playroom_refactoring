using System;

public enum GameMode
{
    Default
}

public enum GameQueue
{
    Solo,
    Team
}

public enum GameTeam
{
    Blue,
    Red
}

public enum GameRole
{
    Archer,
    Swordman,
    King,
    None
}

public enum Map
{
    Default
}

[Serializable]
public class UserData 
{
    public string userName;
    public string userAuthId;
    public GameInfo userGamePreferences = new GameInfo();
}

[Serializable]
public class GameInfo
{
    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;
    public GameTeam gameTeam;
    public GameRole gameRole;
    public string ToMultiplayQueue()
    {
        if (gameQueue == GameQueue.Team)
        {
            return "team-queue";
        }

        return "solo-queue";
    }
}