using System;
using System.Collections.Generic;
using King.Core;

namespace King.Tests
{
    // Picks uniformly among whatever is legal at each decision. The session tests
    // run it over hundreds of seeded games to shake out rule and quota bugs.
    public sealed class RandomLegalAgent : IPlayerAgent
    {
        readonly Random rng;

        public RandomLegalAgent(int seed)
        {
            rng = new Random(seed);
        }

        public ContractCall ChooseContract(Session session, IReadOnlyList<Card> hand, IReadOnlyList<ContractType> available)
        {
            var type = available[rng.Next(available.Count)];
            return type == ContractType.Trump
                ? new ContractCall(type, (Suit)rng.Next(4))
                : new ContractCall(type);
        }

        public Card ChooseCard(DealEngine deal, Seat seat)
        {
            var legal = deal.LegalPlays();
            return legal[rng.Next(legal.Count)];
        }
    }
}
