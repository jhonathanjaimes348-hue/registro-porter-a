using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using PorteriaApp.Modelo;

namespace PorteriaApp.Logica;

/// <summary>
/// Clase de servicio: lleva la lista (BindingList, para que el
/// DataGridView se actualice solo), la matriz de conteos y el informe.
/// No conoce Windows Forms.
/// </summary>
public class RegistroAccesos
{
    private readonly BindingList<RegistroAcceso> registros = new();
    private readonly int[,] matriz = new int[7, 2]; // 7 días x [Residente, Visitante]
    private int siguienteId = 1;
    private int rechazados = 0;

    public BindingList<RegistroAcceso> Registros => registros;

    /// <summary>Intenta registrar un movimiento. Devuelve null si todo salió bien, o el mensaje de error.</summary>
    public string? Registrar(Persona persona, TipoMovimiento movimiento, DateTime fecha, string observaciones)
    {
        string? error = Validador.Datos(persona);
        if (error != null)
        {
            rechazados++;
            return error;
        }

        bool dentro = EstaDentro(persona.Documento);

        if (movimiento == TipoMovimiento.Entrada && dentro)
        {
            rechazados++;
            return $"El documento {persona.Documento} ya tiene una entrada activa.";
        }

        if (movimiento == TipoMovimiento.Salida && !dentro)
        {
            rechazados++;
            return $"El documento {persona.Documento} no tiene una entrada activa.";
        }

        var registro = new RegistroAcceso(siguienteId, persona, movimiento, fecha, observaciones);
        siguienteId++;
        registros.Add(registro);

        int columna = persona.Tipo == "Residente" ? 0 : 1;
        matriz[(int)fecha.DayOfWeek, columna]++;

        return null;
    }

    public bool EstaDentro(string documento)
    {
        bool dentro = false;
        foreach (RegistroAcceso r in registros)
        {
            if (r.Documento == documento)
            {
                dentro = r.Movimiento == TipoMovimiento.Entrada;
            }
        }
        return dentro;
    }

    /// <summary>Reemplaza todo lo que hay (usado al cargar desde archivo).</summary>
    public void Reemplazar(List<RegistroAcceso> nuevos)
    {
        registros.Clear();
        Array.Clear(matriz, 0, matriz.Length);
        rechazados = 0;
        siguienteId = 1;

        foreach (RegistroAcceso r in nuevos)
        {
            registros.Add(r);

            int columna = r.Tipo == "Residente" ? 0 : 1;
            matriz[(int)r.FechaHora.DayOfWeek, columna]++;

            if (r.Id >= siguienteId)
            {
                siguienteId = r.Id + 1;
            }
        }
    }

    /// <summary>Informe con contadores, acumulador y una bandera.</summary>
    public string Informe()
    {
        int entradas = 0, salidas = 0, residentes = 0, visitantes = 0;
        int total = 0; // acumulador
        var documentosVistos = new List<string>();
        var estaAdentro = new Dictionary<string, bool>();

        foreach (RegistroAcceso r in registros)
        {
            total++;

            switch (r.Movimiento)
            {
                case TipoMovimiento.Entrada: entradas++; break;
                case TipoMovimiento.Salida: salidas++; break;
            }

            if (r.Tipo == "Residente") residentes++; else visitantes++;

            if (!documentosVistos.Contains(r.Documento))
            {
                documentosVistos.Add(r.Documento);
            }

            estaAdentro[r.Documento] = r.Movimiento == TipoMovimiento.Entrada;
        }

        bool hayGenteAdentro = false; // bandera
        int cantidadAdentro = 0;
        foreach (bool valor in estaAdentro.Values)
        {
            if (valor)
            {
                hayGenteAdentro = true;
                cantidadAdentro++;
            }
        }

        string[] dias = { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
        var texto = new StringBuilder();

        texto.AppendLine($"Registros válidos: {total}    Rechazados: {rechazados}");
        texto.AppendLine($"Entradas: {entradas}    Salidas: {salidas}");
        texto.AppendLine($"Residentes: {residentes}    Visitantes: {visitantes}    Personas distintas: {documentosVistos.Count}");
        texto.AppendLine(hayGenteAdentro
            ? $"Hay {cantidadAdentro} persona(s) dentro sin salida registrada."
            : "No hay nadie pendiente de salida.");
        texto.AppendLine();
        texto.AppendLine("Movimientos por día (Residente / Visitante):");

        for (int i = 0; i < 7; i++)
        {
            texto.AppendLine($"{dias[i],-10} {matriz[i, 0],3} / {matriz[i, 1]}");
        }

        return texto.ToString();
    }
}
