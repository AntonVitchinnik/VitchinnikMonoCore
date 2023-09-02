using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using VitchinnikMonoCore.Content;
using System;

namespace VitchinnikMonoCore.GUI
{
    public abstract class GUIElement : GameObject
    {
        public event Action<GUIElement> Attached;
        public GUIElement() : base()
        {
            DrawOrder = 4;
        }
        public GUIElement(string path, Vector2 position) : base()
        {
            DrawOrder = 4;
            _view = new GUIElementView(this, path, position);
        }
        public GUIElement(string path, Vector2 position, ref Action<Texture2D> textureProvider) : base()
        {
            DrawOrder = 4;
            _view = new GUIElementView(this, path, position, ref textureProvider);
        }
        public GUIElement(string path, Vector2 position, string tooltipText) : this(path, position)
        {
            HoverEntered += () => Tooltip.Actual = new Tooltip(tooltipText);
        }
        public void SetContent(ObjectContent content, Vector2? margin = null)
        {
            (_view as GUIElementView).SetContent(content, margin);
        }
        public void AttachTo(GUIElement parrentElement)
        {
            Attached?.Invoke(parrentElement);
            DrawOrder = parrentElement.DrawOrder + 1;
        }
        protected class GUIElementView : ObjectView, IContentContainer, IContentPositionProvider
        {
            protected Vector2 _relatedPosition;
            protected Vector2 RelatedPosition => _relatedPosition;
            public Vector2 Dimentions => _texture.Bounds.Size.ToVector2();
            public event Action<GameTime> CallContent;
            public event Action<Vector2> PositionProvider;
            public GUIElementView(GUIElement controlsSource, string path) : base(controlsSource, path)
            {
                controlsSource.Attached += (GUIElement element) =>
                {
                    element.ViewPositionChanged += (Vector2 newPosition) =>
                    {
                        _relatedPosition = newPosition;
                        PositionProvider?.Invoke(_relatedPosition + Position);
                    };
                    _relatedPosition = element.Position ?? Vector2.Zero;
                };
            }
            public GUIElementView(GUIElement controlsSource, string path, Vector2 position) : this(controlsSource, path)
            {
                Position = position;
            }
            public GUIElementView(GUIElement controlsSource, string path, ref Action<Texture2D> textureProvider) : base(controlsSource, path, Vector2.Zero, ref textureProvider)
            {
                controlsSource.Attached += (GUIElement element) =>
                {
                    element.ViewPositionChanged += (Vector2 newPosition) =>
                    {
                        _relatedPosition = newPosition;
                        PositionProvider?.Invoke(_relatedPosition);
                    };
                    _relatedPosition = element.Position ?? Vector2.Zero;
                };
            }
            public GUIElementView(GUIElement controlsSource, string path, Vector2 position, ref Action<Texture2D> textureProvider) : this(controlsSource, path, ref textureProvider)
            {
                Position = position;
            }

            public override bool Contains(Vector2 vector)
            {
                return _texture.Bounds.Contains((int)(vector.X - _relatedPosition.X - Position.X - _offset.X), (int)(vector.Y - _relatedPosition.Y - Position.Y - _offset.Y)); ;
            }
            public void SetContent(ObjectContent content, Vector2? margin = null)
            {
                content.SetOwner(this);
                if (margin != null)
                {
                    content.Margin = margin.Value;
                    return;
                }
                content.AllignCentre();
            }
            protected override void Draw(GameTime gameTime)
            {
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.Draw(_texture, Position + _relatedPosition, null, Color.White, 0f, _offset, 1f, 0, 1);
                Core.SpriteBatchInstance.End();
                CallContent?.Invoke(gameTime);
            }

            Vector2 IContentPositionProvider.GetPosition()
            {
                return Position + RelatedPosition;
            }
        }
    }
}
