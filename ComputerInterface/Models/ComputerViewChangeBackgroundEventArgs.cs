using UnityEngine;

namespace ComputerInterface.Models;

public class ComputerViewChangeBackgroundEventArgs(Texture texture, Color? imageColor = null) {
    public readonly Texture Texture = texture;
    public Color? ImageColor = imageColor;

    public delegate void ComputerViewChangeBackgroundEventHandler(ComputerViewChangeBackgroundEventArgs args);
}