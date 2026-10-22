namespace GriForms.Application.Calculations;

// Kalkulasi baru cukup berupa kelas ICalculation baru dan satu registrasi DI (AddCalculation)
public class CalcRegistry
{
    private readonly IReadOnlyDictionary<string, ICalculation> _byRef;

    public CalcRegistry(IEnumerable<ICalculation> all) => _byRef = all.ToDictionary(c => c.Ref);

    public bool Contains(string calcRef) => _byRef.ContainsKey(calcRef);

    public ICalculation Get(string calcRef) =>
        _byRef.TryGetValue(calcRef, out var calc)
            ? calc
            : throw new NotSupportedException($"Kalkulasi '{calcRef}' tidak terdaftar.");
}
