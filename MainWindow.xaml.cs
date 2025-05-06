using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Win32;
using XmlCompare.Properties;
using Path = System.IO.Path;

namespace XmlCompare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new XmlCompareViewModel();
            ViewModel.CompareFileName = Settings.Default.CompareFileName;
            ViewModel.CompareName = Path.GetFileName(ViewModel.CompareFileName);
            ViewModel.CompareToFileName = Settings.Default.CompareToFileName;
            ViewModel.CompareToName = Path.GetFileName(ViewModel.CompareToFileName);
            DataContext = ViewModel;
        }

        public XmlCompareViewModel ViewModel { get; set; }

        private void OpenCompareClick(object sender, RoutedEventArgs e)
        {
            var showDialogResult = OpenXmlFileDialog(e, ViewModel.CompareFileName, out var xmlFileName);
            if (showDialogResult == true)
            {
                ViewModel.CompareFileName = xmlFileName;
                ViewModel.CompareName = Path.GetFileName(xmlFileName);
                Settings.Default.CompareFileName = xmlFileName;
                Settings.Default.Save();
                RunCompareClick(sender, e);
            }
        }

        private void OpenCompareToClick(object sender, RoutedEventArgs e)
        {
            var showDialogResult = OpenXmlFileDialog(e, ViewModel.CompareToFileName, out var xmlFileName);
            if (showDialogResult == true)
            {
                ViewModel.CompareToFileName = xmlFileName;
                ViewModel.CompareToName = Path.GetFileName(xmlFileName);
                Settings.Default.CompareToFileName = xmlFileName;
                Settings.Default.Save();
                RunCompareClick(sender, e);
            }

        }

        private void RunCompareClick(object sender, RoutedEventArgs e)
        {
            ViewModel.DiffList = new ObservableCollection<AttributeChangeViewModel>();
            ViewModel.HasCompared = false;
            ViewModel.CompareFiles();
            ViewModel.CompareFilesElements();
            if (!string.IsNullOrEmpty(ViewModel.CompareFileName) && !string.IsNullOrEmpty(ViewModel.CompareToFileName))
            {
                if (ViewModel.DiffList.Count == 0)
                {
                    if (ViewModel.HasCompared)
                        MessageBox.Show("All items where equal");
                    else
                        MessageBox.Show($"No Elements with name {ViewModel.ElementName} and ID {ViewModel.IdName} found");
                }
            }
        }

        private static bool? OpenXmlFileDialog(RoutedEventArgs e, string oldFileName, out string newFileName)
        {
            e.Handled = true;
            var ofd = new OpenFileDialog();
            ofd.DefaultExt = ".xml";
            ofd.Filter = "WXM Definition Files (.xml)|*.xml";
            ofd.FileName = oldFileName;
            if (!string.IsNullOrEmpty(oldFileName))
                ofd.InitialDirectory = Path.GetDirectoryName(oldFileName);
            var showDialogResult = ofd.ShowDialog();
            newFileName = ofd.FileName;
            return showDialogResult;
        }

        private void CompareTwoSelectedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var selectedItems = diffGrid.SelectedItems.Cast<AttributeChangeViewModel>().ToList();
            if (selectedItems.Count > 0)
                e.CanExecute = true;
            e.Handled = true;
        }

        private void CompareTwoSelectedExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedItems = diffGrid.SelectedItems.Cast<AttributeChangeViewModel>().ToList();
            XElement firstElement = null;
            XElement secondElement = null;
            if (selectedItems.Count > 1)
            {
                var first = selectedItems[0];
                var second = selectedItems[1];
                if (ViewModel.Xml1Dictionary.TryGetValue(first.ID, out var firstElement1))
                {
                    firstElement = firstElement1;
                    if (ViewModel.Xml2Dictionary.TryGetValue(second.ID, out var secondElement1))
                    {
                        secondElement = secondElement1;
                    }
                }
                else if (ViewModel.Xml2Dictionary.TryGetValue(first.ID, out var firstElement2))
                {
                    firstElement = firstElement2;
                    if (ViewModel.Xml1Dictionary.TryGetValue(second.ID, out var secondElement2))
                    {
                        secondElement = secondElement2;
                    }
                }
                // For Element based compare MBIndexFrom is name of xml parent node (ParameterItems/LiveParameters)
                else if (ViewModel.Xml1Dictionary.TryGetValue(first.MBIndexFrom + first.ID, out var firstElement3))
                {
                    firstElement = firstElement3;
                    if (ViewModel.Xml2Dictionary.TryGetValue(second.MBIndexTo + second.ID, out var secondElement3))
                    {
                        secondElement = secondElement3;
                    }
                }
                // For Element based compare MBIndexFrom is name of xml parent node (ParameterItems/LiveParameters)
                else if (ViewModel.Xml2Dictionary.TryGetValue(first.MBIndexFrom + first.ID, out var firstElement4))
                {
                    firstElement = firstElement4;
                    if (ViewModel.Xml1Dictionary.TryGetValue(second.MBIndexTo + second.ID, out var secondElement4))
                    {
                        secondElement = secondElement4;
                    }
                }
                MessageBox.Show($"{firstElement?.ToString()}\r\n" +
                                $"-------------------\r\n" +
                                $"{secondElement?.ToString()}", "Compare");
            }
            else if (selectedItems.Count == 1)
            {
                var first = selectedItems[0];
                if (ViewModel.Xml1Dictionary.TryGetValue(first.ID, out var firstElement1))
                {
                    firstElement = firstElement1;
                }
                else if (ViewModel.Xml1Dictionary.TryGetValue(first.MBIndexFrom + first.ID, out var firstElement2))
                {
                    firstElement = firstElement2;
                }

                if (ViewModel.Xml2Dictionary.TryGetValue(first.ID, out var secondElement1))
                {
                    secondElement = secondElement1;
                }
                else if(ViewModel.Xml2Dictionary.TryGetValue(first.MBIndexTo + first.ID, out var secondElement2))
                {
                    secondElement = secondElement2;
                }
                MessageBox.Show($"{firstElement?.ToString()}\r\n" +
                                $"-------------------\r\n" +
                                $"{secondElement?.ToString()}", "Compare");
            }
            e.Handled = true;
        }
    }
}
