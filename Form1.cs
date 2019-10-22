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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Bitmap DrawMap(int[,] myArray)
        {
            //0空地 1墙 3箱 4子目的地 6人 7箱子与目的地重合 9人与目的地重合
            pictureBox1.Width = myArray.GetLength(1) * 30;
            pictureBox1.Height = myArray.GetLength(0) * 30;
            Bitmap bit = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bit);
            SolidBrush redBrush = new SolidBrush(Color.Red);
            Image image = new Bitmap("worker.png");
            for (int i = 0; i < myArray.GetLength(0); i++)
            {
                for (int j = 0; j < myArray.GetLength(1); j++)
                {
                    if (myArray[i, j] == 1)
                    {
                        image = new Bitmap("wall.png");
                        g.DrawImage(image, j * 30, i * 30, 30, 30);
                    }
                    if (myArray[i, j] == 6)
                    {
                        image = new Bitmap("worker.png");
                        g.DrawImage(image, j * 30, i * 30, 30, 30);
                    }
                    if (myArray[i, j] == 3)
                    {
                        image = new Bitmap("box.png");
                        g.DrawImage(image, j * 30, i * 30, 30, 30);
                    }
                    if (myArray[i, j] == 0)
                    {
                        image = new Bitmap("land.png");
                        g.DrawImage(image, j * 30, i * 30, 30, 30);
                    }
                    if (myArray[i, j] == 4)
                    {
                        image = new Bitmap("dest.png");
                        g.DrawImage(image, j * 30, i * 30, 30, 30);
                    }
                    if (myArray[i, j] == 9)
                    {
                        image = new Bitmap("WoD.png");
                        g.DrawImage(image, j * 30, i * 30, 30, 30);
                    }
                    if (myArray[i, j] == 7)
                    {
                        image = new Bitmap("BoD.png");
                        g.DrawImage(image, j * 30, i * 30, 30, 30);
                    }
                }
            }
            return bit;
        }
        public string FileName = ".\\map\\configurations.txt";
        private string[] txt = File.ReadAllLines(".\\map\\configurations.txt");
        //疑问 这里直接写File.ReadAllLines(FileName)报错？
        public void Form1_Load(object sender, EventArgs e)
        {
            try { txt = File.ReadAllLines(FileName); }
            catch (FileNotFoundException q)
            {
                MessageBox.Show("找不到地图文件", "提示");
                Close();
                return;
            }
            init_data();
        }
        private int[,,] myArray_old;
        private int[,] myArray;
        private int row_num = 0, col_num = 0; //第n关的行数和列数
        public int[,] ReadMap(string[] txt) 
        {
            //将text file里的第n关map数据写入一个integer array中
            string[] array0 = txt[0].Split(',');
            col_num = array0.Length;
            var array = new string[row_num, col_num];
            for (int i = 0; i < row_num; i++)
            {
                string[] arrayi = txt[i].Split(',');
                for (int j = 0; j < col_num; j++)
                {
                    array.SetValue(arrayi[j], i, j);
                }
            }

            int[,] array1 = new int[row_num, col_num];
            for (int i = 0; i < row_num; i++)
            {
                for (int j = 0; j < col_num; j++)
                {
                    int.TryParse(array[i, j], result: out array1[i, j]);
                }
            }
            //初始化存储撤销数据的数组
            myArray_old = new int[500, row_num, col_num];

            return array1;
        }
        public int row_start = 0, row_end = 0; //第n关的开始和结束行数
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
                for (int i = this.row_start; i < this.row_end; i++)
                {
                    lines.RemoveAt(this.row_start); //删除第 row_start~row_end 行
                }
            }
            string[,] array = new string[myArray.GetLength(0), myArray.GetLength(1)];
            string[] arrayi = new string[myArray.GetLength(1)];
            string[] arrays = new string[myArray.GetLength(0)];
            for (int i = 0; i < myArray.GetLength(0); i++)
            {
                for (int j = 0; j < myArray.GetLength(1); j++)
                {
                    array[i, j] = myArray[i, j].ToString();
                    arrayi.SetValue(array[i, j], j);
                }
                arrays[i] = string.Join(",", arrayi);
            }
            for (int i = this.row_start; i < this.row_start + myArray.GetLength(0); i++)
            {
                lines.Insert(i, arrays[i - this.row_start]); //插入新行
            }
            this.row_end = this.row_start + myArray.GetLength(0);
            txt = lines.ToArray();
        }
        private string[] txt_n;
        internal int flag_last = 0;
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
            for (int i = 0; i < row_num; i++)
            {
                txt_n[i] = string.Copy(txt[row_start + i]);
            }

            return txt_n;
        }
        private int Level = 1;//关卡数
        private int seq = -1;
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
        private int i, j;

        private void RestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            init_data();

        }

        private void Keydown(object sender, KeyEventArgs e) 
        {
            //按下按键时操作
            //得到人的坐标
            for (int x = 0; x < row_num; x++)
            {
                for (int y = 0; y < col_num; y++)
                {
                    if (myArray[x, y] == 6 || myArray[x, y] == 9)
                    {
                        i = x;
                        j = y;
                    }
                }

            }
            //接收按键
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (myArray[i - 1, j] == 0)
                    {
                        myArray[i - 1, j] = 6;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i - 1, j] == 3 && myArray[i - 2, j] == 0)
                    {
                        myArray[i - 1, j] = 6;
                        myArray[i - 2, j] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i - 1, j] == 4)
                    {
                        myArray[i - 1, j] = 9;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i - 1, j] == 7 && myArray[i - 2, j] == 0)
                    {
                        myArray[i - 1, j] = 9;
                        myArray[i - 2, j] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }

                    }
                    else if (myArray[i - 1, j] == 3 && myArray[i - 2, j] == 4)
                    {
                        myArray[i - 1, j] = 6;
                        myArray[i - 2, j] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i - 1, j] == 7 && myArray[i - 2, j] == 4)
                    {
                        myArray[i - 1, j] = 9;
                        myArray[i - 2, j] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }  
                    break;
                case Keys.Left:
                    if (myArray[i, j - 1] == 0)
                    {
                        myArray[i, j - 1] = 6;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j - 1] == 3 && myArray[i, j - 2] == 0)
                    {
                        myArray[i, j - 1] = 6;
                        myArray[i, j - 2] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j - 1] == 4)
                    {
                        myArray[i, j - 1] = 9;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j - 1] == 7 && myArray[i, j - 2] == 0)
                    {
                        myArray[i, j - 1] = 9;
                        myArray[i, j - 2] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }

                    }
                    else if (myArray[i, j - 1] == 3 && myArray[i, j - 2] == 4)
                    {
                        myArray[i, j - 1] = 6;
                        myArray[i, j - 2] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j - 1] == 7 && myArray[i, j - 2] == 4)
                    {
                        myArray[i, j - 1] = 9;
                        myArray[i, j - 2] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    
                    break;
                case Keys.Down:
                    if (myArray[i + 1, j] == 0)
                    {
                        myArray[i + 1, j] = 6;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i + 1, j] == 3 && myArray[i + 2, j] == 0)
                    {
                        myArray[i + 1, j] = 6;
                        myArray[i + 2, j] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i + 1, j] == 4)
                    {
                        myArray[i + 1, j] = 9;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i + 1, j] == 7 && myArray[i + 2, j] == 0)
                    {
                        myArray[i + 1, j] = 9;
                        myArray[i + 2, j] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }

                    }
                    else if (myArray[i + 1, j] == 3 && myArray[i + 2, j] == 4)
                    {
                        myArray[i + 1, j] = 6;
                        myArray[i + 2, j] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i + 1, j] == 7 && myArray[i + 2, j] == 4)
                    {
                        myArray[i + 1, j] = 9;
                        myArray[i + 2, j] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    } 
                    break;
                case Keys.Right:
                    if (myArray[i, j + 1] == 0)
                    {
                        myArray[i, j + 1] = 6;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j + 1] == 3 && myArray[i, j + 2] == 0)
                    {
                        myArray[i, j + 1] = 6;
                        myArray[i, j + 2] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j + 1] == 4)
                    {
                        myArray[i, j + 1] = 9;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j + 1] == 7 && myArray[i, j + 2] == 0)
                    {
                        myArray[i, j + 1] = 9;
                        myArray[i, j + 2] = 3;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }

                    }
                    else if (myArray[i, j + 1] == 3 && myArray[i, j + 2] == 4)
                    {
                        myArray[i, j + 1] = 6;
                        myArray[i, j + 2] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    else if (myArray[i, j + 1] == 7 && myArray[i, j + 2] == 4)
                    {
                        myArray[i, j + 1] = 9;
                        myArray[i, j + 2] = 7;
                        if (myArray[i, j] == 6)
                        {
                            myArray[i, j] = 0;
                        }
                        else
                        {
                            myArray[i, j] = 4;
                        }
                    }
                    //else label2.Text = "无效操作";
                    break;
            }
            pictureBox1.Image = DrawMap(myArray);
            if (isfinish())
            {
                //MessageBox.Show("恭喜你顺利过关", "提示");   
               btnNextLevel_Click(null, null);
                //Thread.Sleep(2000);
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
            for (int i = 0; i < row_num; i++)
                for (int j = 0; j < col_num; j++)
                    if (myArray[i, j] == 4
                        || myArray[i, j] == 9)
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
