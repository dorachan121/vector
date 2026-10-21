using UnityEngine;
using Vector.Core;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>
    /// VECTOR のテーブル画面。共通の土台（CasinoView）の上に、盤面・手札・得点・シュー・スコアボード・配当表・ベットエリアを
    /// 1920×1080 基準の座標で置いて束ねるだけで、進行の判断はしない。
    /// </summary>
    public sealed class TableView
    {
        static readonly Vector2 BoardCenter = new Vector2(0, 125);
        const float PlateOffset = 372f;
        const float SpotRow = -285f;

        public CasinoView Casino { get; private set; }
        public BoardView Board { get; private set; }
        public HandView Hand { get; private set; }
        public ScorePlate PlayerPlate { get; private set; }
        public ScorePlate DealerPlate { get; private set; }
        public ShoeView Shoe { get; private set; }
        public RectTransform Discard { get; private set; }
        public FamilyPicker Picker { get; private set; }
        public ResultBanner Result { get; private set; }
        public ScoreboardView Scoreboard { get; private set; }
        public PaytablePanel Paytable { get; private set; }

        public static TableView Build(GameMode mode, TableRules rules)
        {
            string print = mode == GameMode.Player
                ? $"ALL LINES PAY {rules.MainPays}  ·  TIES PUSH"
                : $"PLAYER {rules.BackPlayerPays}  ·  DEALER {rules.BackDealerPays}  ·  TIE {rules.BackTiePays}";
            var casino = CasinoView.Build("Table", "VECTOR", Labels.Mode(mode),
                $"TABLE LIMITS  {Labels.Money(rules.MinBet)} – {Labels.Money(rules.MaxBet)}", print, Theme.FeltCenter, Theme.FeltEdge);
            var stage = casino.Stage;
            var view = new TableView { Casino = casino };

            view.Scoreboard = ScoreboardView.Create(stage, new Vector2(-705, 190), new Vector2(330, 340));
            view.Paytable = VectorPaytable.Create(stage, new Vector2(705, 125), new Vector2(330, 470), mode, rules);

            view.Discard = Ui.Rect(stage, "Discard", new Vector2(-PlateOffset, 350), CardView.Size * 0.8f);
            Ui.Image(view.Discard, "Slot", Art.RoundedOutline(32, 3f), Theme.FeltPrint.WithAlpha(0.35f), Vector2.zero, CardView.Size * 0.8f).pixelsPerUnitMultiplier = 32f / 9f;
            Ui.Text(view.Discard, "Label", "DISCARD", Fonts.Label, 15, Theme.FeltPrint, Vector2.zero, new Vector2(100, 20));
            view.Shoe = ShoeView.Create(stage, new Vector2(PlateOffset, 350));
            // 負けたチップはシューのあるディーラー側へ飛ばす
            casino.Bank.anchoredPosition = new Vector2(PlateOffset, 350);

            view.Board = BoardView.Create(stage, BoardCenter);
            view.PlayerPlate = ScorePlate.Create(stage, new Vector2(-PlateOffset, BoardCenter.y), mode == GameMode.Player ? "YOU" : "PLAYER", Theme.PlayerBlue);
            view.DealerPlate = ScorePlate.Create(stage, new Vector2(PlateOffset, BoardCenter.y), "DEALER", Theme.DealerRed);
            view.Hand = HandView.Create(stage, new Vector2(0, SpotRow));
            AddSpots(casino, mode, rules);
            view.Picker = FamilyPicker.Create(stage, new Vector2(0, CasinoView.BottomRow));
            view.Result = ResultBanner.Create(stage, new Vector2(0, -282));
            return view;
        }

        static void AddSpots(CasinoView casino, GameMode mode, TableRules rules)
        {
            if (mode == GameMode.Player)
            {
                AddSpot(casino, BetSpot.Main, new Vector2(-430, SpotRow), new Vector2(170, 170), true, $"PAYS {rules.MainPays}", Theme.Gold);
                AddSpot(casino, BetSpot.Bonus, new Vector2(430, SpotRow), new Vector2(140, 140), true, "POKER HAND", Theme.GoldBright);
                return;
            }
            AddSpot(casino, BetSpot.Player, new Vector2(-430, SpotRow), new Vector2(230, 150), false, $"PAYS {rules.BackPlayerPays}", Theme.PlayerBlue);
            AddSpot(casino, BetSpot.Dealer, new Vector2(430, SpotRow), new Vector2(230, 150), false, $"PAYS {rules.BackDealerPays}", Theme.DealerRed);
            AddSpot(casino, BetSpot.Tie, new Vector2(-680, SpotRow), new Vector2(130, 130), true, $"PAYS {rules.BackTiePays}", Theme.TieGreen);
            AddSpot(casino, BetSpot.Bonus, new Vector2(680, SpotRow), new Vector2(130, 130), true, "POKER HAND", Theme.GoldBright);
        }

        static void AddSpot(CasinoView casino, BetSpot spot, Vector2 position, Vector2 size, bool round, string odds, Color accent)
            => casino.AddSpot((int)spot, position, size, round, Labels.Spot(spot), odds, accent);
    }
}
