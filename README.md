<img align="left" width="140" src="logo.png" alt="greenbaize logo">

# greenbaize

King, the Turkish trick-taking game (Rıfkı, if you grew up with it), for one human and three machines. Built in Unity.

<br clear="left">

## The game

King is a compendium game: twenty deals, and each one runs under a different contract. Six are penalty contracts where the goal is to *not* take things — tricks, hearts, queens, kings and jacks, the king of hearts, the last two tricks. The other eight are trump deals where tricks finally score in your favor. The right to call the contract rotates, so you spend part of the evening attacking and the rest dodging whatever your opponents picked to hurt you with.

Scores across a full session always sum to zero. Your win is, exactly, everyone else's loss.

The full rule set as implemented lives in [docs/rules.md](docs/rules.md).

## Layout

```
Assets/
  Scripts/
    Core/    rules engine: deck, contracts, trick resolution, scoring. Plain C#, no UnityEngine.
    AI/      the three computer seats. Also plain C#.
    UI/      table, hand display, trick area, scoreboard, contract picker.
  Scenes/
  Tests/     edit-mode tests for the engine
dev/         thin dotnet harness so engine tests run without the editor
docs/        rules writeup
```

## Running it

Built with Unity 6000.0.80f1 (Unity 6 LTS). Open the repo folder in the editor, load `Assets/Scenes/Main.unity`, hit play. You're South; the other three seats play themselves.

The rules engine and AI don't touch UnityEngine at all, so their tests also run from the command line:

```
cd dev && dotnet test
```

Handy when you want to check a rules change without waiting for the editor.
