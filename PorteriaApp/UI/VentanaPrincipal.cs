using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PorteriaApp.Logica;
using PorteriaApp.Modelo;

namespace PorteriaApp.UI;

/// <summary>
/// Única clase que conoce Windows Forms. Arma los controles por código,
/// atiende los eventos (prefijo "Al") y delega todo lo demás a
/// RegistroAccesos / Validador / Archivo.
/// </summary>
public class VentanaPrincipal : Form
{
    private const string RutaRegistro = "registros_porteria.txt";
    private readonly RegistroAccesos accesos = new();

    // Pestaña "Registro de acceso"
    private TextBox txtNombre = null!;
    private TextBox txtDocumento = null!;
    private ComboBox cboTipo = null!;
    private Label lblExtra1 = null!;
    private TextBox txtExtra1 = null!;
    private Label lblExtra2 = null!;
    private TextBox txtExtra2 = null!;
    private ComboBox cboMovimiento = null!;
    private TextBox txtObservaciones = null!;
    private Button btnRegistrar = null!;
    private Button btnLimpiar = null!;
    private Label lblEstado = null!;
    private DataGridView grilla = null!;
    private ErrorProvider errores = null!;

    // Pestaña "Reportes"
    private TextBox txtInforme = null!;
    private Button btnGenerarInforme = null!;

    // Pestaña "Persistencia"
    private Button btnGuardar = null!;
    private Button btnAbrir = null!;
    private Label lblEstadoArchivo = null!;

    public VentanaPrincipal()
    {
        ConstruirInterfaz();
        ConfigurarGrilla();

        if (File.Exists(RutaRegistro))
        {
            CargarDesde(RutaRegistro);
        }
    }

    // ===================== Construcción de la interfaz =====================

    private void ConstruirInterfaz()
    {
        Text = "Registro de Portería";
        ClientSize = new Size(900, 620);
        MinimumSize = new Size(820, 560);
        StartPosition = FormStartPosition.CenterScreen;

        errores = new ErrorProvider();

        var pestañas = new TabControl { Dock = DockStyle.Fill };
        var tabRegistro = new TabPage("Registro de acceso");
        var tabReportes = new TabPage("Reportes");
        var tabPersistencia = new TabPage("Persistencia");

        ConstruirTabRegistro(tabRegistro);
        ConstruirTabReportes(tabReportes);
        ConstruirTabPersistencia(tabPersistencia);

        pestañas.TabPages.Add(tabRegistro);
        pestañas.TabPages.Add(tabReportes);
        pestañas.TabPages.Add(tabPersistencia);

        Controls.Add(pestañas);
        FormClosing += AlCerrar;
    }

    private void ConstruirTabRegistro(TabPage tab)
    {
        var tabla = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Padding = new Padding(10),
            ColumnCount = 4,
            RowCount = 6,
            AutoSize = true
        };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var lblNombre = new Label { Text = "Nombre:", Anchor = AnchorStyles.Left, AutoSize = true };
        txtNombre = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
        var lblMovimiento = new Label { Text = "Movimiento:", Anchor = AnchorStyles.Left, AutoSize = true };
        cboMovimiento = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
        cboMovimiento.Items.AddRange(new object[] { "Entrada", "Salida" });
        cboMovimiento.SelectedIndex = 0;

