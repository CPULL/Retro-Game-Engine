// Based on code from AForge Image Processing Library
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Color quantization tools.
/// </summary>
///
/// <remarks><para>The class contains methods aimed to simplify work with color quantization
/// algorithms implementing <see cref="IColorQuantizer"/> interface. Using its methods it is possible
/// to calculate reduced color palette for the specified image or reduce colors to the specified number.</para>
/// 
/// <para>Sample usage:</para>
/// <code>
/// // instantiate the images' color quantization class
/// ColorImageQuantizer ciq = new ColorImageQuantizer( new MedianCutQuantizer( ) );
/// // get 16 color palette for a given image
/// Color[] colorTable = ciq.CalculatePalette( image, 16 );
/// 
/// // ... or just reduce colors in the specified image
/// Bitmap newImage = ciq.ReduceColors( image, 16 );
/// </code>
/// 
/// <para><b>Initial image:</b></para>
/// <img src="img/imaging/sample1.jpg" width="480" height="361" />
/// <para><b>Result image:</b></para>
/// <img src="img/imaging/reduced_colors.png" width="480" height="361" />
/// </remarks>
///
public class ColorImageQuantizer {
  private IColorQuantizer quantizer;
  private bool useCaching = false;

  /// <summary>
  /// Color quantization algorithm used by this class to build color palettes for the specified images.
  /// </summary>
  /// 
  public IColorQuantizer Quantizer {
    get { return quantizer; }
    set { quantizer = value; }
  }

