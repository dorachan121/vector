using System.Collections.Generic;
using UnityEngine;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Casino
{
    /// <summary>
    /// 全ゲーム共通のテーブル画面の土台（フェルト・上部のバー・進行表示・チップラック・操作ボタン・演出とダイアログのレイヤー）。
    /// ゲーム固有の部品は <see cref="Stage"/> に置く。座標は 1920×1080 基準で、配置の判断はしない。
    /// </summary>
    public sealed class CasinoView
    {
        static readonly Vector2 TableCenter = new Vector2(0, 10);
        static readonly Vector2 TableSize = new Vector2(1870, 870);
        public const float BottomRow = -478f;

        readonly Dictionary<int, IBetTarget> targets = new Dictionary<int, IBetTarget>();

        public RectTransform Root { get; private set; }

        /// <summary>ゲーム固有の部品を置くレイヤー（フェルトの上、上部のバーと演出の下）。</summary>
        public RectTransform Stage { get; private set; }

        /// <summary>カードやチップを飛ばす演出用の最前面レイヤー（ダイアログより下）。</summary>
        public RectTransform Fx { get; private set; }

        /// <summary>ダイアログとトーストを出すレイヤー。</summary>
        public RectTransform Overlay { get; private set; }

        /// <summary>負けたチップが飛んでいく先（親の位置）。</summary>
        public RectTransform Bank { get; private set; }

        public TableHud Hud { get; private set; }
        public StatusBanner Status { get; private set; }
        public ChipRackView Rack { get; private set; }
        public ActionBar Actions { get; private set; }
        public Toast Toast { get; private set; }

        public IReadOnlyDictionary<int, IBetTarget> Targets => targets;

        /// <param name="title">左上のゲーム名。</param>
        /// <param name="subtitle">ゲーム名の右の小さな表示。</param>
        /// <param name="limits">上部中央のテーブル情報。</param>
        /// <param name="feltPrint">フェルトに印刷する配当の要約。</param>
        public static CasinoView Build(string name, string title, string subtitle, string limits, string feltPrint, Color feltCenter, Color feltEdge)
        {
            var view = new CasinoView();
            var root = Ui.CreateScreen(name, ScreenShape.Landscape);
            view.Root = root;
            Ui.Image(root, "Felt", Art.TableIn(feltCenter, feltEdge), Color.white, TableCenter, TableSize);
            if (!string.IsNullOrEmpty(feltPrint))
                Ui.Text(root, "FeltPrint", feltPrint, Fonts.Label, 19, Theme.FeltPrint, new Vector2(0, 376), new Vector2(1400, 28)).characterSpacing = 6;
            view.Bank = Ui.Rect(root, "Bank", new Vector2(0, 330), new Vector2(10, 10));
            view.Stage = Ui.Stretch(root, "Stage");
            view.Status = StatusBanner.Create(root, new Vector2(0, -172));
            view.Rack = ChipRackView.Create(root, new Vector2(-420, BottomRow));
            view.Actions = ActionBar.Create(root, new Vector2(500, BottomRow));
            view.Hud = TableHud.Create(root, title, subtitle, limits);
            view.Fx = Ui.Stretch(root, "Fx");
            view.Overlay = Ui.Stretch(root, "Overlay");
            view.Toast = Toast.Create(view.Overlay, 144);
            return view;
        }

        /// <summary>フェルトのベットエリアを置く。</summary>
        public BetSpotView AddSpot(int id, Vector2 position, Vector2 size, bool round, string title, string odds, Color accent)
        {
            var spot = BetSpotView.Create(Stage, id, position, size, round, title, odds, accent);
            targets[id] = spot;
            return spot;
        }

        /// <summary>ゲーム独自のベットエリア（オッズ表のマスなど）を登録する。</summary>
        public void AddTarget(IBetTarget target) => targets[target.Id] = target;

        /// <summary>任意の RectTransform の位置を Fx レイヤーの座標に変換する。</summary>
        public Vector2 FxPoint(Vector3 worldPosition) => Fx.InverseTransformPoint(worldPosition);

        public void ShowBettingControls(bool visible)
        {
            Rack.gameObject.SetActive(visible);
            Actions.gameObject.SetActive(visible);
        }
    }
}
