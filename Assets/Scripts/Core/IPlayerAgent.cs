using System.Collections.Generic;

namespace King.Core
{
    public interface IPlayerAgent
    {
        // Called when this player is the caller. available is never empty; the
        // returned call must use one of its types, with a suit attached iff Trump.
        ContractCall ChooseContract(Session session, IReadOnlyList<Card> hand, IReadOnlyList<ContractType> available);

        // Must return a member of deal.LegalPlays().
        Card ChooseCard(DealEngine deal, Seat seat);
    }
}
