﻿using ImGui.Common.Primitive;

namespace ImGui
{
    public partial class GUI
    {
        /// <summary>
        /// Create a label.
        /// </summary>
        /// <param name="rect">position and size</param>
        /// <param name="text">text to display</param>
        public static void Label(Rect rect, string text)
        {
            GUIContext g = GetCurrentContext();
            Window window = GetCurrentWindow();
            if (window.SkipItems)
                return;

            // style apply
            var s = g.StyleStack;
            var style = s.Style;

            // rect
            window.GetRect(rect);

            // render
            DrawList d = window.DrawList;
            d.DrawBoxModel(rect, text, style);
        }
    }

    public partial class GUILayout
    {
        /// <summary>
        /// Create an auto-layout label.
        /// </summary>
        /// <param name="text">text to display</param>
        public static void Label(string text)
        {
            GUIContext g = GetCurrentContext();
            Window window = GetCurrentWindow();
            if (window.SkipItems)
                return;

            int id = window.GetID(text);

            // style apply
            var s = g.StyleStack;
            var style = s.Style;

            // rect
            Size contentSize = style.CalcSize(text, GUIState.Normal);
            Rect rect = window.GetRect(id, contentSize);

            // rendering
            DrawList d = window.DrawList;
            d.DrawBoxModel(rect, text, style);
        }

        /// <summary>
        /// Create a colored auto-layout label.
        /// </summary>
        /// <param name="color">text color</param>
        /// <param name="text">text</param>
        public static void Label(Color color, string text)
        {
            PushFontColor(color);
            Label(text);
            PopStyleVar();
        }

        /// <summary>
        /// Create a auto-layout and disabled label.
        /// </summary>
        /// <param name="text">text</param>
        public static void LabelDisabled(string text)
        {
            Label(Color.TextDisabled, text);
        }

        public static void Label(string format, object arg0)
        {
            Label(string.Format(format, arg0));
        }

        public static void Label(string format, object arg0, object arg1)
        {
            Label(string.Format(format, arg0, arg1));
        }

        public static void Label(string format, object arg0, object arg1, object arg2)
        {
            Label(string.Format(format, arg0, arg1, arg2));
        }

        public static void Label(string format, params object[] args)
        {
            Label(string.Format(format, args));
        }

        public static void Text(string text) => Label(text);
    }

}
