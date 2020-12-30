using CMM.table;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
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
using System.Windows.Media.TextFormatting;
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
        private static Dictionary<int, Point> breakPoints;

        private class BreakPointMargin : AbstractMargin
        {
            private const int margin = 10;
            private TextArea textArea;

            protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
            {
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                return new Size(margin, 0);
            }
            protected override void OnRender(DrawingContext context)
            {
                Size renderSize = this.RenderSize;
                context.DrawRectangle(SystemColors.ControlBrush, null,
                                             new Rect(0, 0, renderSize.Width, renderSize.Height));
                
                foreach(KeyValuePair<int,Point> kv in breakPoints)
                {
                    context.DrawRectangle(VisualOpacityMask, new Pen(Brushes.Red, 8), new Rect(kv.Value,new Size(2,1)));
                }
            }

            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
            {
                base.OnMouseLeftButtonDown(e);
                TextView textView = this.TextView;
                Point pos = e.GetPosition(TextView);
                pos.X = 0;
                ////pos.Y = pos.Y.CoerceValue(0, TextView.ActualHeight);
                //pos.Y = TextView.ActualHeight;
                pos.Y += TextView.VerticalOffset;
                VisualLine vl = TextView.GetVisualLineFromVisualTop(pos.Y);
                TextLine tl = vl.GetTextLineByVisualYPosition(pos.Y);
                int lineNumber = vl.FirstDocumentLine.LineNumber;
                if (breakPoints.ContainsKey(lineNumber))
                {
                    breakPoints.Remove(lineNumber);
                }
                else
                {
                    breakPoints.Add(lineNumber, pos);
                }
                this.InvalidateVisual();
            }


        }
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

        public MainWindow()
        {
            InitializeComponent();
            breakPoints = new Dictionary<int, Point>();
            input.TextArea.LeftMargins.Insert(0, new BreakPointMargin());

            //输出结果
            Constant.outPutAppend += outputAppendText;
            //清空结果
            Constant.outPutClean += outputCleanText;

        }

        private void MenuItem_File_News(object sender, RoutedEventArgs e)
        {
            input.Text = "";
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
                input.Text = str;
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
                SR_writein.Write(input.Text);
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
                    SR_writein.Write(input.Text);
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
                SR_writein.Write(input.Text);
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
            if (IsSave == false && input.Text != null && input.Text.Length > 0)
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
            inputstr = input.Text;


            List<Token> tokens = WordAnalyser.Analyse(inputstr).Tokens;
            if (tokens == null)
            {
                MessageBox.Show("没有文本");
                return;
            }

            foreach (Token token in tokens)
            {
                output.Text += $"<{token.StrValue},{token.TokenType}>\n";
            }
            foreach (RowTabel item in RowAnalyser.run(inputstr))
            {
                output.Text += $"<{item.Name},{item.Id},{item.Row},{item.Num}>\n";
            }
        }

        /// <summary>
        /// 语义分析时输出字符
        /// </summary>
        /// <param name="s">输出</param>
        private void outputAppendText(string s) {
            Dispatcher.Invoke(new Action(() => output.Text += s));
        }
        /// <summary>
        /// 语义分析时清空输出
        /// </summary>
        private void outputCleanText()
        {
            Dispatcher.Invoke(new Action(() => output.Text = ""));
        }
        /// <summary>
        /// 调用这个方法唤醒线程，即在断点代码部分继续执行
        /// </summary>
        private void wake() {
            Constant.mreSet();
        }

        /// <summary>
        /// 运行用户代码，调用后端编译器进行词法、语法、语义分析
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Run_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
