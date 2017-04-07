using System;
using System.IO;
using System.Threading;

class GameCore
{
    // game "textures"
    char block1 = '╝', block2 = '╔', block3 = '║', block4 = '╗', block5 = '╚', block6 = '═';
    char player = '☺';
    char tree1 = '♠';
    char grass = '▓';
    char stone = '█';

    // player coordinates and variables for map scrolling
    int x, y, z1, z2, z3, z4;

    // map
    int mapSize = 200;
    char[,] map = new char[200, 200];
    
    // for random variables
    Random rand = new Random((int)DateTime.Now.Ticks);

    // colors of "textures"
    ConsoleColor block_color = ConsoleColor.Blue;
    ConsoleColor player_color = ConsoleColor.White;
    ConsoleColor stone_color = ConsoleColor.Gray;
    ConsoleColor tree1_color = ConsoleColor.Green;
    ConsoleColor grass_color = ConsoleColor.DarkGreen;

    // what time is it
    int time = 0;

    // day or night
    bool itsNight = false;

    // current action (e.g. movement)
    int mode = 1;

    // what I'm collecting
    int b_mode = 1;
    string b_mode_t = "Wood";

    // items
    int wood_c = 0, stone_c = 0;

    // poor-looking global variable for filename storing
    string saveName_map = null, saveName_vars = null;

    // checking folders and mods
    public void start()
    {
        Console.Title = "2DMinecraft";
        if (!Directory.Exists("save"))
            Directory.CreateDirectory("save");
        if (!Directory.Exists(@"save\map"))
            Directory.CreateDirectory(@"save\map");
        if (!Directory.Exists(@"save\var"))
            Directory.CreateDirectory(@"save\var");

        menu();
    }

    // color changing
    void color(ConsoleColor fgcolor = ConsoleColor.White, ConsoleColor bgcolor = ConsoleColor.Black)
    {
        Console.ForegroundColor = fgcolor;
        Console.BackgroundColor = bgcolor;
    }

    // main menu
    void menu()
    {
        Console.Clear();

        color(ConsoleColor.Yellow);
        Console.WriteLine("╔═════════════╗");
        Console.WriteLine("║ 2DMinecraft ║");
        Console.WriteLine("╚═════════════╝");
        Console.WriteLine();
        color(ConsoleColor.White);
        Console.WriteLine("1. Play");
        Console.WriteLine("2. Quit");

        var c = new ConsoleKeyInfo();
        c = Console.ReadKey();
        if (c.Key == ConsoleKey.D1) checkSave();
        else if (c.Key == ConsoleKey.D2) Environment.Exit(0);
        else menu();
    }

