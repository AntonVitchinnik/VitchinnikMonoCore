﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using VitchinnikMonoCore.Content;
using System;

namespace VitchinnikMonoCore.GUI
{
    public class CommonElement : GUIElement
    {
        public CommonElement(string path, Vector2 position) : base(path, position)
        {

        }
        public CommonElement(string path, Vector2 position, ref Action<Texture2D> textureProvider) : base(path, position, ref textureProvider)
        {

        }
        public static CommonElement Button(string path, Vector2 position, string text, SpriteFont font)
        {
            var output = new CommonElement(path, position);
            var content = new ObjectContent(text, font);
            (output._view as GUIElementView).SetContent(content);
            return output;
        }
        public static CommonElement Button(string path, Vector2 position, ref Action<string> textProvider, SpriteFont font)
        {
            var output = new CommonElement(path, position);
            var content = new ObjectContent(ref textProvider, font);
            (output._view as GUIElementView).SetContent(content);
            return output;
        }
        public static CommonElement Button(string path, Vector2 position, Tooltip tooltip)
        {
            var output = new CommonElement(path, position);
            output.SetTooltip(tooltip);
            
            return output;
        }
        public static CommonElement Button(string path, ref Action<Texture2D> textureProvider, Vector2 position, string text, SpriteFont font)
        {
            var output = new CommonElement(path, position, ref textureProvider);
            var content = new ObjectContent(text, font);
            (output._view as GUIElementView).SetContent(content);
            return output;
        }
        public static CommonElement Button(string path, ref Action<Texture2D> textureProvider, Vector2 position, ref Action<string> textProvider, SpriteFont font)
        {
            var output = new CommonElement(path, position, ref textureProvider);
            var content = new ObjectContent(ref textProvider, font);
            (output._view as GUIElementView).SetContent(content);
            return output;
        }
        public static CommonElement Button(string path, ref Action<Texture2D> textureProvider, Vector2 position, Tooltip tooltip)
        {
            var output = new CommonElement(path, position, ref textureProvider);
            output.SetTooltip(tooltip);

            return output;
        }
        public static CommonElement Label(string path, Vector2 position, string text, SpriteFont font)
        {
            var output = new CommonElement(path, position);
            var content = new ObjectContent(text, font);
            (output._view as GUIElementView).SetContent(content);
            return output;
        }
        public static CommonElement Label(string path, Vector2 position, ref Action<string> textProvider, SpriteFont font)
        {
            var output = new CommonElement(path, position);
            var content = new ObjectContent(ref textProvider, font);
            (output._view as GUIElementView).SetContent(content);
            return output;
        }
    }
}
