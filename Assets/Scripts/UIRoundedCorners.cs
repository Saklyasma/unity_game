using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class UIRoundedCorners : MonoBehaviour
{
    [Range(0, 200)]
    public int radius = 40;

    private void OnEnable() => Refresh();
    private void OnValidate() => Refresh();

    void Refresh()
    {
        var image = GetComponent<Image>();
        image.sprite = GenerateRoundedSprite(
            Mathf.RoundToInt(GetComponent<RectTransform>().rect.width > 0
                ? GetComponent<RectTransform>().rect.width : 300),
            Mathf.RoundToInt(GetComponent<RectTransform>().rect.height > 0
                ? GetComponent<RectTransform>().rect.height : 200),
            radius
        );
    }

    Sprite GenerateRoundedSprite(int w, int h, int r)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color fill = Color.white;
        Color empty = Color.clear;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, IsRounded(x, y, w, h, r) ? fill : empty);
            }
        }

        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f)
        );
    }

    bool IsRounded(int x, int y, int w, int h, int r)
    {
        // Corners
        if (x < r && y < r)
            return Vector2.Distance(new Vector2(x, y), new Vector2(r, r)) <= r;
        if (x > w - r - 1 && y < r)
            return Vector2.Distance(new Vector2(x, y), new Vector2(w - r - 1, r)) <= r;
        if (x < r && y > h - r - 1)
            return Vector2.Distance(new Vector2(x, y), new Vector2(r, h - r - 1)) <= r;
        if (x > w - r - 1 && y > h - r - 1)
            return Vector2.Distance(new Vector2(x, y), new Vector2(w - r - 1, h - r - 1)) <= r;

        return true;
    }
}
