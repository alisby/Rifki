using King.Core;
using UnityEngine;

namespace King.UI
{
    // Opponent card-back decorations were removed deliberately.
    // Player name and quota information now occupy this space.
    public sealed class OpponentsView
    {
        public OpponentsView(Transform canvas)
        {
        }

        // Kept because GameBootstrap calls Refresh.
        public void Refresh(DealEngine deal)
        {
        }
    }
}
