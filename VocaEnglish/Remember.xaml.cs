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

        static StreamReader fileName = new StreamReader(@"VocaEnglish\Words\WordsRemember.tmp", System.Text.Encoding.UTF8);
        string[] txt = fileName.ReadToEnd().Split(new char[] { ';' }, StringSplitOptions.None);
        int countGeneral = 0, countRemember = 0, countIdontRemember = 0;
        bool RememberBool = false, dontRemember = false;
        int Procents = 0;

        private void btnRemember_Click(object sender, RoutedEventArgs e)
        {
            countGeneral++;
            //countRemember++;
            countRemember++;
            RememberBool = true;
            Words();
        }

        private void btnIdontRemember_Click(object sender, RoutedEventArgs e)
        {
            countGeneral++;
            //countIdontRemember++;
            countIdontRemember++;
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
            lbWords.Content = " Сейчас будут появляться слова\n         и вы должны нажать\n            на одну из кнопок\nесли вы помните перевод слова";
            lbWords.Visibility = Visibility.Visible;
            await Task.Delay(5000);

            Words();
        }
        
        private async void Words()
        {
            if (countRemember + countIdontRemember != txt.Length-2)
            {
                btnIdontRemember.IsEnabled = true;
                btnRemember.IsEnabled = true;

                lbWords.Content = txt[countGeneral].ToString();

                if (RememberBool == true)
                {
                    if(countGeneral == 1)
                    {
                        countGeneral--;
                        File.AppendAllText("Data_Remember.tmp", txt[countGeneral].ToString());
                        countGeneral++;
                    }
                    RememberBool = false;
                    
                    File.AppendAllText("Data_Remember.tmp", txt[countGeneral].ToString());
                }
                else if (dontRemember == true)
                {
                    if(countGeneral == 1)
                    {
                        countGeneral--;
                        File.AppendAllText("Data_IdontRemember.tmp", txt[countGeneral].ToString());
                        countGeneral++;
                    }
                    dontRemember = false;
                    
                    File.AppendAllText("Data_IdontRemember.tmp", txt[countGeneral].ToString());
                }
            }
            else
            {
                File.AppendAllText("Data_Remember.tmp", "\n" + countRemember.ToString());
                File.AppendAllText("Data_IdontRemember.tmp", "\n" + countIdontRemember.ToString());

                Procents = (countRemember * 100) / 39;
                lbWords.Content = "Вы помните:\n" + "      " + Procents.ToString("f2") + "%" + "\nиз 100% слов";
                await Task.Delay(5000);

                Close();
            }
        }

    }
}
