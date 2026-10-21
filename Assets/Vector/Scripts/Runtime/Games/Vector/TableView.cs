using System.Collections.Generic;
using UnityEngine;
using Vector.Core;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Table
{
    /// <summary>
    /// テーブル画面のレイアウト。各部品を1920×1080基準の座標に配置して束ねるだけで、進行の判断はしない。
    /// </summary>
    public sealed class TableView
    {
        static readonly Vector2 TableCenter = new Vector2(0, 10);
        static readonly Vector2 TableSize = new Vector2(1870, 870);
        static readonly Vector2 BoardCenter = new Vector2(0, 125);
        const float PlateOffset = 372f;
        const float SpotRow = -285f;
        const float BottomRow = -478f;

        public RectTransform Root { get; private set; }

        /// <summary>カードやチップを飛ばす演出用の最前面レイヤー（ダイアログより下）。</summary>
        public RectTransform Fx { get; private set; }

        /// <summary>ダイアログとトーストを出すレイヤー。</summary>
        public RectTransform Overlay { get; private set; }

        public TableHud Hud { get; private set; }
        public BoardView Board { get; private set; }
        public HandView Hand { get; private set; }
        public ScorePlate PlayerPlate { get; private set; }
        public ScorePlate DealerPlate { get; private set; }
        public ShoeView Shoe { get; private set; }
        public RectTransform Discard { get; private set; }
        public StatusBanner Status { get; private set; }
        public ChipRackView Rack { get; private set; }
        public ActionBar Actions { get; private set; }
        public FamilyPicker Picker { get; private set; }
        public ResultBanner Result { get; private set; }
        public ScoreboardView Scoreboard { get; private set; }
        public PaytableView Paytable { get; private set; }
        public Toast Toast { get; private set; }
        public IReadOnlyDictionary<BetSpot, BetSpotView> Spots => spots;

        readonly Dictionary<BetSpot, BetSpotView> spots = new Dictionary<BetSpot, BetSpotView>();

        public static TableView Build(GameMode mode, TableRules rules)
        {
            var view = new TableView();
            var root = Ui.CreateScreen("Table");
            view.Root = root;

            Ui.Image(root, "Felt", Art.Table, Color.white, TableCenter, TableSize);
            string print = mode == GameMode.Player
                ? $"ALL LINES PAY {rules.MainPays}  ·  TIES PUSH"
                : $"PLAYER {rules.BackPlayerPays}  ·  DEALER {rules.BackDealerPays}  ·  TIE {rules.BackTiePays}";
            Ui.Text(root, "FeltPrint", print, Fonts.Label, 19, Theme.FeltPrint, new Vector2(0, 376), new Vector2(1200, 28)).characterSpacing = 6;

            view.Scoreboard = ScoreboardView.Create(root, new Vector2(-705, 190), new Vector2(330, 340));
            view.Paytable = PaytableView.Create(root, new Vector2(705, 125), new Vector2(330, 470), mode, rules);

            view.Discard = Ui.Rect(root, "Discard", new Vector2(-PlateOffset, 350), CardView.Size * 0.8f);
            Ui.Image(view.Discard, "Slot", Art.RoundedOutline(32, 3f), Theme.FeltPrint.WithAlpha(0.35f), Vector2.zero, CardView.Size * 0.8f).pixelsPerUnitMultiplier = 32f / 9f;
            Ui.Text(view.Discard, "Label", "DISCARD", Fonts.Label, 15, Theme.FeltPrint, Vector2.zero, new Vector2(100, 20));
            view.Shoe = ShoeView.Create(root, new Vector2(PlateOffset, 350));

            view.Board = BoardView.Create(root, BoardCenter);
            view.PlayerPlate = ScorePlate.Create(root, new Vector2(-PlateOffset, BoardCenter.y), mode == GameMode.Player ? "YOU" : "PLAYER", Theme.PlayerBlue);
            view.DealerPlate = ScorePlate.Create(root, new Vector2(PlateOffset, BoardCenter.y), "DEALER", Theme.DealerRed);
            view.Status = StatusBanner.Create(root, new Vector2(0, -172));
            view.Hand = HandView.Create(root, new Vector2(0, SpotRow));
            view.BuildSpots(root, mode, rules);

            view.Rack = ChipRackView.Create(root, new Vector2(-420, BottomRow));
            view.Actions = ActionBar.Create(root, new Vector2(500, BottomRow));
            view.Picker = FamilyPicker.Create(root, new Vector2(0, BottomRow));
            view.Result = ResultBanner.Create(root, new Vector2(0, -282));
            view.Hud = TableHud.Create(root, "VECTOR", Labels.Mode(mode), $"TABLE LIMITS  {Labels.Money(rules.MinBet)} – {Labels.Money(rules.MaxBet)}");

            view.Fx = Ui.Stretch(root, "Fx");
            view.Overlay = Ui.Stretch(root, "Overlay");
            view.Toast = Toast.Create(view.Overlay, new Vector2(0, 396));
            return view;
        }

        void BuildSpots(Transform root, GameMode mode, TableRules rules)
        {
            if (mode == GameMode.Player)
            {
                AddSpot(root, BetSpot.Main, new Vector2(-430, SpotRow), new Vector2(170, 170), true, "MAIN", $"PAYS {rules.MainPays}", Theme.Gold);
                AddSpot(root, BetSpot.Bonus, new Vector2(430, SpotRow), new Vector2(140, 140), true, "BONUS", "POKER HAND", Theme.GoldBright);
                return;
            }
            AddSpot(root, BetSpot.Player, new Vector2(-430, SpotRow), new Vector2(230, 150), false, "PLAYER", $"PAYS {rules.BackPlayerPays}", Theme.PlayerBlue);
            AddSpot(root, BetSpot.Dealer, new Vector2(430, SpotRow), new Vector2(230, 150), false, "DEALER", $"PAYS {rules.BackDealerPays}", Theme.DealerRed);
            AddSpot(root, BetSpot.Tie, new Vector2(-680, SpotRow), new Vector2(130, 130), true, "TIE", $"PAYS {rules.BackTiePays}", Theme.TieGreen);
            AddSpot(root, BetSpot.Bonus, new Vector2(680, SpotRow), new Vector2(130, 130), true, "BONUS", "POKER HAND", Theme.GoldBright);
        }

        void AddSpot(Transform root, BetSpot spot, Vector2 position, Vector2 size, bool round, string title, string odds, Color accent)
            => spots[spot] = BetSpotView.Create(root, (int)spot, position, size, round, title, odds, accent);

        /// <summary>任意の RectTransform の位置を Fx レイヤーの座標に変換する。</summary>
        public Vector2 FxPoint(Vector3 worldPosition) => Fx.InverseTransformPoint(worldPosition);

        public void ShowBettingControls(bool visible)
        {
            Rack.gameObject.SetActive(visible);
            Actions.gameObject.SetActive(visible);
        }
    }
}
