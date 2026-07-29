using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerInterface.Models;

public class ComputerScreenInfo {
    public TextMeshProUGUI ComputerText;

    public RawImage ComputerBackground;

    public Color Color {
        get => ComputerBackground.color;
        set => ComputerBackground.color = value;
    }

    public string Text {
        get => ComputerText.text;
        set => ComputerText.text = value;
    }

    public float FontSize {
        get => ComputerText.fontSize;
        set => ComputerText.fontSize = value;
    }

    public Texture BackgroundTexture {
        get => ComputerBackground.texture;
        set => ComputerBackground.texture = value;
    }
}