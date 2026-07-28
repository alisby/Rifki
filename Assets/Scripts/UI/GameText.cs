using King.Core;

namespace King.UI
{
    // Display names for seats and contracts.
    public static class GameText
    {
        public static string SeatLabel(Seat seat)
        {
            switch (seat)
            {
                case Seat.South: return "South";
                case Seat.West: return "West";
                case Seat.North: return "North";
                default: return "East";
            }
        }

        public static string ContractLabel(ContractType type)
        {
            switch (type)
            {
                case ContractType.NoTricks: return "No tricks";
                case ContractType.NoHearts: return "No hearts";
                case ContractType.NoQueens: return "No queens";
                case ContractType.NoMen: return "No men";
                case ContractType.KingOfHearts: return "King of hearts";
                case ContractType.NoLastTwo: return "No last two";
                default: return "Trump";
            }
        }

        public static string ContractLabel(ContractCall call) =>
            call.Type == ContractType.Trump
                ? "Trump " + CardStyle.SuitGlyph(call.TrumpSuit.Value)
                : ContractLabel(call.Type);
    }
}
