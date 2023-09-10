using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VitchinnikMonoCore
{
    public static class MouseHandler
    {
        private static Action<GameTime> _updateAction;
        private static MouseState _actualMouseState;
        /// <summary>
        /// Обработчик изменения состояния мыши
        /// </summary>
        /// <param name="gameTime">Игровое время</param>
        /// <param name="previousMouseState">Предыдущее состояние мыши</param>
        /// <param name="newMouseState">Новое состояние мыши</param>
        public delegate void MouseStateHandler(GameTime gameTime, MouseState previousMouseState, MouseState newMouseState);
        /// <summary>
        /// Событи вызываемое при изменении состояния мыши
        /// </summary>
        public static event MouseStateHandler MouseStateChanged;
        /// <summary>
        /// Событие, вызываемое при нажатии ЛКМ
        /// </summary>
        public static event Action LMBPressed;
        /// <summary>
        /// Событие, вызываемое при нажатии ПКМ
        /// </summary>
        public static event Action RMBPressed;
        public static Delegate[] RMBPressInvokationList => RMBPressed.GetInvocationList();
        /// <summary>
        /// Событие, вызываемое при отпускании ЛКМ
        /// </summary>
        public static event Action LMBReleased;
        /// <summary>
        /// Событие, вызываемое при отпускании ПКМ
        /// </summary>
        public static event Action RMBReleased;
        public static event Action<GameObject> BindCalled;
        public static event Action<GameObject> UnbindCalled;
        /// <summary>
        /// Обработчик изменения скролла
        /// </summary>
        /// <param name="gameTime">Игровое время</param>
        /// <param name="deltaScroll">Изменение скролла</param>
        public delegate void ScrollHandler(GameTime gameTime, int deltaScroll);
        /// <summary>
        /// Событие, вызываемое при изменении скролла
        /// </summary>
        public static event ScrollHandler ScrollChanged;
        /// <summary>
        /// Обработчки изменения позиции мыши. Необходимость под вопросом.
        /// </summary>
        /// <param name="gameTime"></param>
        public delegate void PositionHandler(GameTime gameTime);
        /// <summary>
        /// Событие, вызываемое при изменеии позиции мыши. Необходимость под вопросом.
        /// </summary>
        public static event PositionHandler PositionChanged;
        /// <summary>
        /// Позиция курсора мыши в пространстве экрана
        /// </summary>
        public static Vector2 Position => _actualMouseState.Position.ToVector2();
        /// <summary>
        /// Список объектов, на которых находится курсор мыши
        /// </summary>
        public static List<GameObject> HoveredObjects { get; private set; }
        public static GameObject LastBindedObject { get; private set; }
        /// <summary>
        /// Инициализация настроек обработчика мыши
        /// </summary>
        public static void Initialize()
        {
            HoveredObjects = new List<GameObject>();
            _actualMouseState = Mouse.GetState();
            Enable();
            MouseStateChanged = (GameTime gameTime, MouseState previousMouseState, MouseState newMouseState) =>
            {
                CheckLMBStateChanged(previousMouseState.LeftButton - newMouseState.LeftButton);
                CheckRMBStateChanged(previousMouseState.RightButton - newMouseState.RightButton);
                CheckScrollChanged(gameTime, newMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue);
                CheckPosition(gameTime, previousMouseState, newMouseState);
                CheckCollisions(newMouseState.Position.ToVector2());
            };
        }
        /// <summary>
        /// Метод, реализующий логику обработки курсора мыши
        /// </summary>
        /// <param name="gameTime">Время прошедшее с последнего вызова метода обновления</param>
        /// <remarks>Данный метод необходимо поместить в метод Update(GameTime) основного игрового класса</remarks>
        public static void Update(GameTime gameTime)
        {
            _updateAction?.Invoke(gameTime);
        }
        private static void DoUpdate(GameTime gameTime)
        {
            MouseState stateToCheck = Mouse.GetState();
            if (_actualMouseState.Equals(stateToCheck))
                return;
            MouseStateChanged?.Invoke(gameTime, _actualMouseState, stateToCheck);
            _actualMouseState = stateToCheck;
        }
        private static void UpdateCollisions()
        {
            CheckCollisions(Position);
        }
        private static void CheckPosition(GameTime gameTime, MouseState previousMouseState, MouseState newMouseState)
        {
            if ((previousMouseState.Position.ToVector2() - newMouseState.Position.ToVector2()).Equals(Vector2.Zero))
                return;
            PositionChanged?.Invoke(gameTime);
        }
        private static void CheckCollisions(Vector2 mousePosition)
        {
            foreach (GameObject graphicsObject in Core.GameInstance.Components.OfType<GameObject>())
            {
                if (graphicsObject.Contains(mousePosition))
                {
                    if (!HoveredObjects.Contains(graphicsObject))
                        HoveredObjects.Add(graphicsObject);
                    continue;
                }
                HoveredObjects.Remove(graphicsObject);
            }
            if (HoveredObjects.Count > 0)
            {
                HoveredObjects.Sort(new Comparison<GameObject>(_SortHovered));
                if (!HoveredObjects[0].Equals(LastBindedObject))
                {
                    LastBindedObject?.UnbindClickInvokeAction();
                    HoveredObjects[0].BindClickInvokeAction();
                    LastBindedObject = HoveredObjects[0];
                }
                return;
            }
            LastBindedObject?.UnbindClickInvokeAction();
            LastBindedObject = null;
        }
        private static int _SortHovered(GameObject object1, GameObject object2) => object2.DrawOrder - object1.DrawOrder;
        private static void CheckLMBStateChanged(int stateChange)
        {
            switch (stateChange)
            {
                case 1:
                    LMBReleased?.Invoke();
                    break;
                case -1:
                    LMBPressed?.Invoke();
                    break;
                default:
                    break;
            }
        }
        private static void CheckRMBStateChanged(int stateChange)
        {
            switch (stateChange)
            {
                case 1:
                    RMBReleased?.Invoke();
                    break;
                case -1:
                    RMBPressed?.Invoke();
                    break;
                default:
                    break;
            }
        }
        private static void CheckScrollChanged(GameTime gameTime, int deltaScroll)
        {
            if (deltaScroll == 0)
                return;
            ScrollChanged?.Invoke(gameTime, deltaScroll);
        }
        /// <summary>
        /// Отрисовка кастомного курсора мыши
        /// </summary>
        public static void Draw()
        {

        }
        /// <summary>
        /// Установка события, при котором необходимо обновить коллизии мыши с объектами
        /// </summary>
        /// <param name="updateCollisionProvider">Событие, при котором нужно обновить коллизии мыши с объектами</param>
        public static void SetUpdtateCollisionProvider(ref Action updateCollisionProvider)
        {
            updateCollisionProvider += UpdateCollisions;
        }
        public static void Enable()
        {
            if (_updateAction?.GetInvocationList().Contains(new Action<GameTime>(DoUpdate)) == true)
                return;
            _updateAction += DoUpdate;
        }
        public static void Disable()
        {
            if (!_updateAction?.GetInvocationList().Contains(new Action<GameTime>(DoUpdate)) == true)
                return;
            _updateAction -= DoUpdate;
        }
        public static void Reset()
        {
            HoveredObjects = new List<GameObject>();
            LastBindedObject?.UnbindClickInvokeAction();
            LastBindedObject = null;
            PositionChanged = null;
            LMBPressed = null;
            LMBReleased = null;
            ScrollChanged = null;
            RMBPressed = null;
            RMBReleased = null;
        }
    }
}
