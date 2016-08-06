﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CSharp.RuntimeBinder;

namespace Snake
{
    [Serializable]
    public class Level
    {
        public CellsInfo CellsInfo;

        /// <summary>
        /// Количество очков для прохождения уровня
        /// </summary>
        public int FinishingScore;

        public int MapWidth;
        public int MapHeight;

        public Direction Direction;

        public List<Coord> PlayerInitCoords = new List<Coord>();

        public List<Coord> ObstaclesCoords = new List<Coord>();

        public Level(CellsInfo cellsInfo, int finishingScore, Direction direction)
        {
            CellsInfo = cellsInfo;
            FinishingScore = finishingScore;

            foreach (var cell in cellsInfo.cells)
            {
                switch (cell.CellType)
                {
                    case CellType.Player:
                        PlayerInitCoords.Add(new Coord(cell.X, cell.Y));
                        break;
                    case CellType.Brick:
                        ObstaclesCoords.Add(new Coord(cell.X, cell.Y));
                        break;
                }
            }

            MapWidth = cellsInfo.width;
            MapHeight = cellsInfo.height;

            Direction = direction;
        }

        public static Level GetLevelFromFile(string path)
        {
            var bf = new BinaryFormatter();
            Level level = null;
            using (Stream s = File.OpenRead(path))
            {
                try
                {
                    level = bf.Deserialize(s) as Level;

                }
                catch
                {
                    MessageBox.Show("Ошибка чтения файла.");
                    return null;
                }
            }
            return level;
        }
    }

    public enum CellType
    {
        Empty,
        Brick,
        Player
    }

    [Serializable]
    public class Cell
    {
        public CellType CellType;

        public int X;
        public int Y;

        public Cell(CellType cellType, int x, int y)
        {
            this.CellType = cellType;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X};{Y};{CellType}";
        }
    }

    [Serializable]
    public class CellsInfo
    {
        public List<Cell> cells;

        public int width;
        public int height;

        public CellsInfo(List<Cell> cells, int width, int height)
        {
            this.cells = cells;
            this.width = width;
            this.height = height;
        }
    }
}