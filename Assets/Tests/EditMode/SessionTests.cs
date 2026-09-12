using System;
using System.Collections.Generic;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class SessionTests
    {
        static void PlayOut(DealEngine deal)
        {
            while (!deal.IsComplete)
                deal.Play(deal.LegalPlays()[0]);
        }

        static void RunDeal(Session session, ContractCall call)
        {
            PlayOut(session.StartDeal(call));
            session.FinishDeal();
        }

        // First available type; good enough to drive a session when the test
        // doesn't care which contract runs.
        static ContractCall AnyCall(Session session)
        {
            var type = session.AvailableContracts()[0];
            return type == ContractType.Trump
                ? new ContractCall(type, Suit.Clubs)
                : new ContractCall(type);
        }

        [Test]
        public void FreshSessionOffersSixPenaltyContractsAndNoTrump()
        {
            var available =
                new Session(1).AvailableContracts();

            Assert.AreEqual(6, available.Count);
            CollectionAssert.AllItemsAreUnique(available);
            CollectionAssert.DoesNotContain(
                available,
                ContractType.Trump);

            for (var t = ContractType.NoTricks;
                 t < ContractType.Trump;
                 t++)
                CollectionAssert.Contains(available, t);
        }

        [Test]
        public void CallerStartsWithDiamondTwoAndRotatesToTheRight()
        {
            var session = new Session(7);

            // The first deal fixes the caller as the holder of ♦2.
            session.DealHands();
            var expected = session.Caller;

            for (int deal = 0; deal < 8; deal++)
            {
                Assert.AreEqual(expected, session.Caller);
                RunDeal(session, AnyCall(session));

                expected =
                    expected == Seat.South ? Seat.East :
                    expected == Seat.East ? Seat.North :
                    expected == Seat.North ? Seat.West :
                    Seat.South;
            }

            Assert.AreEqual(9, session.DealNumber);
        }

        [Test]
        public void DealLifecycleIsEnforced()
        {
            var session = new Session(3);

            Assert.Throws<InvalidOperationException>(
                () => session.FinishDeal());

            session.DealHands();
            var firstCaller = session.Caller;

            var deal = session.StartDeal(
                new ContractCall(
                    ContractType.NoTricks));

            Assert.Throws<InvalidOperationException>(
                () => session.StartDeal(
                    new ContractCall(
                        ContractType.NoHearts)));

            Assert.Throws<InvalidOperationException>(
                () => session.FinishDeal());

            PlayOut(deal);
            session.FinishDeal();

            Assert.AreEqual(2, session.DealNumber);

            var nextCaller =
                firstCaller == Seat.South ? Seat.East :
                firstCaller == Seat.East ? Seat.North :
                firstCaller == Seat.North ? Seat.West :
                Seat.South;

            Assert.AreEqual(nextCaller, session.Caller);
        }

        [Test]
        public void SameSeedDealsTheSameHands()
        {
            var a = new Session(42);
            var b = new Session(42);
            var da = a.StartDeal(new ContractCall(ContractType.NoTricks));
            var db = b.StartDeal(new ContractCall(ContractType.NoTricks));
            for (int s = 0; s < 4; s++)
                CollectionAssert.AreEqual(da.HandOf((Seat)s).ToArray(), db.HandOf((Seat)s).ToArray());

            var c = new Session(43);
            var dc = c.StartDeal(new ContractCall(ContractType.NoTricks));
            bool identical = Enumerable.Range(0, 4)
                .All(s => da.HandOf((Seat)s).SequenceEqual(dc.HandOf((Seat)s)));
            Assert.IsFalse(identical);
        }

        [Test]
        public void EachPenaltyContractIsCallableTwicePerSession()
        {
            var session = new Session(2);
            Assert.AreEqual(2, session.PenaltyCallsLeft(ContractType.NoQueens));
            RunDeal(session, new ContractCall(ContractType.NoQueens)); // South
            Assert.AreEqual(1, session.PenaltyCallsLeft(ContractType.NoQueens));
            RunDeal(session, new ContractCall(ContractType.NoQueens)); // West
            Assert.AreEqual(0, session.PenaltyCallsLeft(ContractType.NoQueens));

            // North can no longer pick it.
            CollectionAssert.DoesNotContain(session.AvailableContracts(), ContractType.NoQueens);
            Assert.Throws<InvalidOperationException>(() => session.StartDeal(new ContractCall(ContractType.NoQueens)));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.PenaltyCallsLeft(ContractType.Trump));
        }

        [Test]
        public void CallerOutOfPenaltySlotsIsForcedToTrump()
        {
            var session = new Session(5);
            session.DealHands();
            var firstCaller = session.Caller;

            // Deals 1-4 must be penalties.
            RunDeal(session, new ContractCall(ContractType.NoTricks));
            RunDeal(session, new ContractCall(ContractType.NoHearts));
            RunDeal(session, new ContractCall(ContractType.NoQueens));
            RunDeal(session, new ContractCall(ContractType.NoMen));

            // First caller's second turn: second penalty.
            RunDeal(session, new ContractCall(ContractType.NoHearts));

            // The other three callers may now use trump.
            RunDeal(
                session,
                new ContractCall(
                    ContractType.Trump,
                    Suit.Clubs));
            RunDeal(
                session,
                new ContractCall(
                    ContractType.Trump,
                    Suit.Clubs));
            RunDeal(
                session,
                new ContractCall(
                    ContractType.Trump,
                    Suit.Clubs));

            // First caller's third penalty slot.
            RunDeal(session, new ContractCall(ContractType.NoQueens));

            RunDeal(session, new ContractCall(ContractType.NoLastTwo));
            RunDeal(session, new ContractCall(ContractType.KingOfHearts));
            RunDeal(session, new ContractCall(ContractType.NoTricks));

            // Deal 13 returns to the first caller. Three penalty slots are
            // exhausted and both trump calls are still available.
            Assert.AreEqual(firstCaller, session.Caller);
            Assert.AreEqual(
                0,
                session.PenaltySlotsLeft(firstCaller));

            CollectionAssert.AreEqual(
                new[] { ContractType.Trump },
                session.AvailableContracts());

            Assert.Throws<InvalidOperationException>(
                () => session.StartDeal(
                    new ContractCall(
                        ContractType.NoMen)));
        }

        [Test]
        public void CallerOutOfTrumpCallsCannotCallTrump()
        {
            var session = new Session(9);
            session.DealHands();
            var firstCaller = session.Caller;

            // Deals 1-4 are penalty-only.
            RunDeal(session, new ContractCall(ContractType.NoTricks));
            RunDeal(session, new ContractCall(ContractType.NoHearts));
            RunDeal(session, new ContractCall(ContractType.NoQueens));
            RunDeal(session, new ContractCall(ContractType.NoMen));

            // First caller spends both trump calls on deals 5 and 9.
            RunDeal(
                session,
                new ContractCall(
                    ContractType.Trump,
                    Suit.Spades));

            RunDeal(session, new ContractCall(ContractType.KingOfHearts));
            RunDeal(session, new ContractCall(ContractType.NoLastTwo));
            RunDeal(session, new ContractCall(ContractType.NoTricks));

            RunDeal(
                session,
                new ContractCall(
                    ContractType.Trump,
                    Suit.Diamonds));

            RunDeal(session, new ContractCall(ContractType.NoHearts));
            RunDeal(session, new ContractCall(ContractType.NoQueens));
            RunDeal(session, new ContractCall(ContractType.NoMen));

            // Deal 13: same caller still has penalty slots, but no trump calls.
            Assert.AreEqual(firstCaller, session.Caller);
            Assert.AreEqual(
                0,
                session.TrumpCallsLeft(firstCaller));

            var available = session.AvailableContracts();

            CollectionAssert.DoesNotContain(
                available,
                ContractType.Trump);

            Assert.IsTrue(available.Count > 0);

            Assert.Throws<InvalidOperationException>(
                () => session.StartDeal(
                    new ContractCall(
                        ContractType.Trump,
                        Suit.Clubs)));
        }

        [Test]
        public void SheetRowsCarryDealCallerAndPoints()
        {
            var session = new Session(21);
            session.DealHands();
            var caller = session.Caller;

            var deal = session.StartDeal(
                new ContractCall(
                    ContractType.KingOfHearts));

            PlayOut(deal);
            var expected = deal.Score();
            session.FinishDeal();

            Assert.AreEqual(1, session.Sheet.Count);

            var row = session.Sheet[0];

            Assert.AreEqual(1, row.DealNumber);
            Assert.AreEqual(caller, row.Caller);
            Assert.AreEqual(
                ContractType.KingOfHearts,
                row.Contract.Type);

            CollectionAssert.AreEqual(
                expected.Points,
                row.Points);
        }

        [Test]
        public void TwentyDealsCompleteTheSessionAndBalanceTheBook()
        {
            var session = new Session(11);
            while (!session.IsComplete)
                RunDeal(session, AnyCall(session));

            Assert.AreEqual(20, session.Sheet.Count);
            Assert.AreEqual(0, session.Totals.Sum());
            Assert.AreEqual(0, session.AvailableContracts().Count);
            Assert.Throws<InvalidOperationException>(() => session.StartDeal(new ContractCall(ContractType.Trump, Suit.Clubs)));

            for (int i = 0; i < 20; i++)
                Assert.AreEqual(i + 1, session.Sheet[i].DealNumber);
            for (int s = 0; s < 4; s++)
                Assert.AreEqual(session.Sheet.Sum(r => r.Points[s]), session.Totals[s]);
        }

        [Test]
        public void TheScoreSheetCannotBeEditedThroughItsOwnAccessors()
        {
            var session = new Session(5);
            RunDeal(session, new ContractCall(ContractType.NoQueens));
            var row = session.Sheet[0];

            Assert.Throws<NotSupportedException>(() => ((IList<int>)row.Points)[0] = 999);
            Assert.Throws<NotSupportedException>(() => ((IList<int>)session.Totals)[0] = 999);
            Assert.AreEqual(-400, row.Points.Sum());
        }

        [Test]
        public void HandsAreDealtBeforeAnyoneHasToNameAContract()
        {
            var session = new Session(7);
            var hands = session.DealHands();

            Assert.AreEqual(4, hands.Length);
            foreach (var hand in hands)
                Assert.AreEqual(13, hand.Count);
            CollectionAssert.AllItemsAreUnique(hands.SelectMany(h => h).ToArray());
        }

        [Test]
        public void LookingAtTheHandTwiceShowsTheSameCards()
        {
            var session = new Session(7);
            var first = session.DealHands();
            var second = session.DealHands();

            for (int s = 0; s < 4; s++)
                CollectionAssert.AreEqual(first[s].ToArray(), second[s].ToArray());

            // And the deal that follows is played with those very cards.
            var deal = session.StartDeal(new ContractCall(ContractType.NoTricks));
            for (int s = 0; s < 4; s++)
                CollectionAssert.AreEqual(first[s].ToArray(), deal.HandOf((Seat)s).ToArray());
        }

        [Test]
        public void PeekingAtTheHandDoesNotChangeWhatGetsDealt()
        {
            var quiet = new Session(23);
            var peeked = new Session(23);
            peeked.DealHands();

            var a = quiet.StartDeal(new ContractCall(ContractType.NoTricks));
            var b = peeked.StartDeal(new ContractCall(ContractType.NoTricks));
            for (int s = 0; s < 4; s++)
                CollectionAssert.AreEqual(a.HandOf((Seat)s).ToArray(), b.HandOf((Seat)s).ToArray());
        }

        [Test]
        public void ARejectedCallLeavesTheHandUntouched()
        {
            var session = new Session(3);
            RunDeal(session, new ContractCall(ContractType.NoTricks));
            RunDeal(session, new ContractCall(ContractType.NoTricks));

            // Both no-tricks calls are spent, so North asking for a third is refused.
            var hands = session.DealHands();
            Assert.Throws<InvalidOperationException>(
                () => session.StartDeal(new ContractCall(ContractType.NoTricks)));

            var again = session.DealHands();
            for (int s = 0; s < 4; s++)
                CollectionAssert.AreEqual(hands[s].ToArray(), again[s].ToArray());
        }
    }
}
