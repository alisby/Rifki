using System;
using System.Collections;
using King.AI;
using King.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace King.UI
{
    // The whole front end hangs off this one script. The scene holds nothing but
    // an empty object with this component; Awake builds the table out of code and
    // Start drives a full 20-deal session through a single coroutine.
    public sealed class GameBootstrap : MonoBehaviour
    {
        const float BotDelay = 0.5f;
        const float TrickLinger = 1f;

        // West, North and East. The South entry stays null; that seat plays by clicks.
        readonly IPlayerAgent[] bots = new IPlayerAgent[4];

        Session session;
        DealEngine deal;

        HandView handView;
        TrickView trickView;
        OpponentsView opponentsView;
        StatusLine statusLine;
        GameProgressPanel gameProgress;
        PlayerQuotaView playerQuota;
        ScoresheetPanel scoresheet;
        RemainingCardsPanel remainingCards;
        NoticeBanner banner;
        ContractPicker picker;
        ChoiceDialog choiceDialog;
        SessionOverScreen sessionOver;

        Transform canvas;
        PlayerNameScreen playerNameScreen;
        DifficultyDialog difficultyDialog;
        BotDifficulty difficulty = BotDifficulty.Normal;

        static bool restartPending;
        static BotDifficulty restartDifficulty = BotDifficulty.Normal;
        static string restartSouth;
        static string restartWest;
        static string restartNorth;
        static string restartEast;

        bool awaitingHuman;
        Card? humanChoice;

        void Awake()
        {
            BuildCamera();
            BuildEventSystem();
            canvas = BuildCanvas();
            UiKit.Stretched("Felt", canvas, CardStyle.Felt);
            RifkiBranding.AddFeltWatermark(canvas);
        }

        void Start()
        {
            difficultyDialog = new DifficultyDialog(canvas);

            if (restartPending)
            {
                restartPending = false;
                GameText.SetSeatNames(
                    restartSouth,
                    restartWest,
                    restartNorth,
                    restartEast);
                StartConfiguredGame(restartDifficulty);
                return;
            }

            playerNameScreen = new PlayerNameScreen(canvas, BeginGame);
            RifkiBranding.ShowSplash(this, canvas);
        }

        void BeginGame(string south, string west, string north, string east)
        {
            GameText.SetSeatNames(south, west, north, east);

            PlayerNameScreen.SaveNames(
                GameText.SeatLabel(Seat.South),
                GameText.SeatLabel(Seat.West),
                GameText.SeatLabel(Seat.North),
                GameText.SeatLabel(Seat.East));

            difficultyDialog.Show(
                difficulty,
                false,
                StartConfiguredGame);
        }

        void StartConfiguredGame(BotDifficulty selectedDifficulty)
        {
            difficulty = selectedDifficulty;

            trickView = new TrickView(canvas);
            opponentsView = new OpponentsView(canvas);
            handView = new HandView(canvas, OnCardClicked);
            statusLine = new StatusLine(canvas);
            gameProgress = new GameProgressPanel(canvas);
            playerQuota = new PlayerQuotaView(canvas);
            BuildNewGameButton();
            new RulesPanel(canvas);

            // Creation order is draw order: the sheet sits over the table,
            // notices over the sheet, and the modals over everything.
            scoresheet = new ScoresheetPanel(canvas);
            remainingCards = new RemainingCardsPanel(canvas);
            banner = new NoticeBanner(canvas);
            picker = new ContractPicker(canvas);
            choiceDialog = new ChoiceDialog(canvas);
            sessionOver = new SessionOverScreen(canvas, ShowNewGameDialog);

            int seed = Environment.TickCount;
            for (int s = 1; s < 4; s++)
                bots[s] = new HeuristicAgent(seed + s, difficulty);

            session = new Session(seed);
            gameProgress.Refresh(session);
            playerQuota.Refresh(session);
            scoresheet.Refresh(session);
            StartCoroutine(RunSession());
        }

        void BuildCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = CardStyle.FeltDark;
            cam.orthographic = true;
            // The overlay canvas draws everything; the camera only clears the screen.
            cam.cullingMask = 0;
        }

        void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        Transform BuildCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go.transform;
        }

        static bool CanBreakTrumpHand(System.Collections.Generic.IReadOnlyList<Card> hand)
        {
            foreach (var card in hand)
                if (card.Rank == Rank.Jack
                    || card.Rank == Rank.Queen
                    || card.Rank == Rank.King
                    || card.Rank == Rank.Ace)
                    return false;

            return true;
        }

        static Seat? FindBotTrumpBreaker(
            System.Collections.Generic.IReadOnlyList<Card>[] hands,
            Suit trump,
            BotDifficulty difficulty)
        {
            for (int s = 1; s < 4; s++)
            {
                if (!CanBreakTrumpHand(hands[s]))
                    continue;

                if (BotShouldBreakTrumpHand(
                    hands[s],
                    trump,
                    difficulty))
                    return (Seat)s;
            }

            return null;
        }

        static bool BotShouldBreakTrumpHand(
            System.Collections.Generic.IReadOnlyList<Card> hand,
            Suit trump,
            BotDifficulty difficulty)
        {
            int trumpCount = 0;
            int trumpQuality = 0;
            int[] suitCounts = new int[4];

            foreach (var card in hand)
            {
                suitCounts[(int)card.Suit]++;

                if (card.Suit != trump)
                    continue;

                trumpCount++;

                switch (card.Rank)
                {
                    case Rank.Ten:
                        trumpQuality += 5;
                        break;
                    case Rank.Nine:
                        trumpQuality += 4;
                        break;
                    case Rank.Eight:
                        trumpQuality += 3;
                        break;
                    case Rank.Seven:
                        trumpQuality += 2;
                        break;
                    case Rank.Six:
                        trumpQuality += 1;
                        break;
                }
            }

            int distribution = 0;

            if (trumpCount > 0)
            {
                for (int s = 0; s < 4; s++)
                {
                    if ((Suit)s == trump)
                        continue;

                    if (suitCounts[s] == 0)
                        distribution += 3;
                    else if (suitCounts[s] == 1)
                        distribution += 1;
                }
            }

            int strength =
                trumpCount * 4
                + trumpQuality
                + distribution;

            switch (difficulty)
            {
                case BotDifficulty.Easy:
                    return strength < 22;

                case BotDifficulty.Hard:
                    return strength < 30;

                default:
                    return strength < 26;
            }
        }

        static bool BotShouldDeclareRifki(System.Collections.Generic.IReadOnlyList<Card> hand, Suit trump)
        {
            int trumps = 0;
            int topTrumps = 0;
            int outsideAces = 0;

            foreach (var card in hand)
            {
                if (card.Suit == trump)
                {
                    trumps++;
                    if (card.Rank >= Rank.Queen)
                        topTrumps++;
                }
                else if (card.Rank == Rank.Ace)
                {
                    outsideAces++;
                }
            }

            return trumps >= 6
                && topTrumps >= 2
                && (trumps >= 7 || outsideAces >= 2);
        }

        IEnumerator RunSession()
        {
            ContractCall? repeatTrumpCall = null;

            while (!session.IsComplete)
            {
                var hands = session.DealHands();
                remainingCards.ResetForNewDeal();
                var available = session.AvailableContracts();
                ContractCall call;
                if (repeatTrumpCall.HasValue)
                {
                    call = repeatTrumpCall.Value;
                    repeatTrumpCall = null;
                    handView.Show(hands[(int)Seat.South]);
                    handView.DisableAll();
                }
                else if (session.Caller == Seat.South)
                {
                    // Deal the cards face up first; picking a contract blind would be
                    // a coin toss.
                    handView.Show(hands[(int)Seat.South]);
                    handView.DisableAll();
                    statusLine.Set($"El {session.DealNumber}/{Session.DealCount}");
                    ContractCall? picked = null;
                    picker.Show(session, available, c => picked = c);
                    while (!picked.HasValue)
                        yield return null;
                    call = picked.Value;
                }
                else
                {
                    int caller = (int)session.Caller;
                    call = bots[caller].ChooseContract(session, hands[caller], available);
                }
                if (call.Type == ContractType.Trump)
                {
                    Seat? botBreaker = FindBotTrumpBreaker(
                        hands,
                        call.TrumpSuit.Value,
                        difficulty);

                    if (botBreaker.HasValue)
                    {
                        handView.Show(hands[(int)Seat.South]);

                        banner.Flash(
                            this,
                            GameText.SeatLabel(botBreaker.Value)
                                + " eli bozdu — koz eli yeniden dağıtılıyor",
                            2.5f);

                        yield return new WaitForSeconds(2.5f);
                        session.RedealUnstarted();
                        repeatTrumpCall = call;
                        continue;
                    }

                    if (CanBreakTrumpHand(hands[(int)Seat.South]))
                    {
                        handView.Show(hands[(int)Seat.South]);
                        handView.DisableAll();

                        bool? breakHand = null;

                        choiceDialog.Show(
                            "Koz Eli",
                            "Elinizde Vale, Kız, Papaz veya As yok. Eli bozabilirsiniz.",
                            "Eli Boz",
                            () => breakHand = true,
                            "Devam Et",
                            () => breakHand = false);

                        while (!breakHand.HasValue)
                            yield return null;

                        if (breakHand.Value)
                        {
                            banner.Flash(
                                this,
                                GameText.SeatLabel(Seat.South)
                                    + " eli bozdu — koz eli yeniden dağıtılıyor",
                                2.5f);

                            yield return new WaitForSeconds(2.5f);
                            session.RedealUnstarted();
                            repeatTrumpCall = call;
                            continue;
                        }
                    }
                }

                if (call.Type == ContractType.Trump)
                {
                    bool declareRifki = false;

                    if (session.Caller == Seat.South)
                    {
                        bool? choice = null;

                        choiceDialog.Show(
                            "Rıfkı",
                            "10 el alarak Rıfkı yapmayı ilan etmek ister misiniz? Başaramazsanız tek başınıza batarsınız.",
                            "Rıfkı İlan Et",
                            () => choice = true,
                            "Normal Oyna",
                            () => choice = false);

                        while (!choice.HasValue)
                            yield return null;

                        declareRifki = choice.Value;
                    }
                    else
                    {
                        int caller = (int)session.Caller;
                        declareRifki =
                            BotShouldDeclareRifki(
                                hands[caller],
                                call.TrumpSuit.Value);
                    }

                    if (declareRifki)
                        call = new ContractCall(
                            ContractType.Trump,
                            call.TrumpSuit,
                            true);
                }

                deal = session.StartDeal(call);
                statusLine.SetRifkiDeclared(call.RifkiDeclared);
                trickView.MarkCaller(session.Caller);
                remainingCards.Refresh(deal);
                playerQuota.Refresh(session);
                playerQuota.RefreshDeal(deal);
                yield return RunDeal();

                if (deal.QueensSplitOneEach)
                {
                    playerQuota.ClearDealCounts();
                    session.CancelDeal();
                    playerQuota.Refresh(session);

                    banner.Flash(
                        this,
                        "Herkes bir kız aldı — el iptal edildi",
                        3f);

                    yield return new WaitForSeconds(3f);
                    deal = null;
                    continue;
                }

                playerQuota.ClearDealCounts();
                session.FinishDeal();
                playerQuota.Refresh(session);
                gameProgress.Refresh(session);
                scoresheet.Refresh(session);
                deal = null;
            }
            sessionOver.Show(session);
        }

        IEnumerator RunDeal()
        {
            bool heartsMatter = deal.Contract.Type == ContractType.NoHearts
                || deal.Contract.Type == ContractType.KingOfHearts;
            trickView.Clear();
            RefreshTable();
            while (!deal.IsComplete)
            {
                Card chosen;
                if (deal.ToPlay == Seat.South)
                {
                    humanChoice = null;
                    awaitingHuman = true;
                    handView.EnableOnly(deal.LegalPlays());
                    while (humanChoice == null)
                        yield return null;
                    awaitingHuman = false;
                    handView.DisableAll();
                    chosen = humanChoice.Value;
                }
                else
                {
                    yield return new WaitForSeconds(BotDelay);
                    chosen = bots[(int)deal.ToPlay].ChooseCard(deal, deal.ToPlay);
                }
                bool wasBroken = deal.HeartsBroken;
                var completed = deal.Play(chosen);
                RefreshTable();
                if (heartsMatter && !wasBroken && deal.HeartsBroken)
                    banner.Flash(this, "Kupa açıldı", 2f);
                if (deal.IsComplete && deal.History.Count < 13 && !deal.QueensSplitOneEach && !deal.RifkiSucceeded && !deal.RifkiFailed)
                    banner.Flash(this, "Puanlanacak kart kalmadı — el erken bitti", 2.5f);
                if (completed != null)
                {
                    // The engine has already swept the trick into history, so put it
                    // back on the table while it lingers.
                    trickView.ShowCompleted(completed);
                    yield return new WaitForSeconds(
                        TrickLinger);
                    trickView.Clear();
                    // Clear() resets all four seat labels, so re-mark whoever leads next.
                    trickView.MarkTurn(deal.IsComplete ? (Seat?)null : deal.ToPlay);
                }
            }
        }

        void RefreshTable()
        {
            handView.Show(deal.HandOf(Seat.South));
            opponentsView.Refresh(deal);
            playerQuota.RefreshDeal(deal);
            remainingCards.Refresh(deal);
            trickView.ShowCurrent(deal.CurrentTrick);
            trickView.MarkTurn(deal.IsComplete ? (Seat?)null : deal.ToPlay);
            statusLine.Set(StatusText());
        }

        string StatusText()
        {
            string turn = deal.IsComplete ? "el bitti"
                : deal.ToPlay == Seat.South ? "sıra sizde"
                : GameText.SeatLabel(deal.ToPlay) + " oynayacak";
            return $"El {session.DealNumber}/{Session.DealCount}   {GameText.SeatLabel(session.Caller)}: {GameText.ContractLabel(deal.Contract)}   {turn}";
        }

        void BuildNewGameButton()
        {
            var rect = UiKit.Rect(
                "NewGameButton",
                canvas,
                Vector2.one,
                Vector2.one,
                new Vector2(-24f, -14f),
                new Vector2(150f, 48f));

            var image = UiKit.RoundedImage(
                rect,
                new Color(0.05f, 0.14f, 0.08f, 0.92f));

            var button = UiKit.MakeButton(image);

            var label = UiKit.Fill(
                "Label",
                rect,
                "Yeni Oyun",
                24,
                CardStyle.Cream,
                TextAnchor.MiddleCenter);

            button.onClick.AddListener(ShowNewGameDialog);
        }

        void ShowNewGameDialog()
        {
            difficultyDialog.Show(
                difficulty,
                true,
                RestartWithDifficulty);
        }

        void RestartWithDifficulty(BotDifficulty selectedDifficulty)
        {
            restartDifficulty = selectedDifficulty;
            restartSouth = GameText.SeatLabel(Seat.South);
            restartWest = GameText.SeatLabel(Seat.West);
            restartNorth = GameText.SeatLabel(Seat.North);
            restartEast = GameText.SeatLabel(Seat.East);
            restartPending = true;

            SceneManager.LoadScene(gameObject.scene.buildIndex);
        }

        void OnCardClicked(Card card)
        {
            // Buttons stay alive between turns, so a stray click can arrive while a
            // bot is thinking or a trick is lingering. Only accept one while the
            // coroutine is actually parked waiting for the human.
            if (!awaitingHuman || deal == null || deal.IsComplete || deal.ToPlay != Seat.South)
                return;
            foreach (var legal in deal.LegalPlays())
            {
                if (legal == card)
                {
                    // Close the gate here, not when the coroutine wakes up next frame,
                    // or a fast double click plays two cards.
                    awaitingHuman = false;
                    humanChoice = card;
                    return;
                }
            }
        }
    }
}
