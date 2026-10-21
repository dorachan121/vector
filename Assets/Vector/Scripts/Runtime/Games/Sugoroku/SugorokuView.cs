using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector.Core.Rpg;
using Vector.Core.Sugoroku;
using Vector.Game.Audio;
using Vector.Game.Casino;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;
using Vector.Game.World;

namespace Vector.Game.Sugoroku
{
    /// <summary>
    /// SUGOROKU の画面。左に巻物の双六盤（振出から上りまで蛇行する道）、上に倍率の段の表、右に2つのサイコロと「この目で進む」ボタン。
    /// 進行の判断はしない（SugorokuDirector から呼ばれるだけ）。駒の動きはエンジンが記録した1歩をなぞる。
    /// </summary>
    public sealed class SugorokuView
    {
        public static readonly Color Lacquer = Theme.Hex("6B1E1A");
        public static readonly Color LacquerEdge = Theme.Hex("1E0806");

        const int Columns = 8;
        const float HopSeconds = 0.13f;
        static readonly Vector2 Pitch = new Vector2(150, 118);
        static readonly Vector2 CellSize = new Vector2(138, 100);
        static readonly Vector2 FirstCell = new Vector2(-815, -292);
        const float LadderY = 228f;
        const float PanelX = 660f;
        static readonly Vector2[] DiceAt = { new Vector2(PanelX - 95, 84), new Vector2(PanelX + 95, 84) };

        /// <summary>マスごとの、止まる目を示す札（丸と数字）。</summary>
        readonly List<(Image disc, TextMeshProUGUI number)> badges = new List<(Image, TextMeshProUGUI)>();
        readonly List<Image> ladder = new List<Image>();
        readonly System.Random jitter = new System.Random();
        SugorokuRules rules;
        RectTransform board;
        CharacterView piece;
        DieView[] dice;
        CasinoButton[] go;
        TextMeshProUGUI turnsText;
        TextMeshProUGUI hintText;
        TextMeshProUGUI banner;

        public CasinoView Casino { get; private set; }

        /// <summary>進む目が選ばれた（0 か 1）。</summary>
        public event Action<int> Chosen;

        public static SugorokuView Build(SugorokuRules rules)
        {
            var view = new SugorokuView { rules = rules };
            var casino = CasinoView.Build("Sugoroku", "SUGOROKU", "BOARD JOURNEY",
                $"{Labels.Money(rules.MinBet)}–{Labels.Money(rules.MaxBet)}  ·  {rules.Turns} ROLLS  ·  TOP {SugorokuLabels.Pay(rules, rules.MaxTier)}",
                "", Lacquer, LacquerEdge);
            view.Casino = casino;
            casino.Actions.SetDealLabel("START");
            ((RectTransform)casino.Status.transform).anchoredPosition = new Vector2(560, 330);
            view.BuildBoard(casino.Stage);
            view.BuildLadder(casino.Stage);
            view.BuildPanel(casino.Stage);
            casino.AddSpot(SugorokuRules.Main, new Vector2(PanelX, -212), new Vector2(136, 136), true, "BET", $"TOP {SugorokuLabels.Pay(rules, rules.MaxTier)}", Theme.Gold);
            casino.Bank.anchoredPosition = CellCenter(rules.Goal);
            view.ResetJourney();
            return view;
        }

        /// <summary>マスの番号から盤の上の位置（行ごとに向きを変えて蛇行する。上りは最後のマスの次）。</summary>
        static Vector2 CellCenter(int index)
        {
            int row = index / Columns, col = index % Columns;
            if (row % 2 == 1)
                col = Columns - 1 - col;
            return FirstCell + new Vector2(col * Pitch.x, row * Pitch.y);
        }

        void BuildBoard(Transform stage)
        {
            var scrollCenter = new Vector2(FirstCell.x + Pitch.x * (Columns - 1) / 2, FirstCell.y + Pitch.y * 1.5f);
            var scrollSize = new Vector2(Pitch.x * Columns + 40, Pitch.y * 4 + 40);
            Ui.Panel(stage, "ScrollShadow", scrollCenter + new Vector2(6, -8), scrollSize, new Color(0, 0, 0, 0.45f), 24);
            Ui.Panel(stage, "Scroll", scrollCenter, scrollSize, Theme.Hex("D9C29A"), 24);
            Ui.Outline(stage, "ScrollRim", scrollCenter, scrollSize, Palette.GoldDark, 24);
            board = Ui.Rect(stage, "Board", Vector2.zero, Vector2.zero);

            // 道（隣り合うマスを結ぶ墨の線）
            for (int i = 0; i < rules.Goal; i++)
            {
                var a = CellCenter(i);
                var b = CellCenter(i + 1);
                var line = Ui.Image(board, "Path", null, SugorokuLabels.Ink.WithAlpha(0.5f), (a + b) / 2, new Vector2(Vector2.Distance(a, b), 8));
                line.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            }
            for (int i = 0; i <= rules.Goal; i++)
                badges.Add(BuildCell(i));
            piece = CharacterView.Create(board, "Piece", Cast.Player, 84, CellCenter(0) + new Vector2(0, 26));
        }

