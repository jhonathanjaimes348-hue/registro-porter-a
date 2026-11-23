using System.Linq;
using PorteriaApp.Modelo;

namespace PorteriaApp.Logica;

/// <summary>
/// Validador estático: no conoce Windows Forms, solo devuelve un mensaje
/// de error o null si los datos son correctos.
/// </summary>
public static class Validador
{
    public static string? Datos(Persona persona)
    {
        if (string.IsNullOrWhiteSpace(persona.Nombre))
        {
            return "El nombre es obligatorio.";
        }

        if (persona.Nombre.Contains(';'))
        {
            return "El nombre no puede llevar punto y coma.";
        }

        if (string.IsNullOrWhiteSpace(persona.Documento) || !persona.Documento.All(char.IsDigit))
        {
            return "El documento es obligatorio y debe contener solo números.";
        }

        return persona switch
        {
            Residente r when string.IsNullOrWhiteSpace(r.Apartamento)
                => "Debe indicar el apartamento del residente.",
            Visitante v when string.IsNullOrWhiteSpace(v.ApartamentoDestino)
                => "Debe indicar el apartamento de destino del visitante.",
            Visitante v when string.IsNullOrWhiteSpace(v.AutorizadoPor)
                => "Debe indicar quién autoriza al visitante.",
            _ => null
        };
    }
}
