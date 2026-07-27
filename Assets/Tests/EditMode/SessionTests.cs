using System;
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
        public void FreshSessionOffersAllSevenContractTypes()
        {
            var available = new Session(1).AvailableContracts();
            Assert.AreEqual(7, available.Count);
            CollectionAssert.AllItemsAreUnique(available);
            CollectionAssert.Contains(available, ContractType.Trump);
            for (var t = ContractType.NoTricks; t < ContractType.Trump; t++)
                CollectionAssert.Contains(available, t);
        }

        [Test]
        public void CallerRotatesClockwiseFromSouth()
        {
            var session = new Session(7);
            var expected = new[]
            {
                Seat.South, Seat.West, Seat.North, Seat.East,
                Seat.South, Seat.West, Seat.North, Seat.East
            };
            foreach (var seat in expected)
            {
                Assert.AreEqual(seat, session.Caller);
                RunDeal(session, AnyCall(session));
            }
            Assert.AreEqual(9, session.DealNumber);
        }

        [Test]
        public void DealLifecycleIsEnforced()
        {
            var session = new Session(3);
            Assert.Throws<InvalidOperationException>(() => session.FinishDeal());

            var deal = session.StartDeal(new ContractCall(ContractType.NoTricks));
            Assert.Throws<InvalidOperationException>(() => session.StartDeal(new ContractCall(ContractType.NoHearts)));
            Assert.Throws<InvalidOperationException>(() => session.FinishDeal()); // still being played

            PlayOut(deal);
            session.FinishDeal();
            Assert.AreEqual(2, session.DealNumber);
            Assert.AreEqual(Seat.West, session.Caller);
            Assert.AreEqual(1, session.PenaltyCallsLeft(ContractType.NoTricks));
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
            RunDeal(session, new ContractCall(ContractType.NoTricks));            // 1 South
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Clubs));   // 2 West
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Clubs));   // 3 North
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Clubs));   // 4 East
            RunDeal(session, new ContractCall(ContractType.NoHearts));            // 5 South
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Hearts));  // 6 West
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Hearts));  // 7 North
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Hearts));  // 8 East
            RunDeal(session, new ContractCall(ContractType.NoQueens));            // 9 South, third penalty
            RunDeal(session, new ContractCall(ContractType.NoTricks));            // 10 West
            RunDeal(session, new ContractCall(ContractType.NoHearts));            // 11 North
            RunDeal(session, new ContractCall(ContractType.NoQueens));            // 12 East

            // Deal 13: South's penalty slots are spent, only trump is on offer.
            Assert.AreEqual(Seat.South, session.Caller);
            Assert.AreEqual(2, session.TrumpCallsLeft(Seat.South));
            CollectionAssert.AreEqual(new[] { ContractType.Trump }, session.AvailableContracts());
            Assert.Throws<InvalidOperationException>(() => session.StartDeal(new ContractCall(ContractType.NoMen)));

            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Spades));  // 13 South
            RunDeal(session, new ContractCall(ContractType.NoMen));               // 14 West
            RunDeal(session, new ContractCall(ContractType.KingOfHearts));        // 15 North
            RunDeal(session, new ContractCall(ContractType.NoMen));               // 16 East

            // Deal 17, South's last call: still trump only.
            CollectionAssert.AreEqual(new[] { ContractType.Trump }, session.AvailableContracts());
        }

        [Test]
        public void CallerOutOfTrumpCallsCannotCallTrump()
        {
            var session = new Session(9);
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Spades));    // 1 South
            RunDeal(session, new ContractCall(ContractType.NoTricks));              // 2 West
            RunDeal(session, new ContractCall(ContractType.NoHearts));              // 3 North
            RunDeal(session, new ContractCall(ContractType.NoQueens));              // 4 East
            RunDeal(session, new ContractCall(ContractType.Trump, Suit.Diamonds));  // 5 South, second trump

            RunDeal(session, new ContractCall(ContractType.NoMen));                 // 6 West
            RunDeal(session, new ContractCall(ContractType.KingOfHearts));          // 7 North
            RunDeal(session, new ContractCall(ContractType.NoLastTwo));             // 8 East

            // Deal 9: South has used both trump calls; only penalties remain.
            Assert.AreEqual(Seat.South, session.Caller);
            Assert.AreEqual(0, session.TrumpCallsLeft(Seat.South));
            Assert.AreEqual(2, session.TrumpCallsLeft(Seat.West));
            var available = session.AvailableContracts();
            CollectionAssert.DoesNotContain(available, ContractType.Trump);
            Assert.IsTrue(available.Count > 0);
            Assert.Throws<InvalidOperationException>(() => session.StartDeal(new ContractCall(ContractType.Trump, Suit.Clubs)));
        }

        [Test]
        public void SheetRowsCarryDealCallerAndPoints()
        {
            var session = new Session(21);
            var deal = session.StartDeal(new ContractCall(ContractType.KingOfHearts));
            PlayOut(deal);
            var expected = deal.Score();
            session.FinishDeal();

            Assert.AreEqual(1, session.Sheet.Count);
            var row = session.Sheet[0];
            Assert.AreEqual(1, row.DealNumber);
            Assert.AreEqual(Seat.South, row.Caller);
            Assert.AreEqual(ContractType.KingOfHearts, row.Contract.Type);
            CollectionAssert.AreEqual(expected.Points.ToArray(), row.Points.ToArray());
            CollectionAssert.AreEqual(expected.Points.ToArray(), session.Totals.ToArray());
            Assert.AreEqual(-320, row.Points.Sum());
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
    }
}
