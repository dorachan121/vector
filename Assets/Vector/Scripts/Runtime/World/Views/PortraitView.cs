using UnityEngine;
using UnityEngine.UI;
using Vector.Game.UI;

namespace Vector.Game.World
{
    /// <summary>
    /// 登場人物の立ち絵（バストアップ）。髪・顔・瞳・まつげ・頬・口元・服と襟を図形で重ねて描く。
    /// 大きさは高さ height で指定し、部品の座標は 520px の原図から比例で決める。
    /// </summary>
    public sealed class PortraitView : MonoBehaviour
    {
        const float Base = 520f;

        /// <summary>角丸の元画像の半径（立ち絵は大きく描くので、ふつうの UI より大きくする）。</summary>
        const int SmoothRadius = 128;

        RectTransform root;
        float scale;

        public static PortraitView Create(Transform parent, string name, CharacterLook look, Vector2 position, float height)
        {
            var rt = Ui.Rect(parent, name, position, new Vector2(height * 0.8f, height));
            var view = rt.gameObject.AddComponent<PortraitView>();
            view.root = rt;
            view.scale = height / Base;
            view.Build(look);
            return view;
        }

        Image Shape(string name, Sprite sprite, Color color, float x, float y, float w, float h, float rotation = 0)
        {
            var img = Ui.Image(root, name, sprite, color, new Vector2(x, y) * scale, new Vector2(w, h) * scale);
            if (rotation != 0)
                img.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);
            return img;
        }

        /// <summary>角丸の四角。大きく描いても縁がぼけないよう、半径の大きい元画像を使う。</summary>
        Image Box(string name, Color color, float x, float y, float w, float h, float radius)
        {
            var img = Ui.Image(root, name, Art.RoundedRect(SmoothRadius), color, new Vector2(x, y) * scale, new Vector2(w, h) * scale);
            img.pixelsPerUnitMultiplier = SmoothRadius / Mathf.Max(1f, radius * scale);
            return img;
        }

        void Build(CharacterLook look)
        {
            var skin = look.Skin;
            var skinShade = Color.Lerp(skin, Theme.Hex("B5654A"), 0.28f);
            var hair = look.Hair;
            var hairLight = Color.Lerp(hair, Color.white, 0.35f);
            var eyes = look.Eyes.a > 0 ? look.Eyes : Color.Lerp(hair, Color.black, 0.45f);
            var ink = Theme.Hex("2A1420");

            if (look.Master)
                Shape("Aura", Art.Glow, Palette.Gold.WithAlpha(0.35f), 0, 40, 560, 560);

            // 後ろ髪
            var hairBack = Color.Lerp(hair, Color.black, 0.18f);
            Shape("HairBack", Art.Circle, hairBack, 0, 112, 300, 300);
            if (look.LongHair)
                Box("HairLong", hairBack, 0, -6, 300, 300, 110);

            // 体と襟
            Box("Body", look.Outfit, 0, -214, 410, 280, 130);
            var shade = Shape("BodyShade", Art.VerticalFade, new Color(0, 0, 0, 0.35f), 0, -250, 410, 210);
            shade.rectTransform.localEulerAngles = new Vector3(0, 0, 180);
            Shape("Shirt", PortraitArt.Triangle, Palette.Ivory, 0, -150, 116, 140, 180);
            var lapel = Color.Lerp(look.Outfit, Color.black, 0.28f);
            Shape("LapelL", PortraitArt.Triangle, lapel, -46, -160, 54, 150, 196);
            Shape("LapelR", PortraitArt.Triangle, lapel, 46, -160, 54, 150, 164);
            Box("Neck", skinShade, 0, -62, 74, 96, 30);
            Shape("NeckShade", Art.Glow, new Color(0.45f, 0.2f, 0.15f, 0.35f), 0, -34, 120, 40);

            // 顔
            Shape("EarL", Art.Circle, skinShade, -116, 62, 50, 58);
            Shape("EarR", Art.Circle, skinShade, 116, 62, 50, 58);
            Shape("Face", Art.Circle, skin, 0, 72, 236, 250);
            Shape("Jaw", Art.Circle, skin, 0, 28, 196, 170);

            // 目（白目・瞳・瞳孔・ハイライト・まつげ）
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 52;
                string s = side < 0 ? "L" : "R";
                Shape("Sclera" + s, Art.Circle, Palette.Ivory, x, 62, 58, 66);
                Shape("Iris" + s, Art.Circle, eyes, x, 58, 46, 58);
                Shape("IrisLight" + s, Art.Circle, Color.Lerp(eyes, Color.white, 0.35f), x, 46, 30, 24);
                Shape("Pupil" + s, Art.Circle, Color.Lerp(eyes, Color.black, 0.6f), x, 62, 22, 30);
                Shape("Glint" + s, Art.Circle, Color.white, x - 10, 74, 16, 18);
                Shape("Glint2" + s, Art.Circle, Color.white.WithAlpha(0.85f), x + 10, 48, 8, 8);
                var lash = Shape("Lash" + s, PortraitArt.Lash, ink, x + side * 2, 88, 76, 34);
                if (side > 0)
                    lash.rectTransform.localScale = new Vector3(-1, 1, 1);
                Shape("Blush" + s, Art.Glow, Theme.Hex("FF7A8A").WithAlpha(0.5f), side * 76, 22, 70, 34);
            }
            Shape("Nose", Art.Circle, skinShade, 0, 34, 9, 6);
            Shape("Mouth", PortraitArt.Smile, Theme.Hex("B23A48"), 0, 6, 36, 18);

            // 前髪と横髪
            float lockLength = look.LongHair ? 250 : 130;
            Box("LockL", hair, -114, 100 - lockLength / 2, 46, lockLength, 22);
            Box("LockR", hair, 114, 100 - lockLength / 2, 46, lockLength, 22);
            Shape("BangShadow", Art.Glow, new Color(0.45f, 0.2f, 0.2f, 0.16f), 0, 120, 230, 70);
            Shape("Bangs", PortraitArt.Bangs, hair, 0, 150, 292, 224);
            Shape("Shine", Art.Glow, hairLight.WithAlpha(0.6f), -40, 206, 130, 34, -12);

            // 胸元の飾りと冠
            Shape("BowL", PortraitArt.Triangle, look.Accent, -20, -112, 40, 34, -90);
            Shape("BowR", PortraitArt.Triangle, look.Accent, 20, -112, 40, 34, 90);
            Shape("Knot", Art.Circle, Color.Lerp(look.Accent, Color.white, 0.25f), 0, -112, 18, 18);
            if (look.Crown)
                Shape("Crown", Icons.Of(IconKind.Crown), Palette.Gold, 0, 238, 120, 96);
        }
    }
}
