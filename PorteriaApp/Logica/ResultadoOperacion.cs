namespace PorteriaApp.Logica
{
    /// <summary>
    /// Resultado de una operación de negocio (por ejemplo, registrar un
    /// movimiento). Permite que la interfaz muestre el mensaje adecuado
    /// (MessageBox / ErrorProvider) sin que la lógica conozca nada de la UI.
    /// </summary>
    public class ResultadoOperacion
    {
        public bool Exitoso { get; }
        public string Mensaje { get; }

        public ResultadoOperacion(bool exitoso, string mensaje)
        {
            Exitoso = exitoso;
            Mensaje = mensaje;
        }

        public static ResultadoOperacion Ok(string mensaje) => new ResultadoOperacion(true, mensaje);
        public static ResultadoOperacion Error(string mensaje) => new ResultadoOperacion(false, mensaje);
    }
}
