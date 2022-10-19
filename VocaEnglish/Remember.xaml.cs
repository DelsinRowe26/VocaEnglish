using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace VocaEnglish
{
    /// <summary>
    /// Логика взаимодействия для Remember.xaml
    /// </summary>
    public partial class Remember : Window
    {

        static StreamReader fileName = new StreamReader(@"VocaEnglish\Words\WordsRemember.tmp", System.Text.Encoding.Default);
        string[] txt = fileName.ReadToEnd().Split(new char[] { ';' }, StringSplitOptions.None);
        int countGeneral = 0, countRemember = 0, countIdontRemember = 0;
        bool RememberBool = false, dontRemember = false;

        private void btnRemember_Click(object sender, RoutedEventArgs e)
        {
            countGeneral++;
            //countRemember++;
            RememberBool = true;
            Words();
        }

        private void btnIdontRemember_Click(object sender, RoutedEventArgs e)
        {
            countGeneral++;
            //countIdontRemember++;
            dontRemember = true;
            Words();
        }

        public Remember()
        {
            InitializeComponent();
        }

        private async void WinRemember_Loaded(object sender, RoutedEventArgs e)
        {
            countGeneral = 0;
            countRemember = 0;
            countIdontRemember = 0;
            File.WriteAllText("Data_Remember.tmp", " ");
            File.WriteAllText("Data_IdontRemember.tmp", " ");
            lbWords.Content = "Сейчас будут появляться слова\nи вы должны нажать\nна одну из кнопок\n";
            lbWords.Visibility = Visibility.Visible;
            await Task.Delay(5000);

            Words();
        }
        
        private void Words()
        {
            if (countRemember + countIdontRemember != txt.Length-2)
            {
                btnIdontRemember.IsEnabled = true;
                btnRemember.IsEnabled = true;

                lbWords.Content = txt[countGeneral].ToString();

                if (RememberBool == true)
                {
                    RememberBool = false;
                    countRemember++;
                    File.AppendAllText("Data_Remember.tmp", txt[countGeneral].ToString());
                }
                else if (dontRemember == true)
                {
                    dontRemember = false;
                    countIdontRemember++;
                    File.AppendAllText("Data_IdontRemember.tmp", txt[countGeneral].ToString());
                }
            }
            else
            {
                File.AppendAllText("Data_Remember.tmp", countRemember.ToString());
                File.AppendAllText("Data_IdontRemember.tmp", countIdontRemember.ToString());
                Close();
            }
        }

    }
}
