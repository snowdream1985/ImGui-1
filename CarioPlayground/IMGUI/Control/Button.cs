﻿//#define INSPECT_STATE
using System.Diagnostics;
using Cairo;
using TinyIoC;

namespace IMGUI
{
    internal class Button : Control
    {
        #region State machine define
        static class ButtonState
        {
            public const string Normal = "Normal";
            public const string Hover = "Hover";
            public const string Active = "Active";
        }

        static class ButtonCommand
        {
            public const string MoveIn = "MoveIn";
            public const string MoveOut = "MoveOut";
            public const string MousePress = "MousePress";
            public const string MouseRelease = "MouseRelease";
        }

        static readonly string[] states =
        {
            ButtonState.Normal, ButtonCommand.MoveIn, ButtonState.Hover,
            ButtonState.Hover, ButtonCommand.MoveOut, ButtonState.Normal,
            ButtonState.Hover, ButtonCommand.MousePress, ButtonState.Active,
            ButtonState.Active, ButtonCommand.MoveOut, ButtonState.Normal,
            ButtonState.Active, ButtonCommand.MouseRelease, ButtonState.Hover
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
        public Rect Rect { get; private set; }
        public bool Result { get; private set; }
        private ToolTip t;
        public override void OnUpdate()
        {
            Layout.MaxWidth = (int)Rect.Width;
            Layout.MaxHeight = (int)Rect.Height;
            Layout.Text = Text;
            
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
            if (Input.Mouse.stateMachine.CurrentState == Input.Mouse.MouseState.Pressed)
            {
                if(stateMachine.MoveNext(ButtonCommand.MousePress))
                {
                    Input.Mouse.stateMachine.MoveNext(Input.Mouse.MouseCommand.Fetch);
                }
            }
            if (Input.Mouse.stateMachine.CurrentState == Input.Mouse.MouseState.Released)
            {
                if(stateMachine.MoveNext(ButtonCommand.MouseRelease))
                {
                    Input.Mouse.stateMachine.MoveNext(Input.Mouse.MouseCommand.Fetch);
                }
            }
#if INSPECT_STATE
            var B = stateMachine.CurrentState;
            Debug.WriteLineIf(A != B, string.Format("Button{0} {1}=>{2}", Name, A, B));
#endif

            var oldState = State;
            bool active = stateMachine.CurrentState == ButtonState.Active;
            bool hover = stateMachine.CurrentState == ButtonState.Hover;
            if (active)
            {
                State = "Active";
            }
            else if (hover)
            {
                State = "Hover";
                //if(t ==null)
                //{
                //    t = new ToolTip();
                //    Application.Forms.Add(t);
                //}
                //t.TipText = Text;
                //t.Show();
            }
            else
            {
                State = "Normal";
            }

            if (State != oldState)
            {
                NeedRepaint = true;
            }
            bool clicked = oldState == "Active" && State == "Hover";
            Result = clicked;
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

        internal Button(string name, BaseForm form, string text, Rect rect)
            : base(name, form)
        {
            stateMachine = new StateMachine(ButtonState.Normal, states);
            Rect = rect;
            Text = text;

            var font = Skin.current.Button[State].Font;
            Format = Application.IocContainer.Resolve<ITextFormat>(
                new NamedParameterOverloads
                    {
                        {"fontFamilyName", font.FontFamily},
                        {"fontWeight", font.FontWeight},
                        {"fontStyle", font.FontStyle},
                        {"fontStretch", font.FontStretch},
                        {"fontSize", (float) font.Size}
                    });
            var textStyle = Skin.current.Button[State].TextStyle;
            Format.Alignment = textStyle.TextAlignment;
            Layout = Application.IocContainer.Resolve<ITextLayout>(
                new NamedParameterOverloads
                    {
                        {"text", Text},
                        {"textFormat", Format},
                        {"maxWidth", (int)Rect.Width},
                        {"maxHeight", (int)Rect.Height}
                    });

        }

        //TODO Control-less DoControl overload (without name parameter)
        internal static bool DoControl(Context g, BaseForm form, Rect rect, string text, string name)
        {
            //The control hasn't been created, create it.
            if (!form.Controls.ContainsKey(name))
            {
                var button = new Button(name, form, text, rect);
                button.OnUpdate();
                button.OnRender(g);
            }

            var control = form.Controls[name] as Button;
            Debug.Assert(control != null);

            //Debug.WriteLine(control.State);

            return control.Result;
        }
    }
}