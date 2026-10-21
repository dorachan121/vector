using System.Collections.Generic;

namespace Vector.Core.Rpg
{
    /// <summary>
    /// アイテムの種類。コンプポイントで交換できる景品（遊戯用チップの引換券）と、フロアの主から受け取る記念品。
    /// どのアイテムもゲームの確率には影響しない。名前と説明は表示側（Runtime）が持つ。
    /// </summary>
    public static class ItemCatalog
    {
        public const string VoucherSmall = "voucher.500";
        public const string VoucherLarge = "voucher.5000";
        public const string LuckyCoin = "lucky.coin";
        public const string TrophyLobby = "trophy.lobby";
        public const string TrophyDice = "trophy.dice";
        public const string TrophyArena = "trophy.arena";
        public const string TrophyTrack = "trophy.track";
        public const string TrophyAlchemy = "trophy.alchemy";
        public const string TrophyVault = "trophy.vault";
        public const string TrophyAquarium = "trophy.aquarium";
        public const string TrophyObservatory = "trophy.observatory";
        public const string TrophyPostTown = "trophy.post_town";
        public const string TrophyRooftop = "trophy.rooftop";
        public const string BlackCard = "black.card";

        static readonly List<ItemDef> items = new List<ItemDef>
        {
            new ItemDef { Id = VoucherSmall, CompPrice = 50, ChipValue = 500 },
            new ItemDef { Id = VoucherLarge, CompPrice = 450, ChipValue = 5_000 },
            new ItemDef { Id = LuckyCoin, CompPrice = 30 },
            new ItemDef { Id = TrophyLobby },
            new ItemDef { Id = TrophyDice },
            new ItemDef { Id = TrophyArena },
            new ItemDef { Id = TrophyTrack },
            new ItemDef { Id = TrophyAlchemy },
            new ItemDef { Id = TrophyVault },
            new ItemDef { Id = TrophyAquarium },
            new ItemDef { Id = TrophyObservatory },
            new ItemDef { Id = TrophyPostTown },
            new ItemDef { Id = TrophyRooftop },
            new ItemDef { Id = BlackCard },
        };

        public static IReadOnlyList<ItemDef> All => items;

        public static ItemDef Find(string id)
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
