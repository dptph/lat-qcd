namespace GaugeField
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.tbBeta = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label2 = new System.Windows.Forms.Label();
            this.tbNs = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNt = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label4 = new System.Windows.Forms.Label();
            this.tbBE = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbBS = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbBSt = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.lblProcess = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tbMsteps = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbTsteps = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.lblTime1 = new System.Windows.Forms.Label();
            this.lblRemain = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.cbClear = new System.Windows.Forms.CheckBox();
            this.button6 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cbDraw = new System.Windows.Forms.CheckBox();
            this.rbS = new System.Windows.Forms.RadioButton();
            this.rbPL = new System.Windows.Forms.RadioButton();
            this.button7 = new System.Windows.Forms.Button();
            this.lblValue = new System.Windows.Forms.Label();
            this.lblError = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tbMark = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbflux = new System.Windows.Forms.TextBox();
            this.button8 = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(24, 166);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 24);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(235, 12);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Legend = "Legend1";
            series1.Name = "S";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(457, 228);
            this.chart1.TabIndex = 1;
            this.chart1.Text = "chart1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(121, 166);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(89, 24);
            this.button2.TabIndex = 2;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(34, 327);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(81, 25);
            this.button3.TabIndex = 3;
            this.button3.Text = "PL";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // tbBeta
            // 
            this.tbBeta.Location = new System.Drawing.Point(55, 44);
            this.tbBeta.Name = "tbBeta";
            this.tbBeta.Size = new System.Drawing.Size(106, 20);
            this.tbBeta.TabIndex = 4;
            this.tbBeta.Text = "5";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "beta";
            // 
            // chart2
            // 
            chartArea2.Name = "ChartArea1";
            this.chart2.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.chart2.Legends.Add(legend2);
            this.chart2.Location = new System.Drawing.Point(235, 274);
            this.chart2.Name = "chart2";
            this.chart2.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.EarthTones;
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series3.Legend = "Legend1";
            series3.Name = "PL";
            this.chart2.Series.Add(series3);
            this.chart2.Size = new System.Drawing.Size(457, 228);
            this.chart2.TabIndex = 6;
            this.chart2.Text = "chart2";
            this.chart2.Click += new System.EventHandler(this.chart2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Ns";
            // 
            // tbNs
            // 
            this.tbNs.Location = new System.Drawing.Point(55, 70);
            this.tbNs.Name = "tbNs";
            this.tbNs.Size = new System.Drawing.Size(106, 20);
            this.tbNs.TabIndex = 7;
            this.tbNs.Text = "4";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Nt";
            // 
            // tbNt
            // 
            this.tbNt.Location = new System.Drawing.Point(55, 96);
            this.tbNt.Name = "tbNt";
            this.tbNt.Size = new System.Drawing.Size(106, 20);
            this.tbNt.TabIndex = 9;
            this.tbNt.Text = "2";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(89, 133);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 17);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "Cold Start";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 422);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "BetaEnd";
            // 
            // tbBE
            // 
            this.tbBE.Location = new System.Drawing.Point(104, 422);
            this.tbBE.Name = "tbBE";
            this.tbBE.Size = new System.Drawing.Size(106, 20);
            this.tbBE.TabIndex = 14;
            this.tbBE.Text = "6";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 396);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "BetaStart";
            // 
            // tbBS
            // 
            this.tbBS.Location = new System.Drawing.Point(104, 396);
            this.tbBS.Name = "tbBS";
            this.tbBS.Size = new System.Drawing.Size(106, 20);
            this.tbBS.TabIndex = 12;
            this.tbBS.Text = "5,5";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(47, 458);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Beta step";
            // 
            // tbBSt
            // 
            this.tbBSt.Location = new System.Drawing.Point(104, 458);
            this.tbBSt.Name = "tbBSt";
            this.tbBSt.Size = new System.Drawing.Size(106, 20);
            this.tbBSt.TabIndex = 16;
            this.tbBSt.Text = "0,1";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(388, 248);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(42, 25);
            this.button4.TabIndex = 18;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // lblProcess
            // 
            this.lblProcess.AutoSize = true;
            this.lblProcess.Location = new System.Drawing.Point(91, 369);
            this.lblProcess.Name = "lblProcess";
            this.lblProcess.Size = new System.Drawing.Size(24, 13);
            this.lblProcess.TabIndex = 19;
            this.lblProcess.Text = "0/0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 286);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "Measure";
            // 
            // tbMsteps
            // 
            this.tbMsteps.Location = new System.Drawing.Point(104, 286);
            this.tbMsteps.Name = "tbMsteps";
            this.tbMsteps.Size = new System.Drawing.Size(106, 20);
            this.tbMsteps.TabIndex = 22;
            this.tbMsteps.Text = "200";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(47, 260);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Termal";
            // 
            // tbTsteps
            // 
            this.tbTsteps.Location = new System.Drawing.Point(104, 260);
            this.tbTsteps.Name = "tbTsteps";
            this.tbTsteps.Size = new System.Drawing.Size(106, 20);
            this.tbTsteps.TabIndex = 20;
            this.tbTsteps.Text = "30";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(121, 327);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(81, 25);
            this.button5.TabIndex = 24;
            this.button5.Text = "Stop PL";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(21, 202);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(52, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Time (ms)";
            // 
            // lblTime1
            // 
            this.lblTime1.AutoSize = true;
            this.lblTime1.Location = new System.Drawing.Point(118, 202);
            this.lblTime1.Name = "lblTime1";
            this.lblTime1.Size = new System.Drawing.Size(13, 13);
            this.lblTime1.TabIndex = 26;
            this.lblTime1.Text = "0";
            // 
            // lblRemain
            // 
            this.lblRemain.AutoSize = true;
            this.lblRemain.Location = new System.Drawing.Point(118, 226);
            this.lblRemain.Name = "lblRemain";
            this.lblRemain.Size = new System.Drawing.Size(13, 13);
            this.lblRemain.TabIndex = 28;
            this.lblRemain.Text = "0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(21, 226);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(43, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "Remain";
            // 
            // cbClear
            // 
            this.cbClear.AutoSize = true;
            this.cbClear.Checked = true;
            this.cbClear.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbClear.Location = new System.Drawing.Point(35, 496);
            this.cbClear.Name = "cbClear";
            this.cbClear.Size = new System.Drawing.Size(93, 17);
            this.cbClear.TabIndex = 29;
            this.cbClear.Text = "Clear graphics";
            this.cbClear.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(124, 490);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(86, 23);
            this.button6.TabIndex = 30;
            this.button6.Text = "Update scale";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(709, 42);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(206, 22);
            this.pictureBox1.TabIndex = 31;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // cbDraw
            // 
            this.cbDraw.AutoSize = true;
            this.cbDraw.Location = new System.Drawing.Point(717, 47);
            this.cbDraw.Name = "cbDraw";
            this.cbDraw.Size = new System.Drawing.Size(86, 17);
            this.cbDraw.TabIndex = 32;
            this.cbDraw.Text = "Draw picture";
            this.cbDraw.UseVisualStyleBackColor = true;
            this.cbDraw.Visible = false;
            // 
            // rbS
            // 
            this.rbS.AutoSize = true;
            this.rbS.Location = new System.Drawing.Point(14, 358);
            this.rbS.Name = "rbS";
            this.rbS.Size = new System.Drawing.Size(32, 17);
            this.rbS.TabIndex = 33;
            this.rbS.Text = "S";
            this.rbS.UseVisualStyleBackColor = true;
            // 
            // rbPL
            // 
            this.rbPL.AutoSize = true;
            this.rbPL.Checked = true;
            this.rbPL.Location = new System.Drawing.Point(170, 358);
            this.rbPL.Name = "rbPL";
            this.rbPL.Size = new System.Drawing.Size(38, 17);
            this.rbPL.TabIndex = 34;
            this.rbPL.TabStop = true;
            this.rbPL.Text = "PL";
            this.rbPL.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(726, 448);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 35;
            this.button7.Text = "start";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // lblValue
            // 
            this.lblValue.AutoSize = true;
            this.lblValue.Location = new System.Drawing.Point(760, 333);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(41, 13);
            this.lblValue.TabIndex = 36;
            this.lblValue.Text = "label10";
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(760, 364);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(41, 13);
            this.lblError.TabIndex = 37;
            this.lblError.Text = "label12";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(714, 409);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 39;
            this.label10.Text = "Mark every";
            // 
            // tbMark
            // 
            this.tbMark.Location = new System.Drawing.Point(780, 406);
            this.tbMark.Name = "tbMark";
            this.tbMark.Size = new System.Drawing.Size(106, 20);
            this.tbMark.TabIndex = 38;
            this.tbMark.Text = "1000";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(714, 493);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(26, 13);
            this.label12.TabIndex = 41;
            this.label12.Text = "Flux";
            // 
            // tbflux
            // 
            this.tbflux.Location = new System.Drawing.Point(780, 490);
            this.tbflux.Name = "tbflux";
            this.tbflux.Size = new System.Drawing.Size(106, 20);
            this.tbflux.TabIndex = 40;
            this.tbflux.Text = "0";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(814, 447);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 42;
            this.button8.Text = "pause";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Опасно!!! Глюоны летают!!!";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(814, 12);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(101, 23);
            this.button9.TabIndex = 43;
            this.button9.Text = "Hide window";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(780, 255);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 44;
            this.button10.Text = "D";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(780, 286);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(75, 23);
            this.button11.TabIndex = 45;
            this.button11.Text = "button11";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(780, 133);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(75, 23);
            this.button12.TabIndex = 46;
            this.button12.Text = "test speed";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(781, 164);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 23);
            this.button13.TabIndex = 47;
            this.button13.Text = "test D";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(780, 86);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(75, 23);
            this.button14.TabIndex = 48;
            this.button14.Text = "button14";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(927, 528);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbflux);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.tbMark);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.rbPL);
            this.Controls.Add(this.rbS);
            this.Controls.Add(this.cbDraw);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.cbClear);
            this.Controls.Add(this.lblRemain);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lblTime1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbMsteps);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbTsteps);
            this.Controls.Add(this.lblProcess);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbBSt);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbBE);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbBS);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbNt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbNs);
            this.Controls.Add(this.chart2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbBeta);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox tbBeta;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbNs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbNt;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbBE;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbBS;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbBSt;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label lblProcess;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbMsteps;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbTsteps;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblTime1;
        private System.Windows.Forms.Label lblRemain;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox cbClear;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox cbDraw;
        private System.Windows.Forms.RadioButton rbS;
        private System.Windows.Forms.RadioButton rbPL;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbMark;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbflux;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
    }
}