        (Image disc, TextMeshProUGUI number) BuildCell(int index)
        {
            var at = CellCenter(index);
            bool goal = index == rules.Goal;
            var square = rules.At(index);
            var color = goal ? Palette.GoldLight : SugorokuLabels.Paper;
            Ui.Panel(board, "Cell" + index, at, CellSize, color, 14);
            Ui.Outline(board, "CellRim", at, CellSize, SugorokuLabels.Ink.WithAlpha(0.55f), 14);
            if (goal)
            {
                Ui.Text(board, "Goal", "上り", Fonts.Body, 40, SugorokuLabels.Vermilion, at + new Vector2(0, 6), CellSize);
                Ui.Text(board, "GoalEn", "GOAL", Fonts.Label, 16, SugorokuLabels.Ink, at + new Vector2(0, -34), CellSize);
            }
            else if (index == 0)
            {
                Ui.Text(board, "Start", "振出", Fonts.Body, 30, SugorokuLabels.Ink, at + new Vector2(0, -24), CellSize);
            }
            else
            {
                Ui.Text(board, "Number", index.ToString(), Fonts.Label, 16, SugorokuLabels.Ink.WithAlpha(0.45f), at + new Vector2(-50, 34), new Vector2(40, 22));
                if (square != Square.Plain)
                {
                    var tint = SugorokuLabels.ColorOf(square);
                    if (square == Square.Coin)
                        Ui.Image(board, "Coin", Chrome.Coin, Color.white, at + new Vector2(0, 10), new Vector2(52, 52));
                    else if (square == Square.Treasure)
                        Ui.Image(board, "Gem", Art.Gem(4), tint, at + new Vector2(0, 10), new Vector2(56, 56));
                    else
                        Ui.Text(board, "Glyph", SugorokuLabels.Glyph(square), Fonts.Body, 40, tint, at + new Vector2(0, 10), CellSize);
                    Ui.Text(board, "Effect", SugorokuLabels.Effect(rules, square), Fonts.Body, 17, tint, at + new Vector2(0, -36), CellSize);
                }
            }
            var badgeAt = at + new Vector2(42, 26);
            var disc = Ui.Image(board, "BadgeDisc", Art.Circle, SugorokuLabels.Vermilion, badgeAt, Vector2.one * 48);
            disc.enabled = false;
            var number = Ui.Text(board, "Badge", "", Fonts.Display, 30, Theme.Ivory, badgeAt, new Vector2(48, 48));
            return (disc, number);
        }

        /// <summary>段ごとの、上がったときの配当の表（いまの段を光らせる）。</summary>
        void BuildLadder(Transform stage)
        {
            Ui.Text(stage, "LadderTitle", "上がったときの配当（小判で段が上がる）", Fonts.Body, 18, Palette.GoldLight, new Vector2(FirstCell.x + 230, LadderY + 58), new Vector2(560, 26));
            for (int tier = 0; tier <= rules.MaxTier; tier++)
            {
                var at = new Vector2(FirstCell.x + tier * Pitch.x, LadderY);
                var cell = Ui.Panel(stage, "Tier" + tier, at, new Vector2(Pitch.x - 12, 70), new Color(0, 0, 0, 0.45f), 12);
                Ui.Text(cell.transform, "Name", $"段 {tier}", Fonts.Body, 15, Theme.Ivory.WithAlpha(0.7f), new Vector2(0, 20), new Vector2(140, 20));
                Ui.Text(cell.transform, "Pay", SugorokuLabels.Pay(rules, tier), Fonts.Display, 30, tier == rules.MaxTier ? Theme.GoldBright : Theme.Ivory, new Vector2(0, -8), new Vector2(140, 36));
                ladder.Add(cell);
            }
            float legendY = LadderY - 66;
            var squares = new[] { Square.Coin, Square.Treasure, Square.Omen, Square.Bridge, Square.Pit };
            for (int i = 0; i < squares.Length; i++)
            {
                var s = squares[i];
                Ui.Text(stage, "Legend", $"<color=#{ColorUtility.ToHtmlStringRGB(SugorokuLabels.ColorOf(s) * 1.6f)}>{SugorokuLabels.Glyph(s)}</color> {SugorokuLabels.Effect(rules, s)}",
                    Fonts.Body, 17, Theme.Ivory.WithAlpha(0.85f), new Vector2(FirstCell.x + 60 + i * 230, legendY), new Vector2(220, 24), TextAlignmentOptions.Left).richText = true;
            }
        }

