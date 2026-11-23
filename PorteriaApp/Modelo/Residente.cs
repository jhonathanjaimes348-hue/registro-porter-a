namespace PorteriaApp.Modelo;

public class Residente : Persona
{
    public string Apartamento { get; }

    public Residente(string nombre, string documento, string apartamento)
        : base(nombre, documento)
    {
        Apartamento = apartamento;
    }

    public override string Tipo => "Residente";
    public override string Detalle => $"Apto {Apartamento}";
}
