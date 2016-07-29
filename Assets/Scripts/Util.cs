using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Drawing;

public class Util {

    public static Texture2D LoadImageFromPath(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            //bmp
            if (String.Equals(Path.GetExtension(filePath).ToLower(), ".bmp"))
            {
                Bitmap bitmap = new Bitmap(filePath);
                tex = new Texture2D(bitmap.Width, bitmap.Height);
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        System.Drawing.Color pixel = bitmap.GetPixel(x, y);
                        float r = Normalize(pixel.R, 0f, 255f);
                        float g = Normalize(pixel.G, 0f, 255f);
                        float b = Normalize(pixel.B, 0f, 255f);
                        tex.SetPixel(x, bitmap.Height - y - 1, new UnityEngine.Color(r, g, b));
                    }
                }
            }
            //png, jpg
            else
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(256, 256);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
        }
        if(tex!= null)
            tex.Apply();

        return tex;
    }

    public static float Normalize(float current, float min, float max)
    {
        return (current - min) / (max - min);
    }

    //old bga used Black as Transparent Color
    public static Texture2D BlackToTransparent(Texture2D tex)
    {
        UnityEngine.Color[] pix = tex.GetPixels(0, 0, tex.width, tex.height);
        for (int p = 0; p < pix.Length; p++)
        {
            if (pix[p].r == 0 && pix[p].g == 0 && pix[p].b == 0)
            {
                pix[p].a = 0;
            }
        }
        tex.SetPixels(0, 0, tex.width, tex.height, pix);
        tex.Apply();
        return tex;
    }
}
