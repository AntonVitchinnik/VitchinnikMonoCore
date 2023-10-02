using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitchinnikMonoCore.Transitions
{
    public class Transition : IGameComponent, IUpdateable
    {
        private Action<GameTime> _updateAction;
        private float _cummulativeTime;
        public Transition(Type type, Vector2 start, Vector2 target, float time, Action<Vector2> positionAccessor, float elastic = 1f, Action onExpired = null, TransitionTerminationToken terminationToken = null)
        {
            if(terminationToken != null)
            {
                Action terminate = () =>
                {
                    Core.GameInstance.Components.Remove(this);
                };
                terminate += () => terminationToken.OnTerminate -= terminate;
                terminationToken.OnTerminate += terminate;
                
                Action forceExpire = () =>
                {
                    Core.GameInstance.Components.Remove(this);
                    onExpired.Invoke();
                };
                onExpired += () => terminationToken.OnForcedExpiration -= forceExpire;
                terminationToken.OnForcedExpiration += forceExpire;              
            }
            Vector2 deltaPosition = target - start;
            switch (type)
            {
                case Type.Linear:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            positionAccessor.Invoke(start + deltaPosition * _cummulativeTime / time);
                            return;
                        }
                        positionAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.EaseIn:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            positionAccessor.Invoke(start + deltaPosition * (float)Math.Pow(_cummulativeTime / time, 2));
                            return;
                        }
                        positionAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.EaseOut:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            positionAccessor.Invoke(target - deltaPosition * (float)Math.Pow(1f - _cummulativeTime / time, 2));
                            return;
                        }
                        positionAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.EaseInOut:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            var easeOut = target - deltaPosition * (float)Math.Pow(1f - _cummulativeTime / time, 2);
                            var easeIn = start + deltaPosition * (float)Math.Pow(_cummulativeTime / time, 2);

                            positionAccessor.Invoke(Vector2.Lerp(easeOut, easeIn, _cummulativeTime / time));
                            return;
                        }
                        positionAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.ElasticIn:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            positionAccessor.Invoke(start + deltaPosition * (float)Math.Pow(_cummulativeTime / time, 2) * ((1 + elastic) * _cummulativeTime / time - elastic));
                            return;
                        }
                        positionAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
            }

        }
        public Transition(Type type, float start, float target, float time, Action<float> valueAccessor, float elastic = 1f, Action onExpired = null)
        {
            float deltaValue = target - start;
            switch (type)
            {
                case Type.Linear:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            valueAccessor.Invoke(start + deltaValue * _cummulativeTime / time);
                            return;
                        }
                        valueAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.EaseIn:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            valueAccessor.Invoke(start + deltaValue * (float)Math.Pow(_cummulativeTime / time, 2));
                            return;
                        }
                        valueAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.EaseOut:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            valueAccessor.Invoke(target - deltaValue * (float)Math.Pow(1f - _cummulativeTime / time, 2));
                            return;
                        }
                        valueAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.EaseInOut:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            var easeOut = target - deltaValue * (float)Math.Pow(1f - _cummulativeTime / time, 2);
                            var easeIn = start + deltaValue * (float)Math.Pow(_cummulativeTime / time, 2);

                            valueAccessor.Invoke(MathHelper.Lerp(easeOut, easeIn, _cummulativeTime / time));
                            return;
                        }
                        valueAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
                case Type.ElasticIn:
                    _updateAction = (gameTime) =>
                    {
                        if (_cummulativeTime + (float)gameTime.ElapsedGameTime.TotalSeconds < time)
                        {
                            _cummulativeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                            valueAccessor.Invoke(start + deltaValue * (float)Math.Pow(_cummulativeTime / time, 2) * ((1 + elastic) * _cummulativeTime / time - elastic));
                            return;
                        }
                        valueAccessor.Invoke(target);
                        onExpired?.Invoke();
                        Core.GameInstance.Components.Remove(this);
                    };
                    break;
            }

        }
        bool IUpdateable.Enabled => true;
        int IUpdateable.UpdateOrder => 1;
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public void Initialize()
        {

        }
        public void Update(GameTime gameTime)
        {
            _updateAction.Invoke(gameTime);
        }
        public enum Type
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            ElasticIn
        }
    }
}
