using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VitchinnikMonoCore.Content;

namespace VitchinnikMonoCore.GUI
{
    public class ListView : GUIElement
    {
        private List<ListViewItem> _items;
        private List<Action> _recalculateCall;
        private List<Action<GameTime>> _callItemsDraw;
        private List<int> _selectedItems;
        public int SelectedItem { get => _selectedItems[0]; private set => _selectedItems[0] = value; }
        public int[] SelectedItems => _selectedItems.ToArray();
        public bool Selectable { get; private set; }
        public bool MultiSelect { get; private set; }
        public int Count { get => _items.Count(); }
        public ListView(string path, Vector2 position, bool selectable = false, bool multiSelect = true)
        {
            _items = new List<ListViewItem>();
            _recalculateCall = new List<Action>();
            _callItemsDraw = new List<Action<GameTime>>();
            _selectedItems = new List<int>();
            _view = new ListViewView(this, path, position);
            _model = new ListViewModel(this);
            this.Selectable = selectable;
            this.MultiSelect = multiSelect;
        }
        public void DeselectAll()
        {
            foreach (var item in _items)
            {
                item.Deselect();
            }
        }
        public void SetItemsRenderer(Vector2 padding, Vector2 innerSize)
        {
            (_view as ListViewView).SetItemsRenderer(padding, innerSize);
        }
        public void SetScrollBar(string path, Vector2 scrollBarPosition, Vector2 scrollBarOffset, float borderValue)
        {
            (_view as ListViewView).SetScrollBar(path, scrollBarPosition, scrollBarOffset, borderValue);
        }
        public void SetPointer(string path)
        {
            (_view as ListViewView).SetPointer(path);
        }
        protected void Remove(ListViewItem item)
        {
            _items.Remove(item);
            _recalculateCall.RemoveAt(item.Index);
            _callItemsDraw.RemoveAt(item.Index);
            for (int i = item.Index; i < _items.Count; i++)
            {
                _recalculateCall[i].Invoke();
            }
            (_view as ListViewView).RecalculateItemsSize();
        }
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            _recalculateCall.RemoveAt(index);
            _callItemsDraw.RemoveAt(index);
            for (int i = index; i < _items.Count; i++)
            {
                _recalculateCall[i].Invoke();
            }
            (_view as ListViewView).RecalculateItemsSize();
        }
        public void AddItem(string path)
        {
            var item = new ListViewItem(path, this);
            (_view as ListViewView).RecalculateItemsSize();
        }
        public void AddVisuallySelectableItem(string path, string selectedPath)
        {
            var item = new ListViewItem(path, selectedPath, this);
            (_view as ListViewView).RecalculateItemsSize();
        }
        public void AddItem(string path, ref Action<Texture2D> textureProvider)
        {
            var item = new ListViewItem(path, this, ref textureProvider);
            (_view as ListViewView).RecalculateItemsSize();
        }
        public void AddItemClickAction(int index, Action action)
        {
            _items[index].Clicked += action;
        }


