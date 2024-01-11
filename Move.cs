using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp.Extensions;
using System.Security.Cryptography;

namespace SS
{
    public partial class Move : Form
    {
        //クリックしたか
        bool click;

        //位置
        float x, y;

        //加速度
        float x_inc = 5f;
        float y_inc = 5f;

        //画面自体の大きさ
        int screen_w;
        int screen_h;
        //画面の初期位置
        int screen_x;
        int screen_y;

        //画像のフルパス
        string path;
        //表示パターン（0:そのまま　1:白黒　2:接触で色変え）
        int num;

        //色変えの順番
        int rnd = 0;
        bool inte_rnd_time;
        private Timer t;

        Bitmap bitmap;
        List<Bitmap> bitmap_list = new List<Bitmap>();
        public Move()
        {
            InitializeComponent();
            init();
            MoveWin();
        }

        async void init()
        {
            await Task.Delay(5);
            screen_x = Left;
            screen_y = Top;
            pictureBox.Image = bitmap;
            Debug.WriteLine(this.Width + ":" + this.Height);
        }

        public void WindowSize(int w, int h)
        {
            this.Width = w + 10;
            this.Height = h + 42;
            screen_h = System.Windows.Forms.Screen.GetWorkingArea(this).Height;
            screen_w = System.Windows.Forms.Screen.GetWorkingArea(this).Width;

            while (this.Width/screen_w>=0.65f)
            {
                this.Width /= 2;
                this.Height /= 2;
            }
            while (this.Height / screen_h >= 0.65f)
            {
                this.Height /= 2;
                this.Width /= 2;
            }
        }


        public void SetImage(string path, int num)
        {
            this.path = path;
            this.num = num;

            using (Mat mat = new Mat(path))
            {
                switch (num)
                {
                    case 1:
                        bitmap = BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2GRAY));
                        break;
                    case 2:
                        init_color_Change(mat);
                        bitmap = bitmap_list[0];
                        break;
                    default:
                        bitmap = BitmapConverter.ToBitmap(mat);
                        break;
                }
            }
        }

        //Mat型を渡すと単色カラーをMat型で返す
        void init_color_Change(Mat mat)
        {
            Mat Zero = Mat.Zeros(mat.Height, mat.Width, MatType.CV_8UC1);
            Mat[] mat_out;
            Mat dest = new Mat();

            //そのまま
            bitmap_list.Add(BitmapConverter.ToBitmap(mat));
            //白黒
            //bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2GRAY)));

            //赤
            Cv2.Split(mat, out mat_out);
            Cv2.Merge(new[] { Zero, Zero, mat_out[2] }, dest);
            bitmap_list.Add(BitmapConverter.ToBitmap(dest));

            //緑
            Cv2.Split(mat, out mat_out);
            Cv2.Merge(new[] { Zero, mat_out[1], Zero }, dest);
            bitmap_list.Add(BitmapConverter.ToBitmap(dest));

            //青
            Cv2.Split(mat, out mat_out);
            Cv2.Merge(new[] { mat_out[0], Zero, Zero }, dest);
            bitmap_list.Add(BitmapConverter.ToBitmap(dest));

            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2RGB)));

            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2Luv)));

            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2HSV)));

            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2Lab)));

            Cv2.BitwiseNot(mat, mat);
            bitmap_list.Add(BitmapConverter.ToBitmap(mat));

            Debug.WriteLine(bitmap_list.Count);
        }


        async void MoveWin()
        {
            //クリックしていない間動き続ける
            while (!click)
            {
                //画面外に出そうな場合
                //右端
                if (x + x_inc + this.Width > screen_w)
                {
                    //加算方向を逆にする
                    x_inc = -x_inc;

                    //もし色変え指定なら色を変える
                    if (num == 2 && !inte_rnd_time)
                    {
                        nextColor();
                    }
                }
                //一番下（タスクバーは除く）
                if (y + y_inc + this.Height > screen_h)
                {
                    y_inc = -y_inc;
                    if (num == 2 && !inte_rnd_time)
                    {
                        nextColor();
                    }
                }
                //左端
                if (x + x_inc < 0)
                {
                    x_inc = -x_inc;
                    if (num == 2 && !inte_rnd_time)
                    {
                        nextColor();
                    }
                }
                //上
                if (y + y_inc < 0)
                {
                    y_inc = -y_inc;
                    if (num == 2 && !inte_rnd_time)
                    {
                        nextColor();
                    }
                }

                //位置を加算していく
                x += x_inc;
                y += y_inc;

                //位置を反映させる
                this.Left = (int)x;
                this.Top = (int)y;
                //20ミリ秒待つ
                await Task.Delay(20);

            }
        }

        void nextColor()
        {
            if (rnd + 1 >= bitmap_list.Count) rnd = -1;
            t = new Timer();
            t.Interval = 10;
            //複数入ってこないようにする
            inte_rnd_time = true;
            //次の色へ
            rnd++;
            Debug.WriteLine(rnd);
            //画像を配列から指定
            bitmap = bitmap_list[rnd];
            //PictureBoxに反映
            pictureBox.Image = bitmap;
            //10ミリ秒後にintervalをリセットする
            t.Tick += new EventHandler(interval_rnd);
            t.Start();
        }

        void interval_rnd(object sender, EventArgs e)
        {
            t.Stop();
            inte_rnd_time = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            click = !click;
            Debug.WriteLine($"Click {click}");
            if (!click)
            {
                //位置を反映させる
                x = this.Left;
                y=this.Top;
                MoveWin();
            }
        }
    }
}
