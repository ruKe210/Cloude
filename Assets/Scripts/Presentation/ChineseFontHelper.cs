using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STS.Presentation
{
    /// <summary>
    /// 提供支持中文的 Font。优先加载 Resources/Fonts/msyh，回退到系统字体。
    /// Legacy UI.Text / TextMesh 对中文支持比运行时生成 TMP 更稳定。
    /// </summary>
    public static class ChineseFontHelper
    {
        private static readonly string[] OsFontNames =
        {
            "Microsoft YaHei",
            "Microsoft YaHei UI",
            "SimHei",
            "PingFang SC",
            "Noto Sans CJK SC"
        };

        private static Font _uiFont;
        private static TMP_FontAsset _tmpFont;

        public static Font UiFont
        {
            get
            {
                if (_uiFont == null)
                {
                    _uiFont = Resources.Load<Font>("Fonts/msyh");
                    if (_uiFont == null)
                    {
                        _uiFont = Font.CreateDynamicFontFromOSFont(OsFontNames, 32);
                    }

                    if (_uiFont == null)
                    {
                        Debug.LogError("[ChineseFontHelper] 无法加载中文字体，请确认 Assets/Resources/Fonts/msyh.ttc 存在。");
                    }
                }

                return _uiFont;
            }
        }

        public static void ApplyTo(Text text)
        {
            if (text == null || UiFont == null)
            {
                return;
            }

            text.font = UiFont;
        }

        public static void ApplyTo(TextMesh textMesh)
        {
            if (textMesh == null || UiFont == null)
            {
                return;
            }

            textMesh.font = UiFont;
        }

        public static void ApplyTo(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            if (_tmpFont == null && UiFont != null)
            {
                _tmpFont = TMP_FontAsset.CreateFontAsset(UiFont);
            }

            if (_tmpFont != null)
            {
                text.font = _tmpFont;
            }
        }
    }
}
