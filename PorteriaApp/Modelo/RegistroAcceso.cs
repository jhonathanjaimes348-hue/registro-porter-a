using System;
using System.Globalization;

namespace PorteriaApp.Modelo;

/// <summary>
/// Un movimiento (entrada o salida) de una Persona. Trae propiedades
/// "planas" (Nombre, Documento, Tipo, Detalle...) para poder enlazar el
/// DataGridView directamente a una lista de RegistroAcceso.
/// </summary>
public class RegistroAcceso
{
    public int Id { get; }
    public Persona Persona { get; }
    public TipoMovimiento Movimiento { get; }
    public DateTime FechaHora { get; }
    public string Observaciones { get; }

    public RegistroAcceso(int id, Persona persona, TipoMovimiento movimiento, DateTime fechaHora, string observaciones)
    {
        Id = id;
        Persona = persona;
        Movimiento = movimiento;
        FechaHora = fechaHora;
        Observaciones = observaciones;
    }

    public string Nombre => Persona.Nombre;
    public string Documento => Persona.Documento;
    public string Tipo => Persona.Tipo;
    public string Detalle => Persona.Detalle;
    public string FechaTexto => FechaHora.ToString("yyyy-MM-dd HH:mm");

    /// <summary>Una línea de texto, delimitada por ';', para el archivo.</summary>
    public string ToLinea()
    {
        string extra1 = Persona is Residente r ? r.Apartamento : ((Visitante)Persona).ApartamentoDestino;
        string extra2 = Persona is Visitante v ? v.AutorizadoPor : string.Empty;

        return string.Join(";",
            Id, Tipo, Nombre, Documento, extra1, extra2,
            Movimiento, FechaHora.ToString("o", CultureInfo.InvariantCulture), Observaciones);
    }

    /// <summary>Reconstruye un RegistroAcceso a partir de una línea del archivo. null si está dañada.</summary>
    public static RegistroAcceso? DesdeLinea(string linea)
    {
        string[] partes = linea.Split(';');
        if (partes.Length < 9) return null;

        Persona? persona = partes[1] switch
        {
            "Residente" => new Residente(partes[2], partes[3], partes[4]),
            "Visitante" => new Visitante(partes[2], partes[3], partes[4], partes[5]),
            _ => null
        };
        if (persona == null) return null;

        if (!int.TryParse(partes[0], out int id)) return null;
        TipoMovimiento movimiento = partes[6] == "Entrada" ? TipoMovimiento.Entrada : TipoMovimiento.Salida;
        if (!DateTime.TryParse(partes[7], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime fecha)) return null;

        return new RegistroAcceso(id, persona, movimiento, fecha, partes[8]);
    }
}
