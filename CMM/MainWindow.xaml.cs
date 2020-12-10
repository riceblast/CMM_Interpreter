using CMM.table;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private String filePath = "";
        private bool isSave = false;
        private string inputstr="";

        public bool IsSave
        {
            get { return isSave; }
            set { isSave = value; }
        }            // 用于判断是否已经保存 
        public String FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        List<(string, TokenType)> tokens;

        public MainWindow()
        {
            InitializeComponent();


            //string a = @"ii98989_ fsd___fsaf fdsafasfd0 12 12.9 12.0 22. bb[] a[11] { } /* sssssssss */  /* Com
            // * me
            // * n
            // * 
            // * t
            // * 
            // * s..
            // * . 
            // */
            // 12
            //   1";
            
        }

        private void MenuItem_File_News(object sender, RoutedEventArgs e)
        {
            TextEditor.Text = "";
            IsSave = false;
            inputstr = "";
        }

        private void MenuItem_File_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "代码文件.*c|*.c";


            if (ofd.ShowDialog() == true)
            {
                FilePath = ofd.FileName;
                FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
                StreamReader reader = new StreamReader(file, System.Text.Encoding.Default);
                string str = reader.ReadToEnd();                    // 如果没有System.Text.Encoding.Default  会出现编码问题
                reader.Close();
                file.Close();
                TextEditor.Text = str;
                //inputstr = str;
            }
            else
            {
                MessageBox.Show("打开文件失败");
            }

            
        }



        private void MenuItem_File_save(object sender, RoutedEventArgs e)
        {
            if (FilePath != null && FilePath.Length > 0)       // 路径存在，则覆盖原文件
            {
                FileStream FS_writein = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
                StreamWriter SR_writein = new StreamWriter(FS_writein, System.Text.Encoding.Default);
                SR_writein.Write(TextEditor.Text);
                SR_writein.Close();
                FS_writein.Close();
                IsSave = true;
            }
            else                                   // 如果该文本文件不存在，则新建文本文件
            {
                SaveFileDialog sfg = new SaveFileDialog();
                if (sfg.ShowDialog() == true)
                {
                    FilePath = sfg.FileName;
                    FileStream FS_writein = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
                    StreamWriter SR_writein = new StreamWriter(FS_writein, System.Text.Encoding.Default);
                    SR_writein.Write(TextEditor.Text);
                    SR_writein.Close();
                    FS_writein.Close();
                    IsSave = true;
                }
                else
                {
                    MessageBox.Show("保存文本失败");
                }

            }

        }

        private void MenuItem_File_saveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfg = new SaveFileDialog();
            if (sfg.ShowDialog() == true)
            {
                FilePath = sfg.FileName;
                FileStream FS_writein = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
                StreamWriter SR_writein = new StreamWriter(FS_writein, System.Text.Encoding.Default);
                SR_writein.Write(TextEditor.Text);
                SR_writein.Close();
                FS_writein.Close();
                IsSave = true;
            }
            else
            {
                MessageBox.Show("保存文本失败");
            }
        }

        private void MenuItem_File_Exsit(object sender, RoutedEventArgs e)
        {
            object obj = this;
            if (IsSave == false && TextEditor.Text != null && TextEditor.Text.Length > 0)
            {
                MessageBoxResult msg = MessageBox.Show("您的文档还没保存，是否退出", "提示", MessageBoxButton.YesNoCancel);
                if (msg == MessageBoxResult.Yes)
                {
                    MainWindow mainwindow = (MainWindow)obj;
                    MessageBox.Show(this.ToString());
                    MessageBox.Show(mainwindow.ToString());
                    this.Close();
                }
                else if (msg == MessageBoxResult.Cancel || msg == MessageBoxResult.No)
                {
                    MessageBox.Show("请保存文本");
                }
            }
            else
            {
                this.Close();
                MessageBox.Show(this.ToString());
            }
        }
        private void Run(object sender, RoutedEventArgs e)
        {
            inputstr = TextEditor.Text;
            

            tokens = WordAnalyser.Analyse(inputstr);
            if (tokens == null)
            {
                MessageBox.Show("没有文本");
                return;
            }

            foreach ((string value, TokenType id) in tokens)
            {
                output.Text += $"<{value},{id}>\n";
            }
            foreach (RowTabel item  in RowAnalyser.run(inputstr))
            {
                output.Text += $"<{item.Name},{item.Id},{item.Row},{item.Num}>\n";
            }
        }

    }
}
