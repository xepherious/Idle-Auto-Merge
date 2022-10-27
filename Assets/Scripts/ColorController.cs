using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorController : MonoBehaviour
{
    [SerializeField] private Color _color1;
    [SerializeField] private Color _color2;
    [SerializeField] private Color _color3;
    [SerializeField] private Color _color4;
    [SerializeField] private Color _color5;

    [SerializeField] private Color _color6;
    [SerializeField] private Color _color7;
    [SerializeField] private Color _color8;
    [SerializeField] private Color _color9;
    [SerializeField] private Color _color10;

    [SerializeField] private Color _color11;
    [SerializeField] private Color _color12;
    [SerializeField] private Color _color13;
    [SerializeField] private Color _color14;
    [SerializeField] private Color _color15;

    [SerializeField] private Color _color16;
    [SerializeField] private Color _color17;
    [SerializeField] private Color _color18;
    [SerializeField] private Color _color19;
    [SerializeField] private Color _color20;

    private static Color[] _colors;

    public void Awake()
    {
        _colors = new Color[] { _color1, _color2, _color3, _color4, _color5, _color6, _color7, _color8, _color9, _color10, _color11, _color12, _color13, _color14, _color15, _color16, _color17, _color18, _color19, _color20 };
    }

    public static Color GetColorForTier(int tier)
    {

        return _colors[tier % 20];
    }
}
