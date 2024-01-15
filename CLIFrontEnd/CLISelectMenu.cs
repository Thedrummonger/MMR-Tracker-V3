using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIFrontEnd
{
    public class CLISelectMenu(IEnumerable<object> Source, IEnumerable<string>? Headers = null)
    {
        public object SelectedObject;
        public int selectedLineIndex = 0;
        public string[] HeaderList = Headers?.ToArray()??[];

        object[] items = Source.ToArray();
        int previousLineIndex = -1;
        public object Run()
        {
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

            } while (pressedKey != ConsoleKey.Enter);
            SelectedObject = items[selectedLineIndex];
            return SelectedObject;
        }
        void UpdateMenu(int index)
        {
            Console.Clear();
            foreach (var header in HeaderList) { Console.WriteLine(header); }
            foreach (var city in items)
            {
                bool isSelected = city == items[index];
                //ChangeLineColor(isSelected);
                Console.WriteLine($"{(isSelected ? "> " : "  ")}{city}");
            }
        }

        void ChangeLineColor(bool shouldHighlight)
        {
            Console.BackgroundColor = shouldHighlight ? ConsoleColor.White : ConsoleColor.Black;
            Console.ForegroundColor = shouldHighlight ? ConsoleColor.Black : ConsoleColor.White;
        }
    }
}
