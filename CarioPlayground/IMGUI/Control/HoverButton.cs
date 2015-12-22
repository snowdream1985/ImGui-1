﻿//#define INSPECT_STATE
using Cairo;
using System.Diagnostics;

namespace ImGui
{
    class HoverButton : Control
    {
        #region State machine define
        static class ButtonState
        {
            public const string Normal = "Normal";
            public const string Active = "Active";
        }

        static class ButtonCommand
        {
            public const string MoveIn = "MoveIn";
            public const string MoveOut = "MoveOut";
        }

        static readonly string[] states =
        {
            ButtonState.Normal, ButtonCommand.MoveIn, ButtonState.Active,
            ButtonState.Active, ButtonCommand.MoveOut, ButtonState.Normal,
        };
        #endregion

        private readonly StateMachine stateMachine;

        public ITextFormat Format { get; private set; }
        public ITextLayout Layout { get; private set; }

        private string text;
        public string Text
        {
            get { return text; }
            private set
            {
                if (Text == value)
                {
                    return;
                }

                text = value;
                NeedRepaint = true;
            }
        }
        public bool Result { get; private set; }
        public override void OnUpdate()
        {
#if INSPECT_STATE
            var A = stateMachine.CurrentState;
#endif
            //Execute state commands
            if (!Rect.Contains(Utility.ScreenToClient(Input.Mouse.LastMousePos, Form)) && Rect.Contains(Utility.ScreenToClient(Input.Mouse.MousePos, Form)))
            {
                stateMachine.MoveNext(ButtonCommand.MoveIn);
            }
            if (Rect.Contains(Utility.ScreenToClient(Input.Mouse.LastMousePos, Form)) && !Rect.Contains(Utility.ScreenToClient(Input.Mouse.MousePos, Form)))
            {
                stateMachine.MoveNext(ButtonCommand.MoveOut);
            }
#if INSPECT_STATE
            var B = stateMachine.CurrentState;
            Debug.WriteLineIf(A != B, string.Format("Button{0} {1}=>{2}", Name, A, B));
#endif

            var oldState = State;
            bool active = stateMachine.CurrentState == ButtonState.Active;
            if (active)
            {
                State = "Active";
            }
            else
            {
                State = "Normal";
            }

            if (State != oldState)
            {
                NeedRepaint = true;
            }
            Result = active;
        }

        public override void OnRender(Context g)
        {
            g.DrawBoxModel(Rect, new Content(Layout), Skin.current.Button[State]);
        }

        public override void Dispose()
        {
            Layout.Dispose();
            Format.Dispose();
        }

        public override void OnClear(Context g)
        {
            g.FillRectangle(Rect, CairoEx.ColorWhite);
        }

        internal HoverButton(string name, BaseForm form, string text, Rect rect)
            : base(name, form)
        {
            stateMachine = new StateMachine(ButtonState.Normal, states);
            Rect = rect;
            Text = text;

            var style = Skin.current.Button[State];
            var font = style.Font;
            Format = Application._map.CreateTextFormat(
                font.FontFamily,
                font.FontWeight,
                font.FontStyle,
                font.FontStretch,
                font.Size);

            var textStyle = style.TextStyle;
            Format.Alignment = textStyle.TextAlignment;
            var contentRect = Utility.GetContentRect(Rect, style);
            Layout = Application._map.CreateTextLayout(
                Text, Format,
                (int) contentRect.Width,
                (int) contentRect.Height);
        }

        internal static bool DoControl(BaseForm form, Rect rect, string text, string name)
        {
            //The control hasn't been created, create it.
            if (!form.Controls.ContainsKey(name))
            {
                var hoverButton = new HoverButton(name, form, text, rect);
            }

            var control = form.Controls[name] as HoverButton;
            Debug.Assert(control != null);
            control.Active = true;

            return control.Result;
        }
    }
}