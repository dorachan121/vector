using TMPro;
using UnityEngine;
using Vector.Core.Rpg;
using Vector.Game.Audio;
using Vector.Game.Persistence;
using Vector.Game.Story;
using Vector.Game.UI;
using Vector.Game.UI.Widgets;

namespace Vector.Game.Lobby
{
    /// <summary>
    /// ショップ。上の切り替えで「交換所」（コンプと景品の交換・引換券の使用・チップの補充）と
    /// 「コレクション」（フロアの主から受け取った記念品）を見る。表示するたびに中身を作り直す。
    /// </summary>
    public sealed class ShopTab : IHomeTab
    {
        static readonly Vector2 TileSize = new Vector2(480, 470);

        readonly StoryProgress story;
        readonly ProfileService profiles;
        readonly long tableMinimum;
        readonly HomeNavigator nav;
        readonly SegmentedTabs pages;
        RectTransform content;

        ShopTab(Transform parent, StoryProgress story, ProfileService profiles, long tableMinimum, HomeNavigator nav)
        {
            this.story = story;
            this.profiles = profiles;
            this.tableMinimum = tableMinimum;
            this.nav = nav;
            Root = Ui.Stretch(parent, "ShopTab");
            Body = HomeLayout.Top(Root);
            PageTitle.Create(Body, 728, "SHOP", "ショップ");
            pages = SegmentedTabs.Create(Body, new Vector2(0, 606), 760, "交換所", "コレクション");
            pages.Selected += _ => Refresh();
        }

        public RectTransform Root { get; }

        /// <summary>中身の土台（上端に寄せる）。</summary>
        RectTransform Body { get; }

        public static ShopTab Create(Transform parent, StoryProgress story, ProfileService profiles, long tableMinimum, HomeNavigator nav)
        {
            var tab = new ShopTab(parent, story, profiles, tableMinimum, nav);
            tab.pages.Select(0);
            return tab;
        }

        public void Refresh()
        {
            if (content != null)
                Object.Destroy(content.gameObject);
            content = Ui.Stretch(Body, "Content");
            if (pages.Current == 1)
                BuildCollection();
            else
                BuildExchange();
        }

        // ------------------------------------------------------------------
        // 交換所
        // ------------------------------------------------------------------

        void BuildExchange()
        {
            BuildBalance(478);
            int slot = 0;
            foreach (var item in ItemCatalog.All)
            {
                if (item.CompPrice > 0)
                    BuildItem(item, TilePosition(slot++));
            }
            BuildReload(TilePosition(slot));
            Ui.Text(content, "Note", "景品とチップはすべて遊戯用です。現金・景品とは交換できません。", Fonts.Body, 21, Palette.Muted, new Vector2(0, -676), new Vector2(1000, 30));
        }

        static Vector2 TilePosition(int slot) => new Vector2((slot % 2 - 0.5f) * (TileSize.x + 24), 140 - slot / 2 * (TileSize.y + 22));

        void BuildBalance(float y)
        {
            var state = story.State;
            var size = new Vector2(1000, 116);
            var rt = Ui.Card(content, "Comps", new Vector2(0, y), size, Theme.Hex("123D57"), 30);
            var jewel = Ui.Image(rt, "Jewel", Chrome.Jewel, Palette.Cyan, new Vector2(-430, 0), Vector2.one * 92);
            Float.Add(jewel.rectTransform, 4f, 1.4f);
            Ui.Text(rt, "Label", "所持コンプ", Fonts.Body, 26, Palette.Muted, new Vector2(-210, 0), new Vector2(200, 34), TextAlignmentOptions.Left);
            var value = Ui.Headline(rt, "Value", state.comps.ToString("N0") + " pt", Fonts.Display, 56, new Vector2(30, 0), new Vector2(300, 66), TextAlignmentOptions.Left);
            value.characterSpacing = 2;
            var rank = Progression.RankFor(state.level);
            Ui.Paragraph(rt, "Rate", $"{StoryNames.Rank(rank)} 会員\n還元率 ×{Progression.CompRate(rank):0.0#}", 21, Palette.GoldLight, new Vector2(370, 0), new Vector2(220, 70), TextAlignmentOptions.Right);
        }

        void BuildItem(ItemDef item, Vector2 at)
        {
            var state = story.State;
            var rt = Ui.Card(content, "Item" + item.Id, at, TileSize, Palette.PanelTop, 28);
            bool voucher = item.ChipValue > 0;
            var art = new Vector2(0, TileSize.y / 2 - 130);
            Ui.Sliced(rt, "ArtBack", Chrome.Shade, Palette.Violet, art, new Vector2(TileSize.x - 36, 220), 22);
            var rays = Ui.Image(rt, "Rays", Chrome.Rays(14), Palette.GoldLight.WithAlpha(0.2f), art, Vector2.one * 300);
            Spin.Add(rays, 8f);
            Ui.Image(rt, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.45f), art, Vector2.one * 240);
            if (voucher)
            {
                bool large = item.ChipValue >= 5_000;
                for (int i = 0; i < (large ? 3 : 1); i++)
                    Ui.Image(rt, "Coin", Chrome.Coin, Color.white, art + new Vector2((i - (large ? 1 : 0)) * 46, i * 8 - (large ? 8 : 0)), Vector2.one * 130);
            }
            else
                Ui.Icon(rt, "Icon", IconKind.Star, Palette.GoldLight, art, 130);
            int owned = state.Count(item.Id);
            Ui.Sliced(rt, "OwnedBack", Chrome.Shade, Palette.NightDeep.WithAlpha(0.85f), art + new Vector2(150, -80), new Vector2(130, 40), 20);
            Ui.Text(rt, "Owned", $"所持 {owned}", Fonts.Body, 22, Palette.Ivory, art + new Vector2(150, -79), new Vector2(124, 30));

