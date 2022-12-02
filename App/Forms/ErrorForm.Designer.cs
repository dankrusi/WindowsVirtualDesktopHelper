
namespace WindowsVirtualDesktopHelper {
    partial class ErrorForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.labelError = new System.Windows.Forms.Label();
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonOpenIssue = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelError
            // 
            this.labelError.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelError.Location = new System.Drawing.Point(12, 9);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(528, 47);
            this.labelError.TabIndex = 0;
            // 
            // textBoxDetails
            // 
            this.textBoxDetails.Location = new System.Drawing.Point(15, 93);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.ReadOnly = true;
            this.textBoxDetails.Size = new System.Drawing.Size(525, 254);
            this.textBoxDetails.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(528, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "Error Details";
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(465, 353);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(75, 23);
            this.buttonExit.TabIndex = 3;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonOpenIssue
            // 
            this.buttonOpenIssue.Location = new System.Drawing.Point(319, 353);
            this.buttonOpenIssue.Name = "buttonOpenIssue";
            this.buttonOpenIssue.Size = new System.Drawing.Size(140, 23);
            this.buttonOpenIssue.TabIndex = 4;
            this.buttonOpenIssue.Text = "Open Issue on GitHub";
            this.buttonOpenIssue.UseVisualStyleBackColor = true;
            this.buttonOpenIssue.Click += new System.EventHandler(this.buttonOpenIssue_Click);
            // 
            // ErrorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 387);
            this.Controls.Add(this.buttonOpenIssue);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxDetails);
            this.Controls.Add(this.labelError);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ErrorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelError;
        private System.Windows.Forms.TextBox textBoxDetails;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button buttonOpenIssue;
    }
}