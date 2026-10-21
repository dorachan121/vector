using UnityEngine;
using Vector.Core;
using Vector.Game.Casino;
using Vector.Game.UI;

namespace Vector.Game.Table
{
    /// <summary>VECTOR の配当表（共通の PaytablePanel に、モードごとのメインの配当とボーナスの役を並べる）。</summary>
    public static class VectorPaytable
    {
        public static PaytablePanel Create(Transform parent, Vector2 position, Vector2 size, GameMode mode, TableRules rules)
        {
            var panel = PaytablePanel.Create(parent, position, size);
            panel.Heading("PAYTABLE");
            if (mode == GameMode.Player)
            {
                panel.Row(Key(BetSpot.Main), "ROW / COLUMN / SLASH", rules.MainPays.ToString());
                panel.Row("push", "TIE", "PUSH");
                panel.Note("ディーラーは残り2種類の高い方で勝負");
            }
            else
            {
                panel.Row(Key(BetSpot.Player), "PLAYER", rules.BackPlayerPays.ToString());
                panel.Row(Key(BetSpot.Dealer), "DEALER", rules.BackDealerPays.ToString());
                panel.Row(Key(BetSpot.Tie), "TIE", rules.BackTiePays.ToString());
                panel.Note("PLAYER / DEALER は引き分けで返金");
            }
            panel.Gap();
            panel.Heading("BONUS  (VECTOR + HAND)");
            foreach (var pair in rules.BonusPays)
                panel.Row(Key(pair.Key), Labels.HandName(pair.Key), pair.Value.ToString());
            return panel;
        }

        /// <summary>ベットエリアの行（勝ったときに光らせる）。</summary>
        public static string Key(BetSpot spot) => "spot." + spot;

        /// <summary>ボーナスの役の行。</summary>
        public static string Key(PokerHand hand) => "bonus." + hand;
    }
}
