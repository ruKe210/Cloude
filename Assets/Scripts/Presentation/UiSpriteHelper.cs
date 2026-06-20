using UnityEngine;
using UnityEngine.UI;

namespace STS.Presentation
{
    public static class UiSpriteHelper
    {
        private static Sprite _whiteSprite;

        public static Sprite WhiteSprite
        {
            get
            {
                if (_whiteSprite != null)
                {
                    return _whiteSprite;
                }

                _whiteSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                if (_whiteSprite == null)
                {
                    _whiteSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
                }

                if (_whiteSprite == null)
                {
                    _whiteSprite = CreateFallbackSprite();
                }

                return _whiteSprite;
            }
        }

        public static void ApplyPanelImage(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = WhiteSprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = true;
            image.useSpriteMesh = false;
        }

        private static Sprite CreateFallbackSprite()
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
