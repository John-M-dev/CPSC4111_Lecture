using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorStrategy
{
    Color SelectColor();
}
public class RandomColor : IColorStrategy
{
    public Color SelectColor()
    {
        return Random.ColorHSV();
    }
}

public class PreselectedColors : IColorStrategy
{
    private Color[] colors = {Color.blue, Color.green, Color.red, Color.magenta, Color.cyan, Color.yellow, 
        Color.gray, Color.white, Color.black};
    private int index;

    public PreselectedColors()
    {
        index = 0;
    }
    public Color SelectColor()
    {
        Color nextColor = colors[index];
        index = (index + 1) % colors.Length;
        return nextColor;
    }
}
