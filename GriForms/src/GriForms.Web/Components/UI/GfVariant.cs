namespace GriForms.Web.Components.UI;

/// <summary>Varian warna untuk semua komponen Gf*.</summary>
public enum GfVariant
{
    Primary,
    Secondary,
    Tertiary,
}

/// <summary>Gaya isi komponen: Filled (solid), Soft (tint lembut), Outlined, Text.</summary>
public enum GfAppearance
{
    Filled,
    Soft,
    Outlined,
    Text,
}

/// <summary>Ukuran ringkas; Md adalah default.</summary>
public enum GfSize
{
    Sm,
    Md,
    Lg,
}

/// <summary>
/// Peta varian ke kelas Tailwind. Semua kelas ditulis utuh (tanpa interpolasi)
/// agar terdeteksi oleh scanner Tailwind (@source "../Components" di Styles/app.css).
/// </summary>
public static class GfStyles
{
    public static string Button(GfVariant variant, GfAppearance appearance) => (variant, appearance) switch
    {
        (GfVariant.Primary, GfAppearance.Filled) => "bg-primary text-on-primary shadow-sm shadow-primary/30 hover:bg-primary-dark",
        (GfVariant.Primary, GfAppearance.Soft) => "bg-primary/10 text-primary-dark hover:bg-primary/20",
        (GfVariant.Primary, GfAppearance.Outlined) => "border-primary/60 text-primary hover:bg-primary/10",
        (GfVariant.Primary, GfAppearance.Text) => "text-primary hover:bg-primary/10",

        (GfVariant.Secondary, GfAppearance.Filled) => "bg-secondary text-on-secondary shadow-sm shadow-secondary/40 hover:bg-secondary-light",
        (GfVariant.Secondary, GfAppearance.Soft) => "bg-secondary/15 text-secondary-dark hover:bg-secondary/25",
        (GfVariant.Secondary, GfAppearance.Outlined) => "border-secondary-dark/50 text-secondary-dark hover:bg-secondary/15",
        (GfVariant.Secondary, GfAppearance.Text) => "text-secondary-dark hover:bg-secondary/15",

        (GfVariant.Tertiary, GfAppearance.Filled) => "bg-tertiary text-on-tertiary shadow-sm shadow-tertiary/30 hover:bg-tertiary-dark",
        (GfVariant.Tertiary, GfAppearance.Soft) => "bg-tertiary/10 text-tertiary hover:bg-tertiary/20",
        (GfVariant.Tertiary, GfAppearance.Outlined) => "border-tertiary/50 text-tertiary hover:bg-tertiary/10",
        (GfVariant.Tertiary, GfAppearance.Text) => "text-tertiary hover:bg-tertiary/10",

        _ => "",
    };

    public static string ButtonSize(GfSize size) => size switch
    {
        GfSize.Sm => "h-7 gap-1 px-2.5 text-[11px]",
        GfSize.Lg => "h-9 gap-2 px-4 text-[13px]",
        _ => "h-8 gap-1.5 px-3 text-xs",
    };

    public static string Chip(GfVariant variant, GfAppearance appearance) => (variant, appearance) switch
    {
        (GfVariant.Primary, GfAppearance.Filled) => "bg-primary text-on-primary",
        (GfVariant.Primary, GfAppearance.Outlined) => "border border-primary/50 text-primary",
        (GfVariant.Primary, _) => "bg-primary/10 text-primary-dark",

        (GfVariant.Secondary, GfAppearance.Filled) => "bg-secondary text-on-secondary",
        (GfVariant.Secondary, GfAppearance.Outlined) => "border border-secondary-dark/50 text-secondary-dark",
        (GfVariant.Secondary, _) => "bg-secondary/15 text-secondary-dark",

        (GfVariant.Tertiary, GfAppearance.Filled) => "bg-tertiary text-on-tertiary",
        (GfVariant.Tertiary, GfAppearance.Outlined) => "border border-tertiary/50 text-tertiary",
        (GfVariant.Tertiary, _) => "bg-tertiary/10 text-tertiary",

        _ => "",
    };

    /// <summary>Warna teks aksen yang terbaca di atas latar putih.</summary>
    public static string Text(GfVariant variant) => variant switch
    {
        GfVariant.Secondary => "text-secondary-dark",
        GfVariant.Tertiary => "text-tertiary",
        _ => "text-primary",
    };

    /// <summary>Latar tint lembut + teks aksen, untuk tile ikon.</summary>
    public static string SoftTile(GfVariant variant) => variant switch
    {
        GfVariant.Secondary => "bg-secondary/15 text-secondary-dark",
        GfVariant.Tertiary => "bg-tertiary/10 text-tertiary",
        _ => "bg-primary/10 text-primary",
    };

    /// <summary>Gradien aksen (garis kartu, tile ikon header).</summary>
    public static string Gradient(GfVariant variant) => variant switch
    {
        GfVariant.Secondary => "from-secondary-dark to-secondary",
        GfVariant.Tertiary => "from-tertiary-dark to-tertiary-light",
        _ => "from-primary-dark via-primary to-primary-light",
    };

    public static string IconSize(GfSize size) => size switch
    {
        GfSize.Sm => "size-4",
        GfSize.Lg => "size-6",
        _ => "size-5",
    };
}
