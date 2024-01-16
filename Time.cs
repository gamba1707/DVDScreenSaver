using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace SS
{
    public partial class Time : Form
    {
        //移動速度
        float speed=1f;

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

        //
        string format;

        int num;

        //色変えの順番
        int rnd = 0;
        //複数回当たらないようにするためのインターバル
        bool inte_rnd_time;
        //インターバルのタイマーを入れておく用
        private Timer t;
        public Time()
        {
            InitializeComponent();
            init();
            
        }

        async void init()
        {
            //エラーになるのでちょっと待つ
            await Task.Delay(5);
            //位置にも代入
            x = screen_x;
            y = screen_y;

            //移動速度を適応
            x_inc *= speed; y_inc*=speed;

            MoveWin();

            //時刻を適応していく（誤差10ミリ秒）
            while (true)
            {
                label1.Text = DateTime.Now.ToString(format);
                await Task.Delay(10);
            }
        }

        public void SetScreen(Screen s)
        {
            screen_x = s.Bounds.X;
            screen_y = s.Bounds.Y;

            //ディスプレイの大きさを代入
            screen_h = s.WorkingArea.Height;
            screen_w =s.WorkingArea.Width;
        }

        public void SetSpeed(float x)
        {
            speed = x;
        }


        public void SetFont(Font f,Color color)
        {
            //フォントを設定
            label1.Font = f;
            //白黒にする以外は選択された色にする
            if(num!=1)label1.ForeColor = color;
            //ウィンドウの大きさを適切な値にする
            Width = (int)((label1.Size.Width + 35 * f.Size* DateTime.Now.ToString(format).Length) * 0.03);
            Height = (int)((label1.Size.Height + 55) * f.Size * 0.03);

            //小さすぎる場合はこの値にする
            if (Width < 130) Width = 130;
            if (Height < 70) Height = 70;
        }

        //日時のフォーマットを設定する
        public void SetFormat(string s)
        {
            format = s;
        }

        //エフェクトの値を控える
        public void SetNum(int a)
        {
            num=a;
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
            //タイマー生成
            t = new Timer();
            //10ミリ秒指定
            t.Interval = 10;
            //複数入ってこないようにする
            inte_rnd_time = true;

            switch (rnd)
            {
                case 0:
                    label1.ForeColor = Color.Black;
                    rnd++;
                    break;
                case 1:
                    label1.ForeColor = Color.Blue;
                    rnd++;
                    break;
                case 2:
                    label1.ForeColor = Color.Red;
                    rnd++;
                    break;
                case 3:
                    label1.ForeColor = Color.Yellow;
                    rnd++;
                    break;
                case 4:
                    label1.ForeColor = Color.Green;
                    rnd++;
                    break;
                case 5:
                    label1.ForeColor = Color.Gold;
                    rnd++;
                    break;
                case 6:
                    label1.ForeColor = Color.HotPink;
                    rnd++;
                    break;
                case 7:
                    label1.ForeColor = Color.Indigo;
                    rnd++;
                    break;
                case 8:
                    label1.ForeColor = Color.LightBlue;
                    rnd++;
                    break;
                case 9:
                    label1.ForeColor = Color.Orange;
                    rnd++;
                    break;
                case 10:
                    label1.ForeColor = Color.LightGreen;
                    rnd++;
                    break;
                case 11:
                    label1.ForeColor = Color.Brown;
                    rnd++;
                    break;
                default:
                    rnd = 0;
                    break;
            }

            //10ミリ秒後にintervalをリセットする
            t.Tick += new EventHandler(interval_rnd);
            t.Start();
        }

        private void label1_Click(object sender, EventArgs e)
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

        //インターバルを終わらせる
        void interval_rnd(object sender, EventArgs e)
        {
            //タイマー終了
            t.Stop();
            //また色を変えれるように
            inte_rnd_time = false;
        }
    }
}
