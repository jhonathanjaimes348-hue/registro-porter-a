using System;

namespace PorteriaApp.Modelo
{
    /// <summary>
    /// Un movimiento (entrada o salida) de una persona por la portería.
    /// Compone un objeto Persona (Residente o Visitante): aquí es donde se
    /// aprovecha el polimorfismo, ya que RegistroAcceso no necesita saber
    /// de qué subclase concreta se trata para describirlo.
    /// </summary>
    public class RegistroAcceso
    {
        public int Id { get; }
        public Persona Persona { get; }
        public TipoMovimiento Movimiento { get; }
        public DateTime FechaHora { get; }
        public string Observaciones { get; set; }

        public RegistroAcceso(int id, Persona persona, TipoMovimiento movimiento, DateTime fechaHora, string observaciones = "")
        {
            Id = id;
            Persona = persona;
            Movimiento = movimiento;
            FechaHora = fechaHora;
            Observaciones = observaciones;
        }

        /// <summary>
        /// Línea de resumen usada tanto en el DataGridView como al exportar
        /// a texto. Usa el método polimórfico DescripcionAcceso().
        /// </summary>
        public string DescripcionCompleta()
        {
            return $"{FechaHora:yyyy-MM-dd HH:mm} | {Movimiento} | {Persona} | {Persona.DescripcionAcceso()}";
        }
    }
}
