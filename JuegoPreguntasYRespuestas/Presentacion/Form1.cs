using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Data; 

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

        private Process procesoMusica; // Nuestro nuevo reproductor nativo para Linux
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

        private void CambiarMusica(string nombreArchivo)
        {
            try 
            {
                // Matamos la música anterior si estaba sonando
                if (procesoMusica != null && !procesoMusica.HasExited) {
                    procesoMusica.Kill();
                    procesoMusica.Dispose(); 
                }

                string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
                string ruta = Path.Combine(rutaBase, "Presentacion", "musica", nombreArchivo);

                if (File.Exists(ruta)) 
                {
                    // Ejecutamos paplay de forma invisible
                    procesoMusica = new Process();
                    procesoMusica.StartInfo.FileName = "paplay";
                    procesoMusica.StartInfo.Arguments = $"\"{ruta}\""; 
                    procesoMusica.StartInfo.UseShellExecute = false;
                    procesoMusica.StartInfo.CreateNoWindow = true; 
                    procesoMusica.Start();
                } 
                else
                {
                    Console.WriteLine("No se encontró la ruta: " + ruta);
                }
            } 
            catch (Exception ex) 
            {
                Console.WriteLine("Error al ejecutar paplay: " + ex.Message);
            }
        }

        private void IniciarParticulas()
        {
            listaCuadros = new List<CuadroAnimado>();
            for (int i = 0; i < 60; i++) {
                listaCuadros.Add(new CuadroAnimado {
                    X = rnd.Next(800), 
                    Y = rnd.Next(600), 
                    Tamaño = rnd.Next(5, 20),
                    VelX = (float)(rnd.NextDouble() * 3 + 1), 
                    VelY = 0, 
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
                if (c.X > 800) c.X = -c.Tamaño; else if (c.X < -c.Tamaño) c.X = 800;
                if (c.Y > 600) c.Y = -c.Tamaño; else if (c.Y < -c.Tamaño) c.Y = 600;
            }
        }

        private void AlterarFondo()
        {
            Color nuevoC = Color.FromArgb(rnd.Next(100, 255), rnd.Next(100, 255), rnd.Next(100, 255));
            foreach (var c in listaCuadros) {
                c.ColorCuadro = nuevoC;
                c.VelX = (float)(rnd.NextDouble() * 8 - 4); 
                c.VelY = (float)(rnd.NextDouble() * 8 - 4); 
            }
        }

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
            
            // ESCUDO ANTICRASH: Solo guarda si jugaste una categoría específica (ID > 0)
            try {
                if (idCategoriaSeleccionada > 0) {
                    new JuegoDAO().GuardarPartida(idCategoriaSeleccionada, JuegoServicio.correctas, JuegoServicio.incorrectas);
                }
            } catch (Exception ex) {
                Console.WriteLine("Error al guardar partida: " + ex.Message);
            }

            pantallaActual = "Puntaje";
            CambiarMusica("tron_music.wav");
        }

        private void CargarPreguntaActual()
        {
            if (JuegoServicio.preguntaActual < JuegoServicio.totalPreguntas) {
                Pregunta pregunta = JuegoServicio.obtenerPreguntaActual();
                opcionesActuales = new JuegoDAO().ObtenerOpcionesPorPregunta(pregunta.IdPregunta);
                
                if (opcionesActuales == null || opcionesActuales.Count == 0) {
                    if (JuegoServicio.siguientePregunta()) {
                        CargarPreguntaActual();
                    } else {
                        FinalizarJuego();
                    }
                    return; 
                }

                tiempoRestante = 20;
                AlterarFondo(); 
            }
        }

        private void IniciarPartida(List<Pregunta> preguntas) 
        {
            if(preguntas == null || preguntas.Count == 0) {
                MessageBox.Show("No se encontraron preguntas para este modo.");
                return;
            }
            JuegoServicio.iniciaJuego(preguntas);
            CambiarMusica("tron_music.wav"); 
            CargarPreguntaActual();
            pantallaActual = "Jugando";
            timerCronometro.Start();
        }

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
                if (categoriasLista != null) {
                    for (int i = 0; i < categoriasLista.Count; i++) {
                        DibujarBoton(g, new Rectangle(200, 130 + (i * 70), 400, 50), categoriasLista[i].NombreCategoria, Color.FromArgb(50, 0, 150), false);
                    }
                    DibujarBoton(g, new Rectangle(200, 130 + (categoriasLista.Count * 70), 400, 50), "🎲 MODO ALEATORIO", Color.FromArgb(150, 0, 50), true);
                }
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
        
                int anchoB = 280;
                int altoB = esImagen ? 120 : 60; 
                int yBase = esImagen ? 200 : 250; 
                int espaciadoV = esImagen ? 135 : 80;

                for (int i = 0; i < opcionesActuales.Count; i++) {
                    int x = (i % 2 == 0) ? 100 : 420;
                    int y = (i < 2) ? yBase : yBase + espaciadoV;
            
                    Rectangle rB = new Rectangle(x, y, anchoB, altoB);
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

        private void DibujarBotonImagen(Graphics g, Rectangle r, string rutaImg, Color bck)
        {
            DibujarBaseBoton(g, r, bck);
            try {
                string fullPath = Path.Combine(Application.StartupPath, "..", "..", rutaImg);
                if (!File.Exists(fullPath)) fullPath = fullPath.ToLower();

                if (File.Exists(fullPath)) {
                    using (Image img = Image.FromFile(fullPath)) {
                        float escala = Math.Min((float)(r.Width - 20) / img.Width, (float)(r.Height - 10) / img.Height);
                        int anchoFinal = (int)(img.Width * escala);
                        int altoFinal = (int)(img.Height * escala);
                        int posX = r.X + (r.Width - anchoFinal) / 2;
                        int posY = r.Y + (r.Height - altoFinal) / 2;
                
                        g.DrawImage(img, new Rectangle(posX, posY, anchoFinal, altoFinal));
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("Error visual: " + ex.Message);
            }
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
                    if (new Rectangle(200, 130 + (i * 70), 400, 50).Contains(e.Location)) {
                        idCategoriaSeleccionada = categoriasLista[i].IdCategoria;
                        IniciarPartida(new JuegoDAO().ObtenerPreguntasPorCategoria(idCategoriaSeleccionada));
                        return;
                    }
                }
                if (new Rectangle(200, 130 + (categoriasLista.Count * 70), 400, 50).Contains(e.Location)) {
                    idCategoriaSeleccionada = 0; 
                    IniciarPartida(new JuegoDAO().ObtenerTodasLasPreguntas());
                }
            }
            else if (pantallaActual == "Jugando" && opcionesActuales != null && indexOpcionSeleccionada == null) {
                Pregunta p = JuegoServicio.obtenerPreguntaActual();
                bool esImagen = JuegoServicio.esTipo(p) == "imagen";
                
                int anchoB = 280;
                int altoB = esImagen ? 120 : 60; 
                int yBase = esImagen ? 200 : 250; 
                int espaciadoV = esImagen ? 135 : 80;

                for (int i = 0; i < opcionesActuales.Count; i++) {
                    int x = (i % 2 == 0) ? 100 : 420;
                    int y = (i < 2) ? yBase : yBase + espaciadoV;
                    
                    if (new Rectangle(x, y, anchoB, altoB).Contains(e.Location)) {
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
                IniciarParticulas(); 
            }
            this.Invalidate();
        }

        // Aquí apagamos el proceso de paplay correctamente al cerrar el juego
        protected override void OnFormClosed(FormClosedEventArgs e) 
        { 
            base.OnFormClosed(e); 
            motorAnimacion?.Stop(); 
            timerCronometro?.Stop(); 
            if (procesoMusica != null && !procesoMusica.HasExited) procesoMusica.Kill(); 
        }
    }
}