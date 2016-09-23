﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Snake;

namespace SnakeLevelDesigner
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            rObstacle.IsChecked = true;

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                OpenFile(args[1]);
            }

            // Создание привязки
            CommandBinding bind = new CommandBinding(ApplicationCommands.New);
            // Присоединение обработчика событий
            bind.Executed += CreateLevel_OnClick;
            // Регистрация привязки
            this.CommandBindings.Add(bind);
            bind = new CommandBinding(ApplicationCommands.Open);
            bind.Executed += Open_OnClick;
            CommandBindings.Add(bind);

            bind = new CommandBinding(ApplicationCommands.Save);
            bind.Executed += Save_OnClick;
            CommandBindings.Add(bind);

            bind = new CommandBinding(ApplicationCommands.ContextMenu);
            bind.Executed += ChangeSettings_OnClick;
            CommandBindings.Add(bind);

            bChangeSettings.Content = "Create";
            bChangeSettings.Click -= ChangeSettings_OnClick;
            bChangeSettings.Click += CreateLevel_OnClick;

            Title += " " + Assembly.GetExecutingAssembly().GetName().Version;

            Properties.Settings.Default.LevelSavePath = Environment.CurrentDirectory + @"\Levels";
        }

        private int width, height;

        private bool CanSave = false;

        private string _currentfile = "Test Level";
        private string CurrentFile
        {
            get { return _currentfile; }
            set
            {
                _currentfile = "Level Designer - " + value;
                this.Title = _currentfile;
            }
        }

        public Data data = new Data();

        #region Buttons and commands

        private void CreateLevel_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = new CreatingLevel();
            settings.Owner = this;


            if (settings.ShowDialog() == true)
            {
                CreateMap();
                CanSave = true;
                bChangeSettings.Content = "Change settings";
                bChangeSettings.Click -= CreateLevel_OnClick;
                bChangeSettings.Click += ChangeSettings_OnClick;
            }
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CanSave)
            {
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Level files (*.lvl)|*.lvl";
            sfd.FileName = "Test Level";
            sfd.InitialDirectory = Properties.Settings.Default.LevelSavePath;
            if (sfd.ShowDialog(this) == true)
            {
                var fileName = sfd.FileName;

                var listCells = new List<Cell>();

                foreach (var child in Map.Children)
                {
                    var border = child as Border;
                    var label = border.Child as Label;
                    listCells.Add(label.Tag as Cell);
                }

                var cellsInfo = new CellsInfo(listCells, width, height);

                var direction = data.Direction;

                var audio = data.BackgroundMusic;

                var speed = data.Speed;

                var level = new Level(cellsInfo, data.FinishingScore, direction, audio, speed);

                var bf = new BinaryFormatter();
                using (Stream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    bf.Serialize(fs, level);
                }

                AddFilePathToRecent(sfd.FileName);
                CurrentFile = sfd.SafeFileName.Split('.')[0];

                MessageBox.Show("File was successfully saved!");

            }
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Level files (*.lvl)|*.lvl";
            if (ofd.ShowDialog(this) == true)
            {
                string fileName = ofd.FileName;

                bool succeded = OpenFile(fileName);
                if (!succeded)
                {
                    return;
                }

                AddFilePathToRecent(ofd.FileName);
                CurrentFile = ofd.SafeFileName.Split('.')[0];

                CanSave = true;
                data.IsInitialized = true;
                bChangeSettings.Content = "Change settings";
                bChangeSettings.Click -= CreateLevel_OnClick;
                bChangeSettings.Click += ChangeSettings_OnClick;
            }
        }

        private void ChangeSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = new CreatingLevel();

            Data oldData = this.data;
            settings.Owner = this;
            settings.bCreate.Content = "Change";
            settings.ShowDialog();

            for (int i = 0; i < Map.Rows; i++)
            {
                for (int j = 0; j < Map.Columns; j++)
                {
                    var label = new Label();
                    label.Tag = new Cell(CellType.Empty, j, i);
                    label.Content = (label.Tag as Cell).CellType.ToString();
                    label.ToolTip = label.Tag;
                    ChangeCellColor(label);

                    var border = new Border();
                    border.BorderThickness = new Thickness(2);
                    border.BorderBrush = Brushes.Gray;
                    border.Child = label;
                    Map.Children.Add(border);
                }
            }

            List<Cell> oldCellsListCutted = new List<Cell>();

            if (data.MapWidth < oldData.MapWidth || data.MapHeight < oldData.MapHeight)
            {
                foreach (var MapChild in Map.Children)
                {
                    var border = MapChild as Border;
                    var label = border.Child as Label;
                    var cell = label.Tag as Cell;
                    if (cell.Y < data.MapWidth && cell.X < data.MapHeight)
                    {
                        oldCellsListCutted.Add(cell);
                    }
                }
            }
            else
            {
                foreach (var MapChild in Map.Children)
                {
                    var border = MapChild as Border;
                    var label = border.Child as Label;
                    var cell = label.Tag as Cell;
                    oldCellsListCutted.Add(cell);
                }
            }

            CellsInfo oldCellsInfo = new CellsInfo(oldCellsListCutted, data.MapWidth, data.MapHeight);
            CreateMap(oldCellsInfo);
        }

        private void AddFilePathToRecent(string path)
        {
            if (Properties.Settings.Default.RecentFiles == null)
            {
                Properties.Settings.Default.RecentFiles = new StringCollection();
            }

            var recent = Properties.Settings.Default.RecentFiles;
            if (recent.Count == 10)
            {
                recent.RemoveAt(0);
            }
            recent.Add(path);

            var strings = RecentFiles;
            miRecentFiles.Items.Clear();
            foreach (var recentFile in RecentFiles)
            {
                var menuitem = new MenuItem()
                {
                    Header = recentFile,
                };

                menuitem.Click += (sender, args) =>
                {
                    OpenFile(recentFile);
                };
                miRecentFiles.Items.Add(menuitem);
            }

        }
        #endregion

        public List<string> RecentFiles
        {
            get
            {
                List<string> filesList = new List<string>();
                if (Properties.Settings.Default.RecentFiles != null)
                {
                    foreach (var recentFile in Properties.Settings.Default.RecentFiles)
                    {
                        filesList.Add(recentFile);
                    }
                }

                return filesList;
            }
        }

        #region Level painting

        private void MapOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                CellClicked();
            }
        }

        private void MapOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                CellClicked();
            }
            mouseEventArgs.Handled = true;

        }

        private void CellClicked()
        {
            foreach (var child in Map.Children)
            {
                var border = child as Border;
                var label = border.Child as Label;
                if (label.IsMouseOver)
                {
                    var cell = label.Tag as Cell;

                    if (rEmpty.IsChecked == true)
                    {
                        bool CanPlaceEmptyCell = false;
                        if (Player.Count == 0)
                        {
                            CanPlaceEmptyCell = true;
                        }
                        else
                        {
                            var HeadTailList = new List<Coord>();
                            HeadTailList.Add(Player.First());
                            HeadTailList.Add(Player.Last());

                            CanPlaceEmptyCell = new Coord(cell.X, cell.Y) == Player.First() || new Coord(cell.X, cell.Y) == Player.Last();

                            if (Player.All(coord => coord.X != cell.X && coord.Y != cell.Y))
                            {
                                CanPlaceEmptyCell = true;
                            }
                        }

                        if (CanPlaceEmptyCell)
                        {
                            Player.Remove(new Coord(cell.X, cell.Y));
                            cell.CellType = CellType.Empty;
                        }
                    }
                    else if (rObstacle.IsChecked == true)
                    {
                        cell.CellType = CellType.Brick;
                    }
                    else if (rPlayer.IsChecked == true)
                    {
                        bool CanPlacePlayerCell = false;
                        bool CanPlaceToEnd = true;
                        if (Player.Count == 0)
                        {
                            CanPlacePlayerCell = true;
                        }
                        else
                        {
                            var HeadTailList = new List<Coord>();
                            HeadTailList.Add(Player.First());
                            HeadTailList.Add(Player.Last());

                            var coord = Player.Last();
                            if (coord.X == cell.X)
                            {
                                if (coord.Y + 1 == cell.Y || coord.Y - 1 == cell.Y)
                                {
                                    CanPlacePlayerCell = true;
                                }
                            }
                            if (coord.Y == cell.Y)
                            {
                                if (coord.X + 1 == cell.X || coord.X - 1 == cell.X)
                                {
                                    CanPlacePlayerCell = true;
                                }
                            }

                            coord = Player.First();
                            if (coord.X == cell.X)
                            {
                                if (coord.Y + 1 == cell.Y || coord.Y - 1 == cell.Y)
                                {
                                    CanPlacePlayerCell = true;
                                    CanPlaceToEnd = false;
                                }
                            }
                            if (coord.Y == cell.Y)
                            {
                                if (coord.X + 1 == cell.X || coord.X - 1 == cell.X)
                                {
                                    CanPlacePlayerCell = true;
                                    CanPlaceToEnd = false;
                                }
                            }
                        }

                        if (Player.Any(coord => coord.X == cell.X && coord.Y == cell.Y))
                        {
                            CanPlacePlayerCell = false;
                        }

                        if (CanPlacePlayerCell)
                        {
                            if (CanPlaceToEnd)
                            {
                                Player.Add(new Coord(cell.X, cell.Y));
                            }
                            else
                            {
                                Player.Insert(0,new Coord(cell.X, cell.Y));
                            }
                            cell.CellType = CellType.Player;
                        }
                    }

                    ChangeCellColor(label);

                    label.Content = (label.Tag as Cell).CellType.ToString();
                    label.ToolTip = label.Tag;
                }
            }

            //tbDebug.Visibility = Visibility.Visible;
            //String debugInfo = "";
            //foreach (var coord in Player)
            //{
            //    debugInfo += coord.ToString() +"; ";
            //}
            //tbDebug.Text = debugInfo;
        }

        private void ChangeCellColor(Label label)
        {
            var cell = label.Tag as Cell;
            switch (cell.CellType)
            {
                case CellType.Empty:
                    label.Background = Brushes.WhiteSmoke;
                    break;
                case CellType.Brick:
                    label.Background = Brushes.DarkSlateGray;
                    break;
                case CellType.Player:
                    label.Background = Brushes.YellowGreen;
                    break;
            }
        }

        private List<Coord> Player = new List<Coord>();
        #endregion

        #region Controller
        private void CreateMap()
        {
            width = data.MapWidth;
            height = data.MapHeight;

            Map.Children.RemoveRange(0, Map.Children.Count);

            Map.Rows = height;
            Map.Columns = width;

            for (int i = 0; i < Map.Rows; i++)
            {
                for (int j = 0; j < Map.Columns; j++)
                {
                    var label = new Label();
                    label.Tag = new Cell(CellType.Empty, j, i);
                    label.Content = (label.Tag as Cell).CellType.ToString();
                    label.ToolTip = label.Tag;
                    ChangeCellColor(label);

                    var border = new Border();
                    border.BorderThickness = new Thickness(2);
                    border.BorderBrush = Brushes.Gray;
                    border.Child = label;
                    Map.Children.Add(border);
                }
            }

            Map.MouseMove += MapOnMouseMove;
            Map.MouseDown += MapOnMouseDown;
            Player = new List<Coord>();
        }

        private void CreateMap(CellsInfo cellsInfo)
        {
            if (Equals(cellsInfo, null))
            {
                CreateMap();
                return;
            }

            width = cellsInfo.width;
            height = cellsInfo.height;

            data.MapWidth = width;
            data.MapHeight = height;


            Map.Children.RemoveRange(0, Map.Children.Count);

            Map.Rows = height;
            Map.Columns = width;


            for (int i = 0; i < Map.Rows; i++)
            {
                for (int j = 0; j < Map.Columns; j++)
                {
                    var label = new Label();
                    var cells = cellsInfo.cells;
                    label.Tag = cells.Exists(cell => cell.X == j && cell.Y == i) ? cells.Find(cell => cell.X == j && cell.Y == i) : new Cell(CellType.Empty, j, i);
                    label.Content = (label.Tag as Cell).CellType.ToString();
                    label.ToolTip = label.Tag;
                    ChangeCellColor(label);

                    var border = new Border
                    {
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.Gray,
                        Child = label
                    };
                    Map.Children.Add(border);
                }
            }

            Map.MouseMove += MapOnMouseMove;
            Map.MouseDown += MapOnMouseDown;
        }

        private bool OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("Файл " + path + " - не существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {

                Level level;

                var bf = new BinaryFormatter();
                using (Stream fs = File.OpenRead(path))
                {
                    level = bf.Deserialize(fs) as Level;
                }

                CreateMap(level.MapCellsInfo);

                data.FinishingScore = level.FinishingScore;
                data.Direction = level.Direction;

                data.BackgroundMusic = level.BackgroundMusic;

                data.Speed = level.Speed;

                Player = level.PlayerInitCoords;
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Выбранный файл имеет неверный формат данных.");
                return false;
            }
        }
        #endregion

        // Новые методы

        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void On_Alt1_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rEmpty.IsChecked = true;
        }

        private void On_Alt2_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rObstacle.IsChecked = true;
        }

        private void On_Alt3_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rPlayer.IsChecked = true;
        }
    }

    public class MyCommands
    {
        static MyCommands()
        {
            _Alt1 = new RoutedCommand("_Alt1Command", typeof(MainWindow));

            _Alt2 = new RoutedCommand("_Alt2Command", typeof(MainWindow));

            _Alt3 = new RoutedCommand("_Alt3Command", typeof(MainWindow));
        }

        public static RoutedCommand _Alt1 { get; set; }

        public static RoutedCommand _Alt2 { get; set; }

        public static RoutedCommand _Alt3 { get; set; }


    }
}
