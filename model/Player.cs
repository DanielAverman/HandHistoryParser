
using System.Globalization;

namespace HandHistoryParser.model
{
    internal class Player(int seatNumber, string nickname, decimal stack, char currency, List<Card> cards)
    {
        public int SeatNumber { get; set; } = seatNumber;
        public string Nickname { get; set; } = nickname;
        public decimal Stack { get; set; } = stack;
        public char Currency { get; set; } = currency;
        public List<Card> Cards { get; set; } = cards;

        public Player updateCards(List<Card> cards)
        {
            return new Player(SeatNumber, Nickname, Stack, Currency, [.. cards]);
        }

        public override bool Equals(object? obj)
        {
            return obj is Player player && Nickname == player.Nickname;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SeatNumber, Nickname, Stack);
        }

        public override string? ToString()
        {
            return $"{Nickname}: seat#{SeatNumber} stack={Currency}{Stack.ToString("#,0.00", CultureInfo.InvariantCulture)} [{string.Join(" ", Cards)}]";
        }
    }
}