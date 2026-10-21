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
            { "coral", ("コーラル", "水族館の飼育員") },
            { "finn", ("フィン", "漁師の少年") },
            { "pearl", ("パール", "真珠採り") },
            { "nemo", ("ネモ", "深海の船長") },
            { "tomo", ("トモ", "飛脚") },
            { "hana", ("ハナ", "茶屋の看板娘") },
            { "genji", ("ゲンジ", "旅の浪人") },
            { "yuki", ("ユキ", "宿場の女将") },
            { "taro", ("タロウ", "屋台の大将") },
            { "mio", ("ミオ", "浴衣の少女") },
            { "goro", ("ゴロウ", "打ち上げの職人") },
            { "kaede", ("カエデ", "花火師") },
            { "luna", ("ルナ", "天文台の案内係") },
            { "orion", ("オリオン", "占星術師") },
            { "tess", ("テス", "彗星ハンター") },
            { "stella", ("ステラ", "星詠みの台長") },
            { "sebas", ("セバス", "執事") },
            { "zero", ("ゼロ", "オーナー") },
        };

        public static string Person(string id) => people.TryGetValue(id, out var p) ? p.name : id;

        public static string Title(string id) => people.TryGetValue(id, out var p) ? p.title : "";

        public static string Floor(FloorId floor) => $"{FloorLevel(floor)} {FloorTitle(floor)}";

        /// <summary>階の表示（1F・B1・PH）。</summary>
        public static string FloorLevel(FloorId floor)
        {
            switch (floor)
            {
                case FloorId.Lobby: return "1F";
                case FloorId.DiceParlor: return "2F";
                case FloorId.Arena: return "3F";
                case FloorId.BeastTrack: return "4F";
                case FloorId.AlchemyHall: return "5F";
                case FloorId.Undervault: return "B1";
                case FloorId.Observatory: return "6F";
                case FloorId.Aquarium: return "B2";
                case FloorId.PostTown: return "7F";
                case FloorId.Rooftop: return "8F";
                default: return "PH";
            }
        }

        public static string FloorTitle(FloorId floor)
        {
            switch (floor)
            {
                case FloorId.Lobby: return "GRAND LOBBY";
                case FloorId.DiceParlor: return "DICE PARLOR";
                case FloorId.Arena: return "ARENA";
                case FloorId.BeastTrack: return "BEAST TRACK";
                case FloorId.AlchemyHall: return "ALCHEMY HALL";
                case FloorId.Undervault: return "UNDERVAULT";
                case FloorId.Observatory: return "OBSERVATORY";
                case FloorId.Aquarium: return "AQUARIUM";
                case FloorId.PostTown: return "POST TOWN";
                case FloorId.Rooftop: return "ROOFTOP";
                default: return "PENTHOUSE";
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
                case FloorId.Observatory: return "天文台";
                case FloorId.Aquarium: return "深海水族館";
                case FloorId.PostTown: return "宿場町";
                case FloorId.Rooftop: return "屋上の花火大会";
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
            { "coral_shots", ("はじめての狩り", "HARPOON で 10 発撃つ") },
            { "finn_big_catch", ("大物狙い", "HARPOON で 50倍以上の獲物を仕留める") },
            { "pearl_jelly", ("光る獲物", "HARPOON で 金のクラゲ を捕まえる") },
            { "tomo_journeys", ("旅の始まり", "SUGOROKU で 3 回旅をする") },
            { "hana_treasure", ("茶屋の宝探し", "SUGOROKU で 宝 のマスに止まる") },
            { "genji_high", ("錦を飾る", "SUGOROKU で 段 5（x2.6）以上で上がる") },
            { "mio_bloom", ("はじめての開花", "HANABI で x2.00 以上で開く") },
            { "goro_grand", ("尺玉", "HANABI で x10.00 以上で開く") },
            { "taro_close", ("ぎりぎりの見極め", "HANABI で 暴発点の 9 割以上まで粘って開く（x1.50 以上）") },
            { "luna_spins", ("星空の案内", "ORBIT を 5 回まわす") },
            { "orion_conjunction", ("合の光線", "ORBIT で 内側の合 か 外側の合 を当てる") },
            { "tess_comet", ("彗星を追って", "ORBIT で 彗星 を当てる") },
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
            { ItemCatalog.TrophyAquarium, ("船長の海図", "ネモに勝った証。深海を制した") },
            { ItemCatalog.TrophyObservatory, ("星図の羅針盤", "ステラに勝った証。6F を制した") },
            { ItemCatalog.TrophyPostTown, ("女将の通行手形", "ユキに勝った証。7F を制した") },
            { ItemCatalog.TrophyRooftop, ("花火師の法被", "カエデに勝った証。屋上を制した") },
            { ItemCatalog.BlackCard, ("ブラックカード", "オーナーに勝った者だけが持つ、VECTOR の最上級会員証") },
        };

        public static string Item(string id) => items.TryGetValue(id, out var i) ? i.name : id;

        public static string ItemDetail(string id) => items.TryGetValue(id, out var i) ? i.detail : "";
    }
}
