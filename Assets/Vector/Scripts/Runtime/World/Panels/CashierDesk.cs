using System;
using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.World
{
    /// <summary>
    /// フロアのキャッシャー。コンプポイントで景品と交換し、引換券を遊戯用チップにし、手持ちがミニマム未満なら補充する。
    /// 操作のたびに開き直して、残高を最新にする。
    /// </summary>
    public static class CashierDesk
    {
        const float RowHeight = 92f;

        public static void Open(Transform root, StoryProgress story, ProfileService profiles, long tableMinimum, Action changed)
        {
            var state = story.State;
            string body = $"コンプ {state.comps:N0} pt  ·  所持金 {Labels.Money(profiles.Profile.balance)}\nコンプポイントは、テーブルで賭けた額に応じて貯まります。";
            var dialog = Dialog.Open(root, "CASHIER", body, new Vector2(1000, 700));
            void Reopen()
            {
                dialog.Close();
                changed();
                Open(root, story, profiles, tableMinimum, changed);
            }

            float top = dialog.Content.sizeDelta.y / 2 - RowHeight / 2;
            int row = 0;
            foreach (var item in ItemCatalog.All)
            {
                if (item.CompPrice <= 0)
                    continue;
                float y = top - row++ * RowHeight;
                float left = -dialog.Content.sizeDelta.x / 2;
                Ui.Text(dialog.Content, "Name", StoryNames.Item(item.Id), Fonts.Body, 26, Theme.Ivory, new Vector2(left + 250, y + 14), new Vector2(480, 34), TextAlignmentOptions.Left);
                Ui.Text(dialog.Content, "Detail", StoryNames.ItemDetail(item.Id), Fonts.Body, 18, Theme.Muted, new Vector2(left + 250, y - 18), new Vector2(480, 26), TextAlignmentOptions.Left);

                var buy = Ui.Button(dialog.Content, "Buy", $"{item.CompPrice} PT", new Vector2(170, y), new Vector2(170, 60), Theme.Gold, Theme.Ink, () =>
                {
                    if (!story.Session.Buy(item.Id))
                        return;
                    story.Save();
                    Sfx.Play(SfxKind.Chip);
                    Reopen();
                }, 24);
                buy.Interactable = state.comps >= item.CompPrice;

                int owned = state.Count(item.Id);
                if (item.ChipValue <= 0)
                {
                    Ui.Text(dialog.Content, "Owned", $"×{owned}", Fonts.Label, 24, Theme.Ivory, new Vector2(360, y), new Vector2(170, 40));
                    continue;
                }
                var use = Ui.Button(dialog.Content, "Use", $"USE ×{owned}", new Vector2(360, y), new Vector2(170, 60), new Color(1, 1, 1, 0.1f), Theme.Ivory, () =>
                {
                    if (story.Session.Redeem(item.Id) <= 0)
                        return;
                    story.Save();
                    Sfx.Play(SfxKind.ChipsCollect);
                    Reopen();
                }, 24);
                use.Interactable = owned > 0;
            }

            if (profiles.Profile.balance < tableMinimum)
            {
                dialog.AddButton("RELOAD", () =>
                {
                    if (profiles.Reload(tableMinimum) > 0)
                        Sfx.Play(SfxKind.ChipsCollect);
                    changed();
                });
            }
            dialog.AddButton("CLOSE", null, true);
        }
    }
}
