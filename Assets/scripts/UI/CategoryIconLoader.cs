using System.Collections.Generic;
using UnityEngine;

public class CategoryIconLoader : MonoBehaviour
{
    private static Dictionary<string, Sprite> icons;

    public static void Load()
    {
        icons = new Dictionary<string, Sprite>();
        var sprites = Resources.LoadAll<Sprite>("Sprites/Category");

        foreach (var s in sprites)
            icons[s.name] = s;
    }

    public static Sprite GetIcon(string name)
    {
        if (icons == null) Load();
        return icons[name];
    }

    public static Sprite GetDefaultIcon()
    {
        if (icons == null) Load();
        return icons["Еда"];
    }

    public static List<string> GetAllIcons()
    {
        if (icons == null) Load();
        return new List<string>(icons.Keys);
    }
}
