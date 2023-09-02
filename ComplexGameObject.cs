using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using VitchinnikMonoCore.GUI;
using MonoGame.Extended;
using System.Reflection.Metadata.Ecma335;

namespace VitchinnikMonoCore
{
    /// <summary>
    /// Game object which consist of several parts with customizable behaviour
    /// </summary>
    public class ComplexGameObject : GameObject
    {
        protected List<ObjectPartView> _partViews = new List<ObjectPartView>();
        protected Dictionary<int, ObjectPartModel> _partModels = new Dictionary<int, ObjectPartModel>();
        protected Dictionary<int, Action> _hoverPartEnter = new Dictionary<int, Action>();
        protected Dictionary<int, Action> _hoverPartExit = new Dictionary<int, Action>();
        protected Dictionary<int, string> _tooltipPart = new Dictionary<int, string>();
        protected List<Action> _partShow = new List<Action>();
        protected List<Action> _partHide = new List<Action>();
        protected Dictionary<int, Action> _enablePart = new Dictionary<int, Action>();
        protected Dictionary<int, Action> _disablePart = new Dictionary<int, Action>();
        protected int _hoveredPartIndex = -1;
        public event Action<int> HoverPartEntered;
        public event Action<int> HoverPartLeft;
        private Action<string> _tooltipTextProvider;
        public ComplexGameObject(string path, Vector2 position) : base()
        {
            _view = new ObjectPartView(this, path, position);
        }
        protected ComplexGameObject() : base()
        {
            SetTooltip(new Tooltip(ref _tooltipTextProvider));
        }
        public int NewPart(string path, Vector2 offset)
        {
            ObjectPartView output = new ObjectPartView(this, offset, path);
            _partViews.Add(output);
            return _partViews.IndexOf(output);
        }
        public void RemovePart(int partHandler)
        {
            _partViews.RemoveAt(partHandler);
        }
        public void SetTooltip(string tooltipText, int partHandler)
        {
            _tooltipPart.TryAdd(partHandler, tooltipText);
        }
        public void SetTooltip(ref Action<string> tooltipTextProvider, int partHandler)
        {
            tooltipTextProvider += _tooltipTextProvider;
        }
        public override bool Contains(Vector2 vector)
        {
            if (!Visible) return false;

            var hoveredPartIndex = -1;

            foreach (var partView in _partViews) if (partView.Contains(vector)) hoveredPartIndex = _partViews.IndexOf(partView);
            if(_hoveredPartIndex != hoveredPartIndex)
            {
                Action hoverExit;
                if (_hoverPartExit.TryGetValue(_hoveredPartIndex, out hoverExit)) hoverExit.Invoke();
                Action hoverEnter;
                if (_hoverPartEnter.TryGetValue(hoveredPartIndex, out hoverEnter)) hoverEnter.Invoke();
                string text;
                if (_tooltipPart.TryGetValue(hoveredPartIndex, out text)) _tooltipTextProvider.Invoke(text);
                Action click;
                _hoveredPartIndex = hoveredPartIndex;
            }
            if (hoveredPartIndex == -1 && !base.Contains(vector))
            {
                IsHovered = false;
                return false;
            }
            IsHovered = true;
            return true;
        }
        public override void SetCamera(OrthographicCamera camera)
        {
            base.SetCamera(camera);
            foreach (var partView in _partViews)
            {
                partView.Camera = camera;
            }
        }
        protected class ObjectPartView : ObjectView
        {
            public event Action<Vector2> OffsetChanged;
            private bool _visible = true;
            public bool Visible => _visible;
            public Vector2 Offset
            {
                get => _offset;
                set
                {
                    if (value == _offset) return;
                    _offset = value;
                    OffsetChanged?.Invoke(value);
                }
            }
            public ObjectPartView(ComplexGameObject controlsSource, string path) : 
                base(controlsSource, path, controlsSource._view?.Position ?? Vector2.Zero)
            {
                Initialize(controlsSource);
            }
            public ObjectPartView(ComplexGameObject controlsSource, string path, Vector2 position) : 
                base(controlsSource, path, position)
            {
                Initialize(controlsSource);
            }
            public ObjectPartView(ComplexGameObject controlsSource, string path, ref Action<Texture2D> textureProvider) : 
                base(controlsSource, path, ref textureProvider)
            {
                Initialize(controlsSource);
            }
            public ObjectPartView(ComplexGameObject controlsSource, string path, Vector2 position, ref Action<Texture2D> textureProvider) : 
                base(controlsSource, path, position, ref textureProvider)
            {
                Initialize(controlsSource);
            }
            public ObjectPartView(ComplexGameObject controlsSource, Vector2 offset, string path) : 
                base(controlsSource, path, controlsSource._view?.Position ?? Vector2.Zero)
            {
                _offset = offset;
                Initialize(controlsSource);
            }
            public ObjectPartView(ComplexGameObject controlsSource, string path, Vector2 position, Vector2 offset) : 
                base(controlsSource, path, position)
            {
                _offset = offset;
                Initialize(controlsSource);
            }
            public ObjectPartView(ComplexGameObject controlsSource, string path, ref Action<Texture2D> textureProvider, Vector2 offset) : 
                base(controlsSource, path, ref textureProvider)
            {
                _offset = offset;
                Initialize(controlsSource);
            }
            public ObjectPartView(ComplexGameObject controlsSource, string path, Vector2 position, ref Action<Texture2D> textureProvider, Vector2 offset) : 
                base(controlsSource, path, position, ref textureProvider)
            {
                _offset = offset;
                Initialize(controlsSource);
            }
            private void Initialize(ComplexGameObject controlsSource)
            {
                if (controlsSource._view is null) return;
                controlsSource._partViews.Add(this);
                controlsSource._partShow.Add(() => { if (!_visible) controlsSource.DrawAction += Draw;
                    _visible = true;
                });
                controlsSource._partHide.Add(() => { controlsSource.DrawAction -= Draw;
                    _visible = false;
                });
                controlsSource._hoverPartEnter.TryAdd(controlsSource._partViews.IndexOf(this), () =>
                {
                    controlsSource.HoverPartEntered?.Invoke(controlsSource._partViews.IndexOf(this));
                });
                controlsSource._hoverPartExit.TryAdd(controlsSource._partViews.IndexOf(this), () =>
                {
                    controlsSource.HoverPartLeft?.Invoke(controlsSource._partViews.IndexOf(this));
                });
            }
        }
        protected class ObjectPartModel : ObjectModel
        {
            public ObjectPartModel(ComplexGameObject controlsSource, int partIndex)
            {
                controlsSource._partModels.TryAdd(partIndex, this);
                controlsSource._hoverPartEnter[partIndex] += () =>
                {
                    OnHoverEnter();
                    controlsSource.UpdateAction += OnHover;
                    controlsSource.ClickEvent += OnClick;
                };
                controlsSource._hoverPartExit[partIndex] += () =>
                {
                    OnHoverExit();
                    controlsSource.UpdateAction -= OnHover;
                    controlsSource.ClickEvent -= OnClick;
                };
                controlsSource._enablePart.TryAdd(partIndex,() =>
                {
                    if (Enabled) return;
                    _enabled = true;
                    OnEnable();
                    controlsSource.UpdateAction += OnUpdate;
                });
                controlsSource._disablePart.TryAdd(partIndex, () =>
                {
                    if (!Enabled) return;
                    _enabled = false;
                    OnDisable();
                    controlsSource.UpdateAction -= OnUpdate;
                });
            }
        }
    }
}
