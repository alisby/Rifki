using System;

namespace King.Core
{
    public static class Contracts
    {
        // Points per scoring unit: a trick taken, a heart captured, a queen, a king or jack,
        // the king of hearts, one of the last two tricks, or a trick in a trump deal.
        public static int UnitValue(ContractType t)
        {
            switch (t)
            {
                case ContractType.NoTricks: return -50;
                case ContractType.NoHearts: return -30;
                case ContractType.NoQueens: return -100;
                case ContractType.NoMen: return -60;
                case ContractType.KingOfHearts: return -320;
                case ContractType.NoLastTwo: return -180;
                case ContractType.Trump: return 50;
                default: throw new ArgumentOutOfRangeException(nameof(t));
            }
        }

        // How many of those units exist in one deal.
        public static int UnitsInDeal(ContractType t)
        {
            switch (t)
            {
                case ContractType.NoTricks: return 13;
                case ContractType.NoHearts: return 13;
                case ContractType.NoQueens: return 4;
                case ContractType.NoMen: return 8;
                case ContractType.KingOfHearts: return 1;
                case ContractType.NoLastTwo: return 2;
                case ContractType.Trump: return 13;
                default: throw new ArgumentOutOfRangeException(nameof(t));
            }
        }

        public static int DealTotal(ContractType t) => UnitValue(t) * UnitsInDeal(t);
    }
}
