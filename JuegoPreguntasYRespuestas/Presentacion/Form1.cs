using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.DAO;

namespace JuegoPreguntasYRespuestas.Presentacion {
    public class CuadroAnimado { public float X, Y, VelX, VelY; public int Tamaño, Opacidad; public Color ColorCuadro; }

    public partial class Form1 : Form {
        private const int TiempoPorPregunta = 20; 
        private const int TotalParticulas = 60;                        
        private readonly Color _colorFondo = Color.FromArgb(5, 5, 25);
        private readonly Color _colorBordes = Color.FromArgb(0, 200, 255);


        private string _pantallaActual = "Inicio";
        private List<Categoria> _categoriasLista;
        private List<Opcion> _opcionesActuales;
        private List<string> _listaHistorial;
        
        private int _idCategoriaSeleccionada;
        private int _tiempoRestante;
        private int? _indexOpcionSeleccionada;
        private bool? _respuestaCorrecta;

        private Process _procesoMusica;
        private System.Media.SoundPlayer _reproductorWindows;
        private Timer _timerCronometro, _timerFeedback;
        private readonly List<CuadroAnimado> _listaCuadros = new List<CuadroAnimado>();
        private readonly Random _rnd = new Random();

        public Form1() {
            InitializeComponent();
            ConfigurarVentana();
            IniciarParticulas();
            CambiarMusica("tron_music.wav"); 
            
            Timer motorAnimacion = new Timer { Interval = 16 };
            motorAnimacion.Tick += (s, e) => { ActualizarParticulas(); Invalidate(); };
            
            (_timerCronometro = new Timer { Interval = 1000 }).Tick += (s, e) => { 
                _tiempoRestante--;
                if (_tiempoRestante <= 0) FinalizarJuego(); 
                Invalidate(); 
            };
            
            (_timerFeedback = new Timer { Interval = 1500 }).Tick += TimerFeedback_Tick;
            
            motorAnimacion.Start();
        }

        private void ConfigurarVentana() {
            Text = @"Trivia Máxima"; 
            DoubleBuffered = true; 
            ClientSize = new Size(800, 600);
            MinimumSize = new Size(800, 600); 
            StartPosition = FormStartPosition.CenterScreen; 
            
            FormBorderStyle = FormBorderStyle.Sizable; 
            BackColor = _colorFondo;
            
            TransparencyKey = Color.Empty; 
        }

        private void CambiarMusica(string n) {
            try {
                if (_procesoMusica != null && !_procesoMusica.HasExited) _procesoMusica.Kill();
                _reproductorWindows?.Stop();
                
                string r = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Presentacion", "musica", n));
                if (!File.Exists(r)) return;

                if (Environment.OSVersion.Platform == PlatformID.Unix) 
                    _procesoMusica = Process.Start(new ProcessStartInfo("paplay", $"\"{r}\"") { CreateNoWindow = true, UseShellExecute = false });
                else {
                    _reproductorWindows = new System.Media.SoundPlayer(r);
                    _reproductorWindows.PlayLooping();
                }
            } catch (Exception ex) { Console.WriteLine(@"Error Audio: " + ex.Message); }
        }

        private void IniciarParticulas() {
            _listaCuadros.Clear();
            for (int i = 0; i < TotalParticulas; i++) 
                _listaCuadros.Add(new CuadroAnimado { 
                    X = _rnd.Next(2000), Y = _rnd.Next(2000),
                    Tamaño = _rnd.Next(5, 20), 
                    VelX = (float)(_rnd.NextDouble() * 3 + 1), 
                    VelY = 0,
                    Opacidad = _rnd.Next(40, 150), 
                    ColorCuadro = Color.FromArgb(0, 150, 255) 
                });
        }

        private void ActualizarParticulas() {
            int ancho = ClientSize.Width + 20;
            int alto = ClientSize.Height + 20;

            _listaCuadros.ForEach(c => { 
                c.X += c.VelX; 
                c.Y += c.VelY; 
                if (c.X > ancho) c.X = -20; if (c.X < -20) c.X = ancho; 
                if (c.Y > alto) c.Y = -20; if (c.Y < -20) c.Y = alto; 
            });
        }

