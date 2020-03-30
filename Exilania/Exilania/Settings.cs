using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public class Settings
    {
        public bool use_seed;
        public int seed_id;
        public bool debugging;
        public int levelupheal;
        public string server_ip;
        public bool allow_strobe;
        public bool force_new_character;
        public UInt16 server_port;
        public UInt16 client_port;
        public bool use_custom_dimensions;
        public int custom_width;
        public int custom_height;
        public bool use_hardware_lighting;
        public string world_load_name;
        public byte max_users;
        public bool show_chat_background;
        public bool show_ping;
        public bool liquid_debugging;
        public int msec_show_chat;
        public Point highdef_window_size = new Point();
        public float mastervolume;

        public Settings()
        {

        }

        public Settings(System.IO.StreamReader r)
        {
            string line = "";
            bool cont = true;
            while (cont)
            {
                line = r.ReadLine();
                if (line[0] == '#')
                {
                    //skip this line
                }
                else
                {
                    string[] items = line.Split(':');
                    switch (items[0].ToLower())
                    {
                        case "mastervolume":
                            mastervolume = (float)(int.Parse(items[1])) / 100f;
                            if (mastervolume > 1f)
                                mastervolume = 1f;
                            if (mastervolume < 0f)
                                mastervolume = 0f;
                            break;
                        case "liquiddebugging":
                            try
                            {
                                liquid_debugging = bool.Parse(items[1]);

                            }
                            catch
                            {
                                liquid_debugging = false;
                            }
                            break;
                        case "highdefwindowheight":
                             try
                            {
                                highdef_window_size.Y = int.Parse(items[1]);
                            }
                            catch
                            {
                                highdef_window_size.Y = 1000;
                            }
                            break;
                        case "highdefwindowwidth":
                            try
                            {
                                highdef_window_size.X = int.Parse(items[1]);
                            }
                            catch
                            {
                                highdef_window_size.X = 2600;
                            }
                            break;
                        case "msecshowchat":
                            try
                            {
                                msec_show_chat = int.Parse(items[1]);
                                Display.show_received_chat_time = (float)msec_show_chat / 1000f;
                            }
                            catch
                            {
                                msec_show_chat = 5000;
                                Display.show_received_chat_time = (float)msec_show_chat / 1000f;
                            }
                            break;
                        case "showchatbackground":
                            try
                            {
                                show_chat_background = bool.Parse(items[1]);
                            }
                            catch
                            {
                                show_chat_background = true;
                            }
                            break;
                        case "max_users":
                            int temp = int.Parse(items[1]);
                            if (temp > 255)
                                temp = 255;
                            max_users = (byte)temp;
                            break;
                        case "forcenewcharacter":
                            force_new_character = Boolean.Parse(items[1]);
                            break;
                        case "allowstrobe":
                            allow_strobe = Boolean.Parse(items[1]);
                            break;
                        case "use_seed":
                            use_seed = Boolean.Parse(items[1]);
                            break;
                        case "seed_id":
                            seed_id = Int32.Parse(items[1]);
                            break;
                        case "debugging":
                            debugging = Boolean.Parse(items[1]);
                            break;
                        case "levelupheal":
                            levelupheal = Int32.Parse(items[1]);
                            break;
                        case "serveripaddress":
                            server_ip = items[1];
                            break;
                        case "serverport":
                            try
                            {
                                server_port = UInt16.Parse(items[1]);
                            }
                            catch
                            {
                                server_port = 50231;
                            }
                            break;
                        case "clientport":
                            try
                            {
                                client_port = UInt16.Parse(items[1]);
                            }
                            catch
                            {
                                client_port = 50231;
                            }
                            break;
                        case "usecustomdimensions":
                            try
                            {
                                use_custom_dimensions = Boolean.Parse(items[1]);
                            }
                            catch
                            {
                                use_custom_dimensions = false;
                            }
                            break;
                        case "customwidth":
                            try
                            {
                                custom_width = Int32.Parse(items[1]);
                            }
                            catch
                            {
                                custom_width = 1920;
                            }
                            break;
                        case "customheight":
                            try
                            {
                                custom_height = Int32.Parse(items[1]);
                            }
                            catch
                            {
                                custom_height = 1080;
                            }
                            break;
                        case "usehardwarelighting":
                            try
                            {
                                use_hardware_lighting = Boolean.Parse(items[1]);
                            }
                            catch
                            {
                                use_hardware_lighting = false;
                            }
                            break;
                        case "worldloadname":
                            try
                            {
                                world_load_name = items[1];
                            }
                            catch
                            {
                                world_load_name = "Default World 1";
                            }
                            break;
                        case "showping":
                            try
                            {
                                show_ping = bool.Parse(items[1]);
                            }
                            catch
                            {
                                show_ping = true;
                            }
                            break;
                    }
                }
                if (r.EndOfStream)
                    cont = false;
            }
        }

        public string modify_settings(string new_setting, World w)
        {
            string returner = "@07";
            string[] items = new_setting.Split(':');
            if (items.Length > 2)
            {
                for (int i = 2; i < items.Length; i++)
                {
                    items[1] += ":"+items[i];
                }
            }
            if (items.Length > 1 && items[1].Length > 0)
                switch (items[0])
                {
                    case "mastervolume":
                        if (items[1].Contains('.'))
                        {
                            Exilania.sounds.master_volume = float.Parse(items[1]);
                        }
                        else
                        {
                            Exilania.sounds.master_volume = ((float)int.Parse(items[1])) / 100f;
                        }
                        if (Exilania.sounds.master_volume < 0f)
                            Exilania.sounds.master_volume = 0f;
                        if (Exilania.sounds.master_volume > 1f)
                            Exilania.sounds.master_volume = 1f;
                        returner += "@07Master Volume set to " + (Exilania.sounds.master_volume * 100) + ".";
                        break;
                    case "give":
                        if (w != null && Exilania.game_my_user_id < w.players.Count && w.players.Count > 0)
                        {
                            string[] info = items[1].Split(':');
                            ushort quantity = 10;
                            item_descriptor cur_item = Acc.get_item_by_name(info[0]);
                            if (info.Length >= 2)
                            {
                                try
                                {
                                    quantity = ushort.Parse(info[1]);
                                }
                                catch
                                {
                                    quantity = 10;
                                }
                            }
                            if (cur_item.item_id > -1)
                            {
                                switch (cur_item.item_type)
                                {
                                    case ItemType.Block:
                                        w.players[Exilania.game_my_user_id].avatar.items.pickup_block((sbyte)cur_item.item_id, quantity);
                                        returner += info[0] + " qauntity " + quantity + " given.";
                                        break;
                                    case ItemType.Material:
                                        w.players[Exilania.game_my_user_id].avatar.items.pickup_material((sbyte)cur_item.item_id, quantity);
                                        returner += info[0] + " qauntity " + quantity + " given.";
                                        break;
                                    case ItemType.Furniture:
                                        w.players[Exilania.game_my_user_id].avatar.items.pickup_furniture((sbyte)cur_item.item_id, quantity);
                                        returner += info[0] + " qauntity " + quantity + " given.";
                                        break;
                                    default:
                                        returner += "@08Feature not added, yet.";
                                        break;
                                }
                            }
                            else
                            {
                                returner += "@08What is a '" + info[0] + "'?";
                            }

                        }
                        else
                        {
                            returner += "@08Start a game first.";
                        }
                        break;
                    case "eval":
                        int val_get = Acc.resolve_die_roll(items[1],0,0);
                        returner += items[1] + "= " + val_get;
                        break;
                    case "time":
                        if (w != null)
                            try
                            {
                                if (Exilania.game_server)
                                {
                                    items = items[1].Split(':');
                                    int minutes = 0;
                                    int hours = 0;
                                    if (items.Length > 1)
                                    {
                                        minutes = int.Parse(items[1]);
                                        if (minutes < 10)
                                            minutes *= 10;
                                        minutes %= 60;
                                    }
                                    hours = int.Parse(items[0])%24;
                                    int cur_day = (int)(w.world_time / w.day_length);
                                    cur_day++;
                                    w.world_time = (float)cur_day * w.day_length;
                                    w.world_time += ((float)hours - 6) / 24f * w.day_length;
                                    Exilania.display.add_message("@00Hour Debug: " + ((float)hours) / 24f);
                                    w.world_time += ((float)minutes) / 1440f * w.day_length;
                                    returner += "Time set to " + hours.ToString().PadLeft(2, '0') + ":" + minutes.ToString().PadLeft(2, '0') + ".";
                                }
                                else
                                    returner += "@06Admin access only.";
                            }
                            catch
                            {
                                returner += "@06Unsupported time format please use HH:MM (24H Clock)";
                            }
                        else
                            returner += "@06FOR USE ONLY IN-GAME.";
                        break;
                    case "liquiddebugging":
                        try
                        {
                            liquid_debugging = bool.Parse(items[1]);
                            returner += "Liquid Debugging set to " + liquid_debugging.ToString() + ".";
                        }
                        catch
                        {
                            liquid_debugging = false;
                            returner += "@06Improper Formatting. Liquid Debugging set to false.@07";
                        }
                        break;
                    case "highdefwindowheight":
                        try
                        {
                            highdef_window_size.Y = int.Parse(items[1]);
                            returner += "HighDef Window Height set to " + highdef_window_size.Y.ToString() + ".";
                        }
                        catch
                        {
                            highdef_window_size.Y = 1000;
                            returner += "@06Improper Formatting. HighDef Window Height set to 1000.@07";
                        }
                        break;
                    case "highdefwindowwidth":
                        try
                        {
                            highdef_window_size.X = int.Parse(items[1]);
                            returner += "HighDef Window Width set to " + highdef_window_size.X.ToString() + ".";
                        }
                        catch
                        {
                            highdef_window_size.X = 2600;
                            returner += "@06Improper Formatting. HighDef Window Width set to 2600.@07";
                        }
                        break;
                    case "msecshowchat":
                        try
                        {
                            msec_show_chat = int.Parse(items[1]);
                            returner += "Msec show chat now set to " + msec_show_chat.ToString() + "ms.";
                            Display.show_received_chat_time = (float)msec_show_chat / 1000f;
                        }
                        catch
                        {
                            msec_show_chat = 5000;
                            Display.show_received_chat_time = (float)msec_show_chat / 1000f;
                            returner += "@06Improper Formatting. Msec show chat set to 5000.@07";
                        }
                        break;
                    case "showchatbackground":
                        try
                        {
                            show_chat_background = bool.Parse(items[1]);
                            returner += "Show Chat Background now set to " + show_chat_background.ToString() + ".";
                        }
                        catch
                        {
                            show_chat_background = true;
                            returner += "@06Improper Formatting. Show Chat Background set to true.@07";
                        }
                        break;
                    case "max_users":
                        try
                        {
                            int temp = int.Parse(items[1]);
                            if (temp > 255)
                                temp = 255;
                            max_users = (byte)temp;
                            returner += "Max Users now set to " + max_users.ToString() + ".";
                        }
                        catch
                        {
                            max_users = 32;
                            returner += "@06Improper Formatting. Max Users now set to 32.@07";
                        }
                        break;
                    case "forcenewcharacter":
                        try
                        {
                            force_new_character = Boolean.Parse(items[1]);
                            returner += "Force New Character now set to " + force_new_character.ToString() + ".";
                        }
                        catch
                        {
                            force_new_character = false; returner += "@06Improper Formatting. Force New Character now set to False.@07";
                        }
                        break;
                    case "allowstrobe":
                        try
                        {
                            allow_strobe = Boolean.Parse(items[1]);
                            returner += "Strobing Text now set to " + allow_strobe.ToString() + ".";
                        }
                        catch
                        {
                            allow_strobe = false; returner += "@06Improper Formatting. Strobing Text now set to False.@07";
                        }
                        break;
                    case "use_seed":
                        use_seed = Boolean.Parse(items[1]);
                        returner += "use seed now set to " + use_seed.ToString() + ". Rand reset.";
                        if (use_seed)
                        {
                            Exilania.rand = new Lidgren.Network.NetRandom(seed_id);
                        }
                        else
                        {
                            Exilania.rand = new Lidgren.Network.NetRandom((int)System.DateTime.Now.Ticks);
                        }
                        break;
                    case "seed_id":
                        try { seed_id = Int32.Parse(items[1]); }
                        catch { seed_id = 10; returner += "@06Improper Formatting. @07"; }
                        returner += "random seed now set to " + seed_id.ToString() + ",";
                        if (use_seed)
                        {
                            Exilania.rand = new Lidgren.Network.NetRandom(seed_id);
                            returner += " Rand reset.";
                        }
                        break;
                    case "debugging":
                        try
                        {
                            debugging = Boolean.Parse(items[1]);
                        }
                        catch
                        {
                            returner += "@06Improper Formatting. @07";
                            debugging = false;
                        }
                        Exilania.draw_debug = debugging;
                        returner += "Debug changed to " + debugging.ToString() + ".";
                        break;
                    case "levelupheal":
                        levelupheal = Int32.Parse(items[1]);
                        returner += "Levelupheal changed to " + levelupheal.ToString() + ".";
                        break;
                    case "serveripaddress":
                        server_ip = items[1];
                        returner += "serveripaddress changed to " + server_ip + ".";
                        break;
                    case "serverport":
                        try
                        {
                            server_port = UInt16.Parse(items[1]);
                        }
                        catch
                        {
                            server_port = 50231;
                        }
                        returner += "serverport changed to " + server_port.ToString() + ".";
                        break;
                    case "clientport":
                        try
                        {
                            client_port = UInt16.Parse(items[1]);
                        }
                        catch
                        {
                            client_port = 50232;
                        }
                        returner += "clientport changed to " + client_port.ToString() + ".";
                        break;
                    case "help":
                        returner += "@05Use commands found in the Exilania.ini text file to commit changes.";
                        break;
                    case "usecustomdimensions":
                        try
                        {
                            use_custom_dimensions = Boolean.Parse(items[1]);
                            returner += " usecustomdimensions changed to " + use_custom_dimensions.ToString() + ".";
                        }
                        catch
                        {
                            returner += "@06Improper Formatting! @07";
                            use_custom_dimensions = false;
                        }
                        break;
                    case "customwidth":
                        try
                        {
                            custom_width = Int32.Parse(items[1]);
                            returner += "customwidth set to " + custom_width.ToString() + ".";

                        }
                        catch
                        {
                            custom_width = 1920;
                            returner += "@06Improper Formatting! @07";
                        }
                        break;
                    case "customheight":
                        try
                        {
                            custom_height = Int32.Parse(items[1]);
                            returner += "customheight set to " + custom_height.ToString() + ".";
                        }
                        catch
                        {
                            custom_height = 1080;
                            returner += "@06Improper Formatting! @07";
                        }
                        break;
                    case "usehardwarelighting":
                        try
                        {
                            use_hardware_lighting = Boolean.Parse(items[1]);
                            returner += "Use Hardware lighting set to " + use_hardware_lighting.ToString() + ".";
                        }
                        catch
                        {
                            use_hardware_lighting = false;
                            returner += "@06Improper Formatting; Use Hardware Lighting set to false.";
                        }
                        break;
                    case "worldloadname":
                        try
                        {
                            world_load_name = items[1];
                            returner += "World to load changed to " + world_load_name + ".";
                        }
                        catch
                        {
                            world_load_name = "Default World 1";
                            returner += "@06Improper Formatting; World to Load now 'Default World 1'.";
                        }
                        break;
                    case "showping":
                        try
                        {
                            show_ping = bool.Parse(items[1]);
                            returner += "Show Ping changed to " + show_ping + ".";
                        }
                        catch
                        {
                            show_ping = true;
                            returner = "@06Improper Formatting; Show Ping set to true.";
                        }
                        break;
                    default:
                        returner += "@05unrecognized command. @06'/" + new_setting + "'@05.";
                        break;
                }
            else
                returner += "@05Missing command parameters for command @06'" + items[0] + "'@05.";
            return returner;
        }
    }
}
