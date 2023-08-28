using Microsoft.Xna.Framework;

namespace VitchinnikMonoCore.Content
{
    public interface IContentContainer
    {
        public void SetContent(ObjectContent content);
        public Vector2 Position { get; }
        public event Action<Vector2> PositionChanged;
        public event Action<GameTime> CallContent;
        public Vector2 Dimentions { get; }
    }
    public interface IContentPositionProvider 
    { 
        public event Action<Vector2> PositionProvider;
        public int test();
    }
}
