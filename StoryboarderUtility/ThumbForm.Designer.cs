namespace StoryboarderUtility
{
    partial class ThumbForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.toolBar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.panelScale = new System.Windows.Forms.Panel();
            this.lblScale = new System.Windows.Forms.Label();
            this.scaleSlider = new System.Windows.Forms.TrackBar();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusFileNameLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelThumb = new System.Windows.Forms.Panel();
            this.toolBar.SuspendLayout();
            this.panelScale.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleSlider)).BeginInit();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolBar
            // 
            this.toolBar.AutoSize = true;
            this.toolBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.toolBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolBar.Controls.Add(this.btnOpen);
            this.toolBar.Controls.Add(this.btnReload);
            this.toolBar.Controls.Add(this.btnSave);
            this.toolBar.Controls.Add(this.panelScale);
            this.toolBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolBar.Location = new System.Drawing.Point(0, 0);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(597, 26);
            this.toolBar.TabIndex = 2;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(0, 0);
            this.btnOpen.Margin = new System.Windows.Forms.Padding(0);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(60, 24);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.TabStop = false;
            this.btnOpen.Text = "&Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(60, 0);
            this.btnReload.Margin = new System.Windows.Forms.Padding(0);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(60, 24);
            this.btnReload.TabIndex = 1;
            this.btnReload.TabStop = false;
            this.btnReload.Text = "&Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(120, 0);
            this.btnSave.Margin = new System.Windows.Forms.Padding(0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(70, 24);
            this.btnSave.TabIndex = 6;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "&Snapshot";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // panelScale
            // 
            this.panelScale.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelScale.Controls.Add(this.lblScale);
            this.panelScale.Controls.Add(this.scaleSlider);
            this.panelScale.Location = new System.Drawing.Point(191, 1);
            this.panelScale.Margin = new System.Windows.Forms.Padding(1);
            this.panelScale.Name = "panelScale";
            this.panelScale.Size = new System.Drawing.Size(159, 22);
            this.panelScale.TabIndex = 4;
            // 
            // lblScale
            // 
            this.lblScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblScale.AutoSize = true;
            this.lblScale.Location = new System.Drawing.Point(3, 5);
            this.lblScale.Name = "lblScale";
            this.lblScale.Size = new System.Drawing.Size(35, 12);
            this.lblScale.TabIndex = 2;
            this.lblScale.Text = "Scale";
            this.lblScale.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // scaleSlider
            // 
            this.scaleSlider.AutoSize = false;
            this.scaleSlider.Dock = System.Windows.Forms.DockStyle.Right;
            this.scaleSlider.Location = new System.Drawing.Point(42, 0);
            this.scaleSlider.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.scaleSlider.Minimum = 1;
            this.scaleSlider.Name = "scaleSlider";
            this.scaleSlider.Size = new System.Drawing.Size(115, 20);
            this.scaleSlider.TabIndex = 3;
            this.scaleSlider.TabStop = false;
            this.scaleSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.scaleSlider.Value = 5;
            this.scaleSlider.Scroll += new System.EventHandler(this.scaleSlider_Scroll);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusFileNameLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 388);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(597, 22);
            this.statusBar.TabIndex = 5;
            this.statusBar.Text = "statusStrip1";
            // 
            // statusFileNameLabel
            // 
            this.statusFileNameLabel.Name = "statusFileNameLabel";
            this.statusFileNameLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // panelThumb
            // 
            this.panelThumb.BackColor = System.Drawing.Color.DimGray;
            this.panelThumb.BackgroundImage = global::StoryboarderUtility.Properties.Resources.photo_256;
            this.panelThumb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panelThumb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelThumb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelThumb.Location = new System.Drawing.Point(0, 26);
            this.panelThumb.Name = "panelThumb";
            this.panelThumb.Size = new System.Drawing.Size(597, 362);
            this.panelThumb.TabIndex = 6;
            this.panelThumb.Paint += new System.Windows.Forms.PaintEventHandler(this.panelThumb_Paint);
            this.panelThumb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelThumb_MouseDown);
            this.panelThumb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelThumb_MouseMove);
            this.panelThumb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelThumb_MouseUp);
            // 
            // ThumbForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 410);
            this.Controls.Add(this.panelThumb);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.toolBar);
            this.Name = "ThumbForm";
            this.Text = "ThumbView";
            this.toolBar.ResumeLayout(false);
            this.panelScale.ResumeLayout(false);
            this.panelScale.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleSlider)).EndInit();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel toolBar;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.TrackBar scaleSlider;
        private System.Windows.Forms.Panel panelScale;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel statusFileNameLabel;
        private System.Windows.Forms.Panel panelThumb;
    }
}

