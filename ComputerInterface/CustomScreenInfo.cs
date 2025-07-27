using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerInterface
{
    public class CustomScreenInfo
    {
        public string SceneName;

        public TextMeshProUGUI TextMeshProUgui;
        public Transform Transform;

        public Renderer Renderer;
        public RawImage Background;

        public Color Color
        {
            get => Background.color;
            set => Background.color = value;
        }

        public string Text
        {
            get => TextMeshProUgui.text;
            set => TextMeshProUgui.text = value;
        }

        public float FontSize
        {
            get => TextMeshProUgui.fontSize;
            set => TextMeshProUgui.fontSize = value;
        }

        public Texture BackgroundTexture
        {
            get => Background.texture;
            set => Background.texture = value;
        }
    }
}