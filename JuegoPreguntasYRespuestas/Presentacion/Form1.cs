using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Data;

namespace JuegoPreguntasYRespuestas
{
    // 1. CREAMOS LA ESTRUCTURA PARA NUESTROS CUADROS ANIMADOS
    public class CuadroAnimado
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Velocidad { get; set; }
        public int Tamaño { get; set; }
        public int Opacidad { get; set; }
    }

    public partial class Form1 : Form
    {
        private string pantallaActual = "Inicio"; 
        private List<Categoria> categoriasDelJuego; 

        // Variables para la animación
        private Timer motorAnimacion;
        private List<CuadroAnimado> listaCuadros;
        private Random rnd = new Random();

        // Colores del tema
        private readonly Color colorFondo = Color.FromArgb(5, 5, 20); // Azul casi negro
        private readonly Color colorBotonBase = Color.FromArgb(0, 30, 150); 
        private readonly Color colorBotonBorde = Color.FromArgb(0, 200, 255); 
        private readonly int grosorPen = 3;

        public Form1()
        {
            InitializeComponent();
            
            // Fundamental para que la animación no parpadee
            this.DoubleBuffered = true; 
            this.ClientSize = new Size(800, 600); 
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle; 
            this.MaximizeBox = false;

            // Inicializar los cuadros animados
            GenerarCuadros();

            // Configurar el reloj (Timer) a ~60 FPS (16 milisegundos)
            motorAnimacion = new Timer();
            motorAnimacion.Interval = 16; 
            motorAnimacion.Tick += Animacion_Tick; // Cada "Tick", ejecuta este método
            motorAnimacion.Start();
        }

        // --- LÓGICA DE ANIMACIÓN ---
        private void GenerarCuadros()
        {
            listaCuadros = new List<CuadroAnimado>();
            // Vamos a crear 60 cuadros flotantes
            for (int i = 0; i < 60; i++)
            {
                listaCuadros.Add(new CuadroAnimado
                {
                    X = rnd.Next(0, 800), // Posición horizontal aleatoria
                    Y = rnd.Next(0, 600), // Posición vertical aleatoria
                    Tamaño = rnd.Next(5, 25), // Unos grandes, otros pequeños
                    Velocidad = (float)(rnd.NextDouble() * 3 + 0.5), // Velocidades distintas (efecto profundidad)
                    Opacidad = rnd.Next(20, 100) // Unos más transparentes que otros
                });
            }
        }

        private void Animacion_Tick(object sender, EventArgs e)
        {
            // Mover cada cuadro hacia la derecha
            foreach (var cuadro in listaCuadros)
            {
                cuadro.X += cuadro.Velocidad;

                // Si el cuadro se sale de la pantalla por la derecha, lo regresamos a la izquierda
                if (cuadro.X > this.Width)
                {
                    cuadro.X = -cuadro.Tamaño;
                    cuadro.Y = rnd.Next(0, this.Height); // Le damos una nueva altura para que sea impredecible
                }
            }

            // Obligamos a la ventana a redibujarse con las nuevas posiciones
            this.Invalidate(); 
        }

        // --- MÉTODOS DE DIBUJO AYUDANTES ---
        private void DibujarTextoConContorno(Graphics g, string texto, Font fuente, Point pos, Color colorTexto, Color colorContorno, int grosor = 2)
        {
            float tamañoPixeles = fuente.Size * 96.0f / 72.0f;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddString(texto, fuente.FontFamily, (int)fuente.Style, tamañoPixeles, pos, StringFormat.GenericDefault);
                using (Pen penContorno = new Pen(Color.FromArgb(200, colorContorno), grosor) { LineJoin = LineJoin.Round })
                {
                    g.DrawPath(penContorno, path);
                }
                using (Brush brochaTexto = new SolidBrush(colorTexto))
                {
                    g.FillPath(brochaTexto, path);
                }
            }
        }

        private void DibujarBotonEstilizado(Graphics g, Rectangle bounds, string texto, Font fuente, Color colorRelleno, Color colorBorde, Color colorTexto)
        {
            int margen = grosorPen * 2;
            int width = bounds.Width - margen;
            int height = bounds.Height - margen;
            int cornerSize = height / 3; 

            Point[] points = {
                new Point(bounds.X + cornerSize, bounds.Y), 
                new Point(bounds.X + width - cornerSize, bounds.Y), 
                new Point(bounds.X + width, bounds.Y + cornerSize), 
                new Point(bounds.X + width, bounds.Y + height - cornerSize), 
                new Point(bounds.X + width - cornerSize, bounds.Y + height), 
                new Point(bounds.X + cornerSize, bounds.Y + height), 
                new Point(bounds.X, bounds.Y + height - cornerSize), 
                new Point(bounds.X, bounds.Y + cornerSize) 
            };

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(points);
                using (LinearGradientBrush brush = new LinearGradientBrush(bounds, Color.FromArgb(20, colorRelleno), colorRelleno, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(colorBorde, grosorPen) { LineJoin = LineJoin.Round })
                {
                    g.DrawPath(pen, path);
                }
            }

            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(texto, fuente, new SolidBrush(colorTexto), bounds, sf);
        }

        // --- EL DIBUJANTE NATIVO ---
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            // 1. Pintar el fondo sólido
            g.Clear(colorFondo);

            // 2. Dibujar el fondo animado (Nuestros cuadros Matrix horizontales)
            foreach (var cuadro in listaCuadros)
            {
                // Usamos el color cyan brillante de tus bordes, pero con opacidad variable
                using (SolidBrush brochaCuadro = new SolidBrush(Color.FromArgb(cuadro.Opacidad, 0, 200, 255)))
                {
                    g.FillRectangle(brochaCuadro, cuadro.X, cuadro.Y, cuadro.Tamaño, cuadro.Tamaño);
                }
            }

            // 3. Dibujar la Interfaz (Títulos y Botones) por encima de la animación
            if (pantallaActual == "Inicio")
            {
                // ¡Ajusté el tamaño de la letra de 64 a 50 para que quepa bien en tu pantalla!
                using (Font fuenteTitulo = new Font("Times New Roman", 50, FontStyle.Bold))
                {
                    DibujarTextoConContorno(g, "TRIVIA MÁXIMA", fuenteTitulo, new Point(110, 100), Color.White, Color.Gold, 4);
                }

                Rectangle boundsBoton = new Rectangle(280, 270, 240, 70);
                using (Font fuenteBoton = new Font("Segoe UI", 24, FontStyle.Bold))
                {
                    DibujarBotonEstilizado(g, boundsBoton, "¡JUGAR!", fuenteBoton, colorBotonBase, colorBotonBorde, Color.Gold);
                }
            }
            else if (pantallaActual == "Categorias")
            {
                using (Font fuenteCatTitle = new Font("Segoe UI", 32, FontStyle.Bold))
                {
                    DibujarTextoConContorno(g, "Elige una Categoría", fuenteCatTitle, new Point(180, 50), Color.White, Color.Gold, 2);
                }

                if (categoriasDelJuego != null)
                {
                    int posY = 130;
                    foreach (Categoria cat in categoriasDelJuego)
                    {
                        Rectangle boundsCat = new Rectangle(200, posY, 400, 60);
                        Color colorBotonCat = Color.FromArgb(50, 0, 150);
                        
                        using (Font fuenteCat = new Font("Segoe UI", 18, FontStyle.Bold))
                        {
                            DibujarBotonEstilizado(g, boundsCat, cat.NombreCategoria, fuenteCat, colorBotonCat, colorBotonBorde, Color.White);
                        }
                        posY += 80; 
                    }
                }
            }
        }

        // --- EL RADAR DE CLICS ---
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (pantallaActual == "Inicio")
            {
                Rectangle hitBoxJugar = new Rectangle(280, 270, 240, 70); 

                if (hitBoxJugar.Contains(e.Location))
                {
                    JuegoDAO dao = new JuegoDAO(); 
                    categoriasDelJuego = dao.ObtenerCategorias(); 

                    pantallaActual = "Categorias";
                }
            }
        }

        // Al cerrar, detenemos el motor para no dejar procesos corriendo en la memoria
        protected override void OnFormClosed(FormClosedEventArgs e) 
        {
            base.OnFormClosed(e);
            if (motorAnimacion != null)
            {
                motorAnimacion.Stop();
                motorAnimacion.Dispose();
            }
        }
    }
}