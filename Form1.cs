using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace MoveBoxes
{
    static class constant
    {
        //空地/墙/箱子
        public const int BLANK = 0, WALL = 1, BOX = 3, DESTINATION = 4, WORKER = 6;
        //箱子与目的地重合/人与目的地重合
        public const int BOX_DES = 7, WOR_DES = 9;
        public const int EDGE = 30;
    }
    public partial class Form1 : Form
    {
        //图片文件名
        public static string imgName = "1";
        public static string imgFileName = imgName + ".png";
        //地图文件名
        public static string FileName = ".\\map\\configurations.txt";
        private string[] txt = File.ReadAllLines(FileName);
        //全局变量
        private int[,] myArray;//存放地图的数组
        private int row_num = 0, col_num = 0; //第n关的行数和列数
        public int row_start = 0, row_end = 0;//第n关的开始和结束行数
        private string[] txt_n;
        internal int flag_last = 0;
        private int Level = 1;//关卡数
        private int seq = -1;
        public Form1()
        {
            InitializeComponent();
        }
        public Bitmap DrawMap(int[,] myArray)
        {
            //读取地图数组的第一维和第二维，绘制地图
            pictureBox1.Width = myArray.GetLength(1) * constant.EDGE;
            pictureBox1.Height = myArray.GetLength(0) * constant.EDGE;
            Bitmap bit = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bit);
            SolidBrush redBrush = new SolidBrush(Color.Red);
            Image image = new Bitmap(imgFileName);
            //绘制地图
            for (int row = 0; row < myArray.GetLength(0); row++)
            {
                for (int col = 0; col < myArray.GetLength(1); col++)
                {
                    imgName = myArray[row, col].ToString();
                    imgFileName = imgName + ".png";
                    image = new Bitmap(imgFileName);
                    g.DrawImage(image, col * constant.EDGE, row * constant.EDGE, constant.EDGE, constant.EDGE);
                }
            }
            return bit;
        }
        public void Form1_Load(object sender, EventArgs e)
        {
            //找不到地图文件抛出异常
            try { txt = File.ReadAllLines(FileName); }
            catch (FileNotFoundException q)
            {
                MessageBox.Show("找不到地图文件", "提示");
                Close();
                return;
            }
            init_data();
        }
        public int[,] ReadMap(string[] txt)
        {
            //将text file里的第n关map数据写入一个integer array中
            string[] array0 = txt[0].Split(',');
            col_num = array0.Length;
            var array = new string[row_num, col_num];
            for (int row = 0; row < row_num; row++)
            {
                string[] arrayi = txt[row].Split(',');
                for (int col = 0; col < col_num; col++)
                {
                    array.SetValue(arrayi[col], row, col);
                }
            }
            int[,] array1 = new int[row_num, col_num];
            for (int row = 0; row < row_num; row++)
            {
                for (int col = 0; col < col_num; col++)
                {
                    int.TryParse(array[row, col], result: out array1[row, col]);
                }
            }
            //初始化存储撤销数据的数组
            int[,,] myArray_old = new int[500, row_num, col_num];
            return array1;
        }
        private void Level_Regenerate()
        {
            //更新某关数据 （从array到txt）
            List<string> lines = txt.ToList<string>();
            if (this.row_start == 0 && this.row_end == 0) //第一关不存在，默认新建一关
            {
                this.row_start = 1;
                lines.Insert(0, "//");
            }
            else
            {
                for (int row = this.row_start; row < this.row_end; row++)
                {
                    lines.RemoveAt(this.row_start); //删除第 row_start~row_end 行
                }
            }
            string[,] array = new string[myArray.GetLength(0), myArray.GetLength(1)];
            string[] arrayi = new string[myArray.GetLength(1)];
            string[] arrays = new string[myArray.GetLength(0)];
            for (int row = 0; row < myArray.GetLength(0); row++)
            {
                for (int col = 0; col < myArray.GetLength(1); col++)
                {
                    array[row, col] = myArray[row, col].ToString();
                    arrayi.SetValue(array[row, col], col);
                }
                arrays[row] = string.Join(",", arrayi);
            }
            for (int row = this.row_start; row < this.row_start + myArray.GetLength(0); row++)
            {
                lines.Insert(row, arrays[row - this.row_start]); //插入新行
            }
            this.row_end = this.row_start + myArray.GetLength(0);
            txt = lines.ToArray();
        }
        public string[] ExtractLines(string[] txt, int n)
        {
            //提取第n关数据
            string array_s = "//";
            //获取开始和结束行
            int num_slash = 0;
            row_start = 0; row_end = 0; row_num = 0; flag_last = 0;
            for (int row = 0; row < txt.Length; row++)
            {
                if (txt[row].Contains(array_s))
                {
                    num_slash++;
                    if (num_slash == n)
                    {
                        row_start = row + 1;
                    }
                    if (num_slash == n + 1)
                    {
                        row_end = row;
                    }
                }
            }
            //判断本关是否已删
            if (num_slash < n)
            {
                flag_last = 1;
                return null;
            }
            //判断是否为最后一关
            if (row_end == 0)
            {
                row_end = txt.Length;
                flag_last = 1;
            }
            row_num = row_end - row_start;
            //为新的array赋值
            txt_n = new string[row_num];
            for (int row = 0; row < row_num; row++)
            {
                txt_n[row] = string.Copy(txt[row_start + row]);
            }
            return txt_n;
        }
        public void init_data()
        {
            txtLeveln.Text = Level.ToString();
            txt_n = ExtractLines(txt, Level);
            if (txt_n != null)
            {
                myArray = ReadMap(txt_n);
                pictureBox1.Image = DrawMap(myArray);
            }
            seq = -1;
        }
        private void RestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            init_data();
        }

        private void Keydown(object sender, KeyEventArgs e)
        {
            //按下按键时操作
            int i = 0, j = 0;
            int now_row = 0, now_col = 0,next_row=0,next_col=0,to_row=0,to_col=0;
            //得到人的坐标
            for (int x = 0; x < row_num; x++)
            {
                for (int y = 0; y < col_num; y++)
                {
                    if (myArray[x, y] == constant.WORKER || myArray[x, y] == constant.WOR_DES)
                    {
                        i = x;
                        j = y;
                    }
                }
            }
            //接收按键
            switch (e.KeyCode)
            {
                case Keys.Up: { now_row = i; next_row = i - 1;to_row = i - 2; next_col=now_col = to_col = j; break; }
                case Keys.Left: { now_row=next_row=to_row = i; now_col = j; next_col = j - 1;to_col = j - 2; break; }
                case Keys.Down: { now_row = i; next_row = i + 1; to_row = i + 2; next_col = now_col = to_col = j; break; }
                case Keys.Right: { now_row = next_row = to_row = i; now_col = j; next_col = j + 1; to_col = j + 2; break; }
            }
            if (myArray[next_row, next_col] == constant.BLANK)
            {
                myArray[next_row, next_col] = constant.WORKER;
                if (myArray[now_row, now_col] == constant.WORKER)
                {
                    myArray[now_row, now_col] = constant.BLANK;
                }
                else
                {
                    myArray[now_row, now_col] = constant.DESTINATION;
                }
            }
            else if (myArray[next_row, next_col] == constant.BOX && myArray[to_row, to_col] == constant.BLANK)
            {
                myArray[next_row, next_col] = constant.WORKER;
                myArray[to_row, to_col] = constant.BOX;
                if (myArray[now_row, now_col] == constant.WORKER)
                {
                    myArray[now_row, now_col] = constant.BLANK;
                }
                else
                {
                    myArray[now_row, now_col] = constant.DESTINATION;
                }
            }
            else if (myArray[next_row, next_col] == constant.DESTINATION)
            {
                myArray[next_row, next_col] = constant.WOR_DES;
                if (myArray[now_row, now_col] == constant.WORKER)
                {
                    myArray[now_row, now_col] = constant.BLANK;
                }
                else
                {
                    myArray[now_row, now_col] = constant.DESTINATION;
                }
            }
            else if (myArray[next_row, next_col] == constant.BOX_DES && myArray[to_row, to_col] == constant.BLANK)
            {
                myArray[next_row, next_col] = constant.WOR_DES;
                myArray[to_row, to_col] = constant.BOX;
                if (myArray[now_row, now_col] == constant.WORKER)
                {
                    myArray[now_row, now_col] = constant.BLANK;
                }
                else
                {
                    myArray[now_row, now_col] = constant.DESTINATION;
                }

            }
            else if (myArray[next_row, next_col] == constant.BOX && myArray[to_row, to_col] == constant.DESTINATION)
            {
                myArray[next_row, next_col] = constant.WORKER;
                myArray[to_row, to_col] = constant.BOX_DES;
                if (myArray[now_row, now_col] == constant.WORKER)
                {
                    myArray[now_row, now_col] = constant.BLANK;
                }
                else
                {
                    myArray[now_row, now_col] = constant.DESTINATION;
                }
            }
            else if (myArray[next_row, next_col] == constant.BOX_DES && myArray[to_row, to_col] == constant.DESTINATION)
            {
                myArray[next_row, next_col] = constant.WOR_DES;
                myArray[to_row, to_col] = constant.BOX_DES;
                if (myArray[now_row, now_col] == constant.WORKER)
                {
                    myArray[now_row, now_col] = constant.BLANK;
                }
                else
                {
                    myArray[now_row, now_col] = constant.DESTINATION;
                }
            }
            pictureBox1.Image = DrawMap(myArray);
            if (isfinish())
            {
                btnNextLevel_Click(null, null);
            }
            return;
        }
        private void MenuClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            Level = int.Parse(tsmi.Text);
            init_data();
        }
        private bool isfinish()
        {
            //完成本关时
            bool bfinish = true;
            for (int row = 0; row < row_num; row++)
                for (int col = 0; col < col_num; col++)
                    if (myArray[row, col] == constant.DESTINATION
                        || myArray[row, col] == constant.WOR_DES)
                        bfinish = false;
            return bfinish;
        }
        private void btnNextLevel_Click(object sender, EventArgs e)
        {
            Level++;
            //判断是否有下一关
            if (flag_last == 1)
            {
                MessageBox.Show("没有下一关了", "提醒");
                Level--;
                return;
            }
            init_data();
        }
        private int count_level(string[] txt)
        {
            string array_s = "//";
            int n = 0;
            for (int row = 0; row < txt.Length; row++)
            {
                if (txt[row].Contains(array_s))
                {
                    n++;
                }
            }
            return n;
        }

        /*要监听键盘的上下左右四个按键时，实际操作中会发现，这样设置完成之后，点击这几个键根本就触发不了keydown事件。后来百度了一下，终于找到了原因：
              方向键是作为系统键来处理的，默认方向键的作用是移动焦点，系统处理完了就不会将键盘的键值传递个窗体或获取焦点的控件，也不会触发窗体的KeyDown事件。 在没有控件的时候没有其他的控件可以移动焦点，系统不处理，这才会将键值传递给窗体，触发KeyDown事件
              解决方法：
              在窗体的.cs文件里面重写默认的系统键处理方式，遇到方向键，则直接返回，系统不处理，这样键值就会被传递到窗体，触发KeyDown事件*/
        private void PreviewKeydown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                e.IsInputKey = true;
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
                return false;
            else
                return base.ProcessDialogKey(keyData);
        }
    }
}
