using System;
using System.Drawing;
using System.Windows.Forms;
using PorteriaApp.Logica;
using PorteriaApp.Modelo;
using PorteriaApp.Persistencia;

namespace PorteriaApp.UI
{
    /// <summary>
    /// Única clase del proyecto que conoce Windows Forms. Todo lo que hace
    /// es: construir los controles por código, reaccionar a sus eventos, y
    /// delegar las decisiones reales a GestorPorteria / PersistenciaArchivo
    /// (requisito 4: lógica separada de la interfaz).
    /// </summary>
    public class MainForm : Form
    {
        private readonly GestorPorteria _gestor = new();
        private const string ArchivoPorDefecto = "registros_porteria.txt";

        // ---- Pestaña "Registro de acceso" ----
        private TabControl tabPrincipal = null!;
        private TextBox txtNombre = null!;
        private TextBox txtDocumento = null!;
        private ComboBox cmbTipoPersona = null!;
        private Label lblCampoExtra1 = null!;
        private TextBox txtCampoExtra1 = null!;
        private Label lblCampoExtra2 = null!;
        private TextBox txtCampoExtra2 = null!;
        private ComboBox cmbMovimiento = null!;
        private TextBox txtObservaciones = null!;
        private Button btnRegistrar = null!;
        private Button btnLimpiar = null!;
        private DataGridView dgvRegistros = null!;
        private ErrorProvider errorProvider = null!;
        private Label lblEstadoRegistro = null!;

        // ---- Pestaña "Reportes" ----
        private TextBox txtResumen = null!;
        private Button btnGenerarReporte = null!;

        // ---- Pestaña "Persistencia" ----
        private TextBox txtRutaArchivo = null!;
        private Button btnGuardar = null!;
        private Button btnCargar = null!;
        private Label lblEstadoPersistencia = null!;

        public MainForm()
        {
            ConstruirInterfaz();
            CargarAlIniciar();
            RefrescarGrid();
        }

        // ================= Construcción de la interfaz =================

        private void ConstruirInterfaz()
        {
            Text = "Registro de Portería";
            Width = 920;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(820, 560);

            errorProvider = new ErrorProvider();

            tabPrincipal = new TabControl { Dock = DockStyle.Fill };

            var tabRegistro = new TabPage("Registro de acceso");
            var tabReportes = new TabPage("Reportes");
            var tabPersistencia = new TabPage("Persistencia");

            ConstruirTabRegistro(tabRegistro);
            ConstruirTabReportes(tabReportes);
            ConstruirTabPersistencia(tabPersistencia);

            tabPrincipal.TabPages.Add(tabRegistro);
            tabPrincipal.TabPages.Add(tabReportes);
            tabPrincipal.TabPages.Add(tabPersistencia);

            Controls.Add(tabPrincipal);

            FormClosing += MainForm_FormClosing;
        }

