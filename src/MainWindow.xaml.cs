﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace nJumpList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<CustomJumpItem> listeCustomJumpItem = new List<CustomJumpItem>();
        string ExecutName = String.Empty;
        string JLIFile = String.Empty;

        public MainWindow()
        {
            listeCustomJumpItem = new List<CustomJumpItem>();
            ExecutName = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
            JLIFile = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ExecutName + ".jli");

            InitializeComponent();

            if (ExecutName != "JumpList")
                this.Title = String.Format("[JumpList] {0}", ExecutName);

            ReadFile();
            RefreshList();
        }

        private void MAJJumpList()
        {
            JumpList jl = JumpList.GetJumpList(App.Current);
            jl.JumpItems.Clear();

            foreach (CustomJumpItem item in listeCustomJumpItem)
            {
                JumpTask ji = new JumpTask();
                ji.CustomCategory = item.Categorie;
                ji.ApplicationPath = item.Chemin;
                ji.IconResourcePath = item.Icone;
                ji.Arguments = item.Arguments;
                ji.Title = item.Titre;
                ji.Description = item.Description;

                jl.JumpItems.Add(ji);
            }

            jl.Apply();
        }
        private void RefreshList()
        {
            list_Tasks.SelectedIndex = -1;
            ClearFields();
            gb_Task.IsEnabled = false;

            listeCustomJumpItem = listeCustomJumpItem.OrderBy(c => c.Affichage).ToList();

            MAJJumpList();

            list_Tasks.Items.Clear();
            foreach (CustomJumpItem item in listeCustomJumpItem)
                list_Tasks.Items.Add(item.Affichage);

            WriteFile();
        }
        private bool TestFields()
        {
            if (String.IsNullOrWhiteSpace(txt_Categ.Text))
            {
                MessageBox.Show("The category must be filled!");
                return false;
            }
            if (String.IsNullOrWhiteSpace(txt_Chemin.Text))
            {
                MessageBox.Show("The path must be filled!");
                return false;
            }
            if (String.IsNullOrWhiteSpace(txt_Nom.Text))
            {
                MessageBox.Show("The title must be filled!");
                return false;
            }
            return true;
        }
        private void AddTask()
        {
            if (!TestFields()) return;
                        
            CustomJumpItem ji = GetSelJumpItem();
            if (ji != null) listeCustomJumpItem.Remove(ji);

            CustomJumpItem item = new CustomJumpItem();
            item.Categorie = txt_Categ.Text;
            item.Chemin = txt_Chemin.Text;
            item.Icone = txt_Icone.Text;
            item.Arguments = txt_Arg.Text;
            item.Titre = txt_Nom.Text;
            item.Description = txt_Description.Text;

            listeCustomJumpItem.Add(item);

            RefreshList();
        }
        private void SupprTask()
        {
            CustomJumpItem ji = GetSelJumpItem();
            if (ji != null)
            {
                listeCustomJumpItem.Remove(ji);
                RefreshList();
            }
        }
        private void ClearFields()
        {
            txt_Categ.Text = String.Empty;
            txt_Chemin.Text = String.Empty;
            txt_Arg.Text = String.Empty;
            txt_Icone.Text = String.Empty;
            txt_Nom.Text = String.Empty;
            txt_Description.Text = String.Empty;
        }
        private CustomJumpItem GetSelJumpItem()
        {
            CustomJumpItem ji = null;

            if (list_Tasks.SelectedIndex != -1)
            {
                string selItem = (list_Tasks.SelectedItem as string);
                ji = listeCustomJumpItem.First(i => i.Affichage == selItem);
            }

            return ji;
        }

        private void ReadFile()
        {
            try
            {
                if (File.Exists(JLIFile))
                {
                    listeCustomJumpItem.Clear();
                    Stream stream = File.OpenRead(JLIFile);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    listeCustomJumpItem = (List<CustomJumpItem>)deserializer.Deserialize(stream);
                    stream.Close();
                }
                else
                    WriteDefaultFile();
            }
            catch { }
        }
        private void WriteDefaultFile()
        {
            try
            {
                if (!File.Exists(JLIFile))
                {
                    CustomJumpItem j1 = new CustomJumpItem();
                    j1.Categorie = "File";
                    j1.Chemin = @"C:\Windows\notepad.exe";
                    j1.Icone = @"C:\Windows\notepad.exe";
                    j1.Titre = "Notepad";
                    j1.Description = "Open notepad.";

                    CustomJumpItem j2 = new CustomJumpItem();
                    j2.Categorie = "Folder";
                    j2.Chemin = @"C:\Windows\explorer.exe";
                    j2.Icone = @"C:\Windows\explorer.exe";
                    j2.Arguments = @"/root,c:\";
                    j2.Titre = "Opens 'C:'";
                    j2.Description = "Open the explorer by pointing directly at C :";

                    listeCustomJumpItem.Clear();
                    listeCustomJumpItem.Add(j1);
                    listeCustomJumpItem.Add(j2);

                    WriteFile();
                }
            }
            catch { }
        }
        private void WriteFile()
        {
            try
            {
                Stream stream = File.Create(JLIFile);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(stream, listeCustomJumpItem);
                stream.Close();
            }
            catch { }
        }

        private void list_Tasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_Modifier.IsEnabled = !(list_Tasks.SelectedIndex == -1);
            btn_Supprimer.IsEnabled = !(list_Tasks.SelectedIndex == -1);

            CustomJumpItem ji = GetSelJumpItem();

            if (ji != null)
            {
                txt_Categ.Text = ji.Categorie;
                txt_Chemin.Text = ji.Chemin;
                txt_Nom.Text = ji.Titre;
                txt_Arg.Text = ji.Arguments;
                txt_Icone.Text = ji.Icone;
                txt_Description.Text = ji.Description;
            }
        }
        private void list_Tasks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (list_Tasks.SelectedIndex != -1)
                gb_Task.IsEnabled = true;
        }
        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            listeCustomJumpItem.Clear();
            RefreshList();
        }
        private void btn_Ajout_Click(object sender, RoutedEventArgs e)
        {
            list_Tasks.SelectedIndex = -1;
            ClearFields();
            gb_Task.IsEnabled = true;
        }
        private void btn_Modifier_Click(object sender, RoutedEventArgs e)
        {
            gb_Task.IsEnabled = true;
        }
        private void btn_Supprimer_Click(object sender, RoutedEventArgs e)
        {
            SupprTask();
            RefreshList();
        }
                
        private void txt_Chemin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txt_Categ.IsEnabled = !String.IsNullOrWhiteSpace(txt_Chemin.Text);
            txt_Chemin.ToolTip = txt_Chemin.Text;
            txt_Arg.IsEnabled = !String.IsNullOrWhiteSpace(txt_Chemin.Text);
            txt_Icone.IsEnabled = !String.IsNullOrWhiteSpace(txt_Chemin.Text);
            btn_Icone.IsEnabled = !String.IsNullOrWhiteSpace(txt_Chemin.Text);
            txt_Icone.Text = txt_Chemin.Text;
            txt_Nom.IsEnabled = !String.IsNullOrWhiteSpace(txt_Chemin.Text);
            txt_Description.IsEnabled = !String.IsNullOrWhiteSpace(txt_Chemin.Text);
            btn_OK.IsEnabled = !String.IsNullOrWhiteSpace(txt_Chemin.Text);
        }
        private void btn_Chemin_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            Nullable<bool> resFile = fd.ShowDialog();
            if (resFile == true)
                txt_Chemin.Text = fd.FileName;
        }
        private void btn_Icone_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            Nullable<bool> resFile = fd.ShowDialog();
            if (resFile == true)
                txt_Icone.Text = fd.FileName;
        }
        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            AddTask();
            list_Tasks.SelectedIndex = -1;
        }
        private void btn_Annuler_Click(object sender, RoutedEventArgs e)
        {
            gb_Task.IsEnabled = false;
            RefreshList();
        }

        private void btn_Help_Click(object sender, RoutedEventArgs e)
        {
            string msg =
@"To open a folder, the trick is to define a 'task' with the path to 'explorer.exe' then go to '/ root, FOLDER' where 'FOLDER' is the directory to open.

When you put a file (other than exe), the icon will be the one of the apps not associated (an ugly black and white icon). It is then necessary to select in the 'Icon' field the exe corresponding to the application that will open this file or any other ico file.

The file for saving lists is here:
'" + JLIFile + @"'
If it's all over, you can always delete it and re-open the app to reset the default values.

You can have as many JumpList as you want, for that you have to copy / paste the exe 'JumpList.exe' and rename it.";

            MessageBox.Show(msg, "Help", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void btn_Contribute_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/aaronmaynard";
            string msg =
@"JumpList Launcher is an open source application.  If you would like to contribute to the application, whether you find bugs or would like to suggest enhancements, please contribute to the GitHub page:

" + url + @"

If you like this utility, you can show your support by clicking on the 'Star' button, and click the 'Watch' button to get notifications when we push an update.

This application is free, and always will be.";

            MessageBox.Show(msg, "Contribute on GitHub", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [Serializable]
        public class CustomJumpItem
        {
            public string Categorie { get; set; }
            public string Chemin { get; set; }
            public string Arguments { get; set; }
            public string Icone { get; set; }
            public string Titre { get; set; }
            public string Description { get; set; }

            public string Affichage
            {
                get
                {
                    return String.Format("[{0}] {1}",
                        Categorie,
                        Titre);
                }
            }

            public CustomJumpItem() { }
        }
    }
}
