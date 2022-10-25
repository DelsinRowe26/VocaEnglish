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
using System.Windows.Shapes;

namespace VocaEnglish
{
    /// <summary>
    /// Логика взаимодействия для NextLessons.xaml
    /// </summary>
    public partial class NextLessons : Window
    {

        public static int ClickLessons = 0;

        public NextLessons()
        {
            InitializeComponent();
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e)
        {
            WinMusicOrNLessons.ClickNextLessons = 1;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(WinMusicOrNLessons.ClickNextLessons == 1)
            {
                //Close();
            }
            else
            {
                ClickLessons = 1;
            }
        }
    }
}
