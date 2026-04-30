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
    
    // ZONA DE CONFIGURACION RAPIDA
    private const int TiempoPorPregunta = 15; 
    private const int TotalParticulas = 75;                        
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
    private int _vidasRestantes = 3;
    private float _puntosTemporales = 0f;

    private Process _procesoMusica;
    private System.Media.SoundPlayer _reproductorWindows;
    private Timer _timerCronometro, _timerFeedback;
    private readonly List<CuadroAnimado> _listaCuadros = new List<CuadroAnimado>();
    private readonly Random _rnd = new Random();

    // INICIALIZACION
    public Form1() {
        InitializeComponent();
        ConfigurarVentana();
        IniciarParticulas();
        CambiarMusica("tron_music.wav"); 
        
        Timer motorAnimacion = new Timer { Interval = 16 };
        motorAnimacion.Tick += (s, e) => { ActualizarParticulas(); Invalidate(); };
        
        (_timerCronometro = new Timer { Interval = 1000 }).Tick += (s, e) => { 
            _tiempoRestante--;
            if (_tiempoRestante <= 0) {
                _timerCronometro.Stop();
                PerderVidaYContinuar();
            }
            Invalidate(); 
        };
        
        (_timerFeedback = new Timer { Interval = 1500 }).Tick += TimerFeedback_Tick;
        
        motorAnimacion.Start();
    }

    // CONFIGURACION DE VENTANA
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

    // CONTROL DE MUSICA
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

    // SISTEMA DE PARTICULAS NORMALES
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

    // EXPLOSION FINAL VICTORIA
    private void IniciarExplosion() {
        _listaCuadros.Clear();
        Color[] coloresExplosion = { Color.Gold, Color.Lime, Color.Cyan, Color.White };
        
        int centroX = ClientSize.Width / 2;
        int centroY = ClientSize.Height / 2;

        for (int i = 0; i < 150; i++) { 
            double angulo = _rnd.NextDouble() * Math.PI * 2;
            double velocidad = _rnd.NextDouble() * 18 + 5; 
            
            _listaCuadros.Add(new CuadroAnimado { 
                X = centroX, 
                Y = centroY, 
                Tamaño = _rnd.Next(4, 12), 
                VelX = (float)(Math.Cos(angulo) * velocidad), 
                VelY = (float)(Math.Sin(angulo) * velocidad),
                Opacidad = 255, 
                ColorCuadro = coloresExplosion[_rnd.Next(coloresExplosion.Length)]
            });
        }
    }

    // CENIZAS FINAL DERROTA
    private void IniciarCenizas() {
        _listaCuadros.Clear();
        for (int i = 0; i < 100; i++) {
            _listaCuadros.Add(new CuadroAnimado {
                X = _rnd.Next(0, ClientSize.Width),
                Y = _rnd.Next(ClientSize.Height, ClientSize.Height + 200),
                Tamaño = _rnd.Next(3, 8),
                VelX = (float)(_rnd.NextDouble() * 2 - 1),
                VelY = (float)(_rnd.NextDouble() * -2 - 1), 
                Opacidad = _rnd.Next(100, 255),
                ColorCuadro = Color.FromArgb(120, 120, 120) 
            });
        }
    }

    // ACTUALIZAR PARTICULAS
    private void ActualizarParticulas() {
        int ancho = ClientSize.Width + 20;
        int alto = ClientSize.Height + 20;

        if (_pantallaActual == "Puntaje") {
            if (JuegoServicio.correctas >= 6) {
                _listaCuadros.ForEach(c => { 
                    c.X += c.VelX; 
                    c.Y += c.VelY;
                    c.VelY += 0.15f; 
                    c.VelX *= 0.96f; 
                    c.VelY *= 0.96f; 
                    
                    if (c.Opacidad > 4) c.Opacidad -= 4; 
                    else {
                        c.X = _rnd.Next(0, ClientSize.Width); 
                        c.Y = _rnd.Next(0, ClientSize.Height);
                        double angulo = _rnd.NextDouble() * Math.PI * 2;
                        double velocidad = _rnd.NextDouble() * 8 + 2;
                        c.VelX = (float)(Math.Cos(angulo) * velocidad);
                        c.VelY = (float)(Math.Sin(angulo) * velocidad);
                        c.Opacidad = 255;
                    }
                });
            } else {
                _listaCuadros.ForEach(c => {
                    c.X += c.VelX;
                    c.Y += c.VelY;
                    c.Opacidad -= 2;
                    if (c.Opacidad <= 0 || c.Y < -10) {
                        c.Y = ClientSize.Height + 10;
                        c.X = _rnd.Next(0, ClientSize.Width);
                        c.Opacidad = _rnd.Next(100, 255);
                    }
                });
            }
        } else {
            _listaCuadros.ForEach(c => { 
                c.X += c.VelX; 
                c.Y += c.VelY; 
                if (c.X > ancho) c.X = -20; if (c.X < -20) c.X = ancho; 
                if (c.Y > alto) c.Y = -20; if (c.Y < -20) c.Y = alto; 
            });
        }
    }

    // FLUJO DEL JUEGO
    private void TimerFeedback_Tick(object sender, EventArgs e) {
        _timerFeedback.Stop(); 
        _indexOpcionSeleccionada = null; 
        _respuestaCorrecta = null;
        
        if (_vidasRestantes > 0 && JuegoServicio.siguientePregunta()) { 
            CargarPreguntaActual(); 
            _timerCronometro.Start(); 
        } else {
            FinalizarJuego();
        }
        Invalidate(); 
    }

    // PERDER VIDA POR TIEMPO
    private void PerderVidaYContinuar() {
        _vidasRestantes--;
        _respuestaCorrecta = false;
        JuegoServicio.incorrectas++;
        
        if (_vidasRestantes <= 0) {
            FinalizarJuego();
        } else {
            _timerFeedback.Start();
        }
    }

    // FINALIZAR JUEGO CON REDONDEO
    private void FinalizarJuego() { 
        _timerCronometro.Stop(); 
        
        JuegoServicio.correctas = (int)Math.Round(_puntosTemporales);

        new JuegoDao().GuardarPartida(_idCategoriaSeleccionada, JuegoServicio.correctas, JuegoServicio.incorrectas); 
        _pantallaActual = "Puntaje"; 
        CambiarMusica("tron_music.wav"); 
        
        if (JuegoServicio.correctas >= 6) {
            IniciarExplosion(); 
        } else {
            IniciarCenizas();
        }
        
        BackColor = _colorFondo;
    }

    // CARGAR PREGUNTA
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
        if (_pantallaActual != "Puntaje") {
            _listaCuadros.ForEach(c => { c.ColorCuadro = nc; c.VelX = _rnd.Next(-5, 6); c.VelY = _rnd.Next(-5, 6); });
        }
    }

    // INICIAR PARTIDA
    private void IniciarPartida(List<Pregunta> l) { 
        if (l == null || l.Count == 0) return; 
        JuegoServicio.iniciaJuego(l);
        _vidasRestantes = 3; 
        _puntosTemporales = 0f;
        
        // --- LIMPIEZA DE VARIABLES DE LA PARTIDA ANTERIOR ---
        _indexOpcionSeleccionada = null; 
        _respuestaCorrecta = null;
        
        CargarPreguntaActual(); 
        _pantallaActual = "Jugando"; 
        _timerCronometro.Start(); 
    }

    // ZONA DE DIBUJO
    protected override void OnPaint(PaintEventArgs e) {
        Graphics g = e.Graphics; 
        g.SmoothingMode = SmoothingMode.HighQuality; 
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        if (_pantallaActual == "Jugando" && _tiempoRestante <= 5 && DateTime.Now.Millisecond < 500) {
            g.Clear(Color.FromArgb(50, 0, 0));
        } else {
            g.Clear(_colorFondo);
        }

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
                DibujarBoton(g, new Rectangle(200, 130 + (_categoriasLista.Count * 65), 400, 50), "MUESTRA ALEATORIA", Color.Maroon);
                DibujarBoton(g, new Rectangle(200, 130 + ((_categoriasLista.Count + 1) * 65), 400, 50), "Regresar", Color.FromArgb(60, 60, 60));
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
            
            string titulo = JuegoServicio.correctas >= 6 ? "RESULTADOS" : "¡BUEN INTENTO!";
            Color colorTitulo = JuegoServicio.correctas >= 6 ? Color.Gold : Color.LightPink;
            
            DibujarTexto(g, titulo, 45, 150, 80, colorTitulo); 
            
            string resultadoTxt = $"Correctas: {JuegoServicio.correctas}\nIncorrectas: {JuegoServicio.incorrectas}\nEfectividad: {JuegoServicio.calcularPorcentajeCorrecto()}%";
            g.DrawString(resultadoTxt, new Font("Segoe UI", 25, FontStyle.Bold), Brushes.White, new Rectangle(100, 180, 600, 200), new StringFormat { Alignment = StringAlignment.Center }); 
            
            string msjLindo = JuegoServicio.correctas >= 6 ? "¡Excelente trabajo!" : "No pasa nada, ¡seguro la próxima te va súper bien!";
            g.DrawString(msjLindo, new Font("Segoe UI", 16, FontStyle.Italic), Brushes.LightGray, new Rectangle(100, 360, 600, 50), new StringFormat { Alignment = StringAlignment.Center });

            DibujarBoton(g, new Rectangle(280, 440, 240, 70), "REINICIAR", Color.SeaGreen); 
        }
    }

    // PANTALLA JUGANDO
    private void DibujarPantallaJuego(Graphics g) {
        var p = JuegoServicio.obtenerPreguntaActual(); 
        if (p == null) return;
        
        DibujarTexto(g, _tiempoRestante.ToString(), 35, 710, 20, _tiempoRestante > 5 ? Color.Cyan : Color.Red);
        DibujarTexto(g, $"Vidas: {_vidasRestantes}", 20, 50, 20, Color.Crimson);
        
        g.DrawString(p.TextoPregunta, new Font("Segoe UI", 18, FontStyle.Bold), Brushes.White, new Rectangle(50, 100, 700, 80), new StringFormat { Alignment = StringAlignment.Center });
        
        bool esImagen = JuegoServicio.esTipo(p) == "imagen";
        
        int shakeX = 0;
        int shakeY = 0;
        if (_tiempoRestante <= 5 && _indexOpcionSeleccionada == null) {
            int intensidad = (6 - _tiempoRestante) * 2;
            shakeX = _rnd.Next(-intensidad, intensidad + 1);
            shakeY = _rnd.Next(-intensidad, intensidad + 1);
        }
        
        if (_opcionesActuales != null) {
            for (int i = 0; i < _opcionesActuales.Count; i++) {
                int baseX = (i % 2 == 0) ? 100 : 420;
                int baseY = esImagen ? 200 + (i / 2 * 135) : 250 + (i / 2 * 80);
                
                Rectangle r = new Rectangle(baseX + shakeX, baseY + shakeY, 280, esImagen ? 120 : 60);
                Color c = (_indexOpcionSeleccionada == i) ? (_respuestaCorrecta == true ? Color.Lime : Color.Crimson) : Color.FromArgb(30, 30, 80);
                
                DibujarBoton(g, r, "", c);
                
                if (esImagen && !string.IsNullOrEmpty(_opcionesActuales[i].RutaImagen)) {
                    try { 
                        string fp = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", _opcionesActuales[i].RutaImagen)); 
                        if (File.Exists(fp)) { 
                            using (Image im = Image.FromFile(fp)) { 
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

    // UTILIDAD DIBUJAR TEXTO
    private void DibujarTexto(Graphics g, string t, int s, int x, int y, Color c) { 
        using (var f = new Font("Segoe UI", s, FontStyle.Bold)) { 
            for (int i = -2; i <= 2; i += 2) for (int j = -2; j <= 2; j += 2) g.DrawString(t, f, Brushes.Black, x + i, y + j); 
            g.DrawString(t, f, new SolidBrush(c), x, y); 
        } 
    }

    // UTILIDAD DIBUJAR BOTON
    private void DibujarBoton(Graphics g, Rectangle r, string t, Color b) {
        using (var path = new GraphicsPath()) { 
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

    // CLICS DEL RATON
    protected override void OnMouseClick(MouseEventArgs e) {
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
                if (new Rectangle(200, 130 + ((_categoriasLista.Count + 1) * 65), 400, 50).Contains(clickReal))
                {
                    _pantallaActual = "Inicio";
                }
            }
        } else if (_pantallaActual == "Historial" && new Rectangle(280, 490, 240, 60).Contains(clickReal)) {
            _pantallaActual = "Inicio";
        } else if (_pantallaActual == "Jugando" && _indexOpcionSeleccionada == null) {
            var p = JuegoServicio.obtenerPreguntaActual(); 
            if (p == null) return;
            bool img = JuegoServicio.esTipo(p) == "imagen";
            
            int shakeX = 0;
            int shakeY = 0;
            if (_tiempoRestante <= 5) {
                int intensidad = (6 - _tiempoRestante) * 2;
                shakeX = _rnd.Next(-intensidad, intensidad + 1);
                shakeY = _rnd.Next(-intensidad, intensidad + 1);
            }

            if (_opcionesActuales != null) {
                for (int i = 0; i < _opcionesActuales.Count; i++) {
                    int baseX = (i % 2 == 0) ? 100 : 420;
                    int baseY = img ? 200 + (i / 2 * 135) : 250 + (i / 2 * 80);
                    Rectangle r = new Rectangle(baseX + shakeX, baseY + shakeY, 280, img ? 120 : 60);
                    
                    if (r.Contains(clickReal)) { 
                        _timerCronometro.Stop(); 
                        _indexOpcionSeleccionada = i; 
                        _respuestaCorrecta = _opcionesActuales[i].EsCorrecta; 
                        
                        if (_respuestaCorrecta == true) {
                            if (_tiempoRestante > 5) {
                                _puntosTemporales += 1f;
                            } else {
                                _puntosTemporales += 0.5f; 
                            }
                        } else {
                            _vidasRestantes--;
                        }

                        JuegoServicio.validaRespuesta(_opcionesActuales[i].IdOpcion, _opcionesActuales); 
                        
                        if (_vidasRestantes <= 0) {
                            FinalizarJuego();
                        } else {
                            _timerFeedback.Start(); 
                        }
                    }
                }
            }
        } else if (_pantallaActual == "Puntaje" && new Rectangle(280, 440, 240, 70).Contains(clickReal)) { 
            _pantallaActual = "Inicio"; 
            
            // --- LIMPIEZA ADICIONAL AL VOLVER AL INICIO ---
            _indexOpcionSeleccionada = null;
            _respuestaCorrecta = null;
            
            IniciarParticulas(); 
        }
        Invalidate(); 
    }
}
}