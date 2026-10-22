using MudBlazor;

namespace GriForms.Web.Theme;

/// <summary>
/// Satu-satunya sumber warna aplikasi (tema "green environment").
/// MudThemeProvider mengeluarkan nilai ini sebagai variabel CSS --mud-palette-*,
/// yang dipetakan ke warna Tailwind (bg-primary, text-secondary, dst.) di Styles/app.css.
/// </summary>
public static class GriFormsTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            // Primary: hijau daun (kontras teks putih 4.6:1)
            Primary = "#12873F",
            PrimaryLighten = "#22C55E",
            PrimaryDarken = "#0B5D2A",
            PrimaryContrastText = "#FFFFFF",

            // Secondary: biru langit/air, cukup terang sehingga teks di atasnya gelap
            Secondary = "#38BDF8",
            SecondaryLighten = "#BAE6FD",
            SecondaryDarken = "#0369A1",
            SecondaryContrastText = "#082F49",

            // Tertiary: coklat tanah
            Tertiary = "#6B3F23",
            TertiaryLighten = "#A0714F",
            TertiaryDarken = "#3F2414",
            TertiaryContrastText = "#FFFFFF",

            Success = "#12873F",
            Info = "#0284C7",
            Warning = "#D97706",
            Error = "#DC2626",

            // Hutan malam untuk app bar & sidebar
            AppbarBackground = "#0B3D22",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#0E2A1B",
            DrawerText = "#E7F5EC",
            DrawerIcon = "#9CC9AE",

            Background = "#F2F8F3",
            Surface = "#FFFFFF",
            TextPrimary = "#12261A",
            TextSecondary = "#4B6354",
            LinesDefault = "#D5E5DA",
            Divider = "#D5E5DA",
        },
        Typography = new Typography
        {
            // Ukuran dibuat ringkas: teks dasar 13px
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize = ".8125rem",
            },
            Body1 = new Body1Typography { FontSize = ".8125rem" },
            Body2 = new Body2Typography { FontSize = ".75rem" },
            Button = new ButtonTypography { FontSize = ".75rem", TextTransform = "none" },
            Caption = new CaptionTypography { FontSize = ".6875rem" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
        },
    };
}
