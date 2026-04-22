namespace JuegoPreguntasYRespuestas.Presentacion
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            // Usamos nombres completos (System.Drawing) para evitar errores si faltan usings
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            
            // CORRECCIÓN: Paréntesis cerrados correctamente y color simplificado
            this.BackColor = System.Drawing.Color.FromArgb(5, 5, 25);
            
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "Form1";
            this.Text = "Trivia Máxima";
            this.ResumeLayout(false);
        }

        #endregion
    }
}