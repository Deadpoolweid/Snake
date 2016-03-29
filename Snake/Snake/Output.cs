﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Output
    {
        static public void DrawMap()
        {
            // Рисование игрового поля
            for (int i = 0; i < Data.MapSize.y; i++)
            {
                for (int j = 0; j < Data.MapSize.x; j++)
                {
                    Console.SetCursorPosition(j, i);
                    Console.Write('-');
                }
            }
        }

        static public void DrawPlayer()
        {
            // Рисование змеи
            SnakeCell cell = new SnakeCell(Data.snake.Tail);
            while (cell!=null)
            {
                if (cell == Data.snake.Head)
                {
                    Console.Write('©');
                }
                else
                {
                    Console.Write('*');
                }
                cell = cell.Next;
            }
        }

        static public void RedrawMapcell(Coord cell)
        {
            if (cell == null)
            {
                return;
            }
            Console.SetCursorPosition(cell.x,cell.y);
            Console.Write('-');
        }

        static public void DrawFood()
        {
            Console.SetCursorPosition(Data.Food.x,Data.Food.y);
            Console.Write('+');
        }
    }
}
