namespace Descon_Report_Generator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gvGetAllCombinations = new System.Windows.Forms.DataGridView();
            this.btnGetAllCombinations = new System.Windows.Forms.Button();
            this.btnGenTheReports = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.gvGetAllCombinations)).BeginInit();
            this.SuspendLayout();
            // 
            // gvGetAllCombinations
            // 
            this.gvGetAllCombinations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvGetAllCombinations.Location = new System.Drawing.Point(27, 143);
            this.gvGetAllCombinations.Name = "gvGetAllCombinations";
            this.gvGetAllCombinations.RowTemplate.Height = 24;
            this.gvGetAllCombinations.Size = new System.Drawing.Size(1269, 371);
            this.gvGetAllCombinations.TabIndex = 0;
            // 
            // btnGetAllCombinations
            // 
            this.btnGetAllCombinations.Location = new System.Drawing.Point(296, 65);
            this.btnGetAllCombinations.Name = "btnGetAllCombinations";
            this.btnGetAllCombinations.Size = new System.Drawing.Size(331, 38);
            this.btnGetAllCombinations.TabIndex = 1;
            this.btnGetAllCombinations.Text = "Get Every Combination of reports";
            this.btnGetAllCombinations.UseVisualStyleBackColor = true;
            this.btnGetAllCombinations.Click += new System.EventHandler(this.btnGetAllCombinations_Click);
            // 
            // btnGenTheReports
            // 
            this.btnGenTheReports.Location = new System.Drawing.Point(750, 65);
            this.btnGenTheReports.Name = "btnGenTheReports";
            this.btnGenTheReports.Size = new System.Drawing.Size(331, 38);
            this.btnGenTheReports.TabIndex = 2;
            this.btnGenTheReports.Text = "Generate the Reports";
            this.btnGenTheReports.UseVisualStyleBackColor = true;
            this.btnGenTheReports.Click += new System.EventHandler(this.btnGenTheReports_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(296, 115);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(370, 22);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "this will generate every possible reports it can be in 1000\'s";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1326, 544);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnGenTheReports);
            this.Controls.Add(this.btnGetAllCombinations);
            this.Controls.Add(this.gvGetAllCombinations);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.gvGetAllCombinations)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gvGetAllCombinations;
        private System.Windows.Forms.Button btnGetAllCombinations;
        private System.Windows.Forms.Button btnGenTheReports;
        private System.Windows.Forms.TextBox textBox1;
    }
}

