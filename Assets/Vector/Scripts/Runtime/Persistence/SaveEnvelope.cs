using System;

namespace Vector.Game.Persistence
{
    /// <summary>セーブファイルの外箱。本体の JSON と、その改ざん検知用の署名を持つ。</summary>
    [Serializable]
    sealed class SaveEnvelope
    {
        public int format;
        public string payload;
        public string mac;
    }
}
