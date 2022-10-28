﻿using System;
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

        static StreamReader reader = new StreamReader(@"VocaEnglish\Words\RussianWords.tmp", System.Text.Encoding.UTF8);
        string[] txtRus = reader.ReadToEnd().Split(new char[] { ';' }, StringSplitOptions.None);

        static StreamReader stream = new StreamReader(@"VocaEnglish\Words\Russian.tmp", System.Text.Encoding.UTF8);
        string[] RandomRussia = stream.ReadToEnd().Split(new char[] { ';' }, StringSplitOptions.None);

        int countGeneral = 0, countRemember = 0, countIdontRemember = 0, countGeneralRus = 0, rightCouunt = 0;
        bool RememberBool = false, dontRemember = false;
        
        int Procents = 0;
        int backRnd = 0;

        private void btnRemember_Click(object sender, RoutedEventArgs e)
        {
            countGeneral++;
            //countRemember++;
            countRemember++;
            RememberBool = true;
            Words();
        }

        private void rbFirst_Checked(object sender, RoutedEventArgs e)
        {

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
            RandomAnswer();
        }

        private void RandomAnswer()
        {
            Random random = new Random();
            int value = random.Next(0, 4);
            int count = 0;
            string wordsno, NoSpace;
            backRnd = value;
            if(value == 0)
            {
                wordsno = txtRus[countGeneral];
                rbFirst.Content = wordsno;
            }
            else if (value == 1)
            {
                wordsno = txtRus[countGeneral];
                rbSecond.Content = wordsno;
            }
            else if (value == 2)
            {
                wordsno = txtRus[countGeneral];
                rbThird.Content = wordsno;
            }
            else if (value == 3)
            {
                wordsno = txtRus[countGeneral];
                rbFour.Content = wordsno;
            }
            else if (value == 4)
            {
                wordsno = txtRus[countGeneral];
                rbFive.Content = wordsno;
            }
            while (count < 5)
            {
                if (count != backRnd)
                {
                    switch (count) 
                    {
                        case 0:
                            NoSpace = RandomRussia[countGeneralRus];
                            rbFirst.Content = NoSpace;
                            countGeneralRus++;
                            break;
                        case 1:
                            NoSpace = RandomRussia[countGeneralRus];
                            rbSecond.Content = NoSpace;
                            countGeneralRus++;
                            break;
                        case 2:
                            NoSpace = RandomRussia[countGeneralRus];
                            rbThird.Content = NoSpace;
                            countGeneralRus++;
                            break;
                        case 3:
                            NoSpace = RandomRussia[countGeneralRus];
                            rbFour.Content = NoSpace;
                            countGeneralRus++;
                            break;
                        case 4:
                            NoSpace = RandomRussia[countGeneralRus];
                            rbFive.Content = NoSpace;
                            countGeneralRus++;
                            break;
                    }
                }
                count++;
            }
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
