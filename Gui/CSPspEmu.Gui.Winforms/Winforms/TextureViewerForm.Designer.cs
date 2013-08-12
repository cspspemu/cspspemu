namespace CSPspEmu.Gui.Winforms.Winforms
{
	partial class TextureViewerForm
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.TextureList = new System.Windows.Forms.ListBox();
			this.TextureViewContainer = new System.Windows.Forms.Panel();
			this.TextureView = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.TextureInfo = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.SaveButton = new System.Windows.Forms.Button();
			this.LoadButton = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.TextureViewContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TextureView)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
			this.tableLayoutPanel1.Controls.Add(this.TextureList, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.TextureViewContainer, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 0);
			this.tableLayoutPanel1.Cursor = System.Windows.Forms.Cursors.Default;
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1054, 546);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// TextureList
			// 
			this.TextureList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextureList.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextureList.FormattingEnabled = true;
			this.TextureList.ItemHeight = 14;
			this.TextureList.Location = new System.Drawing.Point(3, 3);
			this.TextureList.Name = "TextureList";
			this.TextureList.Size = new System.Drawing.Size(154, 540);
			this.TextureList.TabIndex = 1;
			this.TextureList.SelectedIndexChanged += new System.EventHandler(this.TextureList_SelectedIndexChanged);
			// 
			// TextureViewContainer
			// 
			this.TextureViewContainer.AutoScroll = true;
			this.TextureViewContainer.Controls.Add(this.TextureView);
			this.TextureViewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextureViewContainer.Location = new System.Drawing.Point(163, 3);
			this.TextureViewContainer.Name = "TextureViewContainer";
			this.TextureViewContainer.Size = new System.Drawing.Size(688, 540);
			this.TextureViewContainer.TabIndex = 3;
			// 
			// TextureView
			// 
			this.TextureView.Location = new System.Drawing.Point(0, 0);
			this.TextureView.Name = "TextureView";
			this.TextureView.Size = new System.Drawing.Size(128, 128);
			this.TextureView.TabIndex = 0;
			this.TextureView.TabStop = false;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.TextureInfo, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(857, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(194, 540);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// TextureInfo
			// 
			this.TextureInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextureInfo.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextureInfo.Location = new System.Drawing.Point(3, 35);
			this.TextureInfo.Multiline = true;
			this.TextureInfo.Name = "TextureInfo";
			this.TextureInfo.Size = new System.Drawing.Size(188, 502);
			this.TextureInfo.TabIndex = 1;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.SaveButton);
			this.flowLayoutPanel1.Controls.Add(this.LoadButton);
			this.flowLayoutPanel1.Controls.Add(this.button1);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(188, 26);
			this.flowLayoutPanel1.TabIndex = 2;
			// 
			// SaveButton
			// 
			this.SaveButton.Location = new System.Drawing.Point(3, 3);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(54, 23);
			this.SaveButton.TabIndex = 0;
			this.SaveButton.Text = "&Save...";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// LoadButton
			// 
			this.LoadButton.Location = new System.Drawing.Point(63, 3);
			this.LoadButton.Name = "LoadButton";
			this.LoadButton.Size = new System.Drawing.Size(52, 23);
			this.LoadButton.TabIndex = 1;
			this.LoadButton.Text = "&Load...";
			this.LoadButton.UseVisualStyleBackColor = true;
			this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(121, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(53, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "HQ2X";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// TextureViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1054, 546);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "TextureViewerForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "TextureViewerForm";
			this.Load += new System.EventHandler(this.TextureViewerForm_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.TextureViewContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TextureView)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ListBox TextureList;
		private System.Windows.Forms.Panel TextureViewContainer;
		private System.Windows.Forms.PictureBox TextureView;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TextBox TextureInfo;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.Button LoadButton;
		private System.Windows.Forms.Button button1;
	}
}