using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using VitchinnikMonoCore.GUI;
using System;

namespace VitchinnikMonoCore.Content
{
    public class ObjectContent
    {
        private IContentContainer _owner;
        public string Text { get; private set; }
        public SpriteFont Font { get; private set; }
        public Texture2D Image { get; private set; }
        private Action _drawAction;
        private Action _centreAction;
        private Action _centreOffset;
        public Vector2 Position { get; private set; }
        private Vector2 _offset;
        private Vector2 _margin;
        public Vector2 Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                _offset = Vector2.Zero;
                LastAlignment = null;
            }
        }
        private Action LastAlignment;
        public Action<GameTime> DrawAction => Draw;
        public ObjectContent(string text, SpriteFont font)
        {
            this.Text = text;
            _centreOffset = () => _offset = Font.MeasureString(Text) / 2;
            this.Font = font;
            _drawAction += () =>
            {
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.DrawString(Font, Text, Position + Margin, Color.White, 0f, _offset, 1f, 0, 1);
                Core.SpriteBatchInstance.End();
            };
        }
        public ObjectContent(Texture2D image)
        {
            this.Image = image;
            _centreOffset += () => _offset = Image.Bounds.Size.ToVector2() / 2f;
            _drawAction += () =>
            {
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.Draw(Image, Position + Margin, null, Color.White, 0f, _offset, 1f, 0, 1);
                Core.SpriteBatchInstance.End();
            };
        }
        public ObjectContent(string path)
        {
            _centreOffset += () => _offset = Image.Bounds.Size.ToVector2() / 2f;
            if (LoadImage(path))
            {
                _drawAction += () =>
                {
                    Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                    Core.SpriteBatchInstance.Draw(Image, Position + Margin, null, Color.White, 0f, _offset, 1f, 0, 1);
                    Core.SpriteBatchInstance.End();
                };
            }
        }
        public ObjectContent(ref Action<string> textProvider, SpriteFont font) : this("", font)
        {
            textProvider += (string text) =>
            {
                this.Text = text;
                LastAlignment?.Invoke();
                _centreAction?.Invoke();
            };
        }
        // Необходимо добавить некоторую стартовую картинку для варианта с провайдером изображений
        public ObjectContent(ref Action<Texture2D> imageProvider) : this("null_image")
        {
            imageProvider += (Texture2D image) =>
            {
                this.Image = image;
                LastAlignment?.Invoke();
                _centreAction?.Invoke();
            };
        }
        private bool LoadImage(string path)
        {
            try
            {
                Image = Core.ContentManagerInstance.Load<Texture2D>(path);
            }
            catch
            {
                //Log.Message("resource " + path + " doesn't exist.");
                return false;
            }
            return true;
        }
        //public void AttachToElement(GUIElement element)
        //{
        //    (element._view as GUIElementView).CallContent += Draw;
        //    _centreAction += () => _position = element._view.Position + element._view.Texture.Bounds.Size.ToVector2() / 2;
        //    element.ViewPositionChanged += (Vector2 vector) => _actualAlignment?.Invoke();
        //}
        public void SetOwner(IContentContainer container)
        {
            Position = container.Position;
            _centreAction = () => {
                if(container is IContentPositionProvider)
                {
                    Position = (container as IContentPositionProvider).GetPosition();
                }
                else
                {
                    Position = container.Position;
                }               
            };
            _centreAction.Invoke();
            _centreOffset += () => _margin = container.Dimentions / 2f;
            Action<Vector2> setPos = (Vector2 vector) => Position = vector;
            if(_owner != null)
            {
                _owner.PositionChanged -= setPos;
                _owner.CallContent -= Draw;
                if (_owner is IContentPositionProvider)
                {
                    (_owner as IContentPositionProvider).PositionProvider -= setPos;
                }
            }         
            container.CallContent += Draw;
            if(container is IContentPositionProvider)
            {
                (container as IContentPositionProvider).PositionProvider += setPos;
            }
            else
            {
                container.PositionChanged += setPos;
            }
            _owner = container;
        }
        public void AllignCentre()
        {
            LastAlignment = AllignCentre;
            _centreOffset.Invoke();
        }
        internal void Draw(GameTime gameTime)
        {
            _drawAction?.Invoke();
        }
    }
}