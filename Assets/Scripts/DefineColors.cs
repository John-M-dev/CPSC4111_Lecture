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
    private Color[] colors = {  Color.blue, Color.green, Color.red, Color.magenta, Color.cyan, Color.yellow, Color.gray, Color.white, Color.black };
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

public class AstreaChoices : IColorStrategy
{
    private int index;

    public AstreaChoices()
    {
        index = 1;
    }
    public Color SelectColor()
    {
        return Color.HSVToRGB(Hue(index++)/360, Random.Range(0.50f, 1), Random.Range(0.45f, 1));
    }

    public float Hue(int index)
    {
        int p = 2;
        return (360 * (index - Mathf.Pow(p, Mathf.Floor(Mathf.Log(-1 + index) / Mathf.Log(p))) +
        Mathf.Floor((-1 + index - Mathf.Pow(p, Mathf.Floor(Mathf.Log(-1 + index) / Mathf.Log(p)))) / (-1 + p)))) /
    Mathf.Pow(p, Mathf.Ceil(Mathf.Log(index) / Mathf.Log(p))) % 360;

        /*return (360 * Mathf.Pow(p, -Mathf.Ceil(Mathf.Log(index, p))) * (-Mathf.Pow(p, Mathf.Log(index - 1, p)) + Mathf.Floor((-Mathf.Pow(p, Mathf.Log(index - 1, p)) + index - 1) / (p - 1)) + index)) % 360;*/
    }
}
