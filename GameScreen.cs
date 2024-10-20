using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;
using VitchinnikMonoCore.GUI;

namespace VitchinnikMonoCore
{
    public abstract class GameScreen
    {
        public static Action OnZoom;
        public static Action OnCameraMove;
        private static Vector2 _cameraMoveDirection;
        public static OrthographicCamera ActualCamera { get; protected set; }
        public static float ZoomSpeed = 10f;
        public static float CameraSpeed = 10f;
        public static event Action<GameScreen> ScreenLoaded;
        public GameComponentCollection Objects { get; protected set; }

        public GameScreen()
        {
            _cameraMoveDirection = Vector2.Zero;
            Objects = new GameComponentCollection();
            ActualCamera = new OrthographicCamera(Core.GraphicsDevice);
            ActualCamera.MaximumZoom = 11f;
            ActualCamera.MinimumZoom = 0.5f;
            ActualCamera.Origin = Vector2.Zero;
            MouseHandler.SetUpdtateCollisionProvider(ref OnCameraMove);
            MouseHandler.SetUpdtateCollisionProvider(ref OnZoom);
        }
        public virtual void Load()
        {
            Core.ActualScreen = this;
            ScreenLoaded?.Invoke(this);
        }
        public virtual void Update(GameTime gameTime)
        {
            if (!_cameraMoveDirection.Equals(Vector2.Zero))
            {
                var pos = ActualCamera.Position;
                ActualCamera.Move(Vector2.Normalize(_cameraMoveDirection) * CameraSpeed / ActualCamera.Zoom);
                ActualCamera.Position = new Vector2((float)Math.Round(ActualCamera.Position.X), (float)Math.Round(ActualCamera.Position.Y));
                ClampCamera();
                if (!pos.Equals(ActualCamera.Position))
                {
                    OnCameraMove?.Invoke();
                }
            }
        }
        public virtual void Draw(GameTime gameTime)
        {

        }
        protected void Zoom(GameTime gameTime, int deltaScroll)
        {
            if (MouseHandler.HoveredObjects.Count > 0 && MouseHandler.HoveredObjects[0] is GUIElement)
            {
                return;
            }
            var oldZoom = ActualCamera.Zoom;
            var oldMousePosition = ActualCamera.ScreenToWorld(MouseHandler.Position);
            ActualCamera.ZoomIn(deltaScroll * ZoomSpeed * ActualCamera.Zoom * 3f);
            ActualCamera.Position = oldMousePosition - (oldMousePosition - ActualCamera.Position) * oldZoom / ActualCamera.Zoom;
            ClampCamera();
            if (oldZoom != ActualCamera.Zoom)
            {
                OnZoom?.Invoke();
            }
        }
        protected void CameraMove(GameTime gameTime)
        {
            _cameraMoveDirection = Vector2.Zero;
            if (Mouse.GetState().Position.ToVector2().X < 5f)
            {
                _cameraMoveDirection += new Vector2(-1f, 0f);
            }
            if (Mouse.GetState().Position.ToVector2().X > Core.GraphicsDevice.DisplayMode.Width - 5f)
            {
                _cameraMoveDirection += new Vector2(1f, 0f);
            }
            if (Mouse.GetState().Position.ToVector2().Y < 5f)
            {
                _cameraMoveDirection += new Vector2(0f, -1f);
            }
            if (Mouse.GetState().Position.ToVector2().Y > Core.GraphicsDevice.DisplayMode.Height - 5f)
            {
                _cameraMoveDirection += new Vector2(0f, 1f);
            }
        }
        protected virtual void ClampCamera()
        {
            //float clampX = Math.Clamp(ActualCamera.Position.X, MapInstance.Bounds.Left - ActualCamera.BoundingRectangle.Width / 2f, MapInstance.Bounds.Right - ActualCamera.BoundingRectangle.Width / 2f);
            //float clampY = Math.Clamp(ActualCamera.Position.Y, MapInstance.Bounds.Top - ActualCamera.BoundingRectangle.Height / 2f, MapInstance.Bounds.Bottom - ActualCamera.BoundingRectangle.Height / 2f);
            //ActualCamera.Position = new Vector2(clampX, clampY);
        }
    }
}
