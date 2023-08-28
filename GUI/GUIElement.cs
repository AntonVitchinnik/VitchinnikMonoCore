using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using VitchinnikMonoCore.Content;

namespace VitchinnikMonoCore.GUI
{
    public abstract class GUIElement : GameObject
    {
        public event Action<GUIElement> Attached;
        public GUIElement() : base()
        {

        }
        public GUIElement(string path, Vector2 position) : base()
        {
            _view = new GUIElementView(this, path, position);
        }
        public GUIElement(string path, Vector2 position, string tooltipText) : this(path, position)
        {
            HoverEntered += () => Tooltip.Actual = new Tooltip(tooltipText);
        }
        public void AttachTo(GUIElement parrentElement)
        {
            Attached?.Invoke(parrentElement);
        }
        protected class GUIElementView : ObjectView, IContentContainer
        {
            protected Vector2 _relatedPosition;
            protected Vector2 RelatedPosition => _relatedPosition;
            public Vector2 Dimentions => _texture.Bounds.Size.ToVector2();
            public event Action<GameTime> CallContent;
            public GUIElementView(GUIElement controlsSource, string path) : base(controlsSource, path)
            {
                controlsSource.Attached += (GUIElement element) =>
                {
                    element.ViewPositionChanged += (Vector2 newPosition) => Position = newPosition;
                    _relatedPosition = element.Position ?? Vector2.Zero;
                };
            }
            public GUIElementView(GUIElement controlsSource, string path, Vector2 position) : this(controlsSource, path)
            {
                Position = position;
            }
            public override bool Contains(Vector2 vector)
            {
                return _texture.Bounds.Contains((int)(vector.X - _relatedPosition.X - Position.X - _offset.X), (int)(vector.Y - _relatedPosition.Y - Position.Y - _offset.Y)); ;
            }
            public void SetContent(ObjectContent content)
            {
                content.SetOwner(this);
                content.AllignCentre();
            }
            protected override void Draw(GameTime gameTime)
            {
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.Draw(_texture, Position + _relatedPosition, null, Color.White, 0f, _offset, 1f, 0, 1);
                Core.SpriteBatchInstance.End();
                CallContent?.Invoke(gameTime);
            }
        }
    }
}