        void BuildPanel(Transform stage)
        {
            var plateSize = new Vector2(420, 580);
            Ui.Panel(stage, "PanelPlate", new Vector2(PanelX, 0), plateSize, Theme.Panel, 22);
            Ui.Outline(stage, "PanelRim", new Vector2(PanelX, 0), plateSize, Theme.PanelLine, 22);
            Ui.Text(stage, "DiceTitle", "どちらの目で進む？", Fonts.Body, 22, Palette.GoldLight, new Vector2(PanelX, 200), new Vector2(400, 30));
            turnsText = Ui.Text(stage, "Turns", "", Fonts.Label, 22, Theme.Ivory, new Vector2(PanelX, 160), new Vector2(400, 28));
            dice = new DieView[2];
            go = new CasinoButton[2];
            for (int i = 0; i < 2; i++)
            {
                int index = i;
                dice[i] = DieView.Create(stage, "Die" + i, DiceAt[i], 120, Theme.CardFace, SugorokuLabels.Ink);
                dice[i].Clicked += () => Chosen?.Invoke(index);
                go[i] = Ui.Button(stage, "Go" + i, "", DiceAt[i] + new Vector2(0, -114), new Vector2(170, 60), Palette.Gold, Palette.Ink, () => Chosen?.Invoke(index), 24, 16);
            }
            hintText = Ui.Paragraph(stage, "Hint", "", 17, Theme.Muted, new Vector2(PanelX, -112), new Vector2(390, 56), TextAlignmentOptions.Top);
            hintText.richText = true;
            banner = Ui.Text(stage, "Banner", "", Fonts.Display, 84, Theme.GoldBright, CellCenter(rules.Goal / 2) + new Vector2(0, 60), new Vector2(1100, 110));
            Ui.Outline(banner, Palette.Ink, 0.25f);
            banner.alpha = 0;
            HideChoices();
        }

        /// <summary>駒を振出に戻し、段・残り回数・サイコロを初めの状態にする。</summary>
        public void ResetJourney()
        {
            piece.Rect.anchoredPosition = CellCenter(0) + new Vector2(0, 26);
            piece.SetFacing(Directions.Right);
            SetTier(rules.StartTier);
            SetTurns(rules.Turns);
            foreach (var die in dice)
                die.SetFace(0);
            HideChoices();
            banner.alpha = 0;
        }

        public void SetTurns(int left) => turnsText.text = $"残り {left} 回 / {rules.Turns} 回";

        public void SetTier(int tier)
        {
            for (int i = 0; i < ladder.Count; i++)
                ladder[i].color = i == tier ? Palette.Gold.WithAlpha(0.55f) : new Color(0, 0, 0, 0.45f);
        }

        public IEnumerator Roll(int first, int second)
        {
            Sfx.Play(SfxKind.DiceRoll);
            dice[0].Roll(first, 0.6f, jitter);
            dice[1].Roll(second, 0.7f, jitter);
            yield return Tween.Wait(0.75f);
        }

        /// <summary>2つの目それぞれで止まるマスに目の数を出し、ボタンを押せるようにする。hint は勧める目の番号（-1 なら出さない）。</summary>
        public void ShowChoices(int position, int first, int second, int hint, string hintDetail)
        {
            ClearBadges();
            var faces = new[] { first, second };
            for (int i = 0; i < 2; i++)
            {
                int target = Math.Min(rules.Goal, position + faces[i]);
                var (disc, number) = badges[target];
                // 2つの目が同じマスに止まるなら、先に書いた目のままにする
                if (!disc.enabled || i == hint)
                {
                    disc.enabled = true;
                    disc.color = i == hint ? Palette.Gold : SugorokuLabels.Vermilion;
                    number.text = faces[i].ToString();
                    number.color = i == hint ? SugorokuLabels.Ink : Theme.Ivory;
                }
                go[i].SetLabel($"{faces[i]} で進む");
                go[i].gameObject.SetActive(true);
                go[i].Interactable = true;
                dice[i].SetSelectable(true);
                dice[i].SetHint(i == hint);
            }
            hintText.text = hintDetail;
        }

