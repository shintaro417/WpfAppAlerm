using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WpfAppAlerm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 一覧設定ファイルパス存在チェック
        /// </summary>
        /// <returns>設定が有効ならtrue</returns>
        private bool IsExistsSaveConfigPath()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SaveConfigPath) || File.Exists(Properties.Settings.Default.SaveConfigPath) == false)
            {
                //パスの設定が無い、または指定したファイルに存在しない場合
                return false;
            }
            else
            {
                //パスが設定されていて、ファイルが実在する
                return true;
            }
        }
        /// <summary>
        /// 情報メッセージ表示処理
        /// </summary>
        /// <param name="msg"></param>
        private void InfoMsg(string msg)
        {
            MessageBox.Show(msg, Properties.Resources.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);

        }

        /// <summary>
        /// エラーメッセージ表示
        /// </summary>
        /// <param name="msg"></param>
        private void ErrMsg(string msg)
        {
            MessageBox.Show(msg, Properties.Resources.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);

        }
        /// <summary>
        /// 追加ボタンクリック時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime d;

            //時刻チェック
            //(数字2桁:数字2桁か簡易チェックしてOKならDateTime型に変換できるかチェックする
            //^ -> 先頭ヒット $ -> 文末 [0-9] -> 0~9の数字 {2} -> 直前の文字を2回繰り返す
            //TryParse -> 日付型に出来るか判定する。2番目の引数は返還後の日付オブジェクト
            if (Regex.IsMatch(timeText.Text, "^[0-9]{2}:[0-9]{2}$") && DateTime.TryParse("2000/01/01 " + timeText.Text, out d))
            {
                //時刻として正しい

                //メッセージが入力されているか
                if (string.IsNullOrWhiteSpace(msgText.Text))
                {
                    //メッセージ未入力
                    ErrMsg("メッセージを入力してください");
                    msgText.Focus();
                }
                else
                {
                    //時刻もメッセージも入力されている

                    //一覧に追加する
                    //\t -> タブ
                    listBox.Items.Add(timeText.Text + "\t" + msgText.Text);

                    timeText.Clear();
                    msgText.Clear();
                    timeText.Clear();
                }
            }
            else
            {
                //時刻として正しくない
                ErrMsg("時刻を正しく入力してください。例) 12:34");
                timeText.Focus();
            }
        }
        /// <summary>
        /// Window起動時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// window終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
        /// <summary>
        /// 一覧ファイル保存
        /// </summary>
        /// <remarks>
        /// 前提:SaveConfigPathに実在するパスが存在すること
        /// </remarks>
        private void SaveConfigFIle()
        {
            using (var sw = new StreamWriter(Properties.Settings.Default.SaveConfigPath, append: false, encoding: Encoding.UTF8))
            {
                //リストボックスの内容をファイルに出力
                foreach (string item in listBox.Items)
                {
                    //1行書き込み
                    sw.WriteLine(item);

                }
            }
        }
        /// <summary>
        /// ファイル保存ダイアログを表示してファイル保存
        /// </summary>
        private void ShowSaveDialoToConfigFile()
        {
            //ダイアログ生成
            var dlg = new SaveFileDialog();
        }
    }
}
