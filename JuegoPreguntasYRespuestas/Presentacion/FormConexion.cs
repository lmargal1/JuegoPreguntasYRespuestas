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
using Newtonsoft.Json;

namespace JuegoPreguntasYRespuestas.Presentacion {
    public partial class FormConexion : Form {
        private string _pantallaActual = "ClienteLogin"; 
        private string _ipServidor = "127.0.0.1";
        
        private TextBox _cajaIP;
        private TextBox _cajaNombre;
        
        public int IdCategoriaSeleccionada { get; private set; }
        private List<Categoria> _categorias = new List<Categoria>();
        private List<Rectangle> _rectsBotonesCat = new List<Rectangle>();
        
        private List<string> _jugadoresConectados = new List<string>();

        public FormConexion() {
            DoubleBuffered = true; ClientSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = Color.FromArgb(5, 5, 25);

            _cajaNombre = new TextBox { Width = 300, Font = new Font("Segoe UI", 18), TextAlign = HorizontalAlignment.Center, Location = new Point(250, 200) };
            _cajaIP = new TextBox { Width = 300, Font = new Font("Segoe UI", 18), TextAlign = HorizontalAlignment.Center, Text = "127.0.0.1", Location = new Point(250, 300) };
            
            Controls.Add(_cajaIP); Controls.Add(_cajaNombre);
            
            ConfigurarEscucha();
        }

        private void ConfigurarEscucha() {
            RedCliente.AlRecibirMensaje = (json) => {
                var m = JsonConvert.DeserializeObject<MensajeRed>(json);
                if (m == null) return;
                
                this.Invoke(new Action(() => {
                    if (m.Tipo == "ACTUALIZAR_SALA") {
                        _jugadoresConectados = m.Contenido.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        Invalidate();
                    }
                    else if (m.Tipo == "RECIBIR_CATEGORIAS") {
                        _categorias = JsonConvert.DeserializeObject<List<Categoria>>(m.Contenido);
                        _rectsBotonesCat.Clear();
                        for (int i = 0; i < _categorias.Count; i++) {
                            _rectsBotonesCat.Add(new Rectangle(250, 150 + (i * 75), 300, 60));
                        }
                        _pantallaActual = "SeleccionCategoria";
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

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.HighQuality; g.Clear(Color.FromArgb(5, 5, 25));

            _cajaNombre.Visible = (_pantallaActual == "ClienteLogin");
            _cajaIP.Visible = (_pantallaActual == "ClienteLogin");

            if (_pantallaActual == "ClienteLogin") {
                DibujarTexto(g, "UNIRSE A PARTIDA", 35, 150, 50, Color.Gold);
                g.DrawString("Tu Nombre:", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, 250, 160);
                g.DrawString("IP del Servidor:", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, 250, 260);
                DibujarBoton(g, new Rectangle(280, 400, 240, 60), "CONECTAR", Color.SeaGreen);
            } 
            else if (_pantallaActual == "ClienteLobby") {
                DibujarTexto(g, "SALA DE ESPERA", 35, 200, 50, Color.Cyan);
                g.DrawString("Jugadores Conectados:", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.Gold, 250, 160);
                
                for (int i = 0; i < _jugadoresConectados.Count; i++) {
                    g.DrawString($"• {_jugadoresConectados[i]}", new Font("Segoe UI", 14), Brushes.White, 250, 200 + (i * 30));
                }

                bool listo = _jugadoresConectados.Count > 0;
                DibujarBoton(g, new Rectangle(280, 450, 240, 60), listo ? "ELEGIR CATEGORÍA" : "ESPERANDO...", listo ? Color.SeaGreen : Color.Gray);
            } 
            else if (_pantallaActual == "SeleccionCategoria") {
                DibujarTexto(g, "ELIGE LA CATEGORÍA", 35, 150, 60, Color.Gold);
                for (int i = 0; i < _categorias.Count; i++) DibujarBoton(g, _rectsBotonesCat[i], _categorias[i].NombreCategoria.ToUpper(), Color.FromArgb(0, 80, 200));
            }
        }

        protected override async void OnMouseClick(MouseEventArgs e) {
            Point p = e.Location;
            
            if (_pantallaActual == "ClienteLogin" && new Rectangle(280, 400, 240, 60).Contains(p)) {
                string nom = _cajaNombre.Text.Trim();
                if (string.IsNullOrEmpty(nom)) nom = "Jugador" + new Random().Next(1000, 10000).ToString(); 
                RedCliente.NombreLocal = nom;

                string ip = _cajaIP.Text.Trim();
                if (IPAddress.TryParse(ip, out _)) {
                    await RedCliente.ConectarComoClienteAsync(ip);
                    await RedCliente.EnviarAlServidorAsync(JsonConvert.SerializeObject(new MensajeRed { Tipo = "NUEVO_CLIENTE", Contenido = RedCliente.NombreLocal }));
                    _pantallaActual = "ClienteLobby"; 
                    Invalidate();
                }
            } 
            else if (_pantallaActual == "ClienteLobby" && new Rectangle(280, 450, 240, 60).Contains(p)) {
                var m = new MensajeRed { Tipo = "PEDIR_CATEGORIAS", Contenido = "" };
                await RedCliente.EnviarAlServidorAsync(JsonConvert.SerializeObject(m));
            } 
            else if (_pantallaActual == "SeleccionCategoria") {
                for (int i = 0; i < _rectsBotonesCat.Count; i++) {
                    if (_rectsBotonesCat[i].Contains(p)) {
                        this.IdCategoriaSeleccionada = _categorias[i].IdCategoria; 
                        var m = new MensajeRed { Tipo = "INICIAR_CON_CAT", Contenido = this.IdCategoriaSeleccionada.ToString() };
                        await RedCliente.EnviarAlServidorAsync(JsonConvert.SerializeObject(m)); 
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