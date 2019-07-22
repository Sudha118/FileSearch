using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace FileSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SearchText = string.Empty;
        private DataSet data = null;
        private FileStream outputFile = null;
        private bool cancelledByUser = false;
        private string startPath = string.Empty;
        private List<string> matchingPaths = new List<string>();

        /// <summary>
        /// The default constructor. Initializes an instance of the class
        /// </summary>
        public MainWindow()
        {
            SetupDataSet();
            InitializeComponent();
        }

        /// <summary>
        /// The data table containing results of the search
        /// </summary>
        public DataTable Data
        {
            get
            {
                return data.Tables[0];
            }
        }

        /// <summary>
        /// Handler for the btnBrowse click event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event arguments</param>
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {           
                SelectPath();
         
        }
                

        /// <summary>
        /// Used to select a path for searching
        /// </summary>
        private void SelectPath()
        {
            var dlg = new FolderBrowserDialog();

            if(Directory.Exists(txtPath.Text))
            {
                dlg.SelectedPath = txtPath.Text;
            }

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = dlg.SelectedPath;
                Properties.Settings.Default.LastPath = dlg.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Handler for the btnStart click event
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="e">the event arguments</param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.ToString() == "Cancel")
            {
                cancelledByUser = true;
                return;
            }

            lblStatus.Content = string.Empty;
            btnStart.Content = "Cancel";
            btnBrowse.IsEnabled = false;
            data.Tables[0].Rows.Clear();

            if (!string.IsNullOrEmpty(txtPath.Text))
            {
                try
                {
                    if (txtSearchString.Text.Length > 0)
                    {
                        if (ValidateFileOrPath())
                        {

                            matchingPaths.Clear();

                            Properties.Settings.Default.LastPath = txtPath.Text;
                            Properties.Settings.Default.LastSearchString = txtSearchString.Text;
                            Properties.Settings.Default.FileOrPath =  1;                        
                           Properties.Settings.Default.Save();

                            SearchText = txtSearchString.Text;

                            if (CanContinue())
                            {
                                startPath = txtPath.Text;
                                lblStatus.Content = "Searching...";
                                var t = new Thread(SearchPath);
                                t.Start(startPath);
                            }
                        }
                    }
                    else
                    {
                        lblStatus.Content = "Enter a search string.";
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Content = "Error: " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Checks for certain conditions to determine if a search process can continue
        /// </summary>
        /// <returns>True if a search can continue.</returns>
        private bool CanContinue()
        {
            if (!Properties.Settings.Default.EnableCapture || string.IsNullOrEmpty(Properties.Settings.Default.OutputFile))
            {
                outputFile = null;
                return true; // not creating a file so exit
            }

            var result = true;         

            return result;
        }

        /// <summary>
        /// Validates a file or path selection
        /// </summary>
        /// <returns>True if the file or path selection is valid</returns>
        private bool ValidateFileOrPath()
        {
            var result = false;        
            
            if(!string.IsNullOrEmpty(txtPath.Text))
            {
              try
                 {
                    if (Directory.Exists(txtPath.Text))
                        {
                        lblStatus.Content = "Folder Exists";
                        result = true;
                    }
                    else
                    {
                        lblStatus.Content = "Path Not Found or Invalid Path";
                   }
               }
               catch (Exception ex)
               {
                   lblStatus.Content = "Error: " + ex.Message;
               }
            }

            return result;
        }

        /// <summary>
        /// Begins a search on a directory path
        /// </summary>
        /// <param name="path">The path that is to be searached</param>
        private void SearchPath(object path)
        {
            try
            {
                var pathString = (string)path;

                if(isMatch(pathString) && !matchingPaths.Contains(pathString))
                {
                    matchingPaths.Add(pathString);
                    AddRowToGrid("Path Match", pathString, 0, string.Empty, 0);
                }

                if (Properties.Settings.Default.FileOrPath == 0)
                {
                    SearchFile(path);
                }
                else if (!pathString.Contains("$RECYCLE.BIN"))
                {
                    var files = Directory.GetFiles(pathString);
                    var paths = Directory.GetDirectories(pathString);

                    if (!cancelledByUser && Properties.Settings.Default.Recurse)
                    {
                        foreach (var p in paths)
                        {
                            if (cancelledByUser)
                            {
                                break;
                            }

                            SearchPath(p);
                        }
                    }

                    var fileCount = 0;

                    if (!cancelledByUser)
                    {
                        foreach (var f in files)
                        {
                            if (cancelledByUser)
                            {
                                break;
                            }

                            var dot = f.ToUpper().LastIndexOf('.');
                            var ext = dot < 0 ? string.Empty : f.ToUpper().Substring(dot, f.Length - dot);

                            if (!Properties.Settings.Default.ExcludeExtensions.ToUpper().Contains(ext))
                            {
                                fileCount++;
                            }

                            Thread.Yield();
                        }

                        Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        pbProgressFiles.Value = 0;
                                        pbProgressFiles.Minimum = 0;
                                        pbProgressFiles.Maximum = fileCount;
                                    }));

                        if (!cancelledByUser)
                        {
                            foreach (var file in files)
                            {
                                if (cancelledByUser)
                                {
                                    break;
                                }

                                var fi = new FileInfo(file);

                                if (
                                    !Properties.Settings.Default.ExcludeExtensions.ToUpper()
                                         .Contains(fi.Extension.ToUpper()))
                                {
                                   SearchFile(file);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                                {
                                                    if (pbProgressFiles.Value < pbProgressFiles.Maximum)
                                                    {
                                                        pbProgressFiles.Value++;
                                                    }
                                                }));

                                    Thread.Yield();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lblStatus.Content = "Error: " + ex;
                            }));
            }
            finally
            {
                if(path.ToString() == startPath)
                {
                    if (outputFile != null)
                    {
                        outputFile.Close();
                        outputFile = null;
                    }

                    Dispatcher.BeginInvoke(new Action(() =>
                            {
                                btnStart.Content = "Start";
                                btnBrowse.IsEnabled = true;

                                if (cancelledByUser)
                                {
                                    lblStatus.Content = "Operation Cancelled By User";
                                }
                            }));

                    cancelledByUser = false;
                }
            }
        }

        /// <summary>
        /// Begins a search on a file
        /// </summary>
        /// <param name="filePath">The file path to be searched</param>
        private void SearchFile(object filePath)
        {
            try
            {
                var pathString = (string)filePath;

                if (isMatch(Path.GetFileName(pathString)))
                {
                    matchingPaths.Add(pathString);
                    AddRowToGrid(string.Format("FileName Match: {0}", Path.GetFileName(pathString)), pathString, 0, string.Empty, 0);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lblStatus.Content = string.Format("Reading File - {0}", filePath);
                            }));

                var file = new FileInfo((string)filePath);
                var lines = File.ReadAllLines((string)filePath);
                var lineNumber = 1;
                var step = Math.Round((lines.GetUpperBound(0) + 1) / 100d);

                step = step == 0 ? 1 : step;

                foreach (var line in lines)
                {
                    if (cancelledByUser)
                    {
                        break;
                    }

                    var match = isMatch(line);

                    if (match)
                    {
                        var number = lineNumber;
                        var l = line;

                        AddRowToGrid(file.Name, file.FullName, number, l, l.IndexOf(SearchText, StringComparison.Ordinal));

                        CaptureData(l);
                    }

                    lineNumber++;
                }

                Dispatcher.BeginInvoke(new Action(() =>
                        {
                            lblStatus.Content = string.Format("{0} occurences found", data.Tables[0].Rows.Count);
                            data.AcceptChanges();
                        }));

                // give the visual time to update. Grids are a bit slow.
                Thread.Yield();
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() => { lblStatus.Content = "Error: " + ex.Message; }));
            }
        }

        /// <summary>
        /// Adds a row to the results grid
        /// </summary>
        /// <param name="fileName">The file name being searched</param>
        /// <param name="FilePath">The file path</param>        
        /// <param name="content">The content of the line</param>
        private void AddRowToGrid(string fileName, string FilePath, int lineNumber, string content, int linePosition)
        {
            Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var row = data.Tables[0].NewRow();

                        row["Index"] = data.Tables[0].Rows.Count + 1;
                        row["FileName"] = fileName;
                        row["FilePath"] = FilePath;                       
                        row["Content"] = content;                      

                        data.Tables[0].Rows.Add(row);
                    }));
        }

        /// <summary>
        /// Captures data from the matching lines basedo on the parameters entered by the user
        /// </summary>
        /// <param name="line">The line that matches the search criteria</param>
        private void CaptureData(string line)
        {
            try
            {
                var capturedText = string.Empty;
                var outputFilePath = Properties.Settings.Default.OutputFile;

                if (Properties.Settings.Default.EnableCapture && !string.IsNullOrEmpty(outputFilePath))
                {
                    if (outputFile == null)
                    {
                        outputFile = File.OpenWrite(Properties.Settings.Default.OutputFile);
                    }

                    if (Properties.Settings.Default.AllowMultilineCapture)
                    {

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.StartText)
                            && line.Contains(Properties.Settings.Default.StartText))
                        {
                            capturedText = line.Substring(line.IndexOf(Properties.Settings.Default.StartText, StringComparison.Ordinal));

                            if (!Properties.Settings.Default.IncludeStartText)
                            {
                                capturedText = capturedText.Replace(Properties.Settings.Default.StartText, string.Empty);
                            }

                            if (!string.IsNullOrEmpty(Properties.Settings.Default.EndText)
                                && capturedText.Contains(Properties.Settings.Default.EndText))
                            {
                                if (!Properties.Settings.Default.IncludeEndText)
                                {
                                    capturedText = capturedText.Substring(
                                        0,
                                        capturedText.IndexOf(Properties.Settings.Default.EndText, StringComparison.Ordinal) - 1);
                                }
                                else
                                {
                                    capturedText = capturedText.Substring(
                                        0,
                                        capturedText.IndexOf(Properties.Settings.Default.EndText, StringComparison.Ordinal) + Properties.Settings.Default.EndText.Length);
                                }

                                if (!string.IsNullOrEmpty(capturedText))
                                {
                                    if (Properties.Settings.Default.AddText
                                        && !string.IsNullOrEmpty(Properties.Settings.Default.InsertText))
                                    {
                                        capturedText += Properties.Settings.Default.InsertText;
                                    }
                                    
                                    var bytes = new byte[capturedText.Length];
                                    Encoding.UTF8.GetBytes(capturedText, 0, capturedText.Length, bytes, 0);

                                    if (Properties.Settings.Default.AddNewLine)
                                    {
                                        outputFile.Write(bytes, 0, bytes.Length);

                                        // add the new line
                                        bytes = Encoding.UTF8.GetBytes("\r\n".ToCharArray());

                                        outputFile.Write(bytes, 0, bytes.Length);
                                    }
                                    else
                                    {
                                        outputFile.Write(bytes, 0, bytes.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Content = "Error during data capture: " + ex.Message;
            }
        }

        /// <summary>
        /// Determines if the passed text contains a match for the search criteria
        /// </summary>
        /// <param name="txt">The text to search</param>
        /// <returns>True if there is a match in the passed text</returns>
        private bool isMatch(string txt)
        {
            bool isMatch = false;

            if (Properties.Settings.Default.UseRegularExpression)
            {
                var r = new Regex(SearchText);

                isMatch = r.IsMatch(txt);
            }
            else
            {
                isMatch = !string.IsNullOrEmpty(txt) && txt.Contains(SearchText);
            }

            return isMatch;
        }

        /// <summary>
        /// Handles the Window Loaded event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtPath.Text = Properties.Settings.Default.LastPath;
            txtSearchString.Text = Properties.Settings.Default.LastSearchString;       

           
            this.DataContext = Data.DefaultView;
        }

        /// <summary>
        /// Sets up data set for search
        /// </summary>
        private void SetupDataSet()
        {
            data = new DataSet();
            var table = new DataTable();
            data.Tables.Add(table);

            var column = new DataColumn("Index") { DataType = typeof(string) };
            table.Columns.Add(column);
            column = new DataColumn("FileName") { DataType = typeof(string) };
            table.Columns.Add(column);
            column = new DataColumn("FilePath") { DataType = typeof(string) };
            table.Columns.Add(column);         
            column = new DataColumn("Content") { DataType = typeof(string) };
            table.Columns.Add(column);
        }     
            

        /// <summary>
        /// Handles the Window closing event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event arguments</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.LastPath = txtPath.Text;
            Properties.Settings.Default.LastSearchString = txtSearchString.Text; 
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Handles the txtSearchString text changed event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event arguments</param>
        private void txtSearchString_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SearchText = txtSearchString.Text;
            lblStatus.Content = string.Empty;

            if (Properties.Settings.Default.UseRegularExpression)
            {
                try
                {
                    var r = new Regex(SearchText);

                    var b = r.IsMatch("RUTESTING");
                }
                catch(Exception ex)
                {
                    lblStatus.Content = "Error: " + ex.Message;
                }
            }
        }
    }
}