  /// <summary>
  /// Use color caching during color reduction or not.
  /// </summary>
  /// 
  /// <remarks><para>The property has effect only for methods like <see cref="ReduceColors(Bitmap, int)"/> and
  /// specifies if internal cache of already processed colors should be used or not. For each pixel in the original
  /// image the color reduction routine does search in target color palette to find the best matching color.
  /// To avoid doing the search again and again for already processed colors, the class may use internal dictionary
  /// which maps colors of original image to indexes in target color palette.
  /// </para>
  /// 
  /// <para><note>The property provides a trade off. On one hand it may speedup color reduction routine, but on another
  /// hand it increases memory usage. Also cache usage may not be efficient for very small target color tables.</note></para>
  /// 
  /// <para>Default value is set to <see langword="false"/>.</para>
  /// </remarks>
  /// 
  public bool UseCaching {
    get { return useCaching; }
    set { useCaching = value; }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ColorImageQuantizer"/> class.
  /// </summary>
  /// 
  /// <param name="quantizer">Color quantization algorithm to use for processing images.</param>
  /// 
  public ColorImageQuantizer(IColorQuantizer quantizer) {
    this.quantizer = quantizer;
  }


  /// <summary>
  /// Calculate reduced color palette for the specified image.
  /// </summary>
  /// 
  /// <param name="image">Image to calculate palette for.</param>
  /// <param name="paletteSize">Palette size to calculate.</param>
  /// 
  /// <returns>Return reduced color palette for the specified image.</returns>
  /// 
  /// <remarks><para>The method processes the specified image and feeds color value of each pixel
  /// to the specified color quantization algorithm. Finally it returns color palette built by
  /// that algorithm.</para></remarks>
  ///
  /// <exception cref="UnsupportedImageFormatException">Unsupported format of the source image - it must 24 or 32 bpp color image.</exception>
  ///
  public Color32[] CalculatePalette(Texture2D image, int paletteSize) {
    quantizer.Clear();

    int width = image.width;
    int height = image.height;
    Color32[] cols = image.GetPixels32();
    for (int y = 0; y < height; y++) {
      for (int x = 0; x < width; x++) {
        quantizer.AddColor(cols[x + width * y]);
      }
    }
    return quantizer.GetPalette(paletteSize);
  }


  /// <summary>
  /// Create an image with reduced number of colors.
  /// </summary>
  /// 
  /// <param name="image">Source image to process.</param>
  /// <param name="paletteSize">Number of colors to get in the output image, [2, 256].</param>
  /// 
  /// <returns>Returns image with reduced number of colors.</returns>
  /// 
  /// <remarks><para>The method creates an image, which looks similar to the specified image, but contains
  /// reduced number of colors. First, target color palette is calculated using <see cref="CalculatePalette(UnmanagedImage, int)"/>
  /// method and then a new image is created, where pixels from the given source image are substituted by
  /// best matching colors from calculated color table.</para>
  /// 
  /// <para><note>The output image has 4 bpp or 8 bpp indexed pixel format depending on the target palette size -
  /// 4 bpp for palette size 16 or less; 8 bpp otherwise.</note></para>
  /// </remarks>
  /// 
  /// <exception cref="UnsupportedImageFormatException">Unsupported format of the source image - it must 24 or 32 bpp color image.</exception>
  /// <exception cref="ArgumentException">Invalid size of the target color palette.</exception>
  /// 
  public Texture2D ReduceColors(Texture2D image, int paletteSize) {
    if ((paletteSize < 2) || (paletteSize > 254)) {
      throw new ArgumentException("Invalid size of the target color palette.");
    }
    return ReduceColors(image, CalculatePalette(image, paletteSize));
  }


  /// <summary>
  /// Create an image with reduced number of colors using the specified palette.
  /// </summary>
  /// 
  /// <param name="image">Source image to process.</param>
  /// <param name="palette">Target color palette. Must contatin 2-256 colors.</param>
  /// 
  /// <returns>Returns image with reduced number of colors.</returns>
  /// 
  /// <remarks><para>The method creates an image, which looks similar to the specified image, but contains
  /// reduced number of colors. Is substitutes every pixel of the source image with the closest matching color
  /// in the specified paletter.</para>
  /// 
  /// <para><note>The output image has 4 bpp or 8 bpp indexed pixel format depending on the target palette size -
  /// 4 bpp for palette size 16 or less; 8 bpp otherwise.</note></para>
  /// </remarks>
  /// 
  /// <exception cref="UnsupportedImageFormatException">Unsupported format of the source image - it must 24 or 32 bpp color image.</exception>
  /// <exception cref="ArgumentException">Invalid size of the target color palette.</exception>
  /// 
  public Texture2D ReduceColors(Texture2D image, Color32[] palette) {
    if ((palette.Length < 2) || (palette.Length > 256)) {
      throw new ArgumentException("Invalid size of the target color palette.");
    }

    paletteToUse = palette;
    cache.Clear();

    // get image size
    int width = image.width;
    int height = image.height;
    Color32[] data = image.GetPixels32();

    // create destination image
    Texture2D destImage = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    // for each line
    for (int y = 0; y < height; y++) {
      // for each pixels
      for (int x = 0; x < width; x++) {
        // get color from palette, which is the closest to current pixel's value
        byte colorIndex = (byte)GetClosestColor(data[x + width * y]);

        // write color index as pixel's value to destination image
        data[x + width * y] = paletteToUse[colorIndex];
      }
    }
    destImage.SetPixels32(data);

    return destImage;
  }

  #region Helper methods
  [NonSerialized]
  private Color32[] paletteToUse;
  [NonSerialized]
  readonly private Dictionary<Color32, int> cache = new Dictionary<Color32, int>();

  // Get closest color from palette to specified color
  private int GetClosestColor(Color32 color) {
    if ((useCaching) && (cache.ContainsKey(color))) {
      return cache[color];
    }

    int colorIndex = 0;
    int minError = int.MaxValue;

    for (int i = 0, n = paletteToUse.Length; i < n; i++) {
      int dr = color.r - paletteToUse[i].r;
      int dg = color.g - paletteToUse[i].g;
      int db = color.b - paletteToUse[i].b;
      int da = color.a - paletteToUse[i].a;

      int error = dr * dr + dg * dg + db * db + da * da;

      if (error < minError) {
        minError = error;
        colorIndex = (byte)i;
      }
    }

    if (useCaching) {
      cache.Add(color, colorIndex);
    }

    return colorIndex;
  }
  #endregion
}
