using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Data;

namespace JuegoPreguntasYRespuestas {
    public class CuadroAnimado {
        public float X, Y, VelX, VelY;
        public int Tamaño, Opacidad;
        public Color ColorCuadro;
    }

    public partial class Form1 : Form {
        private string pantallaActual = "Inicio";
        private List<Categoria> categoriasLista;
        private List<Opcion> opcionesActuales;
        private List<string> listaHistorial;
        private int idCategoriaSeleccionada, tiempoRestante = 20;
        private int? indexOpcionSeleccionada = null;
        private bool? respuestaCorrecta = null;
        
        private Process procesoMusica;
        private System.Media.SoundPlayer reproductorWindows;
        private Timer motorAnimacion, timerCronometro, timerFeedback;
        private List<CuadroAnimado> listaCuadros = new List<CuadroAnimado>();
        private Random rnd = new Random();
        private readonly Color azulFondo = Color.FromArgb(5, 5, 25), cyanBorde = Color.FromArgb(0, 200, 255);

        public Form1() {
            InitializeComponent();
            this.Text = "Trivia Máxima";
            this.DoubleBuffered = true;
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = azulFondo;

            IniciarParticulas();
            CambiarMusica("tron_music.wav"); 

            motorAnimacion = new Timer { Interval = 16 };
            motorAnimacion.Tick += (s, e) => { ActualizarParticulas(); this.Invalidate(); };
            motorAnimacion.Start();

            timerCronometro = new Timer { Interval = 1000 };
            timerCronometro.Tick += TimerCronometro_Tick;

            timerFeedback = new Timer { Interval = 1500 };
            timerFeedback.Tick += TimerFeedback_Tick;
        }

        private void TimerCronometro_Tick(object sender, EventArgs e) {
            tiempoRestante--;
            if (tiempoRestante <= 0) FinalizarJuego();
            this.Invalidate();
        }

        private void CambiarMusica(string n) {
            try {
                if (procesoMusica != null && !procesoMusica.HasExited) { procesoMusica.Kill(); procesoMusica.Dispose(); }
                if (reproductorWindows != null) { reproductorWindows.Stop(); reproductorWindows.Dispose(); }

                string r = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Presentacion", "musica", n));
                if (File.Exists(r)) {
                    if (Environment.OSVersion.Platform == PlatformID.Unix) 
                        procesoMusica = Process.Start(new ProcessStartInfo("paplay", $"\"{r}\"") { CreateNoWindow = true, UseShellExecute = false });
                    else {
                        reproductorWindows = new System.Media.SoundPlayer(r);
                        reproductorWindows.PlayLooping();
                    }
                }
            } catch { }
        }

        private void IniciarParticulas() {
            listaCuadros.Clear();
            for (int i = 0; i < 60; i++) listaCuadros.Add(new CuadroAnimado { X = rnd.Next(800), Y = rnd.Next(600), Tamaño = rnd.Next(5, 20), VelX = (float)(rnd.NextDouble() * 3 + 1), VelY = 0, Opacidad = rnd.Next(40, 150), ColorCuadro = Color.FromArgb(0, 150, 255) });
        }

        private void ActualizarParticulas() => listaCuadros.ForEach(c => { 
            c.X += c.VelX; c.Y += c.VelY;
            if (c.X > 820) c.X = -20; if (c.X < -20) c.X = 820;
            if (c.Y > 620) c.Y = -20; if (c.Y < -20) c.Y = 620;
        });

        private void AlterarFondo() {
            Color nc = Color.FromArgb(rnd.Next(100, 255), rnd.Next(100, 255), rnd.Next(100, 255));
            listaCuadros.ForEach(c => { c.ColorCuadro = nc; c.VelX = rnd.Next(-5, 6); c.VelY = rnd.Next(-5, 6); });
        }

        private void TimerFeedback_Tick(object sender, EventArgs e) {
            timerFeedback.Stop();
            indexOpcionSeleccionada = null; respuestaCorrecta = null;
            if (JuegoServicio.siguientePregunta()) { CargarPreguntaActual(); timerCronometro.Start(); } else FinalizarJuego();
            Invalidate();
        }

        private void FinalizarJuego() {
            timerCronometro.Stop();
            new JuegoDAO().GuardarPartida(idCategoriaSeleccionada, JuegoServicio.correctas, JuegoServicio.incorrectas);
            pantallaActual = "Puntaje"; CambiarMusica("tron_music.wav");
        }

        private void CargarPreguntaActual() {
            if (JuegoServicio.preguntaActual < JuegoServicio.totalPreguntas) {
                opcionesActuales = new JuegoDAO().ObtenerOpcionesPorPregunta(JuegoServicio.obtenerPreguntaActual().IdPregunta);
                if (opcionesActuales == null || opcionesActuales.Count == 0) { JuegoServicio.siguientePregunta(); CargarPreguntaActual(); return; }
                tiempoRestante = 20; AlterarFondo();
            }
        }

