using System;
using System.Linq;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    // The heavy artillery: whole sessions played by a uniformly random legal
    // agent, across many seeds. Any rules, quota, or lifecycle bug big enough to
    // matter tends to show up here as a broken invariant or a thrown exception.
    [TestFixture]
    public class RandomSessionTests
    {
        static Session PlayFullSession(int seed)
        {
            var session = new Session(seed);
            var agent = new RandomLegalAgent(seed * 7919 + 1);
            while (!session.IsComplete)
            {
                var available = session.AvailableContracts();
                Assert.IsTrue(available.Count > 0, "no contract available on deal " + session.DealNumber + ", seed " + seed);
                var call = agent.ChooseContract(session, Array.Empty<Card>(), available);
                var deal = session.StartDeal(call);
                while (!deal.IsComplete)
                    deal.Play(agent.ChooseCard(deal, deal.ToPlay));
                session.FinishDeal();
            }
            return session;
        }

        [Test]
        public void RandomLegalSessionsAlwaysBalance()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var session = PlayFullSession(seed);
                var sheet = session.Sheet;
                var label = "seed " + seed;

                Assert.AreEqual(20, sheet.Count, label);
                Assert.AreEqual(0, session.Totals.Sum(), label);

                // Every row balances to its contract's deal total, and the running
                // totals really are the column sums.
                foreach (var row in sheet)
                    Assert.AreEqual(Contracts.DealTotal(row.Contract.Type), row.Points.Sum(), label + " deal " + row.DealNumber);
                for (int s = 0; s < 4; s++)
                    Assert.AreEqual(sheet.Sum(r => r.Points[s]), session.Totals[s], label);

                // 12 penalty deals and 8 trump deals, split exactly as the quotas demand.
                Assert.AreEqual(8, sheet.Count(r => r.Contract.Type == ContractType.Trump), label);
                for (var t = ContractType.NoTricks; t < ContractType.Trump; t++)
                    Assert.AreEqual(2, sheet.Count(r => r.Contract.Type == t), label + " " + t);

                for (int s = 0; s < 4; s++)
                {
                    var seat = (Seat)s;
                    Assert.AreEqual(5, sheet.Count(r => r.Caller == seat), label);
                    Assert.AreEqual(2, sheet.Count(r => r.Caller == seat && r.Contract.Type == ContractType.Trump), label);
                    Assert.AreEqual(0, session.TrumpCallsLeft(seat), label);
                }
            }
        }
    }
}
