using System;
using System.Collections.Generic;
using PorteriaApp.Modelo;

namespace PorteriaApp.Logica
{
    /// <summary>
    /// Clase central de la lógica del problema: lleva el listado de
    /// registros (vector/lista), una matriz de conteos por día/tipo de
    /// persona, valida los datos con if/switch y genera el informe final.
    ///
    /// No referencia System.Windows.Forms en ningún lugar (requisito 4):
    /// solo tipos de .NET (List, DateTime, etc.) y clases propias del
    /// Modelo. La interfaz (MainForm) es la única que sabe de controles
    /// gráficos y llama a estos métodos.
    /// </summary>
    public class GestorPorteria
    {
        // ---- Requisito 5: al menos un vector/lista y una matriz ----

        /// <summary>Lista (vector dinámico) con todos los registros válidos.</summary>
        private readonly List<RegistroAcceso> _registros = new();

        /// <summary>
        /// Matriz de conteos: 7 filas (día de la semana, 0=domingo..6=sábado)
        /// x 2 columnas (0 = Residente, 1 = Visitante).
        /// </summary>
        private readonly int[,] _matrizConteo = new int[7, 2];

        private int _siguienteId = 1;
        private int _registrosRechazados = 0;

        public IReadOnlyList<RegistroAcceso> Registros => _registros;

        public int CantidadRechazados => _registrosRechazados;

        /// <summary>
        /// Intenta registrar un movimiento (entrada o salida) de una
        /// persona. Aplica todas las validaciones del problema y, si todo
        /// es correcto, agrega el registro a la lista y actualiza la matriz.
        /// </summary>
        public ResultadoOperacion RegistrarMovimiento(Persona persona, TipoMovimiento movimiento, DateTime fechaHora, string observaciones)
        {
            ResultadoOperacion validacion = ValidarPersona(persona);
            if (!validacion.Exitoso)
            {
                _registrosRechazados++;
                return validacion;
            }

            // Decisión con if: no se puede registrar dos entradas seguidas,
            // ni una salida sin una entrada previa.
            bool dentro = EstaDentro(persona.Documento);

            if (movimiento == TipoMovimiento.Entrada && dentro)
            {
                _registrosRechazados++;
                return ResultadoOperacion.Error(
                    $"El documento {persona.Documento} ya tiene una entrada activa. Debe registrar la salida antes de una nueva entrada.");
            }

            if (movimiento == TipoMovimiento.Salida && !dentro)
            {
                _registrosRechazados++;
                return ResultadoOperacion.Error(
                    $"No se puede registrar una salida para el documento {persona.Documento}: no tiene una entrada activa.");
            }

            var registro = new RegistroAcceso(_siguienteId, persona, movimiento, fechaHora, observaciones ?? string.Empty);
            _siguienteId++;
            _registros.Add(registro);

            // Decisión con switch: columna de la matriz según el tipo de persona.
            int columna;
            switch (persona.TipoPersona)
            {
                case "Residente":
                    columna = 0;
                    break;
                case "Visitante":
                    columna = 1;
                    break;
                default:
                    columna = 1;
                    break;
            }

            int fila = (int)fechaHora.DayOfWeek; // 0=Domingo..6=Sábado
            _matrizConteo[fila, columna]++;

            return ResultadoOperacion.Ok($"Registro #{registro.Id} guardado correctamente ({movimiento}).");
        }

        /// <summary>
        /// Valida los datos de la persona antes de aceptar el movimiento.
        /// Usa if y switch para tomar las decisiones (requisito 2).
        /// </summary>
        private ResultadoOperacion ValidarPersona(Persona persona)
        {
            if (persona == null)
            {
                return ResultadoOperacion.Error("No se indicó la persona para el registro.");
            }

            if (string.IsNullOrWhiteSpace(persona.Nombre))
            {
                return ResultadoOperacion.Error("El nombre no puede estar vacío.");
            }

            if (string.IsNullOrWhiteSpace(persona.Documento) || !SoloDigitos(persona.Documento))
            {
                return ResultadoOperacion.Error("El documento es obligatorio y debe contener solo números.");
            }

            switch (persona)
            {
                case Residente r:
                    if (string.IsNullOrWhiteSpace(r.Apartamento))
                    {
                        return ResultadoOperacion.Error("Debe indicar el apartamento del residente.");
                    }
                    break;

                case Visitante v:
                    if (string.IsNullOrWhiteSpace(v.ApartamentoDestino))
                    {
                        return ResultadoOperacion.Error("Debe indicar el apartamento de destino del visitante.");
                    }
                    if (string.IsNullOrWhiteSpace(v.AutorizadoPor))
                    {
                        return ResultadoOperacion.Error("Debe indicar quién autoriza al visitante.");
                    }
                    break;

                default:
                    return ResultadoOperacion.Error("Tipo de persona no reconocido.");
            }

            return ResultadoOperacion.Ok("Datos válidos.");
        }

