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

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            //コンボボックスにディスプレイのリストを表示する
            this.comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            //デバイス名が表示されるようにする
            this.comboBox3.DisplayMember = "DeviceName";
            this.comboBox3.DataSource = Screen.AllScreens;
            GetAllDisplayInformation();
        }




        private void button1_Click(object sender, EventArgs e)
        {
            Move move = new Move();

            try
            {
                var bmp = new Bitmap(this.textBox1.Text);
                Debug.WriteLine(String.Format("水平分解能 = {0}, 垂直分解能 = {1}", bmp.Height, bmp.Width));
                move.WindowSize(bmp.Width, bmp.Height);

                move.SetImage(this.textBox1.Text, comboBox2.SelectedIndex);

                move.TopMost = checkBox_top.Checked;

                //フォームを表示するディスプレイのScreenを取得する
                Screen s = (Screen)this.comboBox3.SelectedItem;
                Debug.WriteLine(s.Bounds.X+":"+s.Bounds.Y);
                move.Top = s.Bounds.Y;
                move.Left=s.Bounds.X;
                move.Show();
            }
            catch (Exception ex)
            {
                //メッセージボックスを表示する
                MessageBox.Show("正しいファイル場所を指定してください。", "こら！！！",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }

        //参考
        //https://nomux2.net/dialog/
        private void button2_Click(object sender, EventArgs e)
        {
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
                    this.textBox1.Text = dlg.FileName;
                }
            }
        }

        private void ディスプレイ情報ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetAllDisplayInformation(), "ディスプレイ情報");
        }

        /// <summary>
        /// 全てのディスプレイの情報取得
        /// 参考：https://qiita.com/kewpie134134/items/bbdc49dc38fe84a48c1a
        /// </summary>
        private string GetAllDisplayInformation()
        {
            string displayMainTitle = "";
            string displayDeviceName = "";
            string displayBoundsX = "";
            string displayBoundsWidth = "";
            string displayWorkingAreaX = "";
            string displayWorkingAreaWidth = "";
            string displayEnd = "";
            string messages = "";

            try
            {
                displayMainTitle = "-----\n●全てのディスプレイの情報";
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
            return displayMainTitle + messages;
        }
    }
}
