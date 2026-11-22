using System.Text;

namespace PorteriaApp.Logica
{
    /// <summary>
    /// Resultado del informe generado por GestorPorteria.GenerarResumen().
    /// Guarda los totales (contadores/acumuladores) y la matriz de conteos
    /// por día de la semana y tipo de persona.
    /// </summary>
    public class ResumenReporte
    {
        public int TotalRegistros { get; }
        public int TotalEntradas { get; }
        public int TotalSalidas { get; }
        public int TotalResidentes { get; }
        public int TotalVisitantes { get; }
        public int TotalDocumentosDistintos { get; }
        public int TotalRechazados { get; }
        public int PersonasActualmenteDentro { get; }
        public bool HayPersonasDentro { get; }

        /// <summary>
        /// Matriz [7 filas = día de la semana, 2 columnas = tipo de persona
        /// (0 = Residente, 1 = Visitante)] con el conteo de movimientos.
        /// </summary>
        public int[,] MatrizConteo { get; }

        private static readonly string[] NombresDias =
        {
            "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"
        };

        public ResumenReporte(
            int totalRegistros, int totalEntradas, int totalSalidas,
            int totalResidentes, int totalVisitantes, int totalDocumentosDistintos,
            int totalRechazados, int personasActualmenteDentro, bool hayPersonasDentro,
            int[,] matrizConteo)
        {
            TotalRegistros = totalRegistros;
            TotalEntradas = totalEntradas;
            TotalSalidas = totalSalidas;
            TotalResidentes = totalResidentes;
            TotalVisitantes = totalVisitantes;
            TotalDocumentosDistintos = totalDocumentosDistintos;
            TotalRechazados = totalRechazados;
            PersonasActualmenteDentro = personasActualmenteDentro;
            HayPersonasDentro = hayPersonasDentro;
            MatrizConteo = matrizConteo;
        }

        /// <summary>
        /// Genera el texto del informe (lo consume la UI tal cual, sin que
        /// la pantalla tenga que conocer la estructura interna del resumen).
        /// </summary>
        public string ComoTexto()
        {
            var sb = new StringBuilder();

            sb.AppendLine("==== RESUMEN DE REGISTROS DE PORTERÍA ====");
            sb.AppendLine();
            sb.AppendLine($"Total de registros válidos : {TotalRegistros}");
            sb.AppendLine($"Total de registros rechazados : {TotalRechazados}");
            sb.AppendLine($"  - Entradas : {TotalEntradas}");
            sb.AppendLine($"  - Salidas  : {TotalSalidas}");
            sb.AppendLine($"  - Residentes registrados : {TotalResidentes}");
            sb.AppendLine($"  - Visitantes registrados : {TotalVisitantes}");
            sb.AppendLine($"Personas (documentos) distintas atendidas : {TotalDocumentosDistintos}");
            sb.AppendLine();

            if (HayPersonasDentro)
            {
                sb.AppendLine($"⚠ Hay {PersonasActualmenteDentro} persona(s) actualmente dentro del edificio sin salida registrada.");
            }
            else
            {
                sb.AppendLine("No hay personas pendientes de salida en este momento.");
            }

            sb.AppendLine();
            sb.AppendLine("Movimientos por día de la semana y tipo de persona:");
            sb.AppendLine(string.Format("{0,-12}{1,12}{2,12}", "Día", "Residente", "Visitante"));

            for (int fila = 0; fila < 7; fila++)
            {
                sb.AppendLine(string.Format("{0,-12}{1,12}{2,12}",
                    NombresDias[fila], MatrizConteo[fila, 0], MatrizConteo[fila, 1]));
            }

            return sb.ToString();
        }
    }
}
