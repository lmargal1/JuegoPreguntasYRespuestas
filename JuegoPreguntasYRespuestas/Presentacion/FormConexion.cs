using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq; 
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Servicio;
using JuegoPreguntasYRespuestas.Modelo;
using JuegoPreguntasYRespuestas.DAO; 
using Newtonsoft.Json;

namespace JuegoPreguntasYRespuestas.Presentacion {
    public partial class FormConexion : Form {
        private string _pantallaActual = "ModoRed";
        private string _ipServidor = "127.0.0.1";
        
        private TextBox _cajaIP;
        private TextBox _cajaNombre;
        
        public int IdCategoriaSeleccionada { get; private set; }
        private List<Categoria> _categorias = new List<Categoria>();
        private List<Rectangle> _rectsBotonesCat = new List<Rectangle>();

        public FormConexion() {
            DoubleBuffered = true; ClientSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = Color.FromArgb(5, 5, 25);

            _cajaIP = new TextBox { Width = 300, Font = new Font("Segoe UI", 18), TextAlign = HorizontalAlignment.Center, Visible = false };
            _cajaNombre = new TextBox { Width = 300, Font = new Font("Segoe UI", 18), TextAlign = HorizontalAlignment.Center, Visible = false };
            Controls.Add(_cajaIP); Controls.Add(_cajaNombre);
            
            ConfigurarEscucha();
        }

        private void ConfigurarEscucha() {
            Red.AlRecibirMensaje = (json) => {
                var m = JsonConvert.DeserializeObject<MensajeRed>(json);
                if (m == null) return;
                this.Invoke(new Action(async () => {
                    if (m.Tipo == "ACTUALIZAR_SALA") {
                        Red.JugadoresConectados = m.Contenido.Split(',').ToList();
                        Invalidate();
                    }
                    else if (m.Tipo == "NUEVO_CLIENTE" && Red.EsServidor) {
                        if (!Red.JugadoresConectados.Contains(m.Contenido)) {
                            Red.JugadoresConectados.Add(m.Contenido);
                            await Red.DifundirMensajeAsync(JsonConvert.SerializeObject(new MensajeRed { Tipo = "ACTUALIZAR_SALA", Contenido = string.Join(",", Red.JugadoresConectados) }));
                        }
                        Invalidate();
                    }
                    else if (m.Tipo == "INICIAR_CON_CAT") { 
                        this.IdCategoriaSeleccionada = int.Parse(m.Contenido); 
                        this.DialogResult = DialogResult.OK; 
                        this.Close(); 
                    }
                }));
            };
        }

        private void CargarCategoriasReales() {
            try {
                _categorias = new JuegoDao().ObtenerCategorias(); 
                _rectsBotonesCat.Clear();
                for (int i = 0; i < _categorias.Count; i++) _rectsBotonesCat.Add(new Rectangle(250, 150 + (i * 75), 300, 60));
            } catch { }
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.HighQuality; g.Clear(Color.FromArgb(5, 5, 25));

            _cajaNombre.Location = (_pantallaActual == "ModoRed") ? new Point(250, 150) : new Point(-1000, -1000);
            _cajaNombre.Visible = (_pantallaActual == "ModoRed");
            _cajaIP.Location = (_pantallaActual == "ClienteLogin") ? new Point(250, 250) : new Point(-1000, -1000);
            _cajaIP.Visible = (_pantallaActual == "ClienteLogin");

            if (_pantallaActual == "ModoRed") {
                DibujarTexto(g, "TU NOMBRE:", 20, 250, 110, Color.Gold);
                DibujarBoton(g, new Rectangle(280, 240, 240, 60), "CREAR (Servidor)", Color.DarkRed);
                DibujarBoton(g, new Rectangle(280, 320, 240, 60), "UNIRSE (Cliente)", Color.DarkBlue);
            } 
            else if (_pantallaActual == "ServidorLobby" || _pantallaActual == "ClienteLobby") {
                DibujarTexto(g, "SALA DE ESPERA", 35, 200, 50, Color.Cyan);
                if (Red.EsServidor) g.DrawString($"Tu IP (para invitar): {_ipServidor}", new Font("Segoe UI", 12, FontStyle.Bold), Brushes.White, 250, 110);
                
                g.DrawString("Jugadores Conectados:", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.Gold, 250, 160);
                for (int i = 0; i < Red.JugadoresConectados.Count; i++) {
                    g.DrawString($"• {Red.JugadoresConectados[i]}", new Font("Segoe UI", 14), Brushes.White, 250, 200 + (i * 30));
                }

                if (Red.EsServidor) {
                    bool listo = Red.JugadoresConectados.Count > 1;
                    DibujarBoton(g, new Rectangle(280, 450, 240, 60), listo ? "ELEGIR CATEGORÍA" : "ESPERANDO JUGADORES...", listo ? Color.SeaGreen : Color.Gray);
                } else {
                    g.DrawString("Espera a que el anfitrión inicie...", new Font("Segoe UI", 14, FontStyle.Italic), Brushes.LightGray, 220, 470);
                }
            } 
            else if (_pantallaActual == "ClienteLogin") {
                DibujarTexto(g, "UNIRSE A PARTIDA", 35, 150, 100, Color.Gold);
                g.DrawString("Ingresa la IP del Servidor:", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, 250, 210);
                DibujarBoton(g, new Rectangle(280, 350, 240, 60), "CONECTAR", Color.SeaGreen);
                DibujarBoton(g, new Rectangle(280, 430, 240, 50), "Volver", Color.FromArgb(60, 60, 60));
            } 
            else if (_pantallaActual == "SeleccionCategoria") {
                DibujarTexto(g, "ELIGE LA CATEGORÍA", 35, 150, 60, Color.Gold);
                for (int i = 0; i < _categorias.Count; i++) DibujarBoton(g, _rectsBotonesCat[i], _categorias[i].NombreCategoria.ToUpper(), Color.FromArgb(0, 80, 200));
            }
        }

