using BB84Protocol.BusinessLogic.Implementation;
using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Models;

namespace BB84Protocol.BusinessLogic.Factories;

public static class GateTransformerFactory
{
    private static readonly RectilinearTransformer _rectilinearTransformer = new();
    private static readonly DiagonalTransformer _diagonalTransformer = new();

    public static IGateTransformer GetGate(GateType gateType) => gateType switch
    {
        GateType.Rectilinear => _rectilinearTransformer,
        GateType.Diagonal => _diagonalTransformer,
        _ => throw new InvalidOperationException("Invalid gate type.")
    };
}
