
namespace HandHistoryParser.model
{
    internal class Card(char value, char suit)
    {
        public char Value { get; set; } = value;
        public char Suit { get; set; } = suit;
   
        public override bool Equals(object? obj)
        {
            return obj is Card card &&
                   Value == card.Value &&
                   Suit == card.Suit;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Suit);
        }

        public override string? ToString()
        {
            return Value.ToString() + Suit.ToString();
        }
    }
}