        protected override async void OnMouseClick(MouseEventArgs e) {
            Point p = e.Location;
            if (_pantallaActual == "ModoRed") {
                string nom = _cajaNombre.Text.Trim();
                if (string.IsNullOrEmpty(nom)) { 
                    nom = "Jugador" + new Random().Next(1000, 10000).ToString(); 
                }

                if (new Rectangle(280, 240, 240, 60).Contains(p)) {
                    Red.NombreLocal = nom;
                    Red.JugadoresConectados.Clear(); Red.PuntajesGlobales.Clear();
                    Red.JugadoresConectados.Add(nom);
                    
                    _ipServidor = Red.ObtenerIPLocal();
                    _ = Task.Run(() => Red.IniciarServidorAsync()); 
                    _pantallaActual = "ServidorLobby";
                } else if (new Rectangle(280, 320, 240, 60).Contains(p)) {
                    Red.NombreLocal = nom;
                    _pantallaActual = "ClienteLogin";
                }
            } 
            else if (_pantallaActual == "ServidorLobby" && Red.JugadoresConectados.Count > 1) {
                if (new Rectangle(280, 450, 240, 60).Contains(p)) {
                    CargarCategoriasReales(); _pantallaActual = "SeleccionCategoria"; Invalidate();
                }
            } 
            else if (_pantallaActual == "ClienteLogin" && new Rectangle(280, 350, 240, 60).Contains(p)) {
                string ip = _cajaIP.Text.Trim();
                if (IPAddress.TryParse(ip, out _)) {
                    await Red.ConectarComoClienteAsync(ip);
                    await Red.EnviarAlServidorAsync(JsonConvert.SerializeObject(new MensajeRed { Tipo = "NUEVO_CLIENTE", Contenido = Red.NombreLocal }));
                    _pantallaActual = "ClienteLobby"; Invalidate();
                }
            } 
            else if (_pantallaActual == "ClienteLogin" && new Rectangle(280, 430, 240, 50).Contains(p)) {
                _pantallaActual = "ModoRed";
            } 
            else if (_pantallaActual == "SeleccionCategoria") {
                for (int i = 0; i < _rectsBotonesCat.Count; i++) {
                    if (_rectsBotonesCat[i].Contains(p)) {
                        this.IdCategoriaSeleccionada = _categorias[i].IdCategoria; 
                        var m = new MensajeRed { Tipo = "INICIAR_CON_CAT", Contenido = this.IdCategoriaSeleccionada.ToString() };
                        await Red.DifundirMensajeAsync(JsonConvert.SerializeObject(m)); 
                        this.DialogResult = DialogResult.OK; this.Close();
                    }
                }
            }
            Invalidate();
        }

        private void DibujarTexto(Graphics g, string t, int s, int x, int y, Color c) {
            using (var f = new Font("Segoe UI", s, FontStyle.Bold)) {
                for (int i = -2; i <= 2; i += 2) for (int j = -2; j <= 2; j += 2) g.DrawString(t, f, Brushes.Black, x + i, y + j);
                g.DrawString(t, f, new SolidBrush(c), x, y);
            }
        }

        private void DibujarBoton(Graphics g, Rectangle r, string t, Color b) {
            using (var path = new GraphicsPath()) {
                int c = r.Height / 3;
                path.AddPolygon(new[] { new Point(r.X + c, r.Y), new Point(r.Right - c, r.Y), new Point(r.Right, r.Y + c), new Point(r.Right, r.Bottom - c), new Point(r.Right - c, r.Bottom), new Point(r.X + c, r.Bottom), new Point(r.X, r.Bottom - c), new Point(r.X, r.Y + c) });
                g.FillPath(new SolidBrush(b), path); g.DrawPath(new Pen(Color.FromArgb(0, 200, 255), 3), path);
            }
            g.DrawString(t, new Font("Segoe UI", 10, FontStyle.Bold), Brushes.White, r, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }
    }
}