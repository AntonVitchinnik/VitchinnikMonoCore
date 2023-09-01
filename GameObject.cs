using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using VitchinnikMonoCore.GUI;
using VitchinnikMonoCore.Transitions;

namespace VitchinnikMonoCore
{
    /// <summary>
    /// Класс реализующий базовый функционал для всех игровых объектов
    /// </summary>
    public abstract class GameObject : DrawableGameComponent
    {
        /// <summary>
        /// Поле, реализующее представление объекта
        /// </summary>
        /// <remarks>В конструкторе класса наследника нужно обязательно инициализировать это поле</remarks>
        protected ObjectView _view;
        /// <summary>
        /// Поле, реализующее модель объекта
        /// </summary>
        /// <remarks>В конструкторе класса наследника нужно обязательно инициализировать это поле</remarks>
        protected ObjectModel _model;
        private Action<GameTime> _callUpdate;
        protected event Action<GameTime> UpdateAction
        {
            add
            {
                _callUpdate += value;
            }
            remove
            {
                _callUpdate -= value;
            }
        }
        private Action<GameTime> _callDraw;
        protected event Action<GameTime> DrawAction
        {
            add
            {
                _callDraw += value;
            }
            remove
            {
                _callDraw -= value;
            }
        }
        protected Action<Transition.Type, Vector2, float, float, Action, TransitionTerminationToken> _callMove;
        private Action _tooltipShow;
        private Action _tooltipHide;
        public event Action EnableEvent;
        public event Action DisableEvent;
        public event Action HoverEntered;
        public event Action HoverLeft;
        public event Action ClickEvent;
        public event Action ReleaseEvent;
        private bool _clicked;
        public bool Clicked => _clicked;
        public Vector2? Position
        {
            get => _view?.Position;
            set
            {
                if (_view == null)
                    return;
                _view.Position = value ?? Vector2.Zero;
            }
        }
        public event Action<Vector2> ViewPositionChanged;
        public bool IsHovered { get; private protected set; }
        public GameObject() : base(Core.GameInstance)
        {
            Enabled = true;
        }
        public void Enable()
        {
            if (_model.Enabled)
                return;
            EnableEvent?.Invoke();
        }
        public void Disable()
        {
            if (!_model.Enabled)
                return;
            DisableEvent?.Invoke();
        }
        public void Toggle()
        {
            if (_model.Enabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }
        private void HoverEnter()
        {
            IsHovered = true;
            HoverEntered?.Invoke();
        }
        private void HoverLeave()
        {
            IsHovered = false;
            HoverLeft?.Invoke();
        }
        public virtual bool Contains(Vector2 vector)
        {
            var output = Visible && _view?.Contains(vector) == true;
            if (output && !IsHovered)
            {
                HoverEnter();
                return output;
            }
            if (!output && IsHovered)
            {
                HoverLeave();
            }
            return output;
        }
        private protected virtual void Click()
        {
            ClickEvent?.Invoke();
            _clicked = true;
        }
        private protected virtual void Release()
        {
            if (_clicked)
            {
                ReleaseEvent?.Invoke();
                _clicked = false;
                if (IsHovered)
                    return;
                UnbindClickInvokeAction();
            }
        }
        internal void BindClickInvokeAction()
        {
            _tooltipShow?.Invoke();
            MouseHandler.LMBPressed += Click;
            if (_clicked)
                return;
            MouseHandler.LMBReleased += Release;
        }
        internal void UnbindClickInvokeAction()
        {
            _tooltipHide?.Invoke();
            MouseHandler.LMBPressed -= Click;
            if (_clicked)
                return;
            MouseHandler.LMBReleased -= Release;
        }
        public void Hide() => Visible = false;
        public void Show() => Visible = true;
        public void SetTooltip(Tooltip tooltip)
        {
            _tooltipShow = () => Tooltip.Actual = tooltip;
            _tooltipHide = () => Tooltip.Actual = new NullTooltip();
        }
        public void Teleport(Vector2 target)
        {
            if (_view == null)
                return;
            _view.Position = target;
        }
        public void Move(Transition.Type type, Vector2 target, float time, float elastic = 1f, Action onExpired = null, TransitionTerminationToken terminationToken = null)
        {
            _callMove?.Invoke(type, target, time, elastic, onExpired, terminationToken);
        }
        protected void InvokePositionChange(Vector2 vector) => ViewPositionChanged?.Invoke(vector);
        public virtual void SetCamera(OrthographicCamera camera)
        {
            if (_view is null) return;
            _view.Camera = camera;
        }
        public override void Update(GameTime gameTime)
        {
            _callUpdate?.Invoke(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            _callDraw?.Invoke(gameTime);
        }
        protected abstract class ObjectView
        {
            protected Texture2D _texture;
            public Texture2D Texture => _texture;
            private Vector2 _position;
            private OrthographicCamera _camera = null;
            public OrthographicCamera Camera
            {
                get => _camera;
                set
                {
                    _camera = value;
                    if(value is null)
                    {
                        DrawPosition = () => Position;
                    }
                    else
                    {
                        DrawPosition = () => _camera.WorldToScreen(Position);
                    }
                }
            }
            protected Func<Vector2> DrawPosition;
            public Vector2 Position
            {
                get => _position;
                protected internal set
                {
                    if (_position.Equals(value))
                        return;
                    _position = value;
                    PositionChanged?.Invoke(value);
                }
            }
            protected Vector2 _offset;
            public event Action<Vector2> PositionChanged;
            /// <summary>
            /// Стандартный конструктор представления игрового объекта с начальной позицией 0, 0
            /// </summary>
            /// <param name="controlsSource">Источник управляющей логики</param>
            /// <param name="path">Путь к текстуре</param>
            public ObjectView(GameObject controlsSource, string path)
            {
                if (LoadContent(path))
                {
                    controlsSource._callDraw += Draw;
                    controlsSource._callMove += Move;
                    PositionChanged += controlsSource.InvokePositionChange;
                    DrawPosition = () => Position;
                }
            }
            /// <summary>
            /// Конструктор представления игрового объекта с указанной начальной позицией
            /// </summary>
            /// <param name="controlsSource">Источник управляющей логики</param>
            /// <param name="path">Путь к текстуре</param>
            /// <param name="position">Начальная позиция игрового объекта</param>
            public ObjectView(GameObject controlsSource, string path, Vector2 position) : this(controlsSource, path)
            {
                _position = position;
            }
            /// <summary>
            /// Конструктор представления игрового объекта с провайдером текстур и начальным положением 0, 0
            /// </summary>
            /// <param name="controlsSource">Источник управляющей логики</param>
            /// <param name="path">Путь к начальной текстуре представления</param>
            /// <param name="textureProvider">Провайдер текстур</param>
            public ObjectView(GameObject controlsSource, string path, ref Action<Texture2D> textureProvider):this(controlsSource, path)
            {
                textureProvider += (Texture2D texture) => _texture = texture; 
            }
            /// <summary>
            /// Конструктор представления игрового объекта с провайдером текстур и указанной начальной позицией
            /// </summary>
            /// <param name="controlsSource">Источник управляющей логики</param>
            /// <param name="path">Путь к начальной текстуре представления</param>
            /// <param name="position">Начальная позиция игрового объекта</param>
            /// <param name="textureProvider">Провайдер текстур</param>
            public ObjectView(GameObject controlsSource, string path, Vector2 position, ref Action<Texture2D> textureProvider) : this(controlsSource, path, position)
            {
                textureProvider += (Texture2D texture) => _texture = texture;
            }
            protected bool LoadContent(string path)
            {
                try
                {
                    _texture = Core.ContentManagerInstance.Load<Texture2D>(path);
                }
                catch
                {
                    //Log.Message("resource " + path + " doesn't exist.");
                    return false;
                }
                return true;
            }
            public virtual bool Contains(Vector2 vector)
            {
                return _texture.Bounds.Contains((int)(vector.X - _position.X - _offset.X), (int)(vector.Y - _position.Y - _offset.Y));
            }
            protected void Move(Transition.Type type, Vector2 target, float time, float elastic = 1f, Action onExpired = null, TransitionTerminationToken terminationToken = null)
            {
                Core.GameInstance.Components.Add(new Transition(type, Position, target, time, (Vector2 vector) => { Position = vector; }, elastic, onExpired, terminationToken));
            }
            protected virtual void Draw(GameTime gameTime)
            {
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.Draw(_texture, DrawPosition.Invoke(), null, Color.White, 0f, _offset, 1f, 0, 1);
                Core.SpriteBatchInstance.End();
            }
        }
        protected abstract class ObjectModel
        {
            private protected bool _enabled = false;
            public bool Enabled => _enabled;
            public ObjectModel(GameObject controlsSource)
            {
                controlsSource.EnableEvent += () =>
                {
                    _enabled = true;
                    OnEnable();
                    controlsSource._callUpdate += OnUpdate;
                };
                controlsSource.DisableEvent += () =>
                {
                    _enabled = false;
                    controlsSource._callUpdate -= OnUpdate;
                    OnDisable();
                };
                controlsSource.HoverEntered += () =>
                {
                    OnHoverEnter();
                    controlsSource._callUpdate += OnHover;
                };
                controlsSource.HoverLeft += () =>
                {
                    OnHoverExit();
                    controlsSource._callUpdate -= OnHover;
                };
                controlsSource.ClickEvent += OnClick;
                controlsSource.ReleaseEvent += OnRelease;
            }
            protected ObjectModel()
            {

            }
            protected virtual void OnEnable()
            {

            }
            protected virtual void OnUpdate(GameTime gameTime)
            {

            }
            protected virtual void OnDisable()
            {

            }
            protected virtual void OnHoverEnter()
            {

            }
            protected virtual void OnHover(GameTime gameTime)
            {

            }
            protected virtual void OnHoverExit()
            {

            }
            protected virtual void OnClick()
            {

            }
            protected virtual void OnRelease()
            {

            }
        }
    }
}