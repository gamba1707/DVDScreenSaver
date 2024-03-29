﻿using OpenCvSharp;
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
        //移動速度
        float speed = 1f;

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
        //複数回当たらないようにするためのインターバル
        bool inte_rnd_time;
        //インターバルのタイマーを入れておく用
        private Timer t;

        //画像
        Bitmap bitmap;
        //色を変える場合に格納する
        List<Bitmap> bitmap_list = new List<Bitmap>();

        //起動時
        public Move()
        {
            InitializeComponent();
            init();
        }

        //初期設定
        async void init()
        {
            //エラーになるのでちょっと待つ
            await Task.Delay(5);
            //位置にも代入
            x = screen_x;
            y = screen_y;


            //移動速度を適応
            x_inc *= speed; y_inc *= speed;

            //画像を反映
            pictureBox.Image = bitmap;
            Debug.WriteLine(this.Width + ":" + this.Height);

            //動かす
            MoveWin();
        }

        public void SetSpeed(float x)
        {
            speed = x;
        }

        //ウィンドウサイズを指定できる
        public void WindowSize(int w, int h)
        {
            //仮置きで入れておく
            int width = w + 10;
            int height = h + 10;
            
            Debug.WriteLine("リサイズ前：" + width + ":" + height);
            //もし入れたウィンドウサイズが大きすぎる場合は縮小する（画面の65％未満に）
            while (width / screen_w >= 0.65f)
            {
                width /= 2;
                height /= 2;
                Debug.WriteLine("リサイズ：" + width + ":" + height);
            }
            while (height / screen_h >= 0.65f)
            {
                width /= 2;
                height /= 2;
                Debug.WriteLine("リサイズ：" + width + ":" + height);
            }

            //大きさを割り当てる
            this.Width = width;
            this.Height = height;
        }

        public void SetScreen(Screen s)
        {
            screen_x = s.Bounds.X;
            screen_y = s.Bounds.Y;

            //ディスプレイの大きさを代入
            screen_h = s.WorkingArea.Height;
            screen_w = s.WorkingArea.Width;
        }

        //画像を反映させる
        public void SetImage(string path, int num)
        {
            //一応パスとどのエフェクトかを保持する
            this.path = path;
            this.num = num;

            //パスからMat型を生成
            using (Mat mat = new Mat(path))
            {
                //エフェクトによって振り分ける
                switch (num)
                {
                    case 1://白黒
                        //OpenCVで白黒にしてBitMap型に格納
                        bitmap = BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2GRAY));
                        break;
                    case 2://接触で色変え
                        //それぞれの画像を配列に格納
                        init_color_Change(mat);
                        //配列の一番目を指定
                        bitmap = bitmap_list[0];
                        break;
                    default:
                        //よくわからないやつは加工無しで
                        bitmap = BitmapConverter.ToBitmap(mat);
                        break;
                }
            }
        }

        //Mat型を渡すと単色カラーをMat型で返す
        void init_color_Change(Mat mat)
        {
            //色変え用
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

            //ここから擬似色変え
            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2RGB)));

            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2Luv)));

            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2HSV)));

            bitmap_list.Add(BitmapConverter.ToBitmap(mat.CvtColor(ColorConversionCodes.BGR2Lab)));

            //色反転
            Cv2.BitwiseNot(mat, mat);
            bitmap_list.Add(BitmapConverter.ToBitmap(mat));

            Debug.WriteLine(bitmap_list.Count);
        }

        //ウィンドウが動くやつ
        async void MoveWin()
        {
            //クリックしていない間動き続ける
            while (!click)
            {
                if (this.IsDisposed)
                {
                    Debug.WriteLine("finish");
                    this.Dispose();
                    click = true;
                }
                Debug.WriteLine("x:" + x + " y:" + y);

                //画面外に出そうな場合
                //右端
                if (x + x_inc + this.Width > screen_w + screen_x)
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
                if (y + y_inc + this.Height >= screen_h + screen_y)
                {
                    y_inc = -y_inc;
                    if (num == 2 && !inte_rnd_time)
                    {
                        nextColor();
                    }
                }
                //左端
                if (x + x_inc <= screen_x)
                {
                    x_inc = -x_inc;
                    if (num == 2 && !inte_rnd_time)
                    {
                        nextColor();
                    }
                }
                //上
                if (y + y_inc <= screen_y)
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
                //20ミリ秒待つ（やらないと落ちる）
                await Task.Delay(20);
            }
        }

        //接触で色変え
        void nextColor()
        {
            //リストの最後なら最初へ
            if (rnd + 1 >= bitmap_list.Count) rnd = -1;

            //タイマー生成
            t = new Timer();
            //10ミリ秒指定
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

        //インターバルを終わらせる
        void interval_rnd(object sender, EventArgs e)
        {
            //タイマー終了
            t.Stop();
            //また色を変えれるように
            inte_rnd_time = false;
        }

        //画像をクリック（ウィンドウ）
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //反転させる
            click = !click;
            Debug.WriteLine($"Click {click}");

            //再び押した（動き出す）
            if (!click)
            {
                //現在のウィンドウ位置を反映させる
                x = this.Left;
                y = this.Top;

                //マルチスクリーンの場合、正しく反映させる
                Screen screen = Screen.FromControl(this);
                Debug.WriteLine("FormがあるScreen.DeviceName:" + screen.DeviceName);
                Debug.WriteLine("画面の大きさ:" + screen.Bounds.X + "x" + screen.Bounds.Y);
                screen_x = screen.Bounds.X;
                screen_y = screen.Bounds.Y;

                //動かす
                MoveWin();
            }
        }
    }
}
