using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using VitchinnikMonoCore.Content;
using System;

namespace VitchinnikMonoCore.GUI
{
    public class TextBox : GUIElement
    {
        public event Action<string> TextChanged;
        public string Text { get => (_model as TextBoxModel).Text; set => (_model as TextBoxModel).Text = value; }
        public TextBox(string path, Vector2 position, SpriteFont font) : base(path, position)
        {
            _model = new TextBoxModel(this);
            PressEvent += Toggle;
            var content = new ObjectContent(ref TextChanged, font);
            (_view as GUIElementView).SetContent(content);
            TextChanged += (string text) => content.AllignCentre();
            (_model as TextBoxModel).TextChanged += TextChanged.Invoke;
        }
        protected class TextBoxModel : ObjectModel
        {
            private KeyboardListener _writer;
            private string _text;
            private int _charIndex;
            private Action _callDisable;
            public event Action<string> TextChanged;
            public string Text
            {
                get => _text;
                set
                {
                    if (Enabled)
                        _callDisable.Invoke();
                    _text = value;
                    TextChanged.Invoke(_text);
                    _charIndex = value.Length;
                }
            }
            public TextBoxModel(TextBox controlsSource) : base(controlsSource)
            {
                _writer = new KeyboardListener();
                _charIndex = 0;
                _text = "";
                _callDisable = controlsSource.Disable;
            }
            protected override void OnEnable()
            {
                _writer.KeyTyped += WriteKey;
                _writer.KeyPressed += OnKeyPressed;
                _text = _text.Insert(_charIndex, "X");
                _charIndex += 1;
                TextChanged.Invoke(_text);
            }
            protected override void OnUpdate(GameTime gameTime)
            {
                _writer.Update(gameTime);
            }
            protected override void OnDisable()
            {
                _writer.KeyTyped -= WriteKey;
                _writer.KeyPressed -= OnKeyPressed;
                _text = _text.Remove(_charIndex - 1, 1);
                _charIndex -= 1;
                TextChanged.Invoke(_text);
            }
            private void OnKeyPressed(object sender, KeyboardEventArgs e)
            {
                if (e.Key.Equals(Keys.Left))
                {
                    MovePointer(-1);
                }
                if (e.Key.Equals(Keys.Right))
                {
                    MovePointer(1);
                }
                if (e.Key.Equals(Keys.Delete))
                {
                    if (_charIndex > Text.Length - 1)
                        return;
                    _text = _text.Remove(_charIndex, 1);
                }
                TextChanged.Invoke(_text);
            }
            private void MovePointer(int deltaPointer)
            {
                _text = _text.Remove(_charIndex - 1, 1);
                _charIndex += deltaPointer;
                _charIndex = Math.Clamp(_charIndex, 1, Text.Length + 1);
                _text = _text.Insert(_charIndex - 1, "X");
            }
            public void WriteKey(object sender, KeyboardEventArgs keyboardArg)
            {
                string symbol = keyboardArg.Character.ToString();
                if (symbol.Equals("\b"))
                {
                    if (_charIndex == 1)
                        return;
                    _text = _text.Remove(_charIndex - 2, 1);
                    _charIndex -= 1;
                    _charIndex = Math.Clamp(_charIndex, 1, Text.Length);
                    TextChanged.Invoke(_text);
                    return;
                }
                _text = _text.Insert(_charIndex - 1, symbol);
                _charIndex += 1;
                TextChanged.Invoke(_text);
            }
        }
    }
}