        private void TimerFeedback_Tick(object sender, EventArgs e) {
            _timerFeedback.Stop(); 
            _indexOpcionSeleccionada = null; 
            _respuestaCorrecta = null;
            
            if (JuegoServicio.siguientePregunta()) { 
                CargarPreguntaActual(); 
                _timerCronometro.Start(); 
            } else {
                FinalizarJuego();
            }
            Invalidate(); 
        }

        private void FinalizarJuego() { 
            _timerCronometro.Stop(); 
            new JuegoDao().GuardarPartida(_idCategoriaSeleccionada, JuegoServicio.correctas, JuegoServicio.incorrectas); 
            _pantallaActual = "Puntaje"; 
            CambiarMusica("tron_music.wav"); 
        }

        private void CargarPreguntaActual() {
            var p = JuegoServicio.obtenerPreguntaActual();
            if (p == null) return;
            
            _opcionesActuales = new JuegoDao().ObtenerOpcionesPorPregunta(p.IdPregunta);
            
            if (_opcionesActuales == null || _opcionesActuales.Count == 0) { 
                JuegoServicio.siguientePregunta(); 
                CargarPreguntaActual(); 
                return; 
            }
            
            _tiempoRestante = TiempoPorPregunta; 
            
            Color nc = Color.FromArgb(_rnd.Next(100, 255), _rnd.Next(100, 255), _rnd.Next(100, 255));
            _listaCuadros.ForEach(c => { c.ColorCuadro = nc; c.VelX = _rnd.Next(-5, 6); c.VelY = _rnd.Next(-5, 6); });
        }



        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics; 
            g.SmoothingMode = SmoothingMode.HighQuality; 
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            g.Clear(_colorFondo);
            _listaCuadros.ForEach(c => { 
                using (var b = new SolidBrush(Color.FromArgb(c.Opacidad, c.ColorCuadro))) 
                    g.FillRectangle(b, c.X, c.Y, c.Tamaño, c.Tamaño); 
            });

            float offsetX = (ClientSize.Width - 800) / 2f;
            float offsetY = (ClientSize.Height - 600) / 2f;
            g.TranslateTransform(offsetX, offsetY); 
            
            if (_pantallaActual == "Inicio") { 
                DibujarTexto(g, "TRIVIA MÁXIMA", 50, 110, 150, Color.Gold); 
                DibujarBoton(g, new Rectangle(280, 280, 240, 60), "¡JUGAR!", Color.FromArgb(0, 30, 150)); 
                DibujarBoton(g, new Rectangle(280, 360, 240, 60), "HISTORIAL", Color.FromArgb(40, 40, 40)); 
            } else if (_pantallaActual == "Categorias") {
                DibujarTexto(g, "Elige Categoría", 30, 220, 50, Color.Gold);
                
                if (_categoriasLista != null) {
                    for (int i = 0; i < _categoriasLista.Count; i++) {
                        DibujarBoton(g, new Rectangle(200, 130 + (i * 65), 400, 50), _categoriasLista[i].NombreCategoria, Color.FromArgb(50, 0, 150));
                    }
                    DibujarBoton(g, new Rectangle(200, 130 + (_categoriasLista.Count * 65), 400, 50), "🎲 MODO ALEATORIO", Color.Maroon);
                }
            } else if (_pantallaActual == "Historial") {
                DibujarTexto(g, "ÚLTIMAS PARTIDAS", 35, 180, 50, Color.Gold); 
                using (var b = new SolidBrush(Color.FromArgb(180, 10, 10, 40))) g.FillRectangle(b, 150, 120, 500, 350); 
                using (var pen = new Pen(_colorBordes, 2)) g.DrawRectangle(pen, 150, 120, 500, 350);
                
                if (_listaHistorial != null) {
                    for (int i = 0; i < _listaHistorial.Count; i++) {
                        g.DrawString(_listaHistorial[i], new Font("Consolas", 14, FontStyle.Bold), Brushes.White, 170, 140 + (i * 30));
                    }
                }
                
                DibujarBoton(g, new Rectangle(280, 490, 240, 60), "VOLVER", Color.FromArgb(60, 60, 60));
            } else if (_pantallaActual == "Jugando") {
                DibujarPantallaJuego(g);
            } else if (_pantallaActual == "Puntaje") { 
                DibujarTexto(g, "RESULTADOS", 45, 200, 80, Color.Gold); 
                string resultadoTxt = $"Correctas: {JuegoServicio.correctas}\nIncorrectas: {JuegoServicio.incorrectas}\nEfectividad: {JuegoServicio.calcularPorcentajeCorrecto()}%";
                g.DrawString(resultadoTxt, new Font("Segoe UI", 25, FontStyle.Bold), Brushes.White, new Rectangle(100, 200, 600, 200), new StringFormat { Alignment = StringAlignment.Center }); 
                DibujarBoton(g, new Rectangle(280, 420, 240, 70), "REINICIAR", Color.SeaGreen); 
            }
        }

