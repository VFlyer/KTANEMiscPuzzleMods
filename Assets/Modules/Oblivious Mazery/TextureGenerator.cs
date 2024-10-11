using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextureGenerator {

    static public readonly Color[] allowedColors = new[] {
        Color.black, Color.blue, Color.green, Color.cyan, Color.red, Color.magenta, Color.yellow, Color.white, Color.clear, Color.gray };
    const string abbrevAllows = "kbgcrmyw-a";
	public Texture2D[] GenerateTextures(IEnumerable<string> encodings)
    {
		var textures = new Texture2D[encodings.Count()];
        for (var x = 0; x < encodings.Count(); x++)
        {
            var curEncoding = encodings.ElementAt(x);
            var nextTexture = new Texture2D(10, 10);
            textures[x] = nextTexture;
        }
        return textures;
    }
}