        private void ConstruirTabRegistro(TabPage tab)
        {
            var panelFormulario = new Panel { Dock = DockStyle.Top, Height = 230, Padding = new Padding(10) };

            int y = 10;
            const int separacion = 34;

            var lblNombre = new Label { Text = "Nombre:", Left = 10, Top = y, Width = 110 };
            txtNombre = new TextBox { Left = 130, Top = y, Width = 220 };
            y += separacion;

            var lblDocumento = new Label { Text = "Documento:", Left = 10, Top = y, Width = 110 };
            txtDocumento = new TextBox { Left = 130, Top = y, Width = 220 };
            y += separacion;

            var lblTipo = new Label { Text = "Tipo de persona:", Left = 10, Top = y, Width = 110 };
            cmbTipoPersona = new ComboBox { Left = 130, Top = y, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipoPersona.Items.AddRange(new object[] { "Residente", "Visitante" });
            cmbTipoPersona.SelectedIndex = 0;
            y += separacion;

            lblCampoExtra1 = new Label { Text = "Apartamento:", Left = 10, Top = y, Width = 110 };
            txtCampoExtra1 = new TextBox { Left = 130, Top = y, Width = 220 };
            y += separacion;

            lblCampoExtra2 = new Label { Text = "Autorizado por:", Left = 10, Top = y, Width = 110, Visible = false };
            txtCampoExtra2 = new TextBox { Left = 130, Top = y, Width = 220, Visible = false };

            var lblMovimiento = new Label { Text = "Movimiento:", Left = 380, Top = 10, Width = 110 };
            cmbMovimiento = new ComboBox { Left = 500, Top = 10, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMovimiento.Items.AddRange(new object[] { "Entrada", "Salida" });
            cmbMovimiento.SelectedIndex = 0;

            var lblObservaciones = new Label { Text = "Observaciones:", Left = 380, Top = 44, Width = 110 };
            txtObservaciones = new TextBox { Left = 500, Top = 44, Width = 220 };

            btnRegistrar = new Button { Text = "Registrar", Left = 500, Top = 90, Width = 100, Height = 32 };
            btnRegistrar.Click += BtnRegistrar_Click;

            btnLimpiar = new Button { Text = "Limpiar", Left = 610, Top = 90, Width = 100, Height = 32 };
            btnLimpiar.Click += BtnLimpiar_Click;

            lblEstadoRegistro = new Label { Left = 380, Top = 130, Width = 340, Height = 70, ForeColor = Color.DarkRed };

            cmbTipoPersona.SelectedIndexChanged += CmbTipoPersona_SelectedIndexChanged;

            panelFormulario.Controls.AddRange(new Control[]
            {
                lblNombre, txtNombre,
                lblDocumento, txtDocumento,
                lblTipo, cmbTipoPersona,
                lblCampoExtra1, txtCampoExtra1,
                lblCampoExtra2, txtCampoExtra2,
                lblMovimiento, cmbMovimiento,
                lblObservaciones, txtObservaciones,
                btnRegistrar, btnLimpiar,
                lblEstadoRegistro
            });

            dgvRegistros = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Id", FillWeight = 35 });
            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fecha", HeaderText = "Fecha y hora", FillWeight = 110 });
            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn { Name = "Movimiento", HeaderText = "Movimiento", FillWeight = 75 });
            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tipo", HeaderText = "Tipo", FillWeight = 75 });
            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Nombre", FillWeight = 110 });
            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn { Name = "Documento", HeaderText = "Documento", FillWeight = 80 });
            dgvRegistros.Columns.Add(new DataGridViewTextBoxColumn { Name = "Detalle", HeaderText = "Detalle", FillWeight = 180 });

            tab.Controls.Add(dgvRegistros);
            tab.Controls.Add(panelFormulario);
        }

        private void ConstruirTabReportes(TabPage tab)
        {
            btnGenerarReporte = new Button { Text = "Generar informe", Left = 10, Top = 10, Width = 160, Height = 32 };
            btnGenerarReporte.Click += BtnGenerarReporte_Click;

            txtResumen = new TextBox
            {
                Left = 10,
                Top = 55,
                Width = 840,
                Height = 450,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font(FontFamily.GenericMonospace, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            tab.Controls.Add(btnGenerarReporte);
            tab.Controls.Add(txtResumen);
        }

        private void ConstruirTabPersistencia(TabPage tab)
        {
            var lblRuta = new Label { Text = "Archivo:", Left = 10, Top = 15, Width = 70 };
            txtRutaArchivo = new TextBox { Left = 90, Top = 12, Width = 420, Text = ArchivoPorDefecto };

            btnGuardar = new Button { Text = "Guardar en archivo", Left = 520, Top = 10, Width = 160, Height = 32 };
            btnGuardar.Click += BtnGuardar_Click;

            btnCargar = new Button { Text = "Cargar desde archivo", Left = 690, Top = 10, Width = 160, Height = 32 };
            btnCargar.Click += BtnCargar_Click;

            lblEstadoPersistencia = new Label { Left = 10, Top = 60, Width = 840, Height = 100, ForeColor = Color.DarkBlue };

            var lblNota = new Label
            {
                Left = 10,
                Top = 170,
                Width = 840,
                Height = 60,
                ForeColor = Color.Gray,
                Text = "Nota: los registros también se guardan y cargan automáticamente al cerrar/abrir la aplicación " +
                       "usando el archivo por defecto, para que nunca se pierdan."
            };

            tab.Controls.Add(lblRuta);
            tab.Controls.Add(txtRutaArchivo);
            tab.Controls.Add(btnGuardar);
            tab.Controls.Add(btnCargar);
            tab.Controls.Add(lblEstadoPersistencia);
            tab.Controls.Add(lblNota);
        }

        // ============================ Eventos ============================

        private void CmbTipoPersona_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbTipoPersona.SelectedItem?.ToString() == "Visitante")
            {
                lblCampoExtra1.Text = "Apto. destino:";
                lblCampoExtra2.Visible = true;
                txtCampoExtra2.Visible = true;
            }
            else
            {
                lblCampoExtra1.Text = "Apartamento:";
                lblCampoExtra2.Visible = false;
                txtCampoExtra2.Visible = false;
                txtCampoExtra2.Text = string.Empty;
            }
        }

