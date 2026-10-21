using System.Collections.Generic;
using Vector.Core.Rpg;

namespace Vector.Game.Story
{
    /// <summary>ストーリーモードの名前の表示（人・肩書き・フロア・会員ランク・クエスト・アイテム）。</summary>
    public static class StoryNames
    {
        static readonly Dictionary<string, (string name, string title)> people = new Dictionary<string, (string, string)>
        {
            { "lena", ("レナ", "コンシェルジュ") },
            { "mabel", ("メイベル", "観光客") },
            { "gen", ("ゲン", "古株の常連") },
            { "jin", ("ジン", "バーテンダー") },
            { "kai", ("カイ", "ディーラー長") },
            { "pip", ("ピップ", "ダイス少年") },
            { "sora", ("ソラ", "数学者") },
            { "ume", ("ウメ", "カクテルウェイトレス") },
            { "reina", ("レイナ", "ダイスの女王") },
            { "ryo", ("リョウ", "獣の調教師") },
            { "fen", ("フェン", "吟遊詩人") },
            { "nana", ("ナナ", "観客") },
            { "gald", ("ガルド", "無敗の闘技王") },
            { "dash", ("ダッシュ", "予想屋") },
            { "hiro", ("ヒロ", "元騎手") },
            { "momo", ("モモ", "ファンの少女") },
            { "toki", ("トキ", "確率の胴元") },
            { "nico", ("ニコ", "錬金術師見習い") },
            { "vivi", ("ヴィヴィ", "宝石の収集家") },
            { "rue", ("ルー", "機械技師") },
            { "mira", ("ミラ", "錬金術師") },
            { "kit", ("キット", "探検家") },
            { "rook", ("ルーク", "地下の門番") },
            { "ivy", ("アイヴィ", "迷子の冒険者") },
            { "moss", ("モス", "迷宮の番人") },
            { "sebas", ("セバス", "執事") },
            { "zero", ("ゼロ", "オーナー") },
        };

        public static string Person(string id) => people.TryGetValue(id, out var p) ? p.name : id;

        public static string Title(string id) => people.TryGetValue(id, out var p) ? p.title : "";

        public static string Floor(FloorId floor)
        {
            switch (floor)
            {
                case FloorId.Lobby: return "1F GRAND LOBBY";
                case FloorId.DiceParlor: return "2F DICE PARLOR";
                case FloorId.Arena: return "3F ARENA";
                case FloorId.BeastTrack: return "4F BEAST TRACK";
                case FloorId.AlchemyHall: return "5F ALCHEMY HALL";
                case FloorId.Undervault: return "B1 UNDERVAULT";
                default: return "PH PENTHOUSE";
            }
        }

        public static string FloorJapanese(FloorId floor)
        {
            switch (floor)
            {
                case FloorId.Lobby: return "グランドロビー";
                case FloorId.DiceParlor: return "ダイスパーラー";
                case FloorId.Arena: return "アリーナ";
                case FloorId.BeastTrack: return "ビーストトラック";
                case FloorId.AlchemyHall: return "錬金の間";
                case FloorId.Undervault: return "アンダーヴォールト";
                default: return "ペントハウス";
            }
        }

        public static string Rank(VipRank rank) => rank.ToString().ToUpperInvariant();

        static readonly Dictionary<string, (string title, string detail)> quests = new Dictionary<string, (string, string)>
        {
            { "first_win", ("はじめての一勝", "どのテーブルでもいいので、1回勝つ") },
            { "mabel_rounds", ("ロビーの名物", "VECTOR を 5 ラウンド遊ぶ") },
            { "gen_slash", ("斜めの美学", "VECTOR で SLASH を選んで勝つ") },
            { "jin_level", ("常連の証", "レベル 5 になる") },
            { "gen_bonus", ("ボーナスの神様", "VECTOR の BONUS を当てる") },
            { "pip_yacht", ("五つぞろい", "CROWN DICE で YACHT を作る") },
            { "sora_wins", ("期待値の証明", "CROWN DICE で 3 回勝つ") },
            { "ryo_wins", ("獣を知る", "COLOSSEUM で 3 回勝つ") },
            { "fen_critical", ("会心の詩", "COLOSSEUM で K の会心の一撃で獣を倒す") },
            { "dash_exacta", ("馬単の読み", "DERBY で EXACTA（馬単）を当てる") },
            { "hiro_races", ("レースの空気", "DERBY で 5 レース賭ける") },
            { "nico_relic", ("秘宝の錬成", "ALCHEMIA で 秘宝（位4）を作る") },
            { "vivi_stone", ("賢者の石", "ALCHEMIA で 賢者の石（位5）を作る") },
            { "kit_deep", ("深淵からの帰還", "DELVE で B5 以上の深さから CASH OUT する") },
            { "rook_dives", ("地下の常連", "DELVE で 5 回潜る") },
        };

        public static string Quest(string id) => quests.TryGetValue(id, out var q) ? q.title : id;

        public static string QuestDetail(string id) => quests.TryGetValue(id, out var q) ? q.detail : "";

        static readonly Dictionary<string, (string name, string detail)> items = new Dictionary<string, (string, string)>
        {
            { ItemCatalog.VoucherSmall, ("チップ引換券 $500", "キャッシャーで使うと遊戯用チップ $500 になる") },
            { ItemCatalog.VoucherLarge, ("チップ引換券 $5,000", "キャッシャーで使うと遊戯用チップ $5,000 になる") },
            { ItemCatalog.LuckyCoin, ("幸運のコイン", "持っていると気分が良い。確率は変わらない") },
            { ItemCatalog.TrophyLobby, ("ディーラー長のピン", "カイに勝った証。1F を制した") },
            { ItemCatalog.TrophyDice, ("女王の王冠ダイス", "レイナに勝った証。2F を制した") },
            { ItemCatalog.TrophyArena, ("闘技王の盾", "ガルドに勝った証。3F を制した") },
            { ItemCatalog.TrophyTrack, ("胴元の懐中時計", "トキに勝った証。4F を制した") },
            { ItemCatalog.TrophyAlchemy, ("錬金術師の小瓶", "ミラに勝った証。5F を制した") },
            { ItemCatalog.TrophyVault, ("番人の鍵", "モスに勝った証。地下を制した") },
            { ItemCatalog.BlackCard, ("ブラックカード", "オーナーに勝った者だけが持つ、VECTOR の最上級会員証") },
        };

        public static string Item(string id) => items.TryGetValue(id, out var i) ? i.name : id;

        public static string ItemDetail(string id) => items.TryGetValue(id, out var i) ? i.detail : "";
    }
}