        private void IniciarPartida(List<Pregunta> p) {
            if (p == null || p.Count == 0) return;
            JuegoServicio.iniciaJuego(p); CargarPreguntaActual(); pantallaActual = "Jugando"; timerCronometro.Start();
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.HighQuality; g.TextRenderingHint = TextRenderingHint.AntiAlias;
            listaCuadros.ForEach(c => g.FillRectangle(new SolidBrush(Color.FromArgb(c.Opacidad, c.ColorCuadro)), c.X, c.Y, c.Tamaño, c.Tamaño));

            if (pantallaActual == "Inicio") {
                DibujarTexto(g, "TRIVIA MÁXIMA", 50, new Point(110, 150), Color.Gold);
                DibujarBoton(g, new Rectangle(280, 280, 240, 60), "¡JUGAR!", Color.FromArgb(0, 30, 150));
                DibujarBoton(g, new Rectangle(280, 360, 240, 60), "HISTORIAL", Color.FromArgb(40, 40, 40));
            } else if (pantallaActual == "Categorias") {
                DibujarTexto(g, "Elige Categoría", 30, new Point(220, 50), Color.Gold);
                int count = categoriasLista?.Count ?? 0;
                for (int i = 0; i < count; i++) DibujarBoton(g, new Rectangle(200, 130 + (i * 65), 400, 50), categoriasLista[i].NombreCategoria, Color.FromArgb(50, 0, 150));
                DibujarBoton(g, new Rectangle(200, 130 + (count * 65), 400, 50), "🎲 MODO ALEATORIO", Color.Maroon);
            } else if (pantallaActual == "Historial") {
                DibujarPantallaHistorial(g);
            } else if (pantallaActual == "Jugando") {
                DibujarPantallaJuego(g);
            } else if (pantallaActual == "Puntaje") {
                DibujarPantallaPuntaje(g);
            }
        }

        private void DibujarPantallaHistorial(Graphics g) {
            DibujarTexto(g, "ÚLTIMAS PARTIDAS", 35, new Point(180, 50), Color.Gold);
            Rectangle rectTabla = new Rectangle(150, 120, 500, 350);
            g.FillRectangle(new SolidBrush(Color.FromArgb(180, 10, 10, 40)), rectTabla);
            g.DrawRectangle(new Pen(cyanBorde, 2), rectTabla);
            if (listaHistorial != null)
                for (int i = 0; i < listaHistorial.Count; i++)
                    g.DrawString(listaHistorial[i], new Font("Consolas", 14, FontStyle.Bold), Brushes.White, 170, 140 + (i * 30));
            DibujarBoton(g, new Rectangle(280, 490, 240, 60), "VOLVER", Color.FromArgb(60, 60, 60));
        }

        private void DibujarPantallaJuego(Graphics g) {
            Pregunta p = JuegoServicio.obtenerPreguntaActual();
            DibujarTexto(g, tiempoRestante.ToString(), 35, new Point(710, 20), tiempoRestante > 5 ? Color.Cyan : Color.Red);
            g.DrawString(p.TextoPregunta, new Font("Segoe UI", 18, FontStyle.Bold), Brushes.White, new Rectangle(50, 100, 700, 80), new StringFormat { Alignment = StringAlignment.Center });
            bool img = JuegoServicio.esTipo(p) == "imagen";
            for (int i = 0; i < (opcionesActuales?.Count ?? 0); i++) {
                Rectangle r = new Rectangle((i % 2 == 0) ? 100 : 420, img ? 200 + (i / 2 * 135) : 250 + (i / 2 * 80), 280, img ? 120 : 60);
                Color c = (indexOpcionSeleccionada == i) ? (respuestaCorrecta == true ? Color.Lime : Color.Crimson) : Color.FromArgb(30, 30, 80);
                if (img && !string.IsNullOrEmpty(opcionesActuales[i].RutaImagen)) DibujarBotonImg(g, r, opcionesActuales[i].RutaImagen, c);
                else DibujarBoton(g, r, opcionesActuales[i].TextoOpcion, c);
            }
        }

        private void DibujarPantallaPuntaje(Graphics g) {
            DibujarTexto(g, "RESULTADOS", 45, new Point(200, 80), Color.Gold);
            g.DrawString($"Correctas: {JuegoServicio.correctas}\nIncorrectas: {JuegoServicio.incorrectas}\nEfectividad: {JuegoServicio.calcularPorcentajeCorrecto()}%", new Font("Segoe UI", 25, FontStyle.Bold), Brushes.White, new Rectangle(100, 200, 600, 200), new StringFormat { Alignment = StringAlignment.Center });
            DibujarBoton(g, new Rectangle(280, 420, 240, 70), "REINICIAR", Color.SeaGreen);
        }

