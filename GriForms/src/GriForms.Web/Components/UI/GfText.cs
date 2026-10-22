using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using MudBlazor;

namespace GriForms.Web.Components.UI;

/// <summary>
/// Teks dengan skala tipografi ringkas GriForms. Typo menentukan tag (h1-h6, p, span) dan ukuran;
/// Variant mewarnai teks, Muted untuk teks keterangan.
/// </summary>
public sealed class GfText : ComponentBase
{
    [Parameter] public Typo Typo { get; set; } = Typo.body1;
    [Parameter] public GfVariant? Variant { get; set; }
    [Parameter] public bool Muted { get; set; }
    [Parameter] public Align Align { get; set; } = Align.Inherit;
    [Parameter] public bool GutterBottom { get; set; }
    [Parameter] public bool Inline { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, Tag);
        builder.AddAttribute(1, "class", $"{TypoClass} {ColorClass} {AlignClass} {(GutterBottom ? "mb-1.5" : "m-0")} {Class}");
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }

    private string Tag => Typo switch
    {
        Typo.h1 => "h1",
        Typo.h2 => "h2",
        Typo.h3 => "h3",
        Typo.h4 => "h4",
        Typo.h5 => "h5",
        Typo.h6 => "h6",
        _ => Inline ? "span" : "p",
    };

    private string TypoClass => Typo switch
    {
        Typo.h1 => "text-2xl font-bold tracking-tight",
        Typo.h2 => "text-xl font-semibold tracking-tight",
        Typo.h3 => "text-lg font-semibold",
        Typo.h4 => "text-base font-semibold",
        Typo.h5 => "text-sm font-semibold",
        Typo.h6 => "text-[13px] font-semibold",
        Typo.subtitle1 => "text-[13px] font-medium",
        Typo.subtitle2 => "text-xs font-medium",
        Typo.body2 => "text-xs",
        Typo.caption => "text-[11px]",
        Typo.overline => "text-[10px] font-semibold tracking-wider uppercase",
        Typo.button => "text-xs font-semibold",
        _ => "text-[13px]",
    };

    private string ColorClass => Muted ? "text-muted" : Variant is { } v ? GfStyles.Text(v) : "";

    private string AlignClass => Align switch
    {
        Align.Left or Align.Start => "text-left",
        Align.Center => "text-center",
        Align.Right or Align.End => "text-right",
        Align.Justify => "text-justify",
        _ => "",
    };
}
