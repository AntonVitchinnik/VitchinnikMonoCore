using Microsoft.Xna.Framework;
using System;

namespace VitchinnikMonoCore.Content
{
    public interface IContentContainer
    {
        public void SetContent(ObjectContent content, Vector2? margin = null);
        public Vector2 Position { get; }
        public event Action<Vector2> PositionChanged;
        public event Action<GameTime> CallContent;
        public Vector2 Dimentions { get; }
    }
    public interface IContentPositionProvider 
    { 
        public event Action<Vector2> PositionProvider;
        internal Vector2 GetPosition();
    }
}
