using Bunit;
using GriForms.Application.Forms;
using GriForms.Tests.Support;
using GriForms.Web.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace GriForms.Tests.Forms;

public class DynamicFormTests : MudBunitContext
{
    private static readonly FormDefinition Definition = new JsonFormDefinitionReader().Read(SampleDefinitions.Topic403);

    public DynamicFormTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddFormFields();
        Services.AddSingleton<IFormContext>(new FakeFormContext(2026));
        Render<MudBlazor.MudPopoverProvider>();
    }

    [Fact]
    public void Renders_every_input_type_from_definition()
    {
        var state = new FormState();
        state.Set("403-9-B", null, new AnswerValue("true", null));

        var cut = Render<DynamicForm>(p => p
            .Add(x => x.Definition, Definition)
            .Add(x => x.State, state));

        foreach (var q in Definition.Disclosures.SelectMany(d => d.Questions))
            Assert.Contains(q.Label, cut.Markup);
    }

    [Fact]
    public async Task Hides_question_until_visible_when_is_met()
    {
        var state = new FormState();
        var cut = Render<DynamicForm>(p => p
            .Add(x => x.Definition, Definition)
            .Add(x => x.State, state));

        Assert.DoesNotContain("Sumber data", cut.Markup);

        await cut.InvokeAsync(() => state.Set("403-9-B", null, new AnswerValue("true", null)));

        cut.WaitForAssertion(() => Assert.Contains("Sumber data", cut.Markup));
    }
}
