using System;

namespace King.Core
{
    public readonly struct ContractCall
    {
        public ContractType Type { get; }

        // Non-null exactly when Type == Trump.
        public Suit? TrumpSuit { get; }

        public ContractCall(ContractType type, Suit? trumpSuit = null)
        {
            if (type == ContractType.Trump && trumpSuit == null)
                throw new ArgumentException("a trump call must name the trump suit", nameof(trumpSuit));
            if (type != ContractType.Trump && trumpSuit != null)
                throw new ArgumentException("only a trump call names a trump suit", nameof(trumpSuit));

            Type = type;
            TrumpSuit = trumpSuit;
        }
    }
}