    // checking saved maps
    void checkSave()
    {
        Console.Clear();
        string[] saves = Directory.GetFiles(@"save\map\", "*.txt", SearchOption.TopDirectoryOnly);
        if (saves.Length <= 0)
        {
            mapGenerator();
        }
        else
        {
            color(ConsoleColor.Yellow);
            Console.WriteLine("╔═════════════╗");
            Console.WriteLine("║ 2DMinecraft ║");
            Console.WriteLine("╚═════════════╝");
            Console.WriteLine();
            color(ConsoleColor.White);
            Console.WriteLine("Enter your map name or \"new\" for a new map:");
            Console.WriteLine();
            for (int i = 0; i <= saves.Length - 1; i++)
            {
                Console.WriteLine(saves[i].Replace(@"save\map\", "").Replace(".txt", ""));
            }

            var c = Console.ReadLine();

            if (c.ToLower() == "new") mapGenerator();
            else if (File.Exists(@"save\map\" + c.ToLower() + ".txt")) mapGenerator(true, c);
            else checkSave();
        }
    }

    // map generator
    void mapGenerator(bool openfile = false, string filename = null)
    {
        Console.Clear();
        if (!openfile) // creating a new map
        {
            x = mapSize / 2;
            y = mapSize / 2;
            z1 = y - 8;
            z2 = x - 8;
            z3 = y + 8;
            z4 = x + 8;

            for (int i = 0; i <= mapSize - 1; i++)
            {
                for (int j = 0; j <= mapSize - 1; j++)
                {
                    var t = rand.Next(0, 20);
                    if (t == 1) map[i, j] = grass;
                    else if (t == 2) map[i, j] = tree1;
                    else if (t == 4) map[i, j] = stone;
                    else map[i, j] = grass;
                }
            }

            for (int k = 0; k <= mapSize - 1; k++)
            {
                map[k, 0] = block3;
                map[0, k] = block6;
                map[mapSize - 1, k] = block6;
                map[k, mapSize - 1] = block3;
            }

            map[mapSize - 1, 0] = block5;
            map[0, mapSize - 1] = block4;
            map[mapSize - 1, mapSize - 1] = block1;
            map[0, 0] = block2;

            int randValue = rand.Next(1000, 9000);
            saveName_map = @"save\map\map" + randValue + ".txt";
            saveName_vars = @"save\var\var" + randValue + ".txt";
        }
        else // loading map
        {
            saveName_map = @"save\map\" + filename + ".txt";
            saveName_vars = @"save\var\" + filename.Replace("map", "var") + ".txt";

            string[] mapVars = File.ReadAllLines(saveName_vars);

            x = Convert.ToInt32(mapVars[0]);
            y = Convert.ToInt32(mapVars[1]);
            z1 = Convert.ToInt32(mapVars[2]);
            z2 = Convert.ToInt32(mapVars[3]);
            z3 = Convert.ToInt32(mapVars[4]);
            z4 = Convert.ToInt32(mapVars[5]);
            time = Convert.ToInt32(mapVars[6]);
            mode = Convert.ToInt32(mapVars[7]);
            b_mode = Convert.ToInt32(mapVars[8]);
            b_mode_t = mapVars[9];
            wood_c = Convert.ToInt32(mapVars[10]);
            stone_c = Convert.ToInt32(mapVars[11]);
            itsNight = Convert.ToBoolean(mapVars[12]);

            if (itsNight)
            {
                player_color = ConsoleColor.Gray;
                tree1_color = ConsoleColor.DarkGray;
                stone_color = ConsoleColor.DarkGray;
                grass_color = ConsoleColor.Black;
            }
            else
            {
                player_color = ConsoleColor.White;
                tree1_color = ConsoleColor.Green;
                stone_color = ConsoleColor.Gray;
                grass_color = ConsoleColor.DarkGreen;
            }

            string[] mapContent = File.ReadAllLines(saveName_map);

            for (int i = 0; i <= mapContent.Length - 1; i++)
            {
                for (int j = 0; j <= mapContent[i].Length - 1; j++)
                {
                    map[i, j] = Convert.ToChar(mapContent[i][j]);
                }
            }
        }

        map[x, y] = player;
        characterController();
    }

    // collision
    private bool characterCollision(int a = 0, int b = 0) {
        if (map[y + a, x + b] != grass) return false; else return true;
    }

    // showing the map
    void show()
    {
        Console.SetCursorPosition(0, 0); // it's better than Console.Clear();
        for (int i = z1; i <= z3; i++)
        {
            for (int j = z2; j <= z4; j++)
            {
                if (map[i, j] == block1 || map[i, j] == block2 || map[i, j] == block3 || map[i, j] == block4 || map[i, j] == block5 || map[i, j] == block6) color(block_color);
                else if (map[i, j] == stone) color(stone_color);
                else if (map[i, j] == tree1) color(tree1_color);
                else if (map[i, j] == player) color(player_color);
                else if (map[i, j] == grass) color(grass_color);
                Console.Write(map[i, j]);
            }
            Console.WriteLine();
        }
        color(ConsoleColor.White);
        Console.WriteLine();
        Console.WriteLine("Wood = {0}, Stone = {1}", wood_c, stone_c);
        Console.WriteLine("Mode = {0}, >> {1} <<", mode, b_mode_t);
    }

    // saving the map
    private void save()
    {
        color(ConsoleColor.Yellow);
        Console.Clear();
        Console.WriteLine("Loading...");

        if (File.Exists(saveName_map)) File.Delete(saveName_map);
        if (File.Exists(saveName_vars)) File.Delete(saveName_vars);

        using (var f = new StreamWriter(saveName_vars))
        {
            f.WriteLine(x);
            f.WriteLine(y);
            f.WriteLine(z1);
            f.WriteLine(z2);
            f.WriteLine(z3);
            f.WriteLine(z4);
            f.WriteLine(time);
            f.WriteLine(mode);
            f.WriteLine(b_mode);
            f.WriteLine(b_mode_t);
            f.WriteLine(wood_c);
            f.WriteLine(stone_c);
            f.WriteLine(itsNight);
        }

        player = grass; // to avoid player duplication problem

        using (var f = new StreamWriter(saveName_map))
        {
            for (int i = 0; i <= mapSize - 1; i++)
            {
                for (int j = 0; j <= mapSize - 2; j++)
                {
                    f.Write(map[i, j]);
                }
                f.WriteLine(map[i, mapSize - 1]);
                Console.Clear();
                Console.WriteLine("Loading... {0}", i);
            }
        }
    }

    // player movement and other actions
    void characterController()
    {
        show();
        Thread.Sleep(60);
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var c = new ConsoleKeyInfo();
                c = Console.ReadKey();

                if (c.Key == ConsoleKey.Escape) { save(); Environment.Exit(0); } // save and exit
                else if (c.Key == ConsoleKey.Q) // change mode
                {
                    if (mode == 3) mode = 1;
                    else mode++;
                }
                else if (c.Key == ConsoleKey.E) // change item
                {
                    if (b_mode == 2) { b_mode = 1; b_mode_t = "Wood"; }
                    else { b_mode = 2; b_mode_t = "Stone"; }
                }

                if (c.KeyChar == 'w' || c.KeyChar == 'W')
                {
                    if (mode == 1)
                    {
                        if (characterCollision(-1))
                        {

                            map[y, x] = grass;
                            y--;
                            if (y - 8 >= 0 && y + 8 <= mapSize - 1) { z1 = y - 8; z3 = y + 8; }
                            map[y, x] = player;
                        }
                    }
                    else if (mode == 2)
                    {
                        if (map[y - 1, x] == tree1)
                        {
                            map[y - 1, x] = grass;
                            wood_c++;
                        }
                        else if (map[y - 1, x] == stone)
                        {
                            map[y - 1, x] = grass;
                            stone_c++;
                        }
                    }
                    else if (mode == 3)
                    {
                        if (map[y - 1, x] == grass)
                        {
                            if (b_mode == 1)
                            {
                                if (wood_c > 0)
                                {
                                    map[y - 1, x] = tree1;
                                    wood_c--;
                                }
                            }
                            else if (b_mode == 2)
                            {

                                if (stone_c > 0)
                                {
                                    map[y - 1, x] = stone;
                                    stone_c--;
                                }
                            }
                        }
                    }
                }
                else if (c.KeyChar == 's' || c.KeyChar == 'S')
                {
                    if (mode == 1)
                    {
                        if (characterCollision(1))
                        {
                            map[y, x] = grass;
                            y++;
                            if (y - 8 >= 0 && y + 8 <= mapSize - 1) { z1 = y - 8; z3 = y + 8; }
                            map[y, x] = player;
                        }
                    }
                    else if (mode == 2)
                    {
                        if (map[y + 1, x] == tree1)
                        {
                            map[y + 1, x] = grass;
                            wood_c++;
                        }
                        else if (map[y + 1, x] == stone)
                        {
                            map[y + 1, x] = grass;
                            stone_c++;
                        }
                    }
                    else if (mode == 3)
                    {
                        if (map[y + 1, x] == grass)
                        {
                            if (b_mode == 1)
                            {
                                if (wood_c > 0)
                                {
                                    map[y + 1, x] = tree1;
                                    wood_c--;
                                }
                            }
                            else if (b_mode == 2)
                            {

                                if (stone_c > 0)
                                {
                                    map[y + 1, x] = stone;
                                    stone_c--;
                                }
                            }
                        }
                    }
                }
                else if (c.KeyChar == 'a' || c.KeyChar == 'A')
                {
                    if (mode == 1)
                    {
                        if (characterCollision(0, -1))
                        {
                            map[y, x] = grass;
                            x--;
                            if (x - 8 >= 0 && x + 8 <= mapSize - 1) { z2 = x - 8; z4 = x + 8; }
                            map[y, x] = player;
                        }
                    }
                    else if (mode == 2)
                    {
                        if (map[y, x - 1] == tree1)
                        {
                            map[y, x - 1] = grass;
                            wood_c++;
                        }
                        else if (map[y, x - 1] == stone)
                        {
                            map[y, x - 1] = grass;
                            stone_c++;
                        }
                    }
                    else if (mode == 3)
                    {
                        if (map[y, x - 1] == grass)
                        {
                            if (b_mode == 1)
                            {
                                if (wood_c > 0)
                                {
                                    map[y, x - 1] = tree1;
                                    wood_c--;
                                }
                            }
                            else if (b_mode == 2)
                            {

                                if (stone_c > 0)
                                {
                                    map[y, x - 1] = stone;
                                    stone_c--;
                                }
                            }
                        }
                    }
                }
                else if (c.KeyChar == 'd' || c.KeyChar == 'D')
                {
                    if (mode == 1)
                    {
                        if (characterCollision(0, 1))
                        {
                            map[y, x] = grass;
                            x++;
                            if (x - 8 >= 0 && x + 8 <= mapSize - 1) { z2 = x - 8; z4 = x + 8; }
                            map[y, x] = player;
                        }
                    }
                    else if (mode == 2)
                    {
                        if (map[y, x + 1] == tree1)
                        {
                            map[y, x + 1] = grass;
                            wood_c++;
                        }
                        else if (map[y, x + 1] == stone)
                        {
                            map[y, x + 1] = grass;
                            stone_c++;
                        }
                    }
                    else if (mode == 3)
                    {
                        if (map[y, x + 1] == grass)
                        {
                            if (b_mode == 1)
                            {
                                if (wood_c > 0)
                                {
                                    map[y, x + 1] = tree1;
                                    wood_c--;
                                }
                            }
                            else if (b_mode == 2)
                            {

                                if (stone_c > 0)
                                {
                                    map[y, x + 1] = stone;
                                    stone_c--;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                time++;
                if (time == 100) // night
                {
                    player_color = ConsoleColor.Gray;
                    tree1_color = ConsoleColor.DarkGray;
                    stone_color = ConsoleColor.DarkGray;
                    grass_color = ConsoleColor.Black;
                    itsNight = true;
                }
                else if (time == 200) // day
                {
                    player_color = ConsoleColor.White;
                    tree1_color = ConsoleColor.Green;
                    stone_color = ConsoleColor.Gray;
                    grass_color = ConsoleColor.DarkGreen;
                    time = 0;
                    itsNight = false;
                }
            }
            characterController();
        }
    }
}