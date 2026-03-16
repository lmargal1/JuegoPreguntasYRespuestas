using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Data;

namespace JuegoPreguntasYRespuestas
{
    public partial class Form1 : Form
    {
        private string pantallaActual = "Inicio";
        private Bitmap imagenFondo;
        private List<Categoria> categoriasDelJuego;

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            try
            {
                imagenFondo = new Bitmap(@"C:\Users\Abel\source\repos\JuegoPreguntasYRespuestas\JuegoPreguntasYRespuestas\Presentacion\Imagenes\LOGO_SPAIN_2021.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la imagen de fondo: " + ex.Message);
            }
        }

        // === MÉTODO MEJORADO: contorno con GraphicsPath (texto opaco) ===
        private void DibujarTextoConContorno(Graphics g, string texto, Font fuente, Point pos,
            Color colorTexto, Color colorContorno, int grosor = 2)
        {
            // Convertir tamaño de fuente de puntos a píxeles (96 DPI estándar)
            float tamañoPixeles = fuente.Size * 96.0f / 72.0f;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddString(texto, fuente.FontFamily, (int)fuente.Style, tamañoPixeles, pos, StringFormat.GenericDefault);

                using (Pen penContorno = new Pen(colorContorno, grosor) { LineJoin = LineJoin.Round })
                {
                    g.DrawPath(penContorno, path);
                }

                using (Brush brochaTexto = new SolidBrush(colorTexto))
                {
                    g.FillPath(brochaTexto, path);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // === CONFIGURACIÓN DE ALTA CALIDAD PARA LA IMAGEN ===
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            // Para el texto (opcional, GraphicsPath ya maneja el suavizado)
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Fondo negro por si no hay imagen
            g.Clear(Color.Black);

            // Dibujar fondo centrado y escalado manteniendo proporción
            if (imagenFondo != null)
            {
                float escala = Math.Max((float)Width / imagenFondo.Width, (float)Height / imagenFondo.Height);
                int ancho = (int)(imagenFondo.Width * escala);
                int alto = (int)(imagenFondo.Height * escala);
                int posX = (Width - ancho) / 2;
                int posY = (Height - alto) / 2;

                g.DrawImage(imagenFondo, posX, posY, ancho, alto);
            }

            // === INTERFAZ SEGÚN PANTALLA ===
            if (pantallaActual == "Inicio")
            {
                using (Font fuenteTitulo = new Font("Segoe UI", 48, FontStyle.Bold))
                {
                    DibujarTextoConContorno(g, "TRIVIA MÁXIMA", fuenteTitulo, new Point(130, 100), Color.Gold, Color.Black, 3);
                }

                Rectangle botonJugar = new Rectangle(280, 270, 240, 70);
                using (SolidBrush brochaBoton = new SolidBrush(Color.Turquoise))
                {
                    g.FillRectangle(brochaBoton, botonJugar);
                }
                g.DrawRectangle(new Pen(Color.Black, 3), botonJugar);

                using (Font fuenteBoton = new Font("Segoe UI", 18, FontStyle.Bold))
                {
                    DibujarTextoConContorno(g, "¡JUGAR!", fuenteBoton, new Point(345, 288), Color.White, Color.Black, 2);
                }
            }
            else if (pantallaActual == "Categorias")
            {
                using (Font fuenteCat = new Font("Segoe UI", 24, FontStyle.Bold))
                {
                    DibujarTextoConContorno(g, "Elige una Categoría", fuenteCat, new Point(220, 50), Color.Gold, Color.Black, 2);
                }

                if (categoriasDelJuego != null)
                {
                    int posY = 130;
                    foreach (Categoria cat in categoriasDelJuego)
                    {
                        Rectangle botonCat = new Rectangle(250, posY, 280, 55);
                        using (SolidBrush brochaCat = new SolidBrush(Color.MediumSlateBlue))
                        {
                            g.FillRectangle(brochaCat, botonCat);
                        }
                        g.DrawRectangle(new Pen(Color.Black, 2), botonCat);

                        using (Font fuenteLetra = new Font("Segoe UI", 14, FontStyle.Bold))
                        {
                            DibujarTextoConContorno(g, cat.NombreCategoria, fuenteLetra,
                                new Point(270, posY + 13), Color.LightGoldenrodYellow, Color.Black, 1);
                        }

                        posY += 70;
                    }
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (pantallaActual == "Inicio")
            {
                Rectangle botonJugar = new Rectangle(280, 270, 240, 70);
                if (botonJugar.Contains(e.Location))
                {
                    JuegoDAO dao = new JuegoDAO();
                    categoriasDelJuego = dao.ObtenerCategorias();
                    pantallaActual = "Categorias";
                    Invalidate();
                }
            }
            // Aquí irá la lógica para categorías
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            imagenFondo?.Dispose();
        }
    }
}