        private static bool SoloDigitos(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Indica si, según el último movimiento registrado para ese
        /// documento, la persona se encuentra actualmente dentro del edificio.
        /// </summary>
        public bool EstaDentro(string documento)
        {
            bool dentro = false;

            // Recorremos en orden de inserción (cronológico) y nos quedamos
            // con el último estado conocido para ese documento.
            foreach (var registro in _registros)
            {
                if (registro.Persona.Documento == documento)
                {
                    dentro = registro.Movimiento == TipoMovimiento.Entrada;
                }
            }

            return dentro;
        }

        /// <summary>
        /// Genera el informe final recorriendo la lista de registros con
        /// contadores, acumuladores y una bandera (requisito 6).
        /// </summary>
        public ResumenReporte GenerarResumen()
        {
            int contadorEntradas = 0;
            int contadorSalidas = 0;
            int contadorResidentes = 0;
            int contadorVisitantes = 0;
            int acumuladorTotal = 0; // acumulador de registros procesados

            var documentosVistos = new List<string>(); // para contar personas distintas
            var estadoActual = new Dictionary<string, bool>(); // documento -> está dentro

            foreach (var registro in _registros)
            {
                acumuladorTotal++; // se va acumulando con cada vuelta del ciclo

                switch (registro.Movimiento)
                {
                    case TipoMovimiento.Entrada:
                        contadorEntradas++;
                        break;
                    case TipoMovimiento.Salida:
                        contadorSalidas++;
                        break;
                }

                if (registro.Persona.TipoPersona == "Residente")
                {
                    contadorResidentes++;
                }
                else
                {
                    contadorVisitantes++;
                }

                if (!documentosVistos.Contains(registro.Persona.Documento))
                {
                    documentosVistos.Add(registro.Persona.Documento);
                }

                estadoActual[registro.Persona.Documento] = registro.Movimiento == TipoMovimiento.Entrada;
            }

            // Bandera: ¿hay al menos una persona dentro en este momento?
            bool hayPersonasDentro = false;
            int personasDentro = 0;
            foreach (var estaDentro in estadoActual.Values)
            {
                if (estaDentro)
                {
                    personasDentro++;
                    hayPersonasDentro = true;
                }
            }

            return new ResumenReporte(
                totalRegistros: acumuladorTotal,
                totalEntradas: contadorEntradas,
                totalSalidas: contadorSalidas,
                totalResidentes: contadorResidentes,
                totalVisitantes: contadorVisitantes,
                totalDocumentosDistintos: documentosVistos.Count,
                totalRechazados: _registrosRechazados,
                personasActualmenteDentro: personasDentro,
                hayPersonasDentro: hayPersonasDentro,
                matrizConteo: (int[,])_matrizConteo.Clone());
        }

        /// <summary>
        /// Reemplaza el contenido actual por una lista cargada desde disco
        /// (usado por PersistenciaArchivo al leer el archivo de texto).
        /// Recalcula la matriz y el próximo id a partir de los datos cargados.
        /// </summary>
        public void CargarDesde(List<RegistroAcceso> registrosCargados)
        {
            _registros.Clear();
            Array.Clear(_matrizConteo, 0, _matrizConteo.Length);
            _registrosRechazados = 0;
            _siguienteId = 1;

            foreach (var registro in registrosCargados)
            {
                _registros.Add(registro);

                int columna = registro.Persona.TipoPersona == "Residente" ? 0 : 1;
                int fila = (int)registro.FechaHora.DayOfWeek;
                _matrizConteo[fila, columna]++;

                if (registro.Id >= _siguienteId)
                {
                    _siguienteId = registro.Id + 1;
                }
            }
        }
    }
}
