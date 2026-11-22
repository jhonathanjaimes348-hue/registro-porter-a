namespace PorteriaApp.Modelo
{
    /// <summary>
    /// Residente del edificio. Hereda de Persona y sobrescribe
    /// DescripcionAcceso() para dar polimorfismo real al listado y al reporte.
    /// </summary>
    public class Residente : Persona
    {
        public string Apartamento { get; }

        public Residente(string nombre, string documento, string apartamento)
            : base(nombre, documento)
        {
            Apartamento = apartamento;
        }

        public override string TipoPersona => "Residente";

        public override string DescripcionAcceso()
        {
            return $"Residente autorizado del apartamento {Apartamento}";
        }
    }
}
