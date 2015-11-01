﻿using System;
using System.Collections;
using System.Collections.Generic;

//TODO decouple Input from windows => write a stand-alone and cross-platform Input library
namespace IMGUI.Input
{
    /// <summary>
    /// input
    /// </summary>
    public static class Mouse
    {
        /// <summary>
        /// Double click interval time span
        /// </summary>
        /// <remarks>
        /// if the interval between two mouse click is longer than this value,
        /// the two clicking action is not considered as a double-click action.
        /// </remarks>
        internal const float DoubleClickIntervalTimeSpan = 0.2f;

        #region Left button
        /// <summary>
        /// Last recorded left mouse button state
        /// </summary>
        /// <remarks>for detecting left mouse button state' changes</remarks>
        private static InputState lastLeftButtonState = InputState.Up;

        /// <summary>
        /// Left button state
        /// </summary>
        private static InputState leftButtonState = InputState.Up;

        /// <summary>
        /// Last recorded left mouse button state
        /// </summary>
        /// <remarks>for detecting left mouse button state' changes</remarks>
        public static InputState LastLeftButtonState
        {
            get { return lastLeftButtonState; }
        }

        /// <summary>
        /// Button state of left mouse button(readonly)
        /// </summary>
        public static InputState LeftButtonState
        {
            get { return leftButtonState; }
        }

        private static bool leftButtonClicked = false;
        /// <summary>
        /// Is left mouse button clicked?(readonly)
        /// </summary>
        public static bool LeftButtonClicked
        {
            get { return leftButtonClicked; }
            private set { leftButtonClicked = value; }
        }

        #endregion

        #region Right button
        /// <summary>
        /// Last recorded right mouse button state
        /// </summary>
        /// <remarks>for detecting right mouse button state' changes</remarks>
        static InputState lastRightButtonState = InputState.Up;

        /// <summary>
        /// Right button state
        /// </summary>
        static InputState rightButtonState = InputState.Up;

        /// <summary>
        /// Button state of the right mouse button(readonly)
        /// </summary>
        public static InputState RightButtonState
        {
            get { return rightButtonState; }
        }

        /// <summary>
        /// Check if the right mouse button is clicked(readonly)
        /// </summary>
        public static bool RightButtonClicked
        {
            get
            {
                return lastRightButtonState == InputState.Down
                    && rightButtonState == InputState.Up;
            }
        }
        #endregion

        #region Position
        /// <summary>
        /// Mouse position
        /// </summary>
        static Point lastMousePos;

        /// <summary>
        /// Mouse position
        /// </summary>
        static Point mousePos;

        static Point mousePosInScreen;

        /// <summary>
        /// Mouse position
        /// </summary>
        public static Point LastMousePos
        {
            get { return lastMousePos; }
        }

        /// <summary>
        /// Mouse position (readonly)
        /// </summary>
        public static Point MousePos
        {
            get { return mousePos; }
        }

        public static Point MousePosInScreen
        {
            get { return mousePosInScreen; }
        }

        public static bool MouseMoving
        {
            get { return mousePos != lastMousePos; }
        }

        #endregion

        #region Drag

        private static IEnumerator<bool> ClickChecker
        {
            get { return clickChecker; }
            set { clickChecker = value; }
        }

        public static bool MouseDraging { get; private set; }

        public static IEnumerator<bool> DragChecker
        {
            get { return dragChecker; }
        }

        #endregion

        /// <summary>
        /// Refresh mouse states
        /// </summary>
        /// <param name="form">Reference window</param>
        /// <returns>true: successful; false: failed</returns>
        /// <remarks>The mouse states will persist until next call of this method, 
        /// and last states will be recorded.</remarks>
        public static bool Refresh(BaseForm form)
        {
            //Get internal SFML window
            var window = form.InternalForm as SFML.Window.Window;
            if(window == null)
            {
                throw new InvalidCastException("Internal Form is not SFML.Window.Window");
            }

            //Buttons's states
            lastLeftButtonState = leftButtonState;
            leftButtonState = SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left) ? InputState.Down : InputState.Up;
            lastRightButtonState = rightButtonState;
            rightButtonState = SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Right) ? InputState.Down : InputState.Up;
            //Debug.WriteLine("Mouse Left {0}, Right {1}", leftButtonState.ToString(), rightButtonState.ToString());
            //Position
            lastMousePos = mousePos;
            var pos = SFML.Window.Mouse.GetPosition(window);
            mousePos = new Point(pos.X, pos.Y);
            window.SetTitle(string.Format("{0},{1}", pos.X, pos.Y));
            //Now mousePos is the position in the client area

            var posInSceen = SFML.Window.Mouse.GetPosition();
            mousePosInScreen = new Point(posInSceen.X, posInSceen.Y);

            ClickChecker.MoveNext();
            LeftButtonClicked = ClickChecker.Current;

            DragChecker.MoveNext();
            MouseDraging = DragChecker.Current;

            return true;
        }

        private static IEnumerator<bool> clickChecker = ClickStateMachine.Instance.GetEnumerator();
        class ClickStateMachine : IEnumerable<bool>
        {
            enum ClickState
            {
                One,
                Two,
                Three
            }

            private static ClickStateMachine instance;
            public static ClickStateMachine Instance
            {
                get
                {
                    if(instance == null)
                        instance = new ClickStateMachine();
                    return instance;
                }
            }

            private ClickState state;

            public IEnumerator<bool> GetEnumerator()
            {
                while (true)
                {
                    switch(state)
                    {
                        case ClickState.One:
                            if(LastLeftButtonState == InputState.Up && LeftButtonState == InputState.Down)
                            {
                                state = ClickState.Two;
                            }
                            yield return false;
                            break;
                        case ClickState.Two:
                            if(MouseMoving)
                            {
                                state = ClickState.One;
                                yield return false;
                            }
                            if(LeftButtonState == InputState.Up)
                            {
                                state = ClickState.One;
                                yield return false;
                            }
                            state = ClickState.Three;
                            yield return true;
                            break;
                        case ClickState.Three:
                            state = ClickState.One;
                            yield return false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private static IEnumerator<bool> dragChecker = DragStateMachine.Instance.GetEnumerator();

        class DragStateMachine : IEnumerable<bool>
        {
            enum DragState
            {
                One,
                Two,
                Three
            }

            private static DragStateMachine instance;
            public static DragStateMachine Instance
            {
                get
                {
                    if(instance == null)
                        instance = new DragStateMachine();
                    return instance;
                }
            }

            private DragState state;

            public IEnumerator<bool> GetEnumerator()
            {
                while(true)
                {
                    switch(state)
                    {
                        case DragState.One:
                            if(LastLeftButtonState == InputState.Up && LeftButtonState == InputState.Down)
                            {
                                state = DragState.Two;
                            }
                            yield return false;
                            break;
                        case DragState.Two:
                            if(LeftButtonState == InputState.Up)
                            {
                                yield return false;
                            }
                            if(!MouseMoving)
                            {
                                yield return false;
                            }
                            state = DragState.Three;
                            yield return true;
                            break;
                        case DragState.Three:
                            if(LeftButtonState == InputState.Up)
                            {
                                state = DragState.One;
                                yield return false;
                            }
                            else
                            {
                                yield return true;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
