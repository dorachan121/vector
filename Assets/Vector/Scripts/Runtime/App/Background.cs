using System;
using System.Collections;
using System.Threading.Tasks;

namespace Vector.Game
{
    /// <summary>
    /// 重い計算（AIの先読み）をワーカースレッドで走らせ、コルーチンから完了を待つ。
    /// 渡す処理は Unity の API に触れてはいけない（Vector.Core だけを使う）。
    /// </summary>
    public static class Background
    {
        public static IEnumerator Run<T>(Func<T> work, Action<T> done)
        {
            var task = Task.Run(work);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.GetBaseException();
            done(task.Result);
        }
    }
}