        private void DibujarBoton(Graphics g, Rectangle r, string t, Color b) {
            using (GraphicsPath p = GetPath(r)) { g.FillPath(new SolidBrush(b), p); g.DrawPath(new Pen(cyanBorde, 3), p); }
            g.DrawString(t, new Font("Segoe UI", t.Length > 20 ? 9 : 11, FontStyle.Bold), Brushes.White, r, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DibujarBotonImg(Graphics g, Rectangle r, string path, Color b) {
            using (GraphicsPath p = GetPath(r)) { g.FillPath(new SolidBrush(b), p); g.DrawPath(new Pen(cyanBorde, 3), p); }
            try {
                string fp = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", path));
                if (!File.Exists(fp)) fp = fp.ToLower();
                if (File.Exists(fp)) {
                    using (Image i = Image.FromFile(fp)) {
                        float s = Math.Min((float)(r.Width - 10) / i.Width, (float)(r.Height - 10) / i.Height);
                        g.DrawImage(i, r.X + (r.Width - i.Width * s) / 2, r.Y + (r.Height - i.Height * s) / 2, i.Width * s, i.Height * s);
                    }
                }
            } catch { }
        }

        private void DibujarTexto(Graphics g, string t, int s, Point p, Color c) {
            Font f = new Font("Segoe UI", s, FontStyle.Bold);
            for (int i = -2; i <= 2; i += 2) for (int j = -2; j <= 2; j += 2) g.DrawString(t, f, Brushes.Black, p.X + i, p.Y + j);
            g.DrawString(t, f, new SolidBrush(c), p);
        }

        private GraphicsPath GetPath(Rectangle r) {
            GraphicsPath p = new GraphicsPath(); int c = r.Height / 3;
            p.AddPolygon(new[] { new Point(r.X + c, r.Y), new Point(r.Right - c, r.Y), new Point(r.Right, r.Y + c), new Point(r.Right, r.Bottom - c), new Point(r.Right - c, r.Bottom), new Point(r.X + c, r.Bottom), new Point(r.X, r.Bottom - c), new Point(r.X, r.Y + c) });
            return p;
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            if (pantallaActual == "Inicio") {
                if (new Rectangle(280, 280, 240, 60).Contains(e.Location)) { categoriasLista = new JuegoDAO().ObtenerCategorias(); pantallaActual = "Categorias"; }
                else if (new Rectangle(280, 360, 240, 60).Contains(e.Location)) { listaHistorial = new JuegoDAO().ObtenerHistorial(); pantallaActual = "Historial"; }
            } else if (pantallaActual == "Categorias") {
                int count = categoriasLista?.Count ?? 0;
                for (int i = 0; i < count; i++) if (new Rectangle(200, 130 + (i * 65), 400, 50).Contains(e.Location)) { idCategoriaSeleccionada = categoriasLista[i].IdCategoria; IniciarPartida(new JuegoDAO().ObtenerPreguntasPorCategoria(idCategoriaSeleccionada)); return; }
                if (new Rectangle(200, 130 + (count * 65), 400, 50).Contains(e.Location)) { idCategoriaSeleccionada = 0; IniciarPartida(new JuegoDAO().ObtenerTodasLasPreguntas()); }
            } else if (pantallaActual == "Historial" && new Rectangle(280, 490, 240, 60).Contains(e.Location)) {
                pantallaActual = "Inicio";
            } else if (pantallaActual == "Jugando" && indexOpcionSeleccionada == null) {
                bool img = JuegoServicio.esTipo(JuegoServicio.obtenerPreguntaActual()) == "imagen";
                for (int i = 0; i < (opcionesActuales?.Count ?? 0); i++) {
                    Rectangle r = new Rectangle((i % 2 == 0) ? 100 : 420, img ? 200 + (i / 2 * 135) : 250 + (i / 2 * 80), 280, img ? 120 : 60);
                    if (r.Contains(e.Location)) {
                        timerCronometro.Stop(); indexOpcionSeleccionada = i; respuestaCorrecta = opcionesActuales[i].EsCorrecta;
                        JuegoServicio.validaRespuesta(opcionesActuales[i].IdOpcion, opcionesActuales); timerFeedback.Start();
                    }
                }
            } else if (pantallaActual == "Puntaje" && new Rectangle(280, 420, 240, 70).Contains(e.Location)) { pantallaActual = "Inicio"; IniciarParticulas(); }
            Invalidate();
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { if (procesoMusica != null && !procesoMusica.HasExited) procesoMusica.Kill(); base.OnFormClosed(e); }
    }
}