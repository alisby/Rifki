using System;
using King.Core;
using NUnit.Framework;

namespace King.Tests
{
    [TestFixture]
    public class ContractsTests
    {
        [TestCase(ContractType.NoTricks, -50, 13, -650)]
        [TestCase(ContractType.NoHearts, -30, 13, -390)]
        [TestCase(ContractType.NoQueens, -100, 4, -400)]
        [TestCase(ContractType.NoMen, -60, 8, -480)]
        [TestCase(ContractType.KingOfHearts, -320, 1, -320)]
        [TestCase(ContractType.NoLastTwo, -180, 2, -360)]
        [TestCase(ContractType.Trump, 50, 13, 650)]
        public void ValueTablesMatchTheRulesDoc(ContractType t, int unit, int units, int total)
        {
            Assert.AreEqual(unit, Contracts.UnitValue(t));
            Assert.AreEqual(units, Contracts.UnitsInDeal(t));
            Assert.AreEqual(total, Contracts.DealTotal(t));
        }

        [Test]
        public void SessionPoolBalancesToZero()
        {
            // Two of each penalty deal plus eight trump deals must cancel out.
            int penalties = 0;
            foreach (ContractType t in Enum.GetValues(typeof(ContractType)))
                if (t != ContractType.Trump)
                    penalties += 2 * Contracts.DealTotal(t);
            Assert.AreEqual(-5200, penalties);
            Assert.AreEqual(0, penalties + 8 * Contracts.DealTotal(ContractType.Trump));
        }

        [Test]
        public void TrumpCallRequiresASuitAndPenaltiesForbidOne()
        {
            Assert.Throws<ArgumentException>(() => new ContractCall(ContractType.Trump));
            Assert.Throws<ArgumentException>(() => new ContractCall(ContractType.NoHearts, Suit.Spades));

            var trump = new ContractCall(ContractType.Trump, Suit.Clubs);
            Assert.AreEqual(Suit.Clubs, trump.TrumpSuit);

            var penalty = new ContractCall(ContractType.NoQueens);
            Assert.IsNull(penalty.TrumpSuit);
        }
    }
}
