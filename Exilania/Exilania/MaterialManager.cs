using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public class MaterialData
    {
        public string name;
        public string description;
        public int image;
        public int worth;
        public ushort material_id;
        public string use_action;

        public MaterialData()
        {
            name = "";
            description = "";
            use_action = "";
        }


        public bool apply_use_action(World w, Point world_loc)
        {
            if(use_action=="")
                return false;

            string command = Acc.script_remove_content_of_outer_parenthesis(use_action);
            string passed = Acc.script_remove_outer_parentheses(use_action).ToLower();
            switch (command.Trim().ToLower())
            {
                case "plant":
                    if (Exilania.plant_manager.spawn_plant_in_world(w, world_loc, passed))
                    {
                        if (!Exilania.game_server)
                        {
                            Exilania.network_client.send_place_plant(w, w.plants.Count - 1);
                        }
                        return true;
                    }
                    break;
            }


            return false;
        }
    }
    public class MaterialManager
    {
        public List<MaterialData> materials;

        public MaterialManager()
        {
            materials = new List<MaterialData>();
            if (System.IO.File.Exists(@"craft_materials.txt"))
            {
                System.IO.StreamReader r = new System.IO.StreamReader(@"craft_materials.txt");
                string line = "";
                MaterialData p = new MaterialData();
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
                                if (p.name == "")// || p.name == null)
                                {
                                    p.name = items[1].Trim();
                                }
                                else
                                {
                                    p.material_id = (ushort)materials.Count;
                                    materials.Add(p);
                                    Exilania.text_stream.WriteLine("Crafting Material Item '" + p.name + "' Loaded.");
                                    p = new MaterialData();
                                    p.name = items[1].Trim();
                                }
                                break;
                            case "worth":
                                p.worth = Int32.Parse(items[1]);
                                break;
                            case "image":
                                p.image = Int32.Parse(items[1]);
                                break;
                            case "text":
                                p.description = items[1].Trim();
                                break;
                            case "action":
                                p.use_action = items[1].Trim();
                                break;
                            default:
                                Exilania.text_stream.WriteLine("UNHANDLED type " + items[0]);
                                break;
                        }
                    }
                    if (r.EndOfStream)
                    {
                        p.material_id = (ushort)materials.Count;
                        materials.Add(p);
                        Exilania.text_stream.WriteLine("Crafting Material Item '" + p.name + "' Loaded.");
                        cont = false;
                    }
                }
                r.Close();
            }
            else
            {
                Exilania.text_stream.Write("ERROR! No craft_materials.txt file.");
            }
        }
    }
}
