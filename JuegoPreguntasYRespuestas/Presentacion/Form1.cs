using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Media;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Data; //

namespace JuegoPreguntasYRespuestas
{
    public class CuadroAnimado
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelX { get; set; } 
        public float VelY { get; set; } 
        public int Tamaño { get; set; }
        public int Opacidad { get; set; }
        public Color ColorCuadro { get; set; }
    }

    public partial class Form1 : Form
    {
        private string pantallaActual = "Inicio";
        private List<Categoria> categoriasLista; 
        private List<Opcion> opcionesActuales; 
        private int idCategoriaSeleccionada;

        private SoundPlayer reproductorMusica;
        private Timer motorAnimacion;
        private Timer timerCronometro;
        private Timer timerFeedback;
        private int tiempoRestante = 20;
        private List<CuadroAnimado> listaCuadros;
        private Random rnd = new Random();

        private int? indexOpcionSeleccionada = null;
        private bool? respuestaCorrecta = null;

        private readonly Color azulFondo = Color.FromArgb(5, 5, 25);
        private readonly Color cyanBorde = Color.FromArgb(0, 200, 255);

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            IniciarParticulas(); // Movimiento inicial horizontal
            CambiarMusica("tron_music.wav"); 

            motorAnimacion = new Timer { Interval = 16 };
            motorAnimacion.Tick += (s, e) => { ActualizarParticulas(); this.Invalidate(); };
            motorAnimacion.Start();

            timerCronometro = new Timer { Interval = 1000 };
            timerCronometro.Tick += TimerCronometro_Tick;

            timerFeedback = new Timer { Interval = 1500 };
            timerFeedback.Tick += TimerFeedback_Tick;
        }

        // --- LÓGICA DE AUDIO ---
        private void CambiarMusica(string nombreArchivo)
        {
            try {
                if (reproductorMusica != null) reproductorMusica.Stop();
                string ruta = Path.Combine(Application.StartupPath, "..", "..", "Presentacion", "musica", nombreArchivo);
                if (File.Exists(ruta)) {
                    reproductorMusica = new SoundPlayer(ruta);
                    reproductorMusica.PlayLooping();
                }
            } catch { }
        }

        // --- MANEJO DE PARTÍCULAS ---
        private void IniciarParticulas()
        {
            listaCuadros = new List<CuadroAnimado>();
            for (int i = 0; i < 60; i++) {
                listaCuadros.Add(new CuadroAnimado {
                    X = rnd.Next(800), 
                    Y = rnd.Next(600), 
                    Tamaño = rnd.Next(5, 20),
                    VelX = (float)(rnd.NextDouble() * 3 + 1), // Movimiento solo a la derecha
                    VelY = 0, // Sin movimiento vertical al principio
                    Opacidad = rnd.Next(40, 150),
                    ColorCuadro = Color.FromArgb(0, 150, 255)
                });
            }
        }

        private void ActualizarParticulas()
        {
            foreach (var c in listaCuadros) {
                c.X += c.VelX;
                c.Y += c.VelY;
                // Wrapping infinito (reaparecen por el lado opuesto)
                if (c.X > 800) c.X = -c.Tamaño; else if (c.X < -c.Tamaño) c.X = 800;
                if (c.Y > 600) c.Y = -c.Tamaño; else if (c.Y < -c.Tamaño) c.Y = 600;
            }
        }

        private void AlterarFondo()
        {
            // Explosión de color y movimiento al azar para la fase de preguntas
            Color nuevoC = Color.FromArgb(rnd.Next(100, 255), rnd.Next(100, 255), rnd.Next(100, 255));
            foreach (var c in listaCuadros) {
                c.ColorCuadro = nuevoC;
                c.VelX = (float)(rnd.NextDouble() * 8 - 4); // Velocidad aleatoria en X
                c.VelY = (float)(rnd.NextDouble() * 8 - 4); // Velocidad aleatoria en Y
            }
        }

        // --- LÓGICA DEL JUEGO ---
        private void TimerCronometro_Tick(object sender, EventArgs e)
        {
            tiempoRestante--;
            if (tiempoRestante <= 0) FinalizarJuego();
            this.Invalidate();
        }

        private void TimerFeedback_Tick(object sender, EventArgs e)
        {
            timerFeedback.Stop();
            indexOpcionSeleccionada = null;
            respuestaCorrecta = null;

            if (JuegoServicio.siguientePregunta()) {
                CargarPreguntaActual();
                timerCronometro.Start();
            } else {
                FinalizarJuego();
            }
            this.Invalidate();
        }

        private void FinalizarJuego()
        {
            timerCronometro.Stop();
            // Guardar partida en la base de datos
            new JuegoDAO().GuardarPartida(idCategoriaSeleccionada, JuegoServicio.correctas, JuegoServicio.incorrectas);
            pantallaActual = "Puntaje";
            CambiarMusica("tron_music.wav");
        }

        private void CargarPreguntaActual()
        {
            if (JuegoServicio.preguntaActual < JuegoServicio.totalPreguntas) {
                Pregunta pregunta = JuegoServicio.obtenerPreguntaActual();
                opcionesActuales = new JuegoDAO().ObtenerOpcionesPorPregunta(pregunta.IdPregunta);
                tiempoRestante = 20;
                AlterarFondo(); // Activa el movimiento caótico
            }
        }

        // --- DIBUJO ---
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.Clear(azulFondo);

            foreach (var c in listaCuadros) {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(c.Opacidad, c.ColorCuadro)))
                    g.FillRectangle(b, c.X, c.Y, c.Tamaño, c.Tamaño);
            }

            if (pantallaActual == "Inicio") {
                DibujarTextoContorno(g, "TRIVIA MÁXIMA", new Font("Times New Roman", 50, FontStyle.Bold), new Point(110, 150), Color.White, Color.Gold, 3);
                DibujarBoton(g, new Rectangle(280, 300, 240, 70), "¡JUGAR!", Color.FromArgb(0, 30, 150), true);
            }
            else if (pantallaActual == "Categorias") {
                DibujarTextoContorno(g, "Elige Categoría", new Font("Segoe UI", 30, FontStyle.Bold), new Point(220, 50), Color.White, Color.Gold, 2);
                if (categoriasLista != null)
                    for (int i = 0; i < categoriasLista.Count; i++)
                        DibujarBoton(g, new Rectangle(200, 130 + (i * 75), 400, 55), categoriasLista[i].NombreCategoria, Color.FromArgb(50, 0, 150), false);
            }
            else if (pantallaActual == "Jugando") {
                DibujarPantallaJuego(g);
            }
            else if (pantallaActual == "Puntaje") {
                DibujarPantallaPuntaje(g);
            }
        }

        private void DibujarPantallaJuego(Graphics g)
        {
            if (JuegoServicio.preguntaActual >= JuegoServicio.totalPreguntas) return;

            Pregunta p = JuegoServicio.obtenerPreguntaActual();
            Color cReloj = tiempoRestante > 5 ? Color.Cyan : Color.Red;
            DibujarTextoContorno(g, tiempoRestante.ToString(), new Font("Consolas", 35, FontStyle.Bold), new Point(710, 20), cReloj, Color.Black);
            
            Rectangle rectP = new Rectangle(50, 100, 700, 80);
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(p.TextoPregunta, new Font("Segoe UI", 18, FontStyle.Bold), Brushes.White, rectP, sf);

            if (opcionesActuales != null) {
                bool esImagen = JuegoServicio.esTipo(p) == "imagen";
                for (int i = 0; i < opcionesActuales.Count; i++) {
                    int x = (i % 2 == 0) ? 100 : 420;
                    int y = (i < 2) ? 250 : 330;
                    Rectangle rB = new Rectangle(x, y, 280, 60);
                    Color cB = (indexOpcionSeleccionada == i) ? (respuestaCorrecta == true ? Color.Lime : Color.Crimson) : Color.FromArgb(30, 30, 80);

                    if (esImagen && !string.IsNullOrEmpty(opcionesActuales[i].RutaImagen)) {
                        DibujarBotonImagen(g, rB, opcionesActuales[i].RutaImagen, cB);
                    } else {
                        DibujarBoton(g, rB, opcionesActuales[i].TextoOpcion, cB, false);
                    }
                }
            }
        }

        private void DibujarPantallaPuntaje(Graphics g)
        {
            DibujarTextoContorno(g, "RESULTADOS", new Font("Segoe UI", 45, FontStyle.Bold), new Point(200, 80), Color.Gold, Color.Black, 3);
            string s = $"Correctas: {JuegoServicio.correctas}\n" +
                       $"Incorrectas: {JuegoServicio.incorrectas}\n" +
                       $"Efectividad: {JuegoServicio.calcularPorcentajeCorrecto()}%";
            
            Rectangle rS = new Rectangle(100, 200, 600, 200);
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(s, new Font("Segoe UI", 25, FontStyle.Bold), Brushes.White, rS, sf);
            
            DibujarBoton(g, new Rectangle(280, 420, 240, 70), "REINICIAR", Color.FromArgb(0, 150, 50), true);
        }

        private void DibujarBoton(Graphics g, Rectangle r, string txt, Color bck, bool esD)
        {
            DibujarBaseBoton(g, r, bck);
            float fS = (txt.Length > 30) ? 9 : (txt.Length > 20) ? 10 : 12;
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(txt, new Font("Segoe UI", fS, FontStyle.Bold), new SolidBrush(esD ? Color.Gold : Color.White), r, sf);
        }

        private void DibujarBotonImagen(Graphics g, Rectangle r, string ruta, Color bck)
        {
            DibujarBaseBoton(g, r, bck);
            try {
                string p = Path.Combine(Application.StartupPath, "..", "..", "Presentacion", "imagenes", ruta);
                if (File.Exists(p)) {
                    using (Image img = Image.FromFile(p)) {
                        g.DrawImage(img, new Rectangle(r.X + 10, r.Y + 5, r.Width - 20, r.Height - 10));
                    }
                }
            } catch { }
        }

        private void DibujarBaseBoton(Graphics g, Rectangle r, Color bck)
        {
            using (GraphicsPath p = new GraphicsPath()) {
                int c = r.Height / 3;
                p.AddPolygon(new Point[] { new Point(r.X+c, r.Y), new Point(r.Right-c, r.Y), new Point(r.Right, r.Y+c), new Point(r.Right, r.Bottom-c), new Point(r.Right-c, r.Bottom), new Point(r.X+c, r.Bottom), new Point(r.X, r.Bottom-c), new Point(r.X, r.Y+c) });
                g.FillPath(new SolidBrush(bck), p);
                g.DrawPath(new Pen(cyanBorde, 3), p);
            }
        }

        private void DibujarTextoContorno(Graphics g, string t, Font f, Point p, Color cT, Color cC, int gr = 2)
        {
            using (Brush bC = new SolidBrush(cC)) {
                for (int x = -gr; x <= gr; x += gr)
                    for (int y = -gr; y <= gr; y += gr)
                        g.DrawString(t, f, bC, new Point(p.X + x, p.Y + y));
            }
            g.DrawString(t, f, new SolidBrush(cT), p);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (pantallaActual == "Inicio" && new Rectangle(280, 300, 240, 70).Contains(e.Location)) {
                categoriasLista = new JuegoDAO().ObtenerCategorias();
                pantallaActual = "Categorias";
            }
            else if (pantallaActual == "Categorias" && categoriasLista != null) {
                for (int i = 0; i < categoriasLista.Count; i++) {
                    if (new Rectangle(200, 130 + (i * 75), 400, 55).Contains(e.Location)) {
                        idCategoriaSeleccionada = categoriasLista[i].IdCategoria;
                        var pregs = new JuegoDAO().ObtenerPreguntasPorCategoria(idCategoriaSeleccionada);
                        JuegoServicio.iniciaJuego(pregs);
                        CambiarMusica("tron_loop.wav"); 
                        CargarPreguntaActual();
                        pantallaActual = "Jugando";
                        timerCronometro.Start();
                    }
                }
            }
            else if (pantallaActual == "Jugando" && opcionesActuales != null && indexOpcionSeleccionada == null) {
                for (int i = 0; i < opcionesActuales.Count; i++) {
                    int x = (i % 2 == 0) ? 100 : 420;
                    int y = (i < 2) ? 250 : 330;
                    if (new Rectangle(x, y, 280, 60).Contains(e.Location)) {
                        timerCronometro.Stop();
                        indexOpcionSeleccionada = i;
                        respuestaCorrecta = opcionesActuales[i].EsCorrecta;
                        JuegoServicio.validaRespuesta(opcionesActuales[i].IdOpcion, opcionesActuales);
                        timerFeedback.Start();
                    }
                }
            }
            else if (pantallaActual == "Puntaje" && new Rectangle(280, 420, 240, 70).Contains(e.Location)) {
                pantallaActual = "Inicio"; 
                IniciarParticulas(); // Reset de movimiento a horizontal
            }
            this.Invalidate();
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { base.OnFormClosed(e); motorAnimacion?.Stop(); timerCronometro?.Stop(); reproductorMusica?.Stop(); }
    }
}