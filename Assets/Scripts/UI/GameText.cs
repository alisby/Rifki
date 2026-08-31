using King.Core;

namespace King.UI
{
    // Display names for seats and contracts.
    public static class GameText
    {
        public const int MaxPlayerNameLength = 10;

        static readonly string[] seatLabels =
        {
            "Güney",
            "Batı",
            "Kuzey",
            "Doğu"
        };

        static string NormalizeName(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            value = value.Trim();

            return value.Length > MaxPlayerNameLength
                ? value.Substring(0, MaxPlayerNameLength)
                : value;
        }

        public static void SetSeatNames(string south, string west, string north, string east)
        {
            seatLabels[(int)Seat.South] = NormalizeName(south, "Güney");
            seatLabels[(int)Seat.West] = NormalizeName(west, "Batı");
            seatLabels[(int)Seat.North] = NormalizeName(north, "Kuzey");
            seatLabels[(int)Seat.East] = NormalizeName(east, "Doğu");
        }

        public static string SeatLabel(Seat seat)
        {
            switch (seat)
            {
                case Seat.South: return seatLabels[(int)Seat.South];
                case Seat.West: return seatLabels[(int)Seat.West];
                case Seat.North: return seatLabels[(int)Seat.North];
                default: return seatLabels[(int)Seat.East];
            }
        }

        public static string ContractLabel(ContractType type)
        {
            switch (type)
            {
                case ContractType.NoTricks: return "El Almaz";
                case ContractType.NoHearts: return "Kupa Almaz";
                case ContractType.NoQueens: return "Kız Almaz";
                case ContractType.NoMen: return "Erkek Almaz";
                case ContractType.KingOfHearts: return "Rıfkı";
                case ContractType.NoLastTwo: return "Son İki";
                default: return "Koz";
            }
        }

        public static string ContractLabel(ContractCall call) =>
            call.Type == ContractType.Trump
                ? "Koz " + CardStyle.SuitGlyph(call.TrumpSuit.Value)
                : ContractLabel(call.Type);
    }
}
