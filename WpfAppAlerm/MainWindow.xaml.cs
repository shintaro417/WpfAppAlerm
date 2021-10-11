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
using System.Windows.Threading;

namespace WpfAppAlerm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 前回メッセージ表示時間
        /// </summary>
        /// <remarks>
        /// 同じ時間に何度もメッセージが出ないようにするためのもの
        /// </remarks>
        string showTime;

        /// <summary>
        /// 時間とメッセージ一覧
        /// </summary>
        Dictionary<string, string> timeAndMsgs;

        /// <summary>
        /// タイマー
        /// </summary>
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            InitProc();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void InitProc()
        {
            //登録時間一覧初期化
            timeAndMsgs = new Dictionary<string, string>();

            showTime = "";

            //タイマー初期化1
            timer = new DispatcherTimer();

            //イベント発生間隔を200ミリ秒間に設定
            timer.Interval = TimeSpan.FromMilliseconds(200);

            //タイマーイベント設定
            timer.Tick += new EventHandler(timer_Tick);
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

                    //まだ追加されていない時刻かチェックする
                    if (timeAndMsgs.ContainsKey(timeText.Text))
                    {
                        ErrMsg("この時刻は追加済みです。");

                    }
                    else
                    {
                        //一覧に追加
                        listBox.Items.Add(timeText.Text + "\t" + msgText.Text);
                        timeAndMsgs.Add(timeText.Text, msgText.Text);

                        timeText.Clear();
                        msgText.Clear();
                    }

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
            try
            {
                //一覧設定あり？
                if (IsExistsSaveConfigPath())
                {
                    using(var sr = new StreamReader(Properties.Settings.Default.SaveConfigPath,Encoding.UTF8))
                    {
                        //ファイルの終わりまで繰り返し
                        while (sr.EndOfStream == false)
                        {
                            //一行読み込み
                            string line = sr.ReadLine();

                            //分解
                            string[] lineArray = line.Split("\t");

                            if (lineArray.Length == 2)
                            {
                                //追加
                                listBox.Items.Add(line);
                                timeAndMsgs.Add(lineArray[0], lineArray[1]);

                            }
                        }
                    }
                }

                //アラームOn/Off設定値読み込み
                alarmOn.IsChecked = Properties.Settings.Default.AlarmEnabled;

                if (alarmOn.IsChecked == true)
                {
                    //アラームONならタイマー開始
                    timer.Start();
                }
            }
            catch (Exception exc)
            {
                ErrMsg(exc.Message);

            }
        }
        /// <summary>
        /// window終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (listBox.Items.Count > 0)
                {
                    //一覧の保存先パスチェック
                    if (IsExistsSaveConfigPath())
                    {
                        //ファイル保存
                        SaveConfigFIle();
                    }
                    else
                    {
                        //一覧ファイル保存先パスが未設定か、指定した場所にファイルが存在しない場合

                        //ファイル保存ダイアログを表示してファイル保存1
                        ShowSaveDialoToConfigFile();
                    }
                }

                //画面のアラームOn/Off設定値をSettingsに反映
                Properties.Settings.Default.AlarmEnabled = alarmOn.IsChecked == true;

                //設定ファイル保存
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                ErrMsg(ex.Message);

            }
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

            //ダイアログタイトルを生成する
            dlg.Title = "設定を保存する";

            //保存する一覧ファイルの既定ファイル名を設定する
            dlg.FileName = Properties.Resources.AppTitle + "_設定.dat";

            //フィルタ設定
            dlg.Filter = "設定ファイル|*.dat|全てのファイル|*.*";

            //保存ダイアログ表示
            if (dlg.ShowDialog() == true)
            {
                //ダイアログでOKされたらファイルパスを設定
                Properties.Settings.Default.SaveConfigPath = dlg.FileName;

                //ファイル保存
                SaveConfigFIle();
            }

        }

        /// <summary>
        /// タイマーイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            //現在時刻取得
            string time = DateTime.Now.ToString("HH:mm");

            //表示済みの時間では無いか？登録されている時刻か？
            if (time != showTime && timeAndMsgs.ContainsKey(time))
            {
                //メッセージ表示時間を保存
                showTime = time;

                //対応するメッセージを表示
                InfoMsg(timeAndMsgs[time]);

            }
        }

        /// <summary>
        /// アラームON/Offチェック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void alarmOnOff_Checked(object sender, RoutedEventArgs e)
        {
            if (timer == null)
            {
                return;
            }

            if (alarmOn.IsChecked == true)
            {
                timer.Start();
            }else
            {
                timer.Stop();
            }
        }
    }
}
