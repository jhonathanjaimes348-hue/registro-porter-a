using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using PorteriaApp.Modelo;

namespace PorteriaApp.Persistencia
{
    /// <summary>
    /// Guarda y carga los registros de la portería en un archivo de texto
    /// plano, delimitado por '|', para que los datos no se pierdan al
    /// cerrar la aplicación (requisito 7). Tampoco usa System.Windows.Forms.
    /// </summary>
    public class PersistenciaArchivo
    {
        private const char Separador = '|';
        private readonly string _rutaArchivo;

        public PersistenciaArchivo(string rutaArchivo)
        {
            _rutaArchivo = rutaArchivo;
        }

        public string RutaArchivo => _rutaArchivo;

        /// <summary>Escribe todos los registros recibidos, uno por línea.</summary>
        public void Guardar(IEnumerable<RegistroAcceso> registros)
        {
            var lineas = new List<string>();

            foreach (var registro in registros)
            {
                string campoExtra1;
                string campoExtra2;

                // Decisión con switch: qué campos adicionales se guardan
                // según el tipo concreto de persona (polimorfismo + persistencia).
                switch (registro.Persona)
                {
                    case Residente residente:
                        campoExtra1 = residente.Apartamento;
                        campoExtra2 = string.Empty;
                        break;
                    case Visitante visitante:
                        campoExtra1 = visitante.ApartamentoDestino;
                        campoExtra2 = visitante.AutorizadoPor;
                        break;
                    default:
                        campoExtra1 = string.Empty;
                        campoExtra2 = string.Empty;
                        break;
                }

                string linea = string.Join(Separador.ToString(), new[]
                {
                    registro.Id.ToString(CultureInfo.InvariantCulture),
                    registro.Persona.TipoPersona,
                    Limpiar(registro.Persona.Nombre),
                    Limpiar(registro.Persona.Documento),
                    Limpiar(campoExtra1),
                    Limpiar(campoExtra2),
                    registro.Movimiento.ToString(),
                    registro.FechaHora.ToString("o", CultureInfo.InvariantCulture),
                    Limpiar(registro.Observaciones)
                });

                lineas.Add(linea);
            }

            File.WriteAllLines(_rutaArchivo, lineas, Encoding.UTF8);
        }

        /// <summary>
        /// Lee el archivo y reconstruye la lista de registros, recreando el
        /// objeto Persona correcto (Residente o Visitante) para cada línea.
        /// Si el archivo no existe todavía, devuelve una lista vacía.
        /// </summary>
        public List<RegistroAcceso> Cargar()
        {
            var resultado = new List<RegistroAcceso>();

            if (!File.Exists(_rutaArchivo))
            {
                return resultado;
            }

            string[] lineas = File.ReadAllLines(_rutaArchivo, Encoding.UTF8);

            foreach (string linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                string[] partes = linea.Split(Separador);
                if (partes.Length < 9)
                {
                    continue; // línea corrupta o incompleta: se ignora
                }

                int id = int.Parse(partes[0], CultureInfo.InvariantCulture);
                string tipo = partes[1];
                string nombre = partes[2];
                string documento = partes[3];
                string campoExtra1 = partes[4];
                string campoExtra2 = partes[5];
                TipoMovimiento movimiento = partes[6] == "Entrada" ? TipoMovimiento.Entrada : TipoMovimiento.Salida;
                DateTime fecha = DateTime.Parse(partes[7], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                string observaciones = partes[8];

                Persona? persona;
                switch (tipo)
                {
                    case "Residente":
                        persona = new Residente(nombre, documento, campoExtra1);
                        break;
                    case "Visitante":
                        persona = new Visitante(nombre, documento, campoExtra1, campoExtra2);
                        break;
                    default:
                        persona = null;
                        break;
                }

                if (persona == null)
                {
                    continue; // tipo desconocido: se descarta la línea
                }

                resultado.Add(new RegistroAcceso(id, persona, movimiento, fecha, observaciones));
            }

            return resultado;
        }

        private static string Limpiar(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return string.Empty;
            }

            return texto.Replace(Separador, '/').Replace("\r", " ").Replace("\n", " ");
        }
    }
}
