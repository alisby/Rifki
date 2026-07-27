namespace King.Core
{
    // Play order is clockwise: South, West, North, East. The next seat is (seat + 1) % 4.
    public enum Seat
    {
        South = 0,
        West = 1,
        North = 2,
        East = 3
    }
}
