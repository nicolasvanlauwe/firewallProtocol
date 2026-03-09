/// <summary>
/// Représente une entrée du classement en ligne.
/// </summary>
public class LeaderboardEntry
{
    public string pseudo;
    public int bestScore;
    public int highestDay;
    public long timestamp;

    public LeaderboardEntry() { }

    public LeaderboardEntry(string pseudo, int bestScore, int highestDay, long timestamp)
    {
        this.pseudo = pseudo;
        this.bestScore = bestScore;
        this.highestDay = highestDay;
        this.timestamp = timestamp;
    }
}
