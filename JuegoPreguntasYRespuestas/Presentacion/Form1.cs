using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private int idCategoriaSeleccionada, tiempoRestante = 20;
        private int? indexOpcionSeleccionada = null;
        private bool? respuestaCorrecta = null;
        private Process procesoMusica;
        private Timer motorAnimacion, timerCronometro, timerFeedback;
        private List<CuadroAnimado> listaCuadros = new List<CuadroAnimado>();
        private Random rnd = new Random();
        private readonly Color azulFondo = Color.FromArgb(5, 5, 25), cyanBorde = Color.FromArgb(0, 200, 255);

        public Form1() {
            InitializeComponent();
            DoubleBuffered = true;
            ClientSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            
            IniciarParticulas();
            CambiarMusica("tron_music.wav");

            (motorAnimacion = new Timer { Interval = 16 }).Tick += (s, e) => { ActualizarParticulas(); Invalidate(); };
            (timerCronometro = new Timer { Interval = 1000 }).Tick += (s, e) => { if (--tiempoRestante <= 0) FinalizarJuego(); Invalidate(); };
            (timerFeedback = new Timer { Interval = 1500 }).Tick += TimerFeedback_Tick;
            
            motorAnimacion.Start();
        }

        private void CambiarMusica(string n) {
            try {
                if (procesoMusica != null && !procesoMusica.HasExited) { procesoMusica.Kill(); procesoMusica.Dispose(); }
                string r = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Presentacion", "musica", n));
                if (File.Exists(r)) procesoMusica = Process.Start(new ProcessStartInfo("paplay", $"\"{r}\"") { CreateNoWindow = true, UseShellExecute = false });
            } catch { }
        }

        private void IniciarParticulas() {
            listaCuadros.Clear();
            for (int i = 0; i < 60; i++) listaCuadros.Add(new CuadroAnimado { X = rnd.Next(800), Y = rnd.Next(600), Tamaño = rnd.Next(5, 20), VelX = (float)(rnd.NextDouble() * 3 + 1), Opacidad = rnd.Next(40, 150), ColorCuadro = Color.FromArgb(0, 150, 255) });
        }

        private void ActualizarParticulas() => listaCuadros.ForEach(c => { c.X = (c.X + c.VelX) % 850; c.Y = (c.Y + c.VelY) % 650; if (c.X < -20) c.X = 800; });

        private void AlterarFondo() {
            Color nc = Color.FromArgb(rnd.Next(100, 255), rnd.Next(100, 255), rnd.Next(100, 255));
            listaCuadros.ForEach(c => { c.ColorCuadro = nc; c.VelX = rnd.Next(-4, 5); c.VelY = rnd.Next(-4, 5); });
        }

        private void TimerFeedback_Tick(object sender, EventArgs e) {
            timerFeedback.Stop();
            indexOpcionSeleccionada = null; respuestaCorrecta = null;
            if (JuegoServicio.siguientePregunta()) { CargarPreguntaActual(); timerCronometro.Start(); } else FinalizarJuego();
            Invalidate();
        }

        private void FinalizarJuego() {
            timerCronometro.Stop();
            try { if (idCategoriaSeleccionada > 0) new JuegoDAO().GuardarPartida(idCategoriaSeleccionada, JuegoServicio.correctas, JuegoServicio.incorrectas); } catch { }
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
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.HighQuality; g.Clear(azulFondo);
            listaCuadros.ForEach(c => g.FillRectangle(new SolidBrush(Color.FromArgb(c.Opacidad, c.ColorCuadro)), c.X, c.Y, c.Tamaño, c.Tamaño));

            if (pantallaActual == "Inicio") {
                DibujarTexto(g, "TRIVIA MÁXIMA", 50, new Point(110, 150), Color.Gold);
                DibujarBoton(g, new Rectangle(280, 300, 240, 70), "¡JUGAR!", Color.FromArgb(0, 30, 150));
            } else if (pantallaActual == "Categorias") {
                DibujarTexto(g, "Elige Categoría", 30, new Point(220, 50), Color.Gold);
                for (int i = 0; i < (categoriasLista?.Count ?? 0); i++) DibujarBoton(g, new Rectangle(200, 130 + (i * 70), 400, 50), categoriasLista[i].NombreCategoria, Color.FromArgb(50, 0, 150));
                DibujarBoton(g, new Rectangle(200, 130 + (categoriasLista?.Count ?? 0) * 70, 400, 50), "🎲 MODO ALEATORIO", Color.Maroon);
            } else if (pantallaActual == "Jugando") {
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
            } else if (pantallaActual == "Puntaje") {
                DibujarTexto(g, "RESULTADOS", 45, new Point(200, 80), Color.Gold);
                g.DrawString($"Correctas: {JuegoServicio.correctas}\nIncorrectas: {JuegoServicio.incorrectas}\nEfectividad: {JuegoServicio.calcularPorcentajeCorrecto()}%", new Font("Segoe UI", 25, FontStyle.Bold), Brushes.White, new Rectangle(100, 200, 600, 200), new StringFormat { Alignment = StringAlignment.Center });
                DibujarBoton(g, new Rectangle(280, 420, 240, 70), "REINICIAR", Color.SeaGreen);
            }
        }

        private void DibujarBoton(Graphics g, Rectangle r, string t, Color b) {
            using (GraphicsPath p = GetPath(r)) { g.FillPath(new SolidBrush(b), p); g.DrawPath(new Pen(cyanBorde, 3), p); }
            g.DrawString(t, new Font("Segoe UI", t.Length > 20 ? 9 : 12, FontStyle.Bold), Brushes.White, r, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DibujarBotonImg(Graphics g, Rectangle r, string path, Color b) {
            DibujarBoton(g, r, "", b);
            try {
                string fp = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", path));
                if (File.Exists(fp)) {
                    using (Image i = Image.FromFile(fp)) {
                        float s = Math.Min((float)(r.Width - 20) / i.Width, (float)(r.Height - 10) / i.Height);
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
            if (pantallaActual == "Inicio" && new Rectangle(280, 300, 240, 70).Contains(e.Location)) { categoriasLista = new JuegoDAO().ObtenerCategorias(); pantallaActual = "Categorias"; }
            else if (pantallaActual == "Categorias") {
                for (int i = 0; i < (categoriasLista?.Count ?? 0); i++) if (new Rectangle(200, 130 + (i * 70), 400, 50).Contains(e.Location)) { idCategoriaSeleccionada = categoriasLista[i].IdCategoria; IniciarPartida(new JuegoDAO().ObtenerPreguntasPorCategoria(idCategoriaSeleccionada)); return; }
                if (new Rectangle(200, 130 + (categoriasLista?.Count ?? 0) * 70, 400, 50).Contains(e.Location)) { idCategoriaSeleccionada = 0; IniciarPartida(new JuegoDAO().ObtenerTodasLasPreguntas()); }
            } else if (pantallaActual == "Jugando" && indexOpcionSeleccionada == null) {
                bool img = JuegoServicio.esTipo(JuegoServicio.obtenerPreguntaActual()) == "imagen";
                for (int i = 0; i < (opcionesActuales?.Count ?? 0); i++)
                    if (new Rectangle((i % 2 == 0) ? 100 : 420, img ? 200 + (i / 2 * 135) : 250 + (i / 2 * 80), 280, img ? 120 : 60).Contains(e.Location)) {
                        timerCronometro.Stop(); indexOpcionSeleccionada = i; respuestaCorrecta = opcionesActuales[i].EsCorrecta;
                        JuegoServicio.validaRespuesta(opcionesActuales[i].IdOpcion, opcionesActuales); timerFeedback.Start();
                    }
            } else if (pantallaActual == "Puntaje" && new Rectangle(280, 420, 240, 70).Contains(e.Location)) { pantallaActual = "Inicio"; IniciarParticulas(); }
            Invalidate();
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { if (procesoMusica != null && !procesoMusica.HasExited) procesoMusica.Kill(); base.OnFormClosed(e); }
    }
}