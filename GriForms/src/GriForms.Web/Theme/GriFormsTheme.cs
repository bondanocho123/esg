using System.Text;
using MudBlazor;
using MudBlazor.Utilities;

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
        // Hutan malam. Dipasang lewat kelas html.dark (lihat DarkCssVariables), bukan IsDarkMode,
        // supaya juga berlaku di halaman Account yang dirender statis.
        PaletteDark = new PaletteDark
        {
            // Hijau lebih terang dengan teks gelap agar tetap kontras di atas latar gelap
            Primary = "#22C55E",
            PrimaryLighten = "#4ADE80",
            PrimaryDarken = "#16A34A",
            PrimaryContrastText = "#052E16",

            Secondary = "#38BDF8",
            SecondaryLighten = "#BAE6FD",
            SecondaryDarken = "#38BDF8",
            SecondaryContrastText = "#082F49",

            Tertiary = "#A0714F",
            TertiaryLighten = "#C4987A",
            TertiaryDarken = "#7A5235",
            TertiaryContrastText = "#FFFFFF",

            Success = "#22C55E",
            Info = "#38BDF8",
            Warning = "#F59E0B",
            Error = "#F87171",

            AppbarBackground = "#06200F",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#08180F",
            DrawerText = "#E7F5EC",
            DrawerIcon = "#9CC9AE",

            Background = "#0B1711",
            BackgroundGray = "#132219",
            Surface = "#12221A",
            TextPrimary = "#E3F1E8",
            TextSecondary = "#9DB5A6",
            LinesDefault = "#24392D",
            LinesInputs = "#3A5244",
            TableLines = "#24392D",
            Divider = "#24392D",
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

    /// <summary>
    /// Variabel --mud-palette-* dari PaletteDark, untuk blok <c>html.dark { ... }</c> di App.razor.
    /// Nama variabel sama dengan keluaran MudThemeProvider, sehingga menimpa nilai :root miliknya.
    /// </summary>
    public static string DarkCssVariables()
    {
        var p = Default.PaletteDark;
        var css = new StringBuilder();

        void Var(string name, object value) =>
            css.Append("--mud-palette-").Append(name).Append(": ")
               .Append(value is MudColor c ? c.ToString(MudColorOutputFormats.RGBA) : value).Append(';');

        void Accent(string name, MudColor color, MudColor text, string darken, string lighten)
        {
            Var(name, color);
            Var($"{name}-rgb", $"{color.R},{color.G},{color.B}");
            Var($"{name}-text", text);
            Var($"{name}-darken", darken);
            Var($"{name}-lighten", lighten);
            Var($"{name}-hover", color.SetAlpha(p.HoverOpacity));
        }

        Var("black", p.Black);
        Var("white", p.White);
        Accent("primary", p.Primary, p.PrimaryContrastText, p.PrimaryDarken, p.PrimaryLighten);
        Accent("secondary", p.Secondary, p.SecondaryContrastText, p.SecondaryDarken, p.SecondaryLighten);
        Accent("tertiary", p.Tertiary, p.TertiaryContrastText, p.TertiaryDarken, p.TertiaryLighten);
        Accent("info", p.Info, p.InfoContrastText, p.InfoDarken, p.InfoLighten);
        Accent("success", p.Success, p.SuccessContrastText, p.SuccessDarken, p.SuccessLighten);
        Accent("warning", p.Warning, p.WarningContrastText, p.WarningDarken, p.WarningLighten);
        Accent("error", p.Error, p.ErrorContrastText, p.ErrorDarken, p.ErrorLighten);
        Accent("dark", p.Dark, p.DarkContrastText, p.DarkDarken, p.DarkLighten);

        Var("text-primary", p.TextPrimary);
        Var("text-primary-rgb", $"{p.TextPrimary.R},{p.TextPrimary.G},{p.TextPrimary.B}");
        Var("text-secondary", p.TextSecondary);
        Var("text-secondary-rgb", $"{p.TextSecondary.R},{p.TextSecondary.G},{p.TextSecondary.B}");
        Var("text-disabled", p.TextDisabled);
        Var("text-disabled-rgb", $"{p.TextDisabled.R},{p.TextDisabled.G},{p.TextDisabled.B}");
        Var("action-default", p.ActionDefault);
        Var("action-default-hover", p.ActionDefault.SetAlpha(p.HoverOpacity));
        Var("action-disabled", p.ActionDisabled);
        Var("action-disabled-background", p.ActionDisabledBackground);
        Var("surface", p.Surface);
        Var("surface-rgb", $"{p.Surface.R},{p.Surface.G},{p.Surface.B}");
        Var("background", p.Background);
        Var("background-gray", p.BackgroundGray);
        Var("drawer-background", p.DrawerBackground);
        Var("drawer-text", p.DrawerText);
        Var("drawer-icon", p.DrawerIcon);
        Var("appbar-background", p.AppbarBackground);
        Var("appbar-text", p.AppbarText);
        Var("lines-default", p.LinesDefault);
        Var("lines-inputs", p.LinesInputs);
        Var("table-lines", p.TableLines);
        Var("table-striped", p.TableStriped);
        Var("table-hover", p.TableHover);
        Var("divider", p.Divider);
        Var("divider-rgb", $"{p.Divider.R},{p.Divider.G},{p.Divider.B}");
        Var("divider-light", p.DividerLight);
        Var("skeleton", p.Skeleton);
        Var("overlay-dark", p.OverlayDark);
        Var("overlay-light", p.OverlayLight);

        return css.ToString();
    }
}
