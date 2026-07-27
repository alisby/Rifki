# King: how the game works

This is the rule set the engine implements. If code and this document disagree, the code is wrong.

Four players, one standard 52-card deck, 13 cards each. Everyone plays for themselves, no partnerships. Play goes clockwise. Seats are named South, West, North, East; the human sits South.

## The session

A full game is 20 deals. The right to call the contract rotates clockwise every deal, so each player calls five times. The caller picks the contract for that deal, within these limits:

- Each of the six penalty contracts can be called at most twice over the whole session. That's 12 penalty deals.
- Each player must call trump exactly twice among their five calls. That's the other 8 deals.
- So every player ends up calling three penalties and two trumps. When the remaining quota forces the choice (trump calls used up, or nothing but trump slots left), the caller has no say.

The caller leads the first trick. After that, whoever wins a trick leads the next one.

## Contracts

The six penalty deals have no trump suit. Whatever you capture under them costs you:

| contract          | what hurts                    | each | full deal |
|-------------------|-------------------------------|------|-----------|
| No tricks         | every trick you take          | -50  | -650      |
| No hearts         | every heart you capture       | -30  | -390      |
| No queens         | each queen                    | -100 | -400      |
| No men            | each king or jack             | -60  | -480      |
| King of hearts    | capturing the K of hearts     | -320 | -320      |
| No last two       | each of the two final tricks  | -180 | -360      |

In a trump deal the caller also names the trump suit, and every trick is worth +50, so +650 for the deal.

Each penalty called twice puts -5200 into the pool, and eight trump deals put +5200 back. A finished session always sums to zero across the four players. The tests lean on this hard.

## Play

Follow suit if you can. That never goes away.

Trump deals: if you're void in the led suit you must play a trump if you hold one (no obligation to beat trumps already on the table). Highest trump wins the trick; if nobody trumped, highest card of the led suit wins. Leading trumps is always allowed.

Penalty deals have no trump, so the highest card of the led suit takes the trick, always.

Hearts restrictions, in the No hearts and King of hearts deals only:

- You may not lead a heart until a heart has been discarded on an earlier trick ("broken"), unless hearts are all you have left.

Forced dumping, in the four card-penalty deals (No hearts, No queens, No men, King of hearts):

- Void in the led suit? You must discard a penalty card of the current deal if you hold one. A heart, a queen, a king or jack, or the K of hearts, respectively.
- Following suit while the trick already contains a higher card of that suit than a penalty card you hold in it? You must play that penalty card. Classic example: hearts led, the ace is already down, you hold the king of hearts in the King of hearts deal. It goes.

Neither rule applies in No tricks or No last two. There you just follow suit.

A deal ends early once nothing left in it can score: the K of hearts has been captured, all four queens are gone, all eight men are gone, or every heart has been taken. No tricks and No last two always play out all 13 tricks.

## Scoring

Every deal writes one row on a 20-row scoresheet, per player, plus a running total. When deal 20 finishes the session is over and the highest total wins. The four totals must sum to zero; if they don't, there's a bug.
