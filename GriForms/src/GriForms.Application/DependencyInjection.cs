using GriForms.Application.Calculations;
using GriForms.Application.Consolidation;
using GriForms.Application.Forms;
using GriForms.Application.Validation;
using GriForms.Application.Validation.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IFormDefinitionReader, JsonFormDefinitionReader>();
        services.AddScoped<FormContext>();
        services.AddScoped<IFormContext>(sp => sp.GetRequiredService<FormContext>());

        services.AddSingleton<IValidationEngine, ValidationEngine>();
        services
            .AddValidationRule<RequiredRule>()
            .AddValidationRule<RangeRule>()
            .AddValidationRule<IntegerOnlyRule>()
            .AddValidationRule<MaxLengthRule>()
            .AddValidationRule<SelectOptionRule>()
            .AddValidationRule<NotApplicableRule>();

        services.AddSingleton<CalcRegistry>();
        services.AddCalculation<RowTotalCalculation>();

        services.AddSingleton<IConsolidationCardBuilder, ConsolidationCardBuilder>();
        services
            .AddCardFactory<NarrativeCardFactory>()
            .AddCardFactory<MonthlyCardFactory>();

        return services;
    }

    public static IServiceCollection AddValidationRule<TRule>(this IServiceCollection services)
        where TRule : class, IValidationRule
        => services.AddSingleton<IValidationRule, TRule>();

    public static IServiceCollection AddCalculation<TCalc>(this IServiceCollection services)
        where TCalc : class, ICalculation
        => services.AddSingleton<ICalculation, TCalc>();

    public static IServiceCollection AddCardFactory<TFactory>(this IServiceCollection services)
        where TFactory : class, ICardFactory
        => services.AddSingleton<ICardFactory, TFactory>();
}
