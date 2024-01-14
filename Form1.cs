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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SS
{
    public partial class Form1 : Form
    {
        string version = "v1.0\n2024年1月11日配布";

        FontDialog fd;
        Color color;

        //起動時
        public Form1()
        {
            InitializeComponent();
            //プルダウンリストの一番目を入れておく
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            //コンボボックスにディスプレイのリストを表示する
            this.comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            //デバイス名が表示されるようにする
            this.comboBox3.DisplayMember = "DeviceName";
            this.comboBox3.DataSource = Screen.AllScreens;
        }

        //起動ボタンを押した
        private void button1_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    moveImageWindow();
                    break;
                    case 1:
                    moveTimeWindow();
                    break;
            }


        }

        void moveImageWindow()
        {
            //ウィンドウを作る
            Move move = new Move();
            try
            {
                //入れられたパスから画像を作る
                var bmp = new Bitmap(this.textBox1.Text);
                Debug.WriteLine(String.Format("水平分解能 = {0}, 垂直分解能 = {1}", bmp.Height, bmp.Width));
                //画像に対応したウィンドウサイズを指定
                move.WindowSize(bmp.Width, bmp.Height);
                //画像とエフェクト指定を送る
                move.SetImage(this.textBox1.Text, comboBox2.SelectedIndex);
                //最前面指定があればそうする
                move.TopMost = checkBox_top.Checked;

                //フォームを表示するディスプレイのScreenを取得する
                Screen s = (Screen)this.comboBox3.SelectedItem;
                Debug.WriteLine(s.Bounds.X + ":" + s.Bounds.Y);
                //位置を左上にさせる
                move.Top = s.Bounds.Y;
                move.Left = s.Bounds.X;
                //起動
                move.Show();
            }
            catch (Exception ex)
            {
                move.Dispose();
                //メッセージボックスを表示する
                MessageBox.Show("正しいファイル場所を指定してください。", "こら！！！",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        void moveTimeWindow()
        {
            //ウィンドウを作る
            Time time = new Time();
            try
            {
                DateTime.Now.ToString(textBox2.Text);
                time.SetFormat(textBox2.Text);
                if (fd != null&&color!=null)time.SetFont(fd.Font,color);
                time.SetNum(comboBox2.SelectedIndex);
                //最前面指定があればそうする
                time.TopMost = checkBox_top.Checked;

                //フォームを表示するディスプレイのScreenを取得する
                Screen s = (Screen)this.comboBox3.SelectedItem;
                Debug.WriteLine(s.Bounds.X + ":" + s.Bounds.Y);
                //位置を左上にさせる
                time.Top = s.Bounds.Y;
                time.Left = s.Bounds.X;
                //起動
                time.Show();
            }
            catch (FormatException ex)
            {
                time.Dispose();
                //日時フォーマットに違うものを入れた場合に呼ばれる
                //メッセージボックスを表示する
                MessageBox.Show("日時フォーマットが正しくありません！", "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                time.Dispose();
                //とりあえず対応する
                //メッセージボックスを表示する
                MessageBox.Show("エラー\n"+ex, "？？？",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        //参考
        //https://nomux2.net/dialog/
        //
        //画像パスを指定するボタン
        private void button2_Click(object sender, EventArgs e)
        {
            //ファイルを開く
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                //初期フォルダ(マイピクチャ)
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                //Jpgファイルをpngファイル以外のファイルをダイアログに表示しない
                dlg.Filter = "画像ファイル(*.jpg;*.png;*.bmp;*.gif)|*.jpg;*.png;*.bmp;*.gif";
                dlg.FilterIndex = 1;                //Filterの1つめ画像ファイルを指定
                dlg.Title = "画像ファイル指定";     //タイトルを設定する
                dlg.RestoreDirectory = true;        //カレントディレクトリ復元
                dlg.Multiselect = false;            //複数選択可否

                //ダイアログを開く
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    //一旦テキストボックスに反映
                    this.textBox1.Text = dlg.FileName;
                }
            }
        }

        //メニューバーにあるディスプレイ情報
        private void ディスプレイ情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetAllDisplayInformation(), "ディスプレイ情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 全てのディスプレイの情報取得
        /// 参考：https://qiita.com/kewpie134134/items/bbdc49dc38fe84a48c1a
        /// </summary>
        private string GetAllDisplayInformation()
        {
            string displayDeviceName = "";
            string displayBoundsX = "";
            string displayBoundsWidth = "";
            string displayWorkingAreaX = "";
            string displayWorkingAreaWidth = "";
            string displayEnd = "";
            string messages = "";

            try
            {
                foreach (System.Windows.Forms.Screen screen_data in System.Windows.Forms.Screen.AllScreens)
                {
                    displayDeviceName = "\nデバイス名 : " + screen_data.DeviceName;
                    displayBoundsX = "\nディスプレイの位置 : X=" + screen_data.Bounds.X + " - Y=" + screen_data.Bounds.Y;
                    displayBoundsWidth = "\nディスプレイのサイズ : 幅=" + screen_data.Bounds.Width + " - 高さ=" + screen_data.Bounds.Height;
                    displayWorkingAreaX = "\nディスプレイの作業領域の位置 : X" + screen_data.WorkingArea.X + " - Y=" + screen_data.WorkingArea.Y;
                    displayWorkingAreaWidth = "\nディスプレイの作業領域のサイズ : 幅" + screen_data.WorkingArea.Width + " - 高さ=" + screen_data.WorkingArea.Height;
                    displayEnd = "\n-----";

                    messages += displayDeviceName + displayBoundsX + displayBoundsWidth + displayWorkingAreaX + displayWorkingAreaWidth + displayEnd;
                }
            }
            catch
            {

            }
            return "-----" + messages;
        }

        private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(version, "バージョン情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (fd == null)
            {
                fd = new FontDialog();
            }
            //横書きフォントだけを表示する
            fd.AllowVerticalFonts = false;
            fd.ShowColor = true;
            

            if (fd.ShowDialog() == DialogResult.OK)
            {
                Debug.WriteLine("font:" + fd.Font);
                color=fd.Color;
            }
        }
    }
}
