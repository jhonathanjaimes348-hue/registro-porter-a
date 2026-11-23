namespace PorteriaApp.Modelo;

public class Visitante : Persona
{
    public string ApartamentoDestino { get; }
    public string AutorizadoPor { get; }

    public Visitante(string nombre, string documento, string apartamentoDestino, string autorizadoPor)
        : base(nombre, documento)
    {
        ApartamentoDestino = apartamentoDestino;
        AutorizadoPor = autorizadoPor;
    }

    public override string Tipo => "Visitante";
    public override string Detalle => $"Destino {ApartamentoDestino} (autoriza {AutorizadoPor})";
}