        private void DibujarPantallaJuego(Graphics g) {
            var p = JuegoServicio.obtenerPreguntaActual(); 
            if (p == null) return;
            
            // Reloj en la esquina superior derecha
            DibujarTexto(g, _tiempoRestante.ToString(), 35, 710, 20, _tiempoRestante > 5 ? Color.Cyan : Color.Red);
            
            // Texto de la pregunta centrado
            g.DrawString(p.TextoPregunta, new Font("Segoe UI", 18, FontStyle.Bold), Brushes.White, new Rectangle(50, 100, 700, 80), new StringFormat { Alignment = StringAlignment.Center });
            
            bool esImagen = JuegoServicio.esTipo(p) == "imagen";
            
            if (_opcionesActuales != null) {
                for (int i = 0; i < _opcionesActuales.Count; i++) {
                    // Grilla: i%2 da Izquierda/Derecha. i/2 da Fila superior/inferior.
                    Rectangle r = new Rectangle((i % 2 == 0) ? 100 : 420, esImagen ? 200 + (i / 2 * 135) : 250 + (i / 2 * 80), 280, esImagen ? 120 : 60);
                    Color c = (_indexOpcionSeleccionada == i) ? (_respuestaCorrecta == true ? Color.Lime : Color.Crimson) : Color.FromArgb(30, 30, 80);
                    
                    DibujarBoton(g, r, "", c);
                    
                    if (esImagen && !string.IsNullOrEmpty(_opcionesActuales[i].RutaImagen)) {
                        try { 
                            string fp = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", _opcionesActuales[i].RutaImagen)); 
                            if (File.Exists(fp)) { 
                                using (Image im = Image.FromFile(fp)) { 
                                    // Escala proporcionalmente para que las imágenes no se estiren
                                    float s = Math.Min((float)(r.Width - 10) / im.Width, (float)(r.Height - 10) / im.Height); 
                                    g.DrawImage(im, r.X + (r.Width - im.Width * s) / 2, r.Y + (r.Height - im.Height * s) / 2, im.Width * s, im.Height * s); 
                                } 
                            } 
                        } catch (Exception ex) { Console.WriteLine(@"Error Img: " + ex.Message); }
                    } else {
                        g.DrawString(_opcionesActuales[i].TextoOpcion, new Font("Segoe UI", 11, FontStyle.Bold), Brushes.White, r, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    }
                }
            }
        }

        // Borde falso negro para que el texto resalte (estilo sombra)
        private void DibujarTexto(Graphics g, string t, int s, int x, int y, Color c) { 
            using (var f = new Font("Segoe UI", s, FontStyle.Bold)) { 
                for (int i = -2; i <= 2; i += 2) for (int j = -2; j <= 2; j += 2) g.DrawString(t, f, Brushes.Black, x + i, y + j); 
                g.DrawString(t, f, new SolidBrush(c), x, y); 
            } 
        }

        private void DibujarBoton(Graphics g, Rectangle r, string t, Color b) {
            using (var path = new GraphicsPath()) { 
                // Corte de esquinas Sci-Fi (Geometría del botón)
                int c = r.Height / 3; 
                path.AddPolygon(new[] { 
                    new Point(r.X + c, r.Y), new Point(r.Right - c, r.Y), 
                    new Point(r.Right, r.Y + c), new Point(r.Right, r.Bottom - c), 
                    new Point(r.Right - c, r.Bottom), new Point(r.X + c, r.Bottom), 
                    new Point(r.X, r.Bottom - c), new Point(r.X, r.Y + c) 
                }); 
                g.FillPath(new SolidBrush(b), path); 
                g.DrawPath(new Pen(_colorBordes, 3), path); 
            }
            if (!string.IsNullOrEmpty(t)) 
                g.DrawString(t, new Font("Segoe UI", t.Length > 20 ? 9 : 11, FontStyle.Bold), Brushes.White, r, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        // ====================================================================
        // 🖱️ ZONA DE CLICS E INTERACCIÓN
        // ====================================================================
        protected override void OnMouseClick(MouseEventArgs e) {
            // Nota equipo: Se resta el centro al ratón para que los clics no se desfasen al maximizar la ventana
            float offsetX = (ClientSize.Width - 800) / 2f;
            float offsetY = (ClientSize.Height - 600) / 2f;
            Point clickReal = new Point((int)(e.X - offsetX), (int)(e.Y - offsetY));

            if (_pantallaActual == "Inicio") { 
                if (new Rectangle(280, 280, 240, 60).Contains(clickReal)) { _categoriasLista = new JuegoDao().ObtenerCategorias(); _pantallaActual = "Categorias"; } 
                else if (new Rectangle(280, 360, 240, 60).Contains(clickReal)) { _listaHistorial = new JuegoDao().ObtenerHistorial(); _pantallaActual = "Historial"; } 
            } else if (_pantallaActual == "Categorias") {
                if (_categoriasLista != null) {
                    for (int i = 0; i < _categoriasLista.Count; i++) {
                        if (new Rectangle(200, 130 + (i * 65), 400, 50).Contains(clickReal)) { 
                            _idCategoriaSeleccionada = _categoriasLista[i].IdCategoria; 
                            IniciarPartida(new JuegoDao().ObtenerPreguntasPorCategoria(_idCategoriaSeleccionada)); 
                            return; 
                        }
                    }
                    if (new Rectangle(200, 130 + (_categoriasLista.Count * 65), 400, 50).Contains(clickReal)) { 
                        _idCategoriaSeleccionada = 0; 
                        IniciarPartida(new JuegoDao().ObtenerTodasLasPreguntas()); 
                    }
                }
            } else if (_pantallaActual == "Historial" && new Rectangle(280, 490, 240, 60).Contains(clickReal)) {
                _pantallaActual = "Inicio";
            } else if (_pantallaActual == "Jugando" && _indexOpcionSeleccionada == null) {
                var p = JuegoServicio.obtenerPreguntaActual(); 
                if (p == null) return;
                bool img = JuegoServicio.esTipo(p) == "imagen";
                
                if (_opcionesActuales != null) {
                    for (int i = 0; i < _opcionesActuales.Count; i++) {
                        Rectangle r = new Rectangle((i % 2 == 0) ? 100 : 420, img ? 200 + (i / 2 * 135) : 250 + (i / 2 * 80), 280, img ? 120 : 60);
                        if (r.Contains(clickReal)) { 
                            _timerCronometro.Stop(); 
                            _indexOpcionSeleccionada = i; 
                            _respuestaCorrecta = _opcionesActuales[i].EsCorrecta; 
                            JuegoServicio.validaRespuesta(_opcionesActuales[i].IdOpcion, _opcionesActuales); 
                            _timerFeedback.Start(); 
                        }
                    }
                }
            } else if (_pantallaActual == "Puntaje" && new Rectangle(280, 420, 240, 70).Contains(clickReal)) { 
                _pantallaActual = "Inicio"; 
                IniciarParticulas(); 
            }
            Invalidate(); 
        }

        private void IniciarPartida(List<Pregunta> l) { 
            if (l == null || l.Count == 0) return; 
            JuegoServicio.iniciaJuego(l); 
            CargarPreguntaActual(); 
            _pantallaActual = "Jugando"; 
            CambiarMusica("tron_loop.wav"); // Música al entrar a la partida
            _timerCronometro.Start(); 
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { 
            if (_procesoMusica != null && !_procesoMusica.HasExited) _procesoMusica.Kill(); 
            base.OnFormClosed(e); 
        }
    }
}