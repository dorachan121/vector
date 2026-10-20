using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Vector.Game.Persistence
{
    /// <summary>
    /// プレイヤーデータの直列化。本体の JSON に HMAC-SHA256 の署名を付け、読み込み時に検証する。
    /// <para>
    /// 端末内のセーブなので、鍵を解析されれば偽造は防げない。目的はファイル破損と手軽な書き換えの検知であり、
    /// 換金性のある残高をクライアントだけで守る設計にはしないこと（オンライン化する場合はサーバー側で管理する）。
    /// </para>
    /// </summary>
    public static class ProfileCodec
    {
        const int Format = 1;
        static readonly byte[] key = Encoding.UTF8.GetBytes("VECTOR/casino-table/profile/v1");

        public static string Encode(PlayerProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            string json = JsonUtility.ToJson(profile);
            return JsonUtility.ToJson(new SaveEnvelope { format = Format, payload = json, mac = Sign(json) });
        }

        public static bool TryDecode(string text, out PlayerProfile profile)
        {
            profile = null;
            if (string.IsNullOrEmpty(text))
                return false;
            try
            {
                var envelope = JsonUtility.FromJson<SaveEnvelope>(text);
                if (envelope == null || envelope.format != Format || envelope.payload == null || envelope.mac == null)
                    return false;
                if (!FixedTimeEquals(envelope.mac, Sign(envelope.payload)))
                    return false;
                profile = JsonUtility.FromJson<PlayerProfile>(envelope.payload);
            }
            catch (ArgumentException)
            {
                // JsonUtility は不正な JSON に ArgumentException を投げる
                return false;
            }
            if (profile == null)
                return false;
            profile.Sanitize();
            return true;
        }

        static string Sign(string payload)
        {
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>比較にかかる時間から一致した長さを推測されないよう、常に全文字を比べる。</summary>
        static bool FixedTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
