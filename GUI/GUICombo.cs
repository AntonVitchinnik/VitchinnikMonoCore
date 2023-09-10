using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Collections;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace VitchinnikMonoCore.GUI
{
    public class GUICombo : GUIElement, IList<GUIElement>
    {
        private List<GUIElement> _elements;
        public GUICombo() : base() { }
        /// <summary>
        /// Конструктор графического пользовательского интерфейса с предзаданным фоном
        /// </summary>
        /// <param name="path">Путь к файлу фона</param>
        /// <param name="vector">Точка привязки пользовательского интерфейса</param>
        public GUICombo(string path, Vector2 vector) : base(path, vector)
        {

        }
        public GUICombo(string path, Vector2 vector, ref Action<Texture2D> textureProvider) : base(path, vector, ref textureProvider)
        {

        }
        public GUIElement this[int index] { get => ((IList<GUIElement>)_elements)[index]; set => ((IList<GUIElement>)_elements)[index] = value; }
        public int Count => ((ICollection<GUIElement>)_elements).Count;
        public bool IsReadOnly => ((ICollection<GUIElement>)_elements).IsReadOnly;
        public void Add(GUIElement item) => ((ICollection<GUIElement>)_elements).Add(item);
        public void Clear() => ((ICollection<GUIElement>)_elements).Clear();
        public bool Contains(GUIElement item) => ((ICollection<GUIElement>)_elements).Contains(item);
        public void CopyTo(GUIElement[] array, int arrayIndex) => ((ICollection<GUIElement>)_elements).CopyTo(array, arrayIndex);
        public IEnumerator<GUIElement> GetEnumerator() => ((IEnumerable<GUIElement>)_elements).GetEnumerator();
        public int IndexOf(GUIElement item) => ((IList<GUIElement>)_elements).IndexOf(item);
        public void Insert(int index, GUIElement item) => ((IList<GUIElement>)_elements).Insert(index, item);
        public bool Remove(GUIElement item) => ((ICollection<GUIElement>)_elements).Remove(item);
        public void RemoveAt(int index) => ((IList<GUIElement>)_elements).RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_elements).GetEnumerator();
    }
}
