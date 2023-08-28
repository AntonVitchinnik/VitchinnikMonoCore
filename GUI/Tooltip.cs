using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VitchinnikMonoCore.GUI
{
    public class Tooltip
    {
        private static Tooltip _actual = new NullTooltip();
        public static Tooltip Actual
        {
            get => _actual;
            set
            {
                if (_actual.Equals(value))
                    return;
                _actual.ResetDelay();
                _actual = value;
            }
        }
        public static SpriteFont Font;
        private Action<GameTime> _tooltipState;
        private Action<GameTime> _drawState;
        protected string _text;
        public float InitialDelay { get; set; }
        private float _actualDelay;
        public float ZOrder { get; set; }
        public Texture2D Texture { get; protected set; }
        public Vector2 ScaleFactor => Font.MeasureString(_text) + new Vector2(12f, 6f);
        public Tooltip(string tooltipText)
        {
            this.Texture = Core.ContentManagerInstance.Load<Texture2D>("Sprites\\tooltipBox");
            _text = tooltipText;
            ZOrder = 0.8f;
            _actualDelay = 0f;
            _tooltipState += OnInitialization;
            InitialDelay = 1f;
        }
        public Tooltip(ref Action<string> textProvider)
        {
            this.Texture = Core.ContentManagerInstance.Load<Texture2D>("Sprites\\tooltipBox");
            _text = "";
            textProvider += SetText;
            ZOrder = 0.8f;
            _actualDelay = 0f;
            _tooltipState += OnInitialization;
            InitialDelay = 1f;
        }
        private void SetText(string text) => _text = text;
        private void ResetDelay()
        {
            _actualDelay = 0f;
            if (_tooltipState != null)
                return;
            _tooltipState += OnInitialization;
            _drawState -= OnInitialized;
        }
        internal void Update(GameTime gameTime)
        {
            _tooltipState?.Invoke(gameTime);
        }
        private void OnInitialization(GameTime gameTime)
        {
            if (_actualDelay < InitialDelay)
            {
                _actualDelay += (float)gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }
            if (_text?.Equals(string.Empty) == true)
                return;
            _tooltipState -= OnInitialization;
            _drawState += OnInitialized;
        }
        private void OnInitialized(GameTime gameTime)
        {
            Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
            Core.SpriteBatchInstance.Draw(Texture, MouseHandler.Position, null, Color.White, 0f, Vector2.Zero, ScaleFactor, 0, ZOrder);
            Core.SpriteBatchInstance.DrawString(Font, _text, MouseHandler.Position + new Vector2(6f, 3f), Color.White, 0f, Vector2.Zero, 1f, 0, ZOrder + 0.1f);
            Core.SpriteBatchInstance.End();
        }
        public virtual void Draw(GameTime gameTime)
        {
            _drawState?.Invoke(gameTime);
        }
        protected Tooltip()
        {

        }
    }
    public class NullTooltip : Tooltip
    {
        public NullTooltip() : base() { }
        public override void Draw(GameTime gameTime)
        {

        }
    }
}
