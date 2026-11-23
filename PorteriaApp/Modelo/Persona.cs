namespace PorteriaApp.Modelo;

/// <summary>
/// Persona que pasa por la portería. Es la base del polimorfismo:
/// Residente y Visitante sobrescriben Tipo y Detalle.
/// </summary>
public abstract class Persona
{
    public string Nombre { get; }
    public string Documento { get; }

    protected Persona(string nombre, string documento)
    {
        Nombre = nombre;
        Documento = documento;
    }

    public abstract string Tipo { get; }
    public abstract string Detalle { get; }
}
