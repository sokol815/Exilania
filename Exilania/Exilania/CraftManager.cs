using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exilania
{

    public class Output_Type
    {
        public ItemType type;
        public int item_id;
        public int item_quantity;

        public Output_Type()
        {

        }

        public Output_Type(ItemType t, int id, int quant)
        {
            type = t;
            item_id = id;
            item_quantity = quant;
        }
    }
    public class CraftRecipe
    {
        public List<KeyValuePair<string, int>> input;
        public string name;
        public List<Output_Type> output;
        public int crafting_id;
        /// <summary>
        /// anything in here is a furniture piece that must be within 120 px (10 feet)
        /// </summary>
        public string[] furniture_require;
        public int complexity;

        public CraftRecipe()
        {
            name = "";
            input = new List<KeyValuePair<string, int>>();
            output = new List<Output_Type>();
            crafting_id = -1;
            furniture_require = new string[0];
            complexity = 0;
        }

        public override string ToString()
        {
            string tp = "";
            for (int x = 0; x < input.Count; x++)
            {
                if (x == 0)
                    tp = input[x].Value + " " + input[x].Key;
                else 
                    tp += ", " + input[x].Value + " " + input[x].Key;
            }
            return tp;
        }
    }

    public class CraftManager
    {
        public List<CraftRecipe> recipes;

        public CraftManager()
        {
            recipes = new List<CraftRecipe>();
            if (System.IO.File.Exists(@"craft_recipes.txt"))
            {
                System.IO.StreamReader r = new System.IO.StreamReader(@"craft_recipes.txt");
                string line = "";
                CraftRecipe p = new CraftRecipe();
                bool cont = true;
                while (cont)
                {
                    line = r.ReadLine().Trim();
                    if (line.Length == 0 || line[0] == '#')
                    {
                        //skip this line
                    }
                    else
                    {
                        string[] items = line.Split(':');
                        switch (items[0].ToLower())
                        {
                            case "name":
                                if (p.name == "")
                                {
                                    p.name = items[1].Trim();
                                }
                                else
                                {
                                    p.crafting_id = (ushort)recipes.Count;
                                    recipes.Add(p);
                                    Exilania.text_stream.WriteLine("Crafting Recipe '" + p.name + "' Loaded.");
                                    p = new CraftRecipe();
                                    p.name = items[1].Trim();
                                }
                                break;
                            case "output":
                                items = items[1].Split(',');
                                for (int i = 0; i < items.Length; i++)
                                {
                                   
                                    item_descriptor t = Acc.get_item_by_name(Acc.script_remove_content_of_outer_parenthesis(items[i]));
                                    Output_Type pr = new Output_Type(t.item_type, t.item_id, int.Parse(Acc.script_remove_outer_parentheses(items[i])));
                                    p.output.Add(pr);
                                }
                                break;
                            case "input":
                                items = items[1].Split(',');
                                for (int x = 0; x < items.Length; x++)
                                {
                                    p.input.Add(new KeyValuePair<string, int>(Acc.script_remove_content_of_outer_parenthesis(items[x]), int.Parse(Acc.script_remove_outer_parentheses(items[x]))));
                                }
                                break;
                            case "complexity":
                                p.complexity = int.Parse(items[1]);
                                break;
                            case "furniture-require":
                                items = items[1].Split(',');
                                p.furniture_require = new string[items.Length];
                                for (int x = 0; x < items.Length; x++)
                                {
                                    p.furniture_require[x] = items[x];
                                }
                                break;
                            default:
                                Exilania.text_stream.WriteLine("UNHANDLED type " + items[0]);
                                break;
                        }
                    }
                    if (r.EndOfStream)
                    {
                        p.crafting_id = (ushort)recipes.Count;
                        recipes.Add(p);
                        Exilania.text_stream.WriteLine("Crafting Recipe '" + p.name + "' Loaded.");
                        cont = false;
                    }
                }
                r.Close();
                for (int x = 0; x < Exilania.item_manager.item_pieces.Count; x++)
                {
                    p = new CraftRecipe();
                    p.name = Exilania.item_manager.item_pieces[x].name;
                    p.output.Add(new Output_Type(ItemType.ItemPiece, x, 1));
                    if (Exilania.item_manager.item_pieces[x].craft_require != "")
                    {
                        p.furniture_require = Exilania.item_manager.item_pieces[x].craft_require.Split(',');
                    }
                    else
                        p.furniture_require = new string[0];
                    if (Exilania.item_manager.item_pieces[x].materials.Count > 0)
                    {
                        foreach (var mats in Exilania.item_manager.item_pieces[x].materials)
                        {
                            p.input.Add(new KeyValuePair<string, int>(mats.Key, mats.Value));
                        }
                    }
                    p.complexity = Exilania.item_manager.item_pieces[x].complexity;
                    p.crafting_id = (ushort)recipes.Count;
                    recipes.Add(p);
                    Exilania.text_stream.WriteLine("Crafting Recipe '" + p.name + "' Loaded.");
                }
                for (int x = 0; x < Exilania.furniture_manager.furniture.Count; x++)
                {
                    p = new CraftRecipe();
                    p.name = Exilania.furniture_manager.furniture[x].name;
                    p.output.Add(new Output_Type(ItemType.Furniture, x, 1));
                    if (Exilania.furniture_manager.furniture[x].craft_require.Length > 0)
                    {
                        p.furniture_require = Exilania.furniture_manager.furniture[x].craft_require;
                    }
                    if (Exilania.furniture_manager.furniture[x].materials.Count > 0)
                    {
                        foreach (var mats in Exilania.furniture_manager.furniture[x].materials)
                        {
                            p.input.Add(new KeyValuePair<string, int>(mats.Key, mats.Value));
                        }
                    }
                    p.complexity = Exilania.furniture_manager.furniture[x].complexity;
                    p.crafting_id = (ushort)recipes.Count;
                    recipes.Add(p);
                    Exilania.text_stream.WriteLine("Crafting Recipe '" + p.name + "' Loaded.");
                }
            }
            else
            {
                Exilania.text_stream.Write("ERROR! No craft_recipes.txt file.");
            }
        }
    }
}
