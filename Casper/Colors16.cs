using Godot;

namespace Casper
{
    public partial class Colors16 : Node
    {
        public static Color FromColorIndex16(ColorIndex16 i)
        {
            return Palette16[(int)i];
        }

        // https://en.wikipedia.org/wiki/ZX_Spectrum_graphic_modes#Colour_palette
        public static Color[] Palette16 =
        {
            Color.Color8(0, 0, 0), //Black
            Color.Color8(1, 0, 206), // Blue
            Color.Color8(207, 1, 0), // Red
            Color.Color8(207, 1, 206), // Magenta
            Color.Color8(0, 207, 15), // Green
            Color.Color8(1, 207, 207), // Cyan
            Color.Color8(207, 207, 15), // Yellow
            Color.Color8(207, 207, 207), // White
            Color.Color8(0, 0, 0), // Bright Black
            Color.Color8(2, 0, 253), // Bright Blue
            Color.Color8(255, 02, 01), // Bright Red
            Color.Color8(255, 2, 253), // Bright Magenta
            Color.Color8(0, 255, 28), // Bright Green
            Color.Color8(2, 255, 255), // Bright Cyan
            Color.Color8(255, 255, 29), // Bright Yellow
            Color.Color8(255, 255, 255), // Bright White
        };
    }
}
