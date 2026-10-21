using System.Collections;
using UnityEngine;
using Vector.Core;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Casino
{
    /// <summary>
    /// テーブルで行うフロアの主の挑戦の共通処理（上部のバーの表示・途中でやめる確認・終わったときの結果とフロアへの帰還）。
    /// CasinoDirector（全ゲーム共通の進行役）から使う。
    /// </summary>
    public static class ChallengeFlow
    {
        /// <summary>このテーブルのゲームの挑戦が進行中なら、その挑戦用の手持ちを、そうでなければ所持金を使う。</summary>
        public static ITableWallet WalletFor(GameId game, long tableMinimum)
        {
            var run = GameContext.Challenge;
            if (run != null && run.Spec.Game == game && !run.IsOver)
                return new ChallengeWallet(run, tableMinimum);
            return new ProfileWallet(GameContext.Profiles, game);
        }

        /// <summary>上部のバーに相手・目標・残りラウンドを出す。</summary>
        public static void ShowHud(TableHud hud, ChallengeRun run)
        {
            string master = StoryNames.Person(run.Spec.Master);
            // 決着した後は「次のラウンド」を出さない（手持ちが尽きて終わったときに ROUND 8 / 8 と出ていた）
            string progress = run.IsOver
                ? $"{run.RoundsPlayed} / {run.Spec.Rounds} ROUNDS  ·  {(run.State == ChallengeState.Won ? "達成" : "決着")}"
                : $"ROUND {run.RoundsPlayed + 1} / {run.Spec.Rounds}  ·  あと {Labels.Money(run.Remaining)}";
            hud.SetChallenge($"VS {master}  ·  {Labels.Money(run.Bankroll.Balance)} / {Labels.Money(run.Spec.Target)}",
                progress, run.Bankroll.Balance / (float)run.Spec.Target);
        }

        /// <summary>挑戦をやめるか確認し、やめるなら負けとしてフロアへ戻る。</summary>
        public static void ConfirmForfeit(Transform overlay, ChallengeRun run)
        {
            Dialog.Open(overlay, "FORFEIT", $"{StoryNames.Person(run.Spec.Master)}との勝負をやめますか？\n途中でやめると負けになり、参加費は戻りません。", new Vector2(820, 380))
                .AddButton("CONTINUE", null, true)
                .AddButton("FORFEIT", () =>
                {
                    run.Abandon();
                    Resolve(run);
                    SceneFlow.Load(SceneNames.World);
                });
        }

        /// <summary>決着した挑戦の結果を確定してから見せ、フロアへ戻る（結果を見ている間にアプリが終わっても結果は残る）。</summary>
        public static IEnumerator Finish(Transform overlay, ChallengeRun run)
        {
            Resolve(run);
            bool won = run.State == ChallengeState.Won;
            Sfx.Play(won ? SfxKind.BigWin : SfxKind.Lose);
            string body = won
                ? $"目標の {Labels.Money(run.Spec.Target)} に届きました！\n挑戦用のチップ {Labels.Money(run.Bankroll.Balance)}  ·  {run.RoundsPlayed} ラウンド"
                : $"目標の {Labels.Money(run.Spec.Target)} には届きませんでした。\n挑戦用のチップ {Labels.Money(run.Bankroll.Balance)}  ·  {run.RoundsPlayed} ラウンド";
            yield return Dialog.Open(overlay, won ? "CHALLENGE CLEARED" : "CHALLENGE FAILED", body, new Vector2(820, 380))
                .AddButton("RETURN", null, true)
                .WaitUntilClosed();
            SceneFlow.Load(SceneNames.World);
        }

        /// <summary>障害が続いてテーブルを離れるとき、決着していない挑戦は無効にして参加費を返す。</summary>
        public static void CancelAfterFault()
        {
            if (GameContext.Challenge == null)
                return;
            GameContext.Challenge = null;
            GameContext.Story.CancelChallenge();
        }

        static void Resolve(ChallengeRun run)
        {
            GameContext.Challenge = null;
            GameContext.ChallengeResult = GameContext.Story.FinishChallenge(run);
        }

        /// <summary>精算で上がったレベル・達成したクエストなどをトーストで知らせる。</summary>
        public static void AnnounceReport(Toast toast, RoundReport report)
        {
            if (report == null)
                return;
            if (report.LevelsGained > 0)
            {
                string rank = report.NewRank.HasValue ? $"  ·  {StoryNames.Rank(report.NewRank.Value)} MEMBER" : "";
                toast.Show($"LEVEL UP!  Lv {GameContext.Story.State.level}{rank}", Theme.GoldBright);
                Sfx.Play(SfxKind.Sparkle);
            }
            else if (report.CompletedQuests.Count > 0)
            {
                toast.Show($"クエスト達成: {StoryNames.Quest(report.CompletedQuests[0].Id)}（フロアで報酬を受け取れます）", Theme.GoldBright);
                Sfx.Play(SfxKind.Sparkle);
            }
            else if (report.CompletedDailies.Count > 0)
            {
                toast.Show("デイリーミッション達成！（ホームの「ミッション」で報酬を受け取れます）", Theme.GoldBright);
                Sfx.Play(SfxKind.Sparkle);
            }
        }
    }
}
