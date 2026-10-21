using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Persistence;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>フロアのメニュー（会員の状態・クエストの一覧・持ち物・設定）。</summary>
    public static class MemberMenu
    {
        const string MutedTag = "<color=#A7B5AC>";
        static readonly Vector2 PageSize = new Vector2(1000, 1280);

        public static void Open(Transform root, StoryProgress story, ProfileService profiles, Action changed)
        {
            var dialog = Dialog.Open(root, "MENU", null, new Vector2(560, 560));
            var pages = new (string label, Action open)[]
            {
                ("STATUS", () => Page(root, "STATUS", StatusText(story.State), story, profiles, changed)),
                ("QUESTS", () => Page(root, "QUESTS", QuestText(story.State), story, profiles, changed)),
                ("ITEMS", () => Page(root, "ITEMS", ItemText(story.State), story, profiles, changed)),
                ("SETTINGS", () => SettingsDialog.Open(root, profiles, changed)),
            };
            float top = dialog.Content.sizeDelta.y / 2 - 40;
            for (int i = 0; i < pages.Length; i++)
            {
                var page = pages[i];
                Ui.Button(dialog.Content, page.label, page.label, new Vector2(0, top - i * 84), new Vector2(400, 68), new Color(1, 1, 1, 0.1f), Theme.Ivory, () =>
                {
                    dialog.Close();
                    page.open();
                }, 28);
            }
            dialog.AddButton("CLOSE", null, true);
        }

        static void Page(Transform root, string title, string body, StoryProgress story, ProfileService profiles, Action changed)
            => Dialog.Open(root, title, body, PageSize).AddButton("BACK", () => Open(root, story, profiles, changed), true);

        static string StatusText(RpgState state)
        {
            var rank = Progression.RankFor(state.level);
            long need = Progression.XpToNext(state.level);
            var text = new StringBuilder();
            text.AppendLine($"<b>Lv {state.level}</b>  ·  {StoryNames.Rank(rank)} MEMBER");
            text.AppendLine(need > 0 ? $"経験値 {state.xp} / {need}（次のレベルまで {need - state.xp}）" : "経験値 MAX");
            text.AppendLine($"コンプ {state.comps:N0} pt（還元率 ×{Progression.CompRate(rank):0.0#}）");
            text.AppendLine($"賭けた総額 {Labels.Money(state.wagered)}");
            text.AppendLine();
            var defeated = new List<string>();
            foreach (var floor in Floors.All)
            {
                if (state.Has(floor.Master.DefeatedFlag))
                    defeated.Add(StoryNames.Person(floor.Master.Master));
            }
            text.AppendLine($"倒したフロアの主  {defeated.Count} / {Floors.All.Count}");
            if (defeated.Count > 0)
                text.AppendLine(MutedTag + string.Join("、", defeated) + "</color>");
            text.AppendLine();
            text.Append(MutedTag + "経験値・会員ランク・コンプは賭けた額だけで決まり、ゲームの確率や配当は変わりません。</color>");
            return text.ToString();
        }

        static string QuestText(RpgState state)
        {
            var text = new StringBuilder();
            int finished = 0;
            foreach (var progress in state.quests)
            {
                if (progress.claimed)
                {
                    finished++;
                    continue;
                }
                var quest = QuestBook.Find(progress.id);
                string giver = StoryNames.Person(quest.Giver);
                string status = progress.done
                    ? $"★ 達成！ {giver}に報告"
                    : quest.Kind == QuestKind.ReachLevel ? $"Lv {state.level} / {quest.Target}" : $"{progress.count} / {quest.Target}";
                text.AppendLine($"<b>{StoryNames.Quest(quest.Id)}</b>  —  {status}");
                text.AppendLine(MutedTag + StoryNames.QuestDetail(quest.Id) + "</color>");
            }
            if (text.Length == 0)
                text.AppendLine("進行中のクエストはありません。フロアの人に話しかけてみましょう。");
            text.AppendLine();
            text.Append($"完了したクエスト  {finished} / {QuestBook.All.Count}");
            return text.ToString();
        }

        static string ItemText(RpgState state)
        {
            if (state.items.Count == 0)
                return "持ち物はありません。";
            var text = new StringBuilder();
            foreach (var stack in state.items)
            {
                text.AppendLine($"<b>{StoryNames.Item(stack.id)}</b>  ×{stack.count}");
                text.AppendLine(MutedTag + StoryNames.ItemDetail(stack.id) + "</color>");
            }
            return text.ToString().TrimEnd();
        }
    }
}
