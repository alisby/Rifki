using System;
using System.Collections;
using King.AI;
using King.Core;
using UnityEngine;
using UnityEngine.EventSystems;
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

        // Stand-in that makes South's call until the contract picker UI exists.
        IPlayerAgent southCaller;

        Session session;
        DealEngine deal;

        HandView handView;
        TrickView trickView;
        OpponentsView opponentsView;
        StatusLine statusLine;

        bool awaitingHuman;
        Card? humanChoice;

        void Awake()
        {
            BuildCamera();
            BuildEventSystem();
            var canvas = BuildCanvas();
            UiKit.Stretched("Felt", canvas, CardStyle.Felt);
            trickView = new TrickView(canvas);
            opponentsView = new OpponentsView(canvas);
            handView = new HandView(canvas, OnCardClicked);
            statusLine = new StatusLine(canvas);
        }

        void Start()
        {
            int seed = Environment.TickCount;
            for (int s = 1; s < 4; s++)
                bots[s] = new HeuristicAgent(seed + s);
            southCaller = new HeuristicAgent(seed + 4);
            session = new Session(seed);
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
                var available = session.AvailableContracts();
                // Hands are dealt inside StartDeal, so contract calls are made blind.
                var caller = session.Caller;
                var call = caller == Seat.South
                    ? southCaller.ChooseContract(session, Array.Empty<Card>(), available)
                    : bots[(int)caller].ChooseContract(session, Array.Empty<Card>(), available);
                deal = session.StartDeal(call);
                yield return RunDeal();
                session.FinishDeal();
                deal = null;
            }
            var t = session.Totals;
            statusLine.Set($"Session over   South {t[0]}   West {t[1]}   North {t[2]}   East {t[3]}");
        }

        IEnumerator RunDeal()
        {
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
                var completed = deal.Play(chosen);
                RefreshTable();
                if (completed != null)
                {
                    // The engine has already swept the trick into history, so put it
                    // back on the table while it lingers.
                    trickView.ShowCompleted(completed);
                    yield return new WaitForSeconds(TrickLinger);
                    trickView.Clear();
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
            string turn = deal.IsComplete ? "deal over"
                : deal.ToPlay == Seat.South ? "your turn"
                : GameText.SeatLabel(deal.ToPlay) + " to play";
            return $"Deal {session.DealNumber}/{Session.DealCount}   {GameText.SeatLabel(session.Caller)} called {GameText.ContractLabel(deal.Contract)}   {turn}";
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
                    humanChoice = card;
                    return;
                }
            }
        }
    }
}
