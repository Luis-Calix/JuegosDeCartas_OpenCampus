using System.Windows;
using JuegosDeCartas_OpenCampus.ViewModels;

namespace JuegosDeCartas_OpenCampus
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