        public void SetContent(int index, ObjectContent content)
        {
            _items[index].SetContent(content);
        }
        protected IEnumerator<ListViewItem> GetEnumerator() => _items.GetEnumerator();
        protected class ListViewView : GUIElementView
        {
            private Vector2 _orientation;
            public Vector2 Orientation => _orientation;
            private Vector2 _padding;
            private Vector2 _innerSize;
            private Vector2 _itemsOffset;
            private Vector2 _itemsSize;
            public Rectangle CropItems => new Rectangle((_itemsOffset * Orientation).ToPoint(), _innerSize.ToPoint());
            private int _hoveredItem = -1;
            public int HoveredItem => _hoveredItem;
            private Vector2 _scrollBarPosition;
            private Vector2 _scrollBarOffset;
            private float _borderValue;
            private Vector2 _pointerPosition;
            public Vector2 PointerPosition
            {
                get => _pointerPosition;
                private set
                {
                    var prev = _pointerPosition;
                    _pointerPosition = Vector2.Clamp(value, Vector2.Zero, new Vector2(_borderValue)) * _orientation;
                    if (prev != _pointerPosition)
                    {
                        PointerPositionChanged?.Invoke(_pointerPosition);
                    }
                }
            }
            private Vector2 _pointerOffset;
            private Vector2 _lastChecked;
            private Texture2D _scrollBarTexture;
            private Texture2D _pointerTexture;
            private Action _enterScrollBar;
            private Action _exitScrollBar;
            private Action _enterPointer;
            private Action _exitPointer;
            private Action _recalculateItemsSizeAction;
            public event Action<int> EnterItem;
            public event Action<int> ExitItem;
            private Action<GameTime> _callItemsRenderer;
            private Action<Vector2> _callItemsContain;
            private RenderTarget2D _itemsRenderer = new RenderTarget2D(Core.GameInstance.GraphicsDevice, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            public event Action<Vector2> PointerPositionChanged;
            public bool PointerHovered { get; private set; }
            public bool ScrollBarHovered { get; private set; }
            public ListViewView(ListView controlsSource, string path, Vector2 position) : base(controlsSource, path, position)
            {
                _orientation = Vector2.UnitY;
                Action<GameTime> pointerMovement = (GameTime gameTime) => MovePointer();
                Action subMov = () => controlsSource.UpdateAction += pointerMovement;
                _enterPointer += () => controlsSource.ClickEvent += subMov;
                controlsSource.ReleaseEvent += () => controlsSource.UpdateAction -= pointerMovement;
                _exitPointer += () => controlsSource.ClickEvent -= subMov;
                _enterScrollBar += () => controlsSource.ClickEvent += subMov;
                _exitScrollBar += () => controlsSource.ClickEvent -= subMov;
                _callItemsRenderer += (GameTime gameTime) =>
                {
                    Core.GameInstance.GraphicsDevice.SetRenderTarget(_itemsRenderer);
                    Core.GameInstance.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                    foreach (var item in controlsSource._items)
                    {
                        controlsSource._callItemsDraw[item.Index].Invoke(gameTime);
                    }
                    Core.GameInstance.GraphicsDevice.SetRenderTarget(Core.DefaultRenderTarget);
                    Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                    Core.SpriteBatchInstance.Draw(_itemsRenderer, Position + _padding, CropItems, Color.White);
                    Core.SpriteBatchInstance.End();
                };
                _callItemsContain += (Vector2 vector) =>
                {
                    foreach (var item in controlsSource._items)
                    {
                        if (item.Contains(vector))
                        {
                            if (_hoveredItem == item.Index)
                                return;
                            if (_hoveredItem == -1)
                            {
                                _hoveredItem = item.Index;
                                EnterItem?.Invoke(_hoveredItem);
                                return;
                            }
                            ExitItem?.Invoke(_hoveredItem);
                            _hoveredItem = item.Index;
                            EnterItem?.Invoke(_hoveredItem);
                        }
                    }
                    if (_hoveredItem != -1)
                    {
                        ExitItem?.Invoke(_hoveredItem);
                        _hoveredItem = -1;
                    }

                };
                _recalculateItemsSizeAction = () =>
                {
                    if (controlsSource.Count == 0)
                    {
                        _itemsSize = Vector2.Zero;
                        _itemsOffset = Vector2.Zero;
                        return;
                    }
                    var size = controlsSource._items[controlsSource.Count - 1].Position + controlsSource._items[controlsSource.Count - 1].Dimentions;
                    if ((size * Orientation).Length() < (_innerSize * Orientation).Length())
                    {
                        _itemsSize = Vector2.Zero;
                        _itemsOffset = Vector2.Zero;
                        return;
                    }
                    _itemsSize = size;
                    _itemsRenderer = new RenderTarget2D(Core.GameInstance.GraphicsDevice, (int)_itemsSize.X, (int)_itemsSize.Y);
                    OnPointerMove(PointerPosition);
                };
                controlsSource.HoverEntered += () => MouseHandler.ScrollChanged += Scroll;
                controlsSource.HoverLeft += () => MouseHandler.ScrollChanged -= Scroll;
                PointerPositionChanged += OnPointerMove;
            }

            private void OnPointerMove(Vector2 position)
            {
                var value = position.Length() / _borderValue;
                if (_itemsSize != Vector2.Zero)
                    _itemsOffset = (_itemsSize - _innerSize) * value * Orientation;
            }

            public void RecalculateItemsSize()
            {
                _recalculateItemsSizeAction.Invoke();
            }
            private void Scroll(GameTime gameTime, int deltaScroll)
            {
                PointerPosition -= deltaScroll * Orientation * 0.1f;
            }

            public void SetItemsRenderer(Vector2 padding, Vector2 innerSize)
            {
                _padding = padding;
                _innerSize = innerSize;
            }
            public void SetScrollBar(string path, Vector2 scrollBarPosition, Vector2 scrollBarOffset, float borderValue)
            {
                _scrollBarTexture = Core.ContentManagerInstance.Load<Texture2D>(path);
                _scrollBarPosition = scrollBarPosition;
                _scrollBarOffset = scrollBarOffset;
                _borderValue = borderValue;
            }
            public void SetPointer(string path)
            {
                _pointerTexture = Core.ContentManagerInstance.Load<Texture2D>(path);
                _pointerOffset = Vector2.Floor(_pointerTexture.Bounds.Size.ToVector2() / 2f);
            }
            private void MovePointer()
            {
                PointerPosition = _lastChecked - Position - _scrollBarPosition;
            }
            public override bool Contains(Vector2 vector)
            {
                _lastChecked = vector;
                if (base.Contains(vector))
                {
                    if (_pointerTexture.Bounds.Contains((int)(vector.X - RelatedPosition.X - Position.X - _offset.X - _pointerPosition.X + _pointerOffset.X - _scrollBarPosition.X), (int)(vector.Y - RelatedPosition.Y - Position.Y - _offset.Y - _pointerPosition.Y + _pointerOffset.Y - _scrollBarPosition.Y)))
                    {
                        if (ScrollBarHovered)
                        {
                            _exitScrollBar.Invoke();
                            ScrollBarHovered = false;
                        }
                        if (PointerHovered)
                            return true;
                        PointerHovered = true;
                        _enterPointer.Invoke();
                        if (_hoveredItem != -1)
                        {
                            ExitItem?.Invoke(_hoveredItem);
                            _hoveredItem = -1;
                        }
                        return true;
                    }
                    if (PointerHovered)
                    {
                        _exitPointer.Invoke();
                        PointerHovered = false;
                    }

                    if (_scrollBarTexture.Bounds.Contains((int)(vector.X - RelatedPosition.X - Position.X - _offset.X - _scrollBarPosition.X + _scrollBarOffset.X), (int)(vector.Y - RelatedPosition.Y - Position.Y - _offset.Y - _scrollBarPosition.Y + _scrollBarOffset.Y)))
                    {
                        if (ScrollBarHovered)
                            return true;
                        ScrollBarHovered = true;
                        _enterScrollBar.Invoke();
                        if (_hoveredItem != -1)
                        {
                            ExitItem?.Invoke(_hoveredItem);
                            _hoveredItem = -1;
                        }
                        return true;
                    }
                    if (ScrollBarHovered)
                    {
                        _exitScrollBar.Invoke();
                        ScrollBarHovered = false;
                    }
                    _callItemsContain(vector - Position - _padding + _itemsOffset);
                    return true;
                }
                else
                {
                    if (_hoveredItem != -1)
                    {
                        ExitItem?.Invoke(_hoveredItem);
                        _hoveredItem = -1;
                    }
                    if (PointerHovered)
                    {
                        _exitPointer.Invoke();
                        PointerHovered = false;
                    }
                    if (ScrollBarHovered)
                    {
                        _exitScrollBar.Invoke();
                        ScrollBarHovered = false;
                    }
                    return false;
                }
            }
            protected override void Draw(GameTime gameTime)
            {
                base.Draw(gameTime);
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.Draw(_scrollBarTexture, Position + _scrollBarPosition, null, Color.White, 0f, _scrollBarOffset, 1f, 0, 1);
                Core.SpriteBatchInstance.End();
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.Draw(_pointerTexture, Position + _scrollBarPosition + _pointerPosition, null, Color.White, 0f, _pointerOffset, 1f, 0, 1);
                Core.SpriteBatchInstance.End();
                _callItemsRenderer.Invoke(gameTime);
            }

        }
        protected class ListViewModel : ObjectModel
        {
            private Action _defaultClickAction;
            public ListViewModel(ListView controlsSource) : base(controlsSource)
            {
                _defaultClickAction = () =>
                {
                    if ((controlsSource._view as ListViewView).HoveredItem != -1)
                    {
                        controlsSource._items[(controlsSource._view as ListViewView).HoveredItem].Click();
                    }
                };
            }
            protected override void OnClick()
            {
                _defaultClickAction.Invoke();
            }
        }
        protected class ListViewItem : GUIElementView
        {
            private int _index;
            public int Index => _index;
            private ListView _owner;
            public event Action Clicked;
            private bool _selected;
            public event Action<int> SelectionEvent;
            public event Action<int> DeselectionEvent;
            public bool Selected
            {
                get => _selected;
                private set
                {
                    if (_selected == value)
                        return;
                    if (value)
                    {
                        SelectionEvent?.Invoke(_index);
                        _selected = value;
                        return;
                    }
                    DeselectionEvent?.Invoke(_index);
                    _selected = value;
                }
            }
            public ListViewItem(string path, ListView owner) : base(owner, path, Vector2.Zero)
            {
                _owner = owner;
                _texture = Core.ContentManagerInstance.Load<Texture2D>(path);
                owner.DrawAction -= Draw;
                owner._callMove -= Move;
                owner._items.Add(this);
                owner._recalculateCall.Add(RecalculatePosition);
                owner._callItemsDraw.Add(Draw);
                RecalculatePosition();
                if (owner.Selectable)
                    Clicked += () => Selected = true;
                if (!owner.MultiSelect)
                    SelectionEvent += (int item) => owner.DeselectAll();
            }
            public ListViewItem(string path, string selectedPath, ListView owner) : this(path, owner)
            {
                var deselectedTex = _texture;
                var selectedTex = Core.ContentManagerInstance.Load<Texture2D>(selectedPath);
                SelectionEvent += (int item) => _texture = selectedTex;
                DeselectionEvent += (int item) => _texture = deselectedTex;
            }
            public void Deselect() => Selected = false;
            internal void Click() => Clicked?.Invoke();
            public ListViewItem(string path, ListView owner, ref Action<Texture2D> textureProvider) : this(path, owner)
            {
                textureProvider += (Texture2D texture) => _texture = texture;
            }
            private void RecalculatePosition()
            {
                _index = _owner._items.IndexOf(this);
                if (_index > 0)
                {
                    Position = (_owner._items[_index - 1].Position + _owner._items[_index - 1].Dimentions + new Vector2(2f, 2f)) * (_owner._view as ListViewView).Orientation;
                    return;
                }
                Position = Vector2.Zero;
            }
        }
    }
}
