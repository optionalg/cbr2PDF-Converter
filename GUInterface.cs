﻿//Program: Cbr2PDF Converter
//Author: Koen
//Version: 8.0
//License: GPL/GNU
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Chilkat;
using System.Security;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Security.Principal;
using cbr2pdf;
using System.Threading;

namespace CbrToPdf
{
    public partial class GUIInterface : Form, ProgessListener
    {
        public string input_bestand;
        private bool finished = false;
        

        public GUIInterface(string[] args)
        {
            InitializeComponent();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnApplicationExit);

            if (args.Length == 0)
            {
                DialogResult mf = MessageBox.Show("Do you want to convert a whole folder? Press Yes\n\nIf you only want to convert a single file, Press No", "CBR To PDF Conveter", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (mf == DialogResult.Yes)
                {
                    using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                    {
                        dlg.Description = "Select a folder";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            MessageBox.Show("You selected: " + dlg.SelectedPath);
                        }
                    }
                }
                else
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Filter = "CBR files (*.cbr)|*.cbr|CBZ files (*.cbz)|*.cbz";

                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); ;
                    dialog.Title = "Select a CBR/CBZ  File";
                    DialogResult result = dialog.ShowDialog();

                    if (result.Equals(DialogResult.Cancel) || result.Equals(DialogResult.Abort))
                    {
                        MessageBox.Show("You didn't select a file, exiting CBR to PDF Converter.", "CBR To PDF Conveter", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        System.Environment.Exit(1);
                    }
                    else if (result.Equals(DialogResult.OK))
                    {
                        input_bestand = dialog.FileName;
                    }
                }

              
            }
            else
            {
                input_bestand = args[0];
            }

            if (string.IsNullOrEmpty(input_bestand))
            {
                System.Environment.Exit(1);
            }
            else
            {


                string dir = Path.GetDirectoryName(input_bestand);
                string testFile = dir + "\\" + Path.GetFileNameWithoutExtension(input_bestand) + ".lock"; ;
                try
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(testFile);
                    file.Close();

                    File.Delete(testFile);
                }
                catch (System.UnauthorizedAccessException)
                {
                    MessageBox.Show("No write access to this directory: " + dir, "CBR To PDF Conveter", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    System.Environment.Exit(1);
                }

                //backgroundWorker1.RunWorkerAsync();
                // //backgroundWorker1.WorkerReportsProgress = true;
                //backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
                //


                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.WorkerSupportsCancellation = true;
                backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
                backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
                backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
                backgroundWorker1.RunWorkerAsync();
                label1.Text = "Processing '" + Path.GetFileName(input_bestand) + "'...";
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();
            Application.DoEvents();
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                backgroundWorker1.CancelAsync();
            }
            catch(System.InvalidOperationException){
                System.Environment.Exit(1);
            }
            System.Environment.Exit(1);
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundWorker1 != null)
            {
                while (!backgroundWorker1.CancellationPending && finished == false)
                {
                    ProcessFile pF = new ProcessFile(input_bestand);
                    pF.setProgressListener(this);
                    Thread th = new Thread(new ThreadStart(pF.startConvertingFile));
                    th.Start();
                  //  int prevValue = 0;
                   // while (ProcessFile.percentage != prevValue)
                  //  {
                    //    Console.WriteLine("Prevalue: " + prevValue);
                   //     prevValue = pF.percentageCompleted();
                    //    if (pF.percentageCompleted() == 100)
                   //     {
                            try
                            {
                                
                                th.Join();
                            }
                            catch (Exception _) { }
                            finished = true;
                            
                    //    }
                   //     else
                   //     {
                           // backgroundWorker1.ReportProgress(pF.percentageCompleted());
                   //     }
                   // }
                }
            }
        }

        public void progressUpdate(ProcessFile pwFS, int percentageCompleted)
        {
            backgroundWorker1.ReportProgress(percentageCompleted);
            Console.WriteLine(pwFS.ToString() + " has a percentage of: " + percentageCompleted);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

            if (e.ProgressPercentage == 100)
            {
                label1.Text = "Conversion Completed!";
            }
        }

        public void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100;
            string inputfile = input_bestand;
            string file = Path.GetFileName(inputfile);
            notifyIcon1.Visible = true;

            notifyIcon1.ShowBalloonTip(5000, "Completion", "File '" + file + "' has been converted to PDF.", ToolTipIcon.Info);
            
            System.Environment.Exit(1);
        }
    }
}
