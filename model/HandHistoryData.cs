
namespace HandHistoryParser.model
{
    internal class HandHistoryData
    {
        public long HandHistoryNumber { get; set; }
        public List<Player> Players { get; set; }

        public HandHistoryData()
        {
            Players = [];
        }

        public HandHistoryData(long handHistoryNumber, List<Player> players)
        {
            this.HandHistoryNumber = handHistoryNumber;
            this.Players = players;
        }

        public HandHistoryData WithNewHandHistoryNumber(long handHistoryNumber)
        {
            return new HandHistoryData(handHistoryNumber, [.. Players]);
        }

        internal HandHistoryData WithNewPlayers(List<Player> players)
        {
            return new HandHistoryData(HandHistoryNumber, players);
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string? ToString()
        {
            return $"HandHistory#{HandHistoryNumber}\r\nPlayers: [\n  {string.Join("\n  ", Players)}\n]";
        }
    }
}