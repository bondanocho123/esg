using GriForms.Application.Forms;
using Microsoft.AspNetCore.Components;

namespace GriForms.Web.Components.Forms;

// Kontrak untuk semua turunan (Liskov): hormati ReadOnly, tampilkan Errors,
// baca dan tulis hanya lewat State, dan jangan melempar untuk nilai kosong.
public abstract class FieldBase : ComponentBase
{
    [Parameter, EditorRequired] public QuestionDef Question { get; set; } = null!;
    [Parameter, EditorRequired] public FormState State { get; set; } = null!;
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    protected bool HasErrors => Errors.Count > 0;
}
