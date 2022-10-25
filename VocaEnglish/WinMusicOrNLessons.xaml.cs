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
    /// Логика взаимодействия для WinMusicOrNLessons.xaml
    /// </summary>
    public partial class WinMusicOrNLessons : Window
    {

        public static int ClickNextLessons = 0;

        public WinMusicOrNLessons()
        {
            InitializeComponent();
        }

        private void btnWait_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnNextLessons_Click(object sender, RoutedEventArgs e)
        {
            ClickNextLessons = 1;
            Close();
        }
    }
}
