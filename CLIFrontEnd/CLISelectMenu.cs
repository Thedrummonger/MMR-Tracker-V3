using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIFrontEnd
{
    public class CLISelectMenu(IEnumerable<object> Source)
    {
        public object SelectedObject;
        public int selectedLineIndex = 0;
        public string[] HeaderList;

        object[] items = Source.ToArray();
        public object Run(IEnumerable<string>? Headers = null)
        {
            int previousLineIndex = -1;
            HeaderList = Headers?.ToArray() ?? [];
            ConsoleKey pressedKey;
            do
            {
                if (previousLineIndex != selectedLineIndex)
                {
                    UpdateMenu(selectedLineIndex);
                    previousLineIndex = selectedLineIndex;
                }
                pressedKey = Console.ReadKey().Key;

                if (pressedKey == ConsoleKey.DownArrow && selectedLineIndex + 1 < items.Length)
                    selectedLineIndex++;

                else if (pressedKey == ConsoleKey.UpArrow && selectedLineIndex - 1 >= 0)
                    selectedLineIndex--;

            } 
            while (pressedKey != ConsoleKey.Enter);
            SelectedObject = items[selectedLineIndex];
            return SelectedObject;
        }
        void UpdateMenu(int index)
        {
            Console.Clear();
            foreach (var header in HeaderList) { Console.WriteLine(header); }
            foreach (var i in items)
            {
                bool isSelected = i == items[index];
                //ChangeLineColor(isSelected); //For some reason this changes the entire console background to white sometimes
                Console.WriteLine($"{(isSelected ? "> " : "  ")}{i}");
            }
        }

        void ChangeLineColor(bool shouldHighlight)
        {
            Console.BackgroundColor = shouldHighlight ? ConsoleColor.White : ConsoleColor.Black;
            Console.ForegroundColor = shouldHighlight ? ConsoleColor.Black : ConsoleColor.White;
        }
    }
}