        private void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            errorProvider.Clear();
            lblEstadoRegistro.Text = string.Empty;

            if (!ValidarCamposFormulario())
            {
                return;
            }

            string tipo = cmbTipoPersona.SelectedItem!.ToString()!;
            Persona persona;

            // Decisión con if: qué clase concreta construir según el tipo elegido.
            if (tipo == "Residente")
            {
                persona = new Residente(txtNombre.Text.Trim(), txtDocumento.Text.Trim(), txtCampoExtra1.Text.Trim());
            }
            else
            {
                persona = new Visitante(txtNombre.Text.Trim(), txtDocumento.Text.Trim(), txtCampoExtra1.Text.Trim(), txtCampoExtra2.Text.Trim());
            }

            TipoMovimiento movimiento = cmbMovimiento.SelectedItem!.ToString() == "Entrada"
                ? TipoMovimiento.Entrada
                : TipoMovimiento.Salida;

            ResultadoOperacion resultado = _gestor.RegistrarMovimiento(persona, movimiento, DateTime.Now, txtObservaciones.Text.Trim());

            if (resultado.Exitoso)
            {
                lblEstadoRegistro.ForeColor = Color.DarkGreen;
                lblEstadoRegistro.Text = resultado.Mensaje;
                RefrescarGrid();
                LimpiarFormulario();
            }
            else
            {
                lblEstadoRegistro.ForeColor = Color.DarkRed;
                lblEstadoRegistro.Text = resultado.Mensaje;
                MessageBox.Show(resultado.Mensaje, "No se pudo registrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Validación de los campos del formulario con ErrorProvider antes
        /// de construir la Persona (requisito 2). La validación "de negocio"
        /// (duplicados, entradas/salidas) la hace GestorPorteria.
        /// </summary>
        private bool ValidarCamposFormulario()
        {
            bool esValido = true;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errorProvider.SetError(txtNombre, "El nombre es obligatorio.");
                esValido = false;
            }

            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                errorProvider.SetError(txtDocumento, "El documento es obligatorio.");
                esValido = false;
            }
            else
            {
                bool soloNumeros = true;
                foreach (char c in txtDocumento.Text)
                {
                    if (!char.IsDigit(c))
                    {
                        soloNumeros = false;
                        break;
                    }
                }

                if (!soloNumeros)
                {
                    errorProvider.SetError(txtDocumento, "El documento debe contener solo números.");
                    esValido = false;
                }
            }

            if (string.IsNullOrWhiteSpace(txtCampoExtra1.Text))
            {
                string mensaje = cmbTipoPersona.SelectedItem?.ToString() == "Visitante"
                    ? "Debe indicar el apartamento de destino."
                    : "Debe indicar el apartamento del residente.";
                errorProvider.SetError(txtCampoExtra1, mensaje);
                esValido = false;
            }

            if (cmbTipoPersona.SelectedItem?.ToString() == "Visitante" && string.IsNullOrWhiteSpace(txtCampoExtra2.Text))
            {
                errorProvider.SetError(txtCampoExtra2, "Debe indicar quién autoriza al visitante.");
                esValido = false;
            }

            if (!esValido)
            {
                MessageBox.Show("Revise los campos marcados en rojo antes de continuar.", "Datos incompletos",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return esValido;
        }

        private void BtnLimpiar_Click(object? sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtDocumento.Clear();
            txtCampoExtra1.Clear();
            txtCampoExtra2.Clear();
            txtObservaciones.Clear();
            cmbTipoPersona.SelectedIndex = 0;
            cmbMovimiento.SelectedIndex = 0;
            errorProvider.Clear();
            txtNombre.Focus();
        }

        private void RefrescarGrid()
        {
            dgvRegistros.Rows.Clear();

            foreach (var registro in _gestor.Registros)
            {
                string detalle;

                // Polimorfismo en uso: según el tipo real de Persona, se
                // muestra un detalle distinto en la tabla.
                switch (registro.Persona)
                {
                    case Residente r:
                        detalle = $"Apto {r.Apartamento}";
                        break;
                    case Visitante v:
                        detalle = $"Destino {v.ApartamentoDestino} (autoriza: {v.AutorizadoPor})";
                        break;
                    default:
                        detalle = string.Empty;
                        break;
                }

                dgvRegistros.Rows.Add(
                    registro.Id,
                    registro.FechaHora.ToString("yyyy-MM-dd HH:mm"),
                    registro.Movimiento.ToString(),
                    registro.Persona.TipoPersona,
                    registro.Persona.Nombre,
                    registro.Persona.Documento,
                    detalle);
            }
        }

        private void BtnGenerarReporte_Click(object? sender, EventArgs e)
        {
            ResumenReporte resumen = _gestor.GenerarResumen();
            txtResumen.Text = resumen.ComoTexto();
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            string ruta = ObtenerRutaArchivo();

            try
            {
                var persistencia = new PersistenciaArchivo(ruta);
                persistencia.Guardar(_gestor.Registros);
                lblEstadoPersistencia.ForeColor = Color.DarkGreen;
                lblEstadoPersistencia.Text = $"Se guardaron {_gestor.Registros.Count} registro(s) en '{ruta}'.";
            }
            catch (Exception ex)
            {
                lblEstadoPersistencia.ForeColor = Color.DarkRed;
                lblEstadoPersistencia.Text = $"No se pudo guardar el archivo: {ex.Message}";
                MessageBox.Show(lblEstadoPersistencia.Text, "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCargar_Click(object? sender, EventArgs e)
        {
            string ruta = ObtenerRutaArchivo();

            try
            {
                var persistencia = new PersistenciaArchivo(ruta);
                var registros = persistencia.Cargar();
                _gestor.CargarDesde(registros);
                RefrescarGrid();
                lblEstadoPersistencia.ForeColor = Color.DarkGreen;
                lblEstadoPersistencia.Text = $"Se cargaron {registros.Count} registro(s) desde '{ruta}'.";
            }
            catch (Exception ex)
            {
                lblEstadoPersistencia.ForeColor = Color.DarkRed;
                lblEstadoPersistencia.Text = $"No se pudo cargar el archivo: {ex.Message}";
                MessageBox.Show(lblEstadoPersistencia.Text, "Error al cargar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerRutaArchivo()
        {
            string ruta = txtRutaArchivo.Text.Trim();
            return string.IsNullOrWhiteSpace(ruta) ? ArchivoPorDefecto : ruta;
        }

        private void CargarAlIniciar()
        {
            try
            {
                var persistencia = new PersistenciaArchivo(ArchivoPorDefecto);
                var registros = persistencia.Cargar();
                if (registros.Count > 0)
                {
                    _gestor.CargarDesde(registros);
                }
            }
            catch
            {
                // Si el archivo no existe o está corrupto, se inicia con la
                // lista vacía en lugar de bloquear el arranque de la app.
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                var persistencia = new PersistenciaArchivo(ArchivoPorDefecto);
                persistencia.Guardar(_gestor.Registros);
            }
            catch
            {
                // No se interrumpe el cierre de la aplicación por un error al guardar.
            }
        }
    }
}