        public void HideChoices()
        {
            ClearBadges();
            for (int i = 0; i < 2; i++)
            {
                go[i].gameObject.SetActive(false);
                dice[i].SetSelectable(false);
                dice[i].SetHint(false);
            }
            hintText.text = "";
        }

        void ClearBadges()
        {
            foreach (var (disc, number) in badges)
            {
                disc.enabled = false;
                number.text = "";
            }
        }

        /// <summary>選んだ目の数だけ1マスずつ跳ねて進み、止まったマスの効果（段の増減・橋・落とし穴）を見せる。</summary>
        public IEnumerator Move(SugorokuStep step)
        {
            HideChoices();
            piece.SetFacing(Directions.Right);
            for (int at = step.From + 1; at <= step.Landed; at++)
                yield return Hop(CellCenter(at));
            var landed = CellCenter(step.Landed) + new Vector2(0, 80);
            switch (step.Square)
            {
                case Square.Coin:
                case Square.Treasure:
                    Sfx.Play(SfxKind.ChipsCollect);
                    ParticleBurst.Sparkles(board, landed, Palette.GoldLight, 14, 260);
                    FloatingText.Show(board, step.Square == Square.Treasure ? "宝！ 段 +2" : "小判 段 +1", Theme.GoldBright, landed, 34, 60, 0.9f);
                    break;
                case Square.Omen:
                    Sfx.Play(SfxKind.Lose, 0.6f);
                    FloatingText.Show(board, "凶 段 −1", SugorokuLabels.Vermilion, landed, 34, 60, 0.9f);
                    break;
                case Square.Bridge:
                    Sfx.Play(SfxKind.CardDeal);
                    FloatingText.Show(board, "橋を渡る", Theme.Ivory, landed, 30, 50, 0.8f);
                    yield return Slide(CellCenter(step.To), 0.45f);
                    break;
                case Square.Pit:
                    Sfx.Play(SfxKind.DoorOpen, 0.6f);
                    FloatingText.Show(board, "落とし穴！", SugorokuLabels.Vermilion, landed, 30, 50, 0.8f);
                    yield return Slide(CellCenter(step.To), 0.45f);
                    break;
            }
            if (step.TierAfter != step.TierBefore)
                SetTier(step.TierAfter);
            yield return Tween.Wait(step.Square == Square.Plain ? 0.1f : 0.35f);
        }

        IEnumerator Hop(Vector2 to)
        {
            var from = piece.Rect.anchoredPosition;
            var target = to + new Vector2(0, 26);
            Sfx.Play(SfxKind.Tick, 0.4f);
            Tween.Run(piece, "hop", HopSeconds, Ease.Linear, t =>
                piece.Rect.anchoredPosition = Vector2.Lerp(from, target, t) + new Vector2(0, Mathf.Sin(t * Mathf.PI) * 34));
            yield return Tween.Wait(HopSeconds);
            piece.Rect.anchoredPosition = target;
        }

        IEnumerator Slide(Vector2 to, float seconds)
        {
            Tween.Move(piece.Rect, to + new Vector2(0, 26), seconds, Ease.InOutQuad);
            yield return Tween.Wait(seconds);
        }

        /// <summary>道中の結末（上りなら段の配当、着けなければ没収）を大きく見せる。</summary>
        public void ShowFinish(SugorokuResult result)
        {
            banner.text = result.ReachedGoal ? $"上り！  {SugorokuLabels.Pay(rules, result.Tier)}" : "日が暮れた…";
            banner.color = result.ReachedGoal ? Theme.GoldBright : Theme.Muted;
            banner.rectTransform.localScale = Vector3.one * 0.6f;
            Tween.Scale(banner.rectTransform, Vector3.one, 0.35f, Ease.OutBack);
            Tween.Run(banner, "fade", 2.4f, Ease.Linear, t => banner.alpha = t < 0.12f ? t / 0.12f : t > 0.8f ? (1 - t) / 0.2f : 1);
            if (result.ReachedGoal)
                piece.Bob(1.2f);
        }
    }
}
