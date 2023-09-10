using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace VitchinnikMonoCore.GUI
{
    public class ProgressBar : GUIElement
    {
        public float Progress => (_view as ProgressBarView).Progress;
        public ProgressBar(string path, Vector2 position, ref Action<float> progressProvider, string progressStyle, Vector2 progressOffset) : base(path, position)
        {
            _view = new ProgressBarView(this, path, position, ref progressProvider, progressStyle, progressOffset);
        }
        protected class ProgressBarView : GUIElementView
        {
            private float _progress;
            public float Progress => _progress;
            private Texture2D _progressTexture;
            private Vector2 _progressOffset;
            public ProgressBarView(ProgressBar controlsSource, string path, Vector2 position, ref Action<float> progressProvider, string progressStyle, Vector2 progressOffset): base(controlsSource, path, position)
            {
                progressProvider += (float progress) => _progress = progress;
                _progressTexture = Core.ContentManagerInstance.Load<Texture2D>(progressStyle);
                _progressOffset = progressOffset;
            }
            protected override void Draw(GameTime gameTime)
            {
                Core.SpriteBatchInstance.Begin(samplerState: SamplerState.PointClamp);
                Core.SpriteBatchInstance.Draw(_texture, Position + _relatedPosition, null, Color.White, 0f, _offset, 1f, 0, 1);
                Core.SpriteBatchInstance.Draw(_progressTexture, Position + _relatedPosition + _progressOffset, null, Color.White, 0f, Vector2.Zero, new Vector2(Progress, 1f), 0, 1);
                Core.SpriteBatchInstance.End();
            }
        }
    }
}
