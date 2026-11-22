namespace PorteriaApp.Modelo
{
    /// <summary>
    /// Clase base del árbol de herencia del problema. Representa a cualquier
    /// persona que puede pasar por la portería (residente o visitante).
    ///
    /// Esta clase, y todas las del espacio de nombres Modelo/Logica/Persistencia,
    /// NO usan absolutamente nada de System.Windows.Forms: son lógica pura del
    /// problema, independiente de la interfaz gráfica (requisito 4).
    /// </summary>
    public abstract class Persona
    {
        public string Nombre { get; }
        public string Documento { get; }

        protected Persona(string nombre, string documento)
        {
            Nombre = nombre;
            Documento = documento;
        }

        /// <summary>
        /// Identifica el tipo concreto de persona ("Residente" / "Visitante").
        /// Se usa, por ejemplo, para indexar la matriz de conteos y para
        /// reconstruir el objeto correcto al cargar desde archivo.
        /// </summary>
        public abstract string TipoPersona { get; }

        /// <summary>
        /// Método sobrescrito por cada subclase (polimorfismo). Devuelve una
        /// descripción del motivo/condición de acceso de esa persona concreta.
        /// </summary>
        public abstract string DescripcionAcceso();

        public override string ToString() => $"{Nombre} ({Documento}) - {TipoPersona}";
    }
}
