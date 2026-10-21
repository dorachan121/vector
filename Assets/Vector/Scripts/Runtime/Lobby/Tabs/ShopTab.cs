using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.Story;
using Vector.Game.UI;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ショップのタブ（キャッシャー）。コンプポイントと景品の交換、引換券の使用、手持ちがミニマム未満のときの補充、フロアの主から受け取った記念品のコレクション。
    /// 表示するたびに中身を作り直す。
    /// </summary>
    public sealed class ShopTab : IHomeTab
    {
        static readonly Vector2 CardSize = new Vector2(1000, 140);

        readonly StoryProgress story;
        readonly ProfileService profiles;
        readonly long tableMinimum;
        readonly HomeNavigator nav;
        RectTransform content;

        ShopTab(Transform parent, StoryProgress story, ProfileService profiles, long tableMinimum, HomeNavigator nav)
        {
            this.story = story;
            this.profiles = profiles;
            this.tableMinimum = tableMinimum;
            this.nav = nav;
            Root = Ui.Stretch(parent, "ShopTab");
            Body = HomeLayout.Top(Root);
        }

        public RectTransform Root { get; }

        /// <summary>中身の土台（上端に寄せる）。</summary>
        RectTransform Body { get; }

        public static ShopTab Create(Transform parent, StoryProgress story, ProfileService profiles, long tableMinimum, HomeNavigator nav)
        {
            var tab = new ShopTab(parent, story, profiles, tableMinimum, nav);
            Ui.Ribbon(tab.Body, "Title", "CASHIER", new Vector2(0, 706), 560, Palette.Ruby, 36);
            Ui.Text(tab.Body, "Exchange", "EXCHANGE", Fonts.Label, 26, Palette.GoldLight, new Vector2(-300, 452), new Vector2(400, 32), TextAlignmentOptions.Left);
            Ui.Ribbon(tab.Body, "CollectionTitle", "COLLECTION", new Vector2(0, -250), 520, Palette.Violet, 34);
            Ui.Text(tab.Body, "Note", "景品とチップはすべて遊戯用です。現金・景品とは交換できません。", Fonts.Body, 18, Palette.Muted, new Vector2(0, -735), new Vector2(1000, 26));
            return tab;
        }

        public void Refresh()
        {
            if (content != null)
                Object.Destroy(content.gameObject);
            content = Ui.Stretch(Body, "Content");
            BuildBalance();
            float y = 362;
            foreach (var item in ItemCatalog.All)
            {
                if (item.CompPrice <= 0)
                    continue;
                BuildItem(item, y);
                y -= 150;
            }
            BuildReload(-110);
            BuildCollection();
        }

        void BuildBalance()
        {
            var state = story.State;
            var size = new Vector2(1000, 150);
            var rt = Ui.Card(content, "Comps", new Vector2(0, 560), size, Theme.Hex("123D57"), 26);
            var jewel = Ui.Image(rt, "Jewel", Chrome.Jewel, Palette.Cyan, new Vector2(-410, 0), Vector2.one * 110);
            Float.Add(jewel.rectTransform, 5f, 1.4f);
            Ui.Text(rt, "Label", "COMP POINTS", Fonts.Label, 22, Palette.Muted, new Vector2(-150, 36), new Vector2(400, 28), TextAlignmentOptions.Left);
            var value = Ui.Text(rt, "Value", state.comps.ToString("N0"), Fonts.Display, 60, Palette.Ivory, new Vector2(-150, -16), new Vector2(400, 70), TextAlignmentOptions.Left);
            Ui.Outline(value, Palette.Ink, 0.2f);
            var rank = Progression.RankFor(state.level);
            Ui.Paragraph(rt, "Rate", $"テーブルで賭けた額に応じて貯まります。\n{StoryNames.Rank(rank)} 会員の還元率 ×{Progression.CompRate(rank):0.0#}",
                19, Palette.GoldLight, new Vector2(300, 0), new Vector2(360, 80));
        }

        void BuildItem(ItemDef item, float y)
        {
            var state = story.State;
            var rt = Ui.Card(content, "Item", new Vector2(0, y), CardSize, Palette.PanelTop, 22, false);
            float left = -CardSize.x / 2;
            bool voucher = item.ChipValue > 0;
            Ui.Image(rt, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.3f), new Vector2(left + 76, 0), Vector2.one * 150);
            if (voucher)
                Ui.Image(rt, "Icon", Chrome.Coin, Color.white, new Vector2(left + 76, 0), Vector2.one * (item.ChipValue >= 5_000 ? 96 : 80));
            else
                Ui.Icon(rt, "Icon", IconKind.Star, Palette.GoldLight, new Vector2(left + 76, 0), 76);
            Ui.Text(rt, "Name", StoryNames.Item(item.Id), Fonts.Body, 27, Palette.Ivory, new Vector2(left + 380, 22), new Vector2(480, 36), TextAlignmentOptions.Left);
            Ui.Text(rt, "Detail", StoryNames.ItemDetail(item.Id), Fonts.Body, 18, Palette.Muted, new Vector2(left + 380, -22), new Vector2(480, 26), TextAlignmentOptions.Left);

            int owned = state.Count(item.Id);
            var buy = Ui.Button(rt, "Buy", $"{item.CompPrice} pt", new Vector2(CardSize.x / 2 - 104, 0), new Vector2(176, 76), Palette.Gold, Palette.Ink, () => Buy(item), 30, 22);
            buy.Interactable = state.comps >= item.CompPrice;
            var at = new Vector2(CardSize.x / 2 - 296, 0);
            if (!voucher)
            {
                Ui.Text(rt, "Owned", $"×{owned}", Fonts.Display, 34, Palette.Ivory, at, new Vector2(160, 44));
                return;
            }
            var use = Ui.Button(rt, "Use", $"USE ×{owned}", at, new Vector2(176, 76), new Color(1, 1, 1, 0.1f), Palette.Ivory, () => Redeem(item), 26, 22);
            use.Interactable = owned > 0;
        }

        void BuildReload(float y)
        {
            var size = new Vector2(1000, 150);
            var rt = Ui.Card(content, "Reload", new Vector2(0, y), size, Theme.Hex("0E5E4A"), 24, false);
            Ui.Icon(rt, "Icon", IconKind.Plus, Palette.Emerald, new Vector2(-424, 0), 70);
            Ui.Text(rt, "Title", "CHIP RELOAD", Fonts.Label, 30, Palette.Ivory, new Vector2(-120, 36), new Vector2(500, 36), TextAlignmentOptions.Left);
            Ui.Paragraph(rt, "Text", $"手持ちが {Labels.Money(tableMinimum)} 未満になったら、遊戯用チップを {Labels.Money(PlayerProfile.StartingBalance)} まで補充できます。",
                19, Palette.Muted, new Vector2(-120, -22), new Vector2(500, 64));
            var reload = Ui.Button(rt, "Do", "RELOAD", new Vector2(size.x / 2 - 120, 0), new Vector2(196, 80), Palette.Emerald, Palette.Ink, Reload, 30, 22);
            reload.Interactable = profiles.Profile.balance < tableMinimum;
        }

        void BuildCollection()
        {
            var floors = FloorOrder.BottomUp;
            for (int i = 0; i < floors.Length; i++)
            {
                var spec = Floors.Of(floors[i]).Master;
                var position = new Vector2((i % 4 - 1.5f) * 250, -404 - i / 4 * 206);
                bool owned = story.State.Count(spec.Trophy) > 0;
                var rt = Ui.Card(content, "Trophy", position, new Vector2(232, 190), owned ? Palette.VioletBright : Palette.NightDeep, 22, owned);
                if (owned)
                {
                    Ui.Image(rt, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.45f), new Vector2(0, 26), Vector2.one * 150);
                    Ui.Icon(rt, "Icon", spec.Trophy == ItemCatalog.BlackCard ? IconKind.Crown : IconKind.Trophy, Palette.Gold, new Vector2(0, 26), 86);
                }
                else
                {
                    Ui.Icon(rt, "Icon", IconKind.Lock, Palette.Slate, new Vector2(0, 26), 70);
                }
                Ui.Text(rt, "Name", owned ? StoryNames.Item(spec.Trophy) : StoryNames.FloorLevel(floors[i]) + "  ？？？", Fonts.Body, 19, owned ? Palette.Ivory : Palette.Muted,
                    new Vector2(0, -58), new Vector2(220, 28));
            }
        }

        void Buy(ItemDef item)
        {
            if (!story.Session.Buy(item.Id))
                return;
            story.Save();
            Sfx.Play(SfxKind.Chip);
            nav.Changed();
        }

        void Redeem(ItemDef item)
        {
            if (story.Session.Redeem(item.Id) <= 0)
                return;
            story.Save();
            Sfx.Play(SfxKind.ChipsCollect);
            nav.Changed();
        }

        void Reload()
        {
            if (profiles.Reload(tableMinimum) <= 0)
                return;
            Sfx.Play(SfxKind.ChipsCollect);
            nav.Changed();
        }
    }
}
