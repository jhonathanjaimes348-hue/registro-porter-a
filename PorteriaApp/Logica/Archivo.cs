using System.Collections.Generic;
using System.IO;
using PorteriaApp.Modelo;

namespace PorteriaApp.Logica;

/// <summary>
/// Guarda y carga una lista de RegistroAcceso en un archivo de texto
/// (una línea por registro, vía RegistroAcceso.ToLinea()/DesdeLinea()).
/// </summary>
public static class Archivo
{
    public static void Guardar(List<RegistroAcceso> registros, string ruta)
    {
        using var escritor = new StreamWriter(ruta);
        foreach (RegistroAcceso r in registros)
        {
            escritor.WriteLine(r.ToLinea());
        }
    }

    public static List<RegistroAcceso> Cargar(string ruta)
    {
        var registros = new List<RegistroAcceso>();
        if (!File.Exists(ruta))
        {
            return registros;
        }

        using var lector = new StreamReader(ruta);
        string? linea;
        while ((linea = lector.ReadLine()) != null)
        {
            RegistroAcceso? r = RegistroAcceso.DesdeLinea(linea);
            if (r != null)
            {
                registros.Add(r);
            }
        }
        return registros;
    }
}
