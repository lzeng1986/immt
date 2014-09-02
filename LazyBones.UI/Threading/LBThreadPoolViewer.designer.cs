namespace LazyBones.Threading
{
    partial class LBThreadPoolViewer
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
            this.components = new System.ComponentModel.Container();
            this.labelJobInQueue = new System.Windows.Forms.Label();
            this.labelJobInPool = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridViewJobInQueue = new System.Windows.Forms.DataGridView();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.inQueueTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.workingTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.workJobBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.numericMinThreads = new System.Windows.Forms.NumericUpDown();
            this.numericMaxThreads = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labelWorkThreads = new System.Windows.Forms.Label();
            this.labelThreadsInPool = new System.Windows.Forms.Label();
            this.timerPerformance = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.threadPoolInfoViewThread = new ThreadPoolInfoView();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.threadPoolInfoViewJob = new ThreadPoolInfoView();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJobInQueue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.workJobBindingSource)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMinThreads)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMaxThreads)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelJobInQueue
            // 
            this.labelJobInQueue.AutoSize = true;
            this.labelJobInQueue.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelJobInQueue.Location = new System.Drawing.Point(189, 41);
            this.labelJobInQueue.Name = "labelJobInQueue";
            this.labelJobInQueue.Size = new System.Drawing.Size(11, 12);
            this.labelJobInQueue.TabIndex = 15;
            this.labelJobInQueue.Text = "0";
            this.labelJobInQueue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelJobInPool
            // 
            this.labelJobInPool.AutoSize = true;
            this.labelJobInPool.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelJobInPool.Location = new System.Drawing.Point(71, 41);
            this.labelJobInPool.Name = "labelJobInPool";
            this.labelJobInPool.Size = new System.Drawing.Size(11, 12);
            this.labelJobInPool.TabIndex = 14;
            this.labelJobInPool.Text = "0";
            this.labelJobInPool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(12, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "作业总数";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(123, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "等待作业数";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.dataGridViewJobInQueue);
            this.groupBox1.Location = new System.Drawing.Point(2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(281, 220);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "线程池内作业";
            // 
            // dataGridViewJobInQueue
            // 
            this.dataGridViewJobInQueue.AllowUserToAddRows = false;
            this.dataGridViewJobInQueue.AllowUserToDeleteRows = false;
            this.dataGridViewJobInQueue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewJobInQueue.AutoGenerateColumns = false;
            this.dataGridViewJobInQueue.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewJobInQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewJobInQueue.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.inQueueTimeDataGridViewTextBoxColumn,
            this.workingTimeDataGridViewTextBoxColumn,
            this.statusDataGridViewTextBoxColumn});
            this.dataGridViewJobInQueue.DataSource = this.workJobBindingSource;
            this.dataGridViewJobInQueue.Location = new System.Drawing.Point(6, 20);
            this.dataGridViewJobInQueue.MultiSelect = false;
            this.dataGridViewJobInQueue.Name = "dataGridViewJobInQueue";
            this.dataGridViewJobInQueue.ReadOnly = true;
            this.dataGridViewJobInQueue.RowHeadersVisible = false;
            this.dataGridViewJobInQueue.RowTemplate.Height = 23;
            this.dataGridViewJobInQueue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewJobInQueue.Size = new System.Drawing.Size(269, 194);
            this.dataGridViewJobInQueue.TabIndex = 19;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "名称";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn.Width = 54;
            // 
            // inQueueTimeDataGridViewTextBoxColumn
            // 
            this.inQueueTimeDataGridViewTextBoxColumn.DataPropertyName = "InQueueTime";
            this.inQueueTimeDataGridViewTextBoxColumn.HeaderText = "等待时长";
            this.inQueueTimeDataGridViewTextBoxColumn.Name = "inQueueTimeDataGridViewTextBoxColumn";
            this.inQueueTimeDataGridViewTextBoxColumn.ReadOnly = true;
            this.inQueueTimeDataGridViewTextBoxColumn.Width = 78;
            // 
            // workingTimeDataGridViewTextBoxColumn
            // 
            this.workingTimeDataGridViewTextBoxColumn.DataPropertyName = "WorkingTime";
            this.workingTimeDataGridViewTextBoxColumn.HeaderText = "运行时长";
            this.workingTimeDataGridViewTextBoxColumn.Name = "workingTimeDataGridViewTextBoxColumn";
            this.workingTimeDataGridViewTextBoxColumn.ReadOnly = true;
            this.workingTimeDataGridViewTextBoxColumn.Width = 78;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            this.statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            this.statusDataGridViewTextBoxColumn.HeaderText = "状态";
            this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            this.statusDataGridViewTextBoxColumn.ReadOnly = true;
            this.statusDataGridViewTextBoxColumn.Width = 54;
            // 
            // workJobBindingSource
            // 
            //this.workJobBindingSource.DataSource = typeof(Task);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.labelTime);
            this.groupBox3.Controls.Add(this.labelStatus);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.numericMinThreads);
            this.groupBox3.Controls.Add(this.labelJobInQueue);
            this.groupBox3.Controls.Add(this.numericMaxThreads);
            this.groupBox3.Controls.Add(this.labelJobInPool);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.labelWorkThreads);
            this.groupBox3.Controls.Add(this.labelThreadsInPool);
            this.groupBox3.Location = new System.Drawing.Point(2, 228);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(281, 151);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "线程池信息";
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(94, 67);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(29, 12);
            this.labelTime.TabIndex = 19;
            this.labelTime.Text = "时长";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(14, 67);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(29, 12);
            this.labelStatus.TabIndex = 18;
            this.labelStatus.Text = "空闲";
            // 
            // numericMinThreads
            // 
            this.numericMinThreads.Location = new System.Drawing.Point(96, 122);
            this.numericMinThreads.Name = "numericMinThreads";
            this.numericMinThreads.Size = new System.Drawing.Size(95, 21);
            this.numericMinThreads.TabIndex = 17;
            this.numericMinThreads.ValueChanged += new System.EventHandler(this.numericMinThreads_ValueChanged);
            // 
            // numericMaxThreads
            // 
            this.numericMaxThreads.Location = new System.Drawing.Point(96, 96);
            this.numericMaxThreads.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericMaxThreads.Name = "numericMaxThreads";
            this.numericMaxThreads.Size = new System.Drawing.Size(95, 21);
            this.numericMaxThreads.TabIndex = 16;
            this.numericMaxThreads.ValueChanged += new System.EventHandler(this.numericMaxThreads_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.Location = new System.Drawing.Point(12, 126);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 15;
            this.label11.Text = "最小线程数";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(12, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "最大线程数";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(12, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 9;
            this.label7.Text = "运行线程";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(124, 17);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 10;
            this.label8.Text = "线程总数量";
            // 
            // labelWorkThreads
            // 
            this.labelWorkThreads.AutoSize = true;
            this.labelWorkThreads.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelWorkThreads.Location = new System.Drawing.Point(72, 17);
            this.labelWorkThreads.Name = "labelWorkThreads";
            this.labelWorkThreads.Size = new System.Drawing.Size(11, 12);
            this.labelWorkThreads.TabIndex = 13;
            this.labelWorkThreads.Text = "0";
            // 
            // labelThreadsInPool
            // 
            this.labelThreadsInPool.AutoSize = true;
            this.labelThreadsInPool.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelThreadsInPool.Location = new System.Drawing.Point(190, 17);
            this.labelThreadsInPool.Name = "labelThreadsInPool";
            this.labelThreadsInPool.Size = new System.Drawing.Size(11, 12);
            this.labelThreadsInPool.TabIndex = 12;
            this.labelThreadsInPool.Text = "0";
            // 
            // timerPerformance
            // 
            this.timerPerformance.Tick += new System.EventHandler(this.timerPerformance_Tick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(283, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(752, 377);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.threadPoolInfoViewThread);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(746, 182);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "线程信息";
            // 
            // threadPoolInfoViewThread
            // 
            this.threadPoolInfoViewThread.BackColor = System.Drawing.Color.Black;
            this.threadPoolInfoViewThread.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threadPoolInfoViewThread.Location = new System.Drawing.Point(3, 17);
            this.threadPoolInfoViewThread.Name = "threadPoolInfoViewThread";
            this.threadPoolInfoViewThread.Size = new System.Drawing.Size(740, 162);
            this.threadPoolInfoViewThread.TabIndex = 18;
            this.threadPoolInfoViewThread.Text = "threadPoolInfoView1";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.threadPoolInfoViewJob);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 191);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(746, 183);
            this.groupBox4.TabIndex = 21;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "作业信息";
            // 
            // threadPoolInfoViewJob
            // 
            this.threadPoolInfoViewJob.BackColor = System.Drawing.Color.Black;
            this.threadPoolInfoViewJob.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threadPoolInfoViewJob.Location = new System.Drawing.Point(3, 17);
            this.threadPoolInfoViewJob.Name = "threadPoolInfoViewJob";
            this.threadPoolInfoViewJob.Size = new System.Drawing.Size(740, 163);
            this.threadPoolInfoViewJob.TabIndex = 19;
            this.threadPoolInfoViewJob.Text = "threadPoolInfoView1";
            // 
            // FormImmtThreadPoolManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1036, 380);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormImmtThreadPoolManage";
            this.Text = "线程池管理器";
            this.Load += new System.EventHandler(this.FormImmtThreadPoolManage_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJobInQueue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.workJobBindingSource)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMinThreads)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericMaxThreads)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelJobInQueue;
        private System.Windows.Forms.Label labelJobInPool;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dataGridViewJobInQueue;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelWorkThreads;
        private System.Windows.Forms.Label labelThreadsInPool;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericMinThreads;
        private System.Windows.Forms.NumericUpDown numericMaxThreads;
        private System.Windows.Forms.Timer timerPerformance;
        private System.Windows.Forms.BindingSource workJobBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn inQueueTimeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn workingTimeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private ThreadPoolInfoView threadPoolInfoViewThread;
        private ThreadPoolInfoView threadPoolInfoViewJob;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Label labelStatus;
    }
}