<img align="left" width="140" src="logo.png" alt="greenbaize logo">

# greenbaize

King, the Turkish trick-taking game (Rıfkı, if you grew up with it), for one human and three machines. Built in Unity.

<br clear="left">

## The game

King is a compendium game: twenty deals, and the player whose turn it is to call decides what each one is played for. There are six penalty contracts, where the goal is to *not* take things — tricks, hearts, queens, kings and jacks, the king of hearts, the last two tricks. Each gets called twice over the night, which accounts for twelve deals. The other eight are trump deals, two per player, where tricks finally score in your favour. So you spend part of the evening attacking and the rest dodging whatever your opponents picked to hurt you with.

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

## CI and builds

Every push and PR runs the engine tests on a plain ubuntu runner with dotnet 8. That job needs zero setup and is the merge gate: if the rules engine is broken, nothing lands.

The rest of the pipeline builds the actual game — WebGL, macOS, Windows and Android, each uploaded as a downloadable artifact — and pushes the WebGL build to GitHub Pages, so the latest main is always playable at https://naltinbas.github.io/greenbaize/. Those jobs need a Unity license, so they check for the license secrets first and quietly skip when they're missing. Nothing fails, the tests still run, you just don't get builds.

To light the builds up:

1. Get a Unity license file. Easiest route is game-ci's activation flow: run their [unity-request-activation-file workflow](https://game.ci/docs/github/activation) (or `game-ci/unity-request-activation-file` locally), upload the `.alf` at license.unity3d.com, and you get a `.ulf` back. Personal licenses work fine.
2. In the repo settings, add three Actions secrets: `UNITY_LICENSE` (the full contents of the `.ulf` file), `UNITY_EMAIL` and `UNITY_PASSWORD` (the Unity account it belongs to).
3. Make sure Pages is set to deploy from GitHub Actions (Settings → Pages → Source → GitHub Actions). This repo already has that flipped via the API, so normally there's nothing to do here.

Next push to main after that runs editmode tests inside the editor, builds all four platforms, and the Pages link goes live.