            Ui.Text(rt, "Name", StoryNames.Item(item.Id), Fonts.Body, 28, Palette.Ivory, new Vector2(0, -26), new Vector2(TileSize.x - 30, 38));
            Ui.Paragraph(rt, "Detail", StoryNames.ItemDetail(item.Id), 20, Palette.Muted, new Vector2(0, -88), new Vector2(TileSize.x - 50, 60), TextAlignmentOptions.Top);

            float y = -TileSize.y / 2 + 58;
            if (voucher)
            {
                var use = Ui.Button(rt, "Use", "使う", new Vector2(-112, y), new Vector2(196, 80), new Color(1, 1, 1, 0.1f), Palette.Ivory, () => Redeem(item), 30, 24);
                use.Label.font = Fonts.Body;
                use.Interactable = owned > 0;
            }
            var buy = Ui.Button(rt, "Buy", $"{item.CompPrice} pt", new Vector2(voucher ? 112 : 0, y), new Vector2(voucher ? 196 : 300, 80), Palette.Gold, Palette.Ink, () => Buy(item), 32, 24);
            buy.Interactable = state.comps >= item.CompPrice;
        }

        void BuildReload(Vector2 at)
        {
            var rt = Ui.Card(content, "Reload", at, TileSize, Theme.Hex("0E5E4A"), 28);
            var art = new Vector2(0, TileSize.y / 2 - 130);
            Ui.Sliced(rt, "ArtBack", Chrome.Shade, Theme.Hex("0A3F33"), art, new Vector2(TileSize.x - 36, 220), 22);
            Ui.Image(rt, "Glow", Art.Glow, Palette.Emerald.WithAlpha(0.5f), art, Vector2.one * 240);
            Ui.Image(rt, "Coin", Chrome.Coin, Color.white, art + new Vector2(-30, -10), Vector2.one * 120);
            Ui.IconButton(rt, "Plus", IconKind.Plus, art + new Vector2(60, 40), 80, Palette.Emerald, Reload);
            Ui.Text(rt, "Name", "チップの補充", Fonts.Body, 28, Palette.Ivory, new Vector2(0, -26), new Vector2(TileSize.x - 30, 38));
            Ui.Paragraph(rt, "Detail", $"手持ちが {Labels.Money(tableMinimum)} 未満になったら、{Labels.Money(PlayerProfile.StartingBalance)} まで補充できます", 20, Palette.Muted,
                new Vector2(0, -88), new Vector2(TileSize.x - 50, 60), TextAlignmentOptions.Top);
            var reload = Ui.Button(rt, "Do", "補充する", new Vector2(0, -TileSize.y / 2 + 58), new Vector2(300, 80), Palette.Emerald, Palette.Ink, Reload, 30, 24);
            reload.Label.font = Fonts.Body;
            reload.Interactable = profiles.Profile.balance < tableMinimum;
        }

        // ------------------------------------------------------------------
        // コレクション
        // ------------------------------------------------------------------

        void BuildCollection()
        {
            var floors = FloorOrder.BottomUp;
            int owned = 0;
            for (int i = 0; i < floors.Length; i++)
            {
                var spec = Floors.Of(floors[i]).Master;
                bool has = story.State.Count(spec.Trophy) > 0;
                if (has)
                    owned++;
                var size = new Vector2(480, 230);
                var at = i == floors.Length - 1 && floors.Length % 2 == 1
                    ? new Vector2(0, 410 - i / 2 * 250)
                    : new Vector2((i % 2 - 0.5f) * (size.x + 24), 410 - i / 2 * 250);
                var rt = Ui.Card(content, "Trophy", at, size, has ? Palette.VioletBright : Palette.NightDeep, 26, has);
                if (has)
                {
                    Ui.Image(rt, "Glow", Art.Glow, Palette.GoldLight.WithAlpha(0.5f), new Vector2(-150, 0), Vector2.one * 220);
                    Ui.Icon(rt, "Icon", spec.Trophy == ItemCatalog.BlackCard ? IconKind.Crown : IconKind.Trophy, Palette.Gold, new Vector2(-150, 0), 120);
                }
                else
                    Ui.Icon(rt, "Icon", IconKind.Lock, Palette.Slate, new Vector2(-150, 0), 90);
                Ui.Text(rt, "Floor", StoryNames.Floor(floors[i]), Fonts.Label, 22, has ? Palette.GoldLight : Palette.Muted, new Vector2(70, 64), new Vector2(260, 28), TextAlignmentOptions.Left);
                Ui.Text(rt, "Name", has ? StoryNames.Item(spec.Trophy) : "？？？", Fonts.Body, 27, has ? Palette.Ivory : Palette.Muted, new Vector2(70, 22), new Vector2(260, 36), TextAlignmentOptions.Left);
                Ui.Paragraph(rt, "Detail", has ? StoryNames.ItemDetail(spec.Trophy) : $"{StoryNames.Person(spec.Master)}に勝つと手に入る", 19, Palette.Muted,
                    new Vector2(70, -50), new Vector2(260, 80));
            }
            Ui.Headline(content, "Count", $"{owned} / {floors.Length}", Fonts.Display, 60, new Vector2(0, -580), new Vector2(600, 70));
            Ui.Text(content, "CountLabel", "集めた記念品", Fonts.Body, 24, Palette.Muted, new Vector2(0, -636), new Vector2(600, 32));
        }

        // ------------------------------------------------------------------
        // 操作
        // ------------------------------------------------------------------

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