        var lblDocumento = new Label { Text = "Documento:", Anchor = AnchorStyles.Left, AutoSize = true };
        txtDocumento = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
        var lblObservaciones = new Label { Text = "Observaciones:", Anchor = AnchorStyles.Left, AutoSize = true };
        txtObservaciones = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };

        var lblTipo = new Label { Text = "Tipo de persona:", Anchor = AnchorStyles.Left, AutoSize = true };
        cboTipo = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
        cboTipo.Items.AddRange(new object[] { "Residente", "Visitante" });
        cboTipo.SelectedIndex = 0;
        cboTipo.SelectedIndexChanged += AlCambiarTipo;

        lblExtra1 = new Label { Text = "Apartamento:", Anchor = AnchorStyles.Left, AutoSize = true };
        txtExtra1 = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
        lblExtra2 = new Label { Text = "Autorizado por:", Anchor = AnchorStyles.Left, AutoSize = true, Visible = false };
        txtExtra2 = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Visible = false };

        var panelBotones = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 8, 0, 4) };
        btnRegistrar = new Button { Text = "Registrar", AutoSize = true };
        btnRegistrar.Click += AlRegistrar;
        btnLimpiar = new Button { Text = "Limpiar", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        btnLimpiar.Click += (s, e) => Limpiar();
        panelBotones.Controls.Add(btnRegistrar);
        panelBotones.Controls.Add(btnLimpiar);

        lblEstado = new Label { AutoSize = true, MaximumSize = new Size(820, 0), ForeColor = Color.DarkRed };

        tabla.Controls.Add(lblNombre, 0, 0);
        tabla.Controls.Add(txtNombre, 1, 0);
        tabla.Controls.Add(lblMovimiento, 2, 0);
        tabla.Controls.Add(cboMovimiento, 3, 0);

        tabla.Controls.Add(lblDocumento, 0, 1);
        tabla.Controls.Add(txtDocumento, 1, 1);
        tabla.Controls.Add(lblObservaciones, 2, 1);
        tabla.Controls.Add(txtObservaciones, 3, 1);

        tabla.Controls.Add(lblTipo, 0, 2);
        tabla.Controls.Add(cboTipo, 1, 2);

        tabla.Controls.Add(lblExtra1, 0, 3);
        tabla.Controls.Add(txtExtra1, 1, 3);
        tabla.Controls.Add(lblExtra2, 2, 3);
        tabla.Controls.Add(txtExtra2, 3, 3);

        tabla.SetColumnSpan(panelBotones, 4);
        tabla.Controls.Add(panelBotones, 0, 4);

        tabla.SetColumnSpan(lblEstado, 4);
        tabla.Controls.Add(lblEstado, 0, 5);

        grilla = new DataGridView { Dock = DockStyle.Fill };

        tab.Controls.Add(grilla);
        tab.Controls.Add(tabla);
    }

    private void ConstruirTabReportes(TabPage tab)
    {
        btnGenerarInforme = new Button { Text = "Generar informe", Left = 10, Top = 10, AutoSize = true };
        btnGenerarInforme.Click += (s, e) => txtInforme.Text = accesos.Informe();

        txtInforme = new TextBox
        {
            Left = 10,
            Top = 46,
            Width = 850,
            Height = 480,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font(FontFamily.GenericMonospace, 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        tab.Controls.Add(btnGenerarInforme);
        tab.Controls.Add(txtInforme);
    }

    private void ConstruirTabPersistencia(TabPage tab)
    {
        btnGuardar = new Button { Text = "Guardar como...", Left = 10, Top = 10, AutoSize = true };
        btnGuardar.Click += AlGuardar;

        btnAbrir = new Button { Text = "Abrir archivo...", Left = 140, Top = 10, AutoSize = true };
        btnAbrir.Click += AlAbrir;

        lblEstadoArchivo = new Label { Left = 10, Top = 50, Width = 850, Height = 60, ForeColor = Color.DarkBlue };

        var nota = new Label
        {
            Left = 10,
            Top = 120,
            Width = 850,
            Height = 50,
            ForeColor = Color.Gray,
            Text = $"Además de estos botones, la app guarda automáticamente en '{RutaRegistro}' al cerrarse " +
                   "y lo vuelve a cargar al abrirse, para que los registros nunca se pierdan."
        };

        tab.Controls.Add(btnGuardar);
        tab.Controls.Add(btnAbrir);
        tab.Controls.Add(lblEstadoArchivo);
        tab.Controls.Add(nota);
    }

    private void ConfigurarGrilla()
    {
        grilla.AutoGenerateColumns = false;
        grilla.ReadOnly = true;
        grilla.AllowUserToAddRows = false;
        grilla.AllowUserToDeleteRows = false;
        grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Id", FillWeight = 30 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FechaTexto", HeaderText = "Fecha y hora", FillWeight = 100 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Movimiento", HeaderText = "Movimiento", FillWeight = 70 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Tipo", HeaderText = "Tipo", FillWeight = 70 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre", FillWeight = 110 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Documento", HeaderText = "Documento", FillWeight = 80 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Detalle", HeaderText = "Detalle", FillWeight = 180 });

        // Al usar BindingList como origen de datos, la grilla se refresca
        // sola cada vez que accesos.Registrar(...) agrega un registro.
        grilla.DataSource = accesos.Registros;
    }

    // ============================ Eventos "Al..." ============================

    private void AlCambiarTipo(object? remitente, EventArgs e)
    {
        bool esVisitante = cboTipo.SelectedItem?.ToString() == "Visitante";
        lblExtra1.Text = esVisitante ? "Apto. destino:" : "Apartamento:";
        lblExtra2.Visible = esVisitante;
        txtExtra2.Visible = esVisitante;
        if (!esVisitante) txtExtra2.Text = string.Empty;
    }

    private void AlRegistrar(object? remitente, EventArgs e)
    {
        errores.Clear();

        string tipo = cboTipo.SelectedItem!.ToString()!;
        Persona persona = tipo == "Residente"
            ? new Residente(txtNombre.Text.Trim(), txtDocumento.Text.Trim(), txtExtra1.Text.Trim())
            : new Visitante(txtNombre.Text.Trim(), txtDocumento.Text.Trim(), txtExtra1.Text.Trim(), txtExtra2.Text.Trim());

        TipoMovimiento movimiento = cboMovimiento.SelectedItem!.ToString() == "Entrada"
            ? TipoMovimiento.Entrada
            : TipoMovimiento.Salida;

        // La validación real ocurre en Validador/RegistroAccesos, no aquí.
        string? error = accesos.Registrar(persona, movimiento, DateTime.Now, txtObservaciones.Text.Trim());

        if (error == null)
        {
            Informar(lblEstado, $"Registrado correctamente ({movimiento}).", true);
            Limpiar();
        }
        else
        {
            errores.SetError(txtNombre, error);
            Informar(lblEstado, error, false);
        }
    }

    private void Limpiar()
    {
        txtNombre.Clear();
        txtDocumento.Clear();
        txtExtra1.Clear();
        txtExtra2.Clear();
        txtObservaciones.Clear();
        cboTipo.SelectedIndex = 0;
        cboMovimiento.SelectedIndex = 0;
        errores.Clear();
        txtNombre.Focus();
    }

    private void AlGuardar(object? remitente, EventArgs e)
    {
        using var dialogo = new SaveFileDialog { Filter = "Archivo de texto (*.txt)|*.txt", FileName = RutaRegistro };
        if (dialogo.ShowDialog() == DialogResult.OK)
        {
            GuardarEn(dialogo.FileName);
        }
    }

    private void AlAbrir(object? remitente, EventArgs e)
    {
        using var dialogo = new OpenFileDialog { Filter = "Archivo de texto (*.txt)|*.txt" };
        if (dialogo.ShowDialog() == DialogResult.OK)
        {
            CargarDesde(dialogo.FileName);
        }
    }

    private void AlCerrar(object? remitente, FormClosingEventArgs e)
    {
        GuardarEn(RutaRegistro);
    }

    private void GuardarEn(string ruta)
    {
        try
        {
            Archivo.Guardar(accesos.Registros.ToList(), ruta);
            Informar(lblEstadoArchivo, $"Guardados {accesos.Registros.Count} registro(s) en {Path.GetFileName(ruta)}.", true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Informar(lblEstadoArchivo, "No se pudo guardar: " + ex.Message, false);
        }
    }

    private void CargarDesde(string ruta)
    {
        try
        {
            accesos.Reemplazar(Archivo.Cargar(ruta));
            Informar(lblEstadoArchivo, $"Cargados {accesos.Registros.Count} registro(s) de {Path.GetFileName(ruta)}.", true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Informar(lblEstadoArchivo, "No se pudo abrir: " + ex.Message, false);
        }
    }

    private static void Informar(Label etiqueta, string mensaje, bool exito)
    {
        etiqueta.Text = mensaje;
        etiqueta.ForeColor = exito ? Color.DarkGreen : Color.DarkRed;
    }
}
