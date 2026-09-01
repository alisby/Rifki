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
        NoticeBanner banner;
        ContractPicker picker;
        SessionOverScreen sessionOver;

        Transform canvas;
        PlayerNameScreen playerNameScreen;

        bool awaitingHuman;
        Card? humanChoice;

        void Awake()
        {
            BuildCamera();
            BuildEventSystem();
            canvas = BuildCanvas();
            UiKit.Stretched("Felt", canvas, CardStyle.Felt);
        }

        void Start()
        {
            playerNameScreen = new PlayerNameScreen(canvas, BeginGame);
        }

        void BeginGame(string south, string west, string north, string east)
        {
            GameText.SetSeatNames(south, west, north, east);

            PlayerNameScreen.SaveNames(
                GameText.SeatLabel(Seat.South),
                GameText.SeatLabel(Seat.West),
                GameText.SeatLabel(Seat.North),
                GameText.SeatLabel(Seat.East));

            trickView = new TrickView(canvas);
            opponentsView = new OpponentsView(canvas);
            handView = new HandView(canvas, OnCardClicked);
            statusLine = new StatusLine(canvas);
            gameProgress = new GameProgressPanel(canvas);
            playerQuota = new PlayerQuotaView(canvas);

            // Creation order is draw order: the sheet sits over the table,
            // notices over the sheet, and the modals over everything.
            scoresheet = new ScoresheetPanel(canvas);
            banner = new NoticeBanner(canvas);
            picker = new ContractPicker(canvas);
            sessionOver = new SessionOverScreen(canvas, Restart);

            int seed = Environment.TickCount;
            for (int s = 1; s < 4; s++)
                bots[s] = new HeuristicAgent(seed + s);

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

        IEnumerator RunSession()
        {
            while (!session.IsComplete)
            {
                var hands = session.DealHands();
                var available = session.AvailableContracts();
                ContractCall call;
                if (session.Caller == Seat.South)
                {
                    // Deal the cards face up first; picking a contract blind would be
                    // a coin toss.
                    handView.Show(hands[(int)Seat.South]);
                    handView.DisableAll();
                    statusLine.Set($"El {session.DealNumber}/{Session.DealCount}   kontratı seçin");
                    ContractCall? picked = null;
                    picker.Show(available, c => picked = c);
                    while (!picked.HasValue)
                        yield return null;
                    call = picked.Value;
                }
                else
                {
                    int caller = (int)session.Caller;
                    call = bots[caller].ChooseContract(session, hands[caller], available);
                }
                deal = session.StartDeal(call);
                playerQuota.Refresh(session);
                yield return RunDeal();
                session.FinishDeal();
                gameProgress.Refresh(session);
                scoresheet.Refresh(session);
                deal = null;
            }
            statusLine.Set("Oyun bitti");
            sessionOver.Show(session.Totals);
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
                if (deal.IsComplete && deal.History.Count < 13)
                    banner.Flash(this, "Puanlanacak kart kalmadı — el erken bitti", 2.5f);
                if (completed != null)
                {
                    // The engine has already swept the trick into history, so put it
                    // back on the table while it lingers.
                    trickView.ShowCompleted(completed);
                    yield return new WaitForSeconds(TrickLinger);
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

        void Restart()
        {
            // The whole table is rebuilt from code, so a fresh session is just a
            // scene reload.
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
