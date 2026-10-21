using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// アイテムの種類。コンプポイントで交換できる景品（遊戯用チップの引換券）と、フロアの主から受け取る記念品。
    /// どのアイテムもゲームの確率には影響しない。名前と説明は表示側（Runtime）が持つ。
    /// </summary>
    public static class ItemCatalog
    {
        public sealed class Item
        {
            public string Id;

            /// <summary>コンプでの交換に必要なポイント（0 なら交換できない）。</summary>
            public long CompPrice;

            /// <summary>使うと受け取れる遊戯用チップ（0 なら使えない）。</summary>
            public long ChipValue;
        }

        public const string VoucherSmall = "voucher.500";
        public const string VoucherLarge = "voucher.5000";
        public const string LuckyCoin = "lucky.coin";
        public const string TrophyLobby = "trophy.lobby";
        public const string TrophyDice = "trophy.dice";
        public const string TrophyArena = "trophy.arena";
        public const string TrophyTrack = "trophy.track";
        public const string TrophyAlchemy = "trophy.alchemy";
        public const string TrophyVault = "trophy.vault";
        public const string BlackCard = "black.card";

        static readonly List<Item> items = new List<Item>
        {
            new Item { Id = VoucherSmall, CompPrice = 50, ChipValue = 500 },
            new Item { Id = VoucherLarge, CompPrice = 450, ChipValue = 5_000 },
            new Item { Id = LuckyCoin, CompPrice = 30 },
            new Item { Id = TrophyLobby },
            new Item { Id = TrophyDice },
            new Item { Id = TrophyArena },
            new Item { Id = TrophyTrack },
            new Item { Id = TrophyAlchemy },
            new Item { Id = TrophyVault },
            new Item { Id = BlackCard },
        };

        public static IReadOnlyList<Item> All => items;

        public static Item Find(string id)
        {
            foreach (var item in items)
            {
                if (item.Id == id)
                    return item;
            }
            return null;
        }
    }
}
