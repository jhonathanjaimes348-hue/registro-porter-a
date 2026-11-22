namespace PorteriaApp.Modelo
{
    /// <summary>
    /// Visitante externo. Hereda de Persona y sobrescribe DescripcionAcceso()
    /// con una redacción distinta a la de Residente (polimorfismo).
    /// </summary>
    public class Visitante : Persona
    {
        public string ApartamentoDestino { get; }
        public string AutorizadoPor { get; }

        public Visitante(string nombre, string documento, string apartamentoDestino, string autorizadoPor)
            : base(nombre, documento)
        {
            ApartamentoDestino = apartamentoDestino;
            AutorizadoPor = autorizadoPor;
        }

        public override string TipoPersona => "Visitante";

        public override string DescripcionAcceso()
        {
            return $"Visitante dirigido al apartamento {ApartamentoDestino}, autorizado por {AutorizadoPor}";
        }
    }
}
