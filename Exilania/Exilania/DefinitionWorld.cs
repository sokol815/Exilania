using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public class CaveReverseChance
    {
        int chance;
        int lock_turns;

        public CaveReverseChance()
        {

        }

        public CaveReverseChance(string vals, int pwidth, int pheight)
        {
            string[] temp = vals.Split(',');
            chance = Acc.parse_float_or_int_to_val(temp[0], 1000);
            lock_turns = Acc.parse_float_or_int_to_val(temp[1], pwidth);
        }
    }
    public class PassCave
    {
        public int num_make;
        public int lode_size;
        public string method_name;
        public int min_width;
        public int max_width;
        public List<CaveReverseChance> reverse_chance;

        public PassCave()
        {

        }

        public PassCave(string vals, int pwidth, int pheight)
        {
            num_make = 12;
            lode_size = 600;
            method_name = "weighdowndrunkenwalk";
            min_width = 4;
            max_width = 10;
            reverse_chance = new List<CaveReverseChance>();
            reverse_chance.Add(new CaveReverseChance("5,50", pwidth, pheight));

            string[] spl = vals.Split(';');
            string[] temp = new string[0];
            for (int x = 0; x < spl.Length; x++)
            {
                string type = Acc.script_remove_content_of_outer_parenthesis(spl[x]).ToLower().Trim();
                vals = Acc.script_remove_outer_parentheses(spl[x]).Trim();
                switch (type)
                {
                    case "numcaves":
                        num_make = Acc.parse_float_or_int_to_val(vals, 50);
                        break;
                    case "lodesize":
                        lode_size = Acc.parse_float_or_int_to_val(vals, pwidth * pheight);
                        break;
                    case "method":
                        temp = vals.Split(',');
                        method_name = temp[0].Trim().ToLower();
                        min_width = Acc.parse_float_or_int_to_val(temp[1], pwidth);
                        max_width = Acc.parse_float_or_int_to_val(temp[2], pwidth);
                        break;
                    case "directionswitch":
                        reverse_chance = new List<CaveReverseChance>();
                        temp = vals.Split('|');
                        for (int i = 0; i < temp.Length; i++)
                        {
                            reverse_chance.Add(new CaveReverseChance(temp[i], pwidth, pheight));
                        }
                        break;
                }
            }
        }
    }
    public class PassLake
    {
        public int num_make;
        public int size_lake;
        public string method;
        public int min_depth;
        public int max_depth;

        public PassLake()
        {

        }

        public PassLake(string vals, int pwidth, int pheight)
        {
            num_make = 1;
            size_lake = 500;
            method = "ray";
            min_depth = 100;
            max_depth = pheight;
            
            string[] spl = vals.Split(';');
            for (int x = 0; x < spl.Length; x++)
            {
                string type = Acc.script_remove_content_of_outer_parenthesis(spl[x]).ToLower().Trim();
                vals = Acc.script_remove_outer_parentheses(spl[x]).Trim();
                switch (type)
                {
                    case "numlakes":
                        num_make = Acc.parse_float_or_int_to_val(vals, 200);
                        break;
                    case "lodesize":
                        size_lake = Acc.parse_float_or_int_to_val(vals, pwidth * pheight);
                        break;
                    case "method":
                        method = vals.ToLower().Trim();
                        break;
                    case "mindepth":
                        min_depth = Acc.parse_float_or_int_to_val(vals, pheight);
                        break;
                    case "maxdepth":
                        max_depth = Acc.parse_float_or_int_to_val(vals, pheight);
                        break;
                }
            }
        }
    }
    public class PassBiome
    {
        public string name;
        public string block_type;
        public int instances;
        public int width;
        public int depth;

        public PassBiome()
        {

        }
        public PassBiome(string vals, int pwidth, int pheight)
        {
            name = "Desert";
            block_type = "Sand";
            instances = 1;
            width = 200;
            depth = 60;
            string[] spl = vals.Split(';');
            name = spl[0].Trim();
            block_type = spl[1].Trim();
            for (int x = 2; x < spl.Length; x++)
            {
                string cur = Acc.script_remove_content_of_outer_parenthesis(spl[x]).Trim().ToLower();
                vals = Acc.script_remove_outer_parentheses(spl[x]).Trim();
                switch (vals)
                {
                    case "instances":
                        instances = Acc.parse_float_or_int_to_val(vals, 10);
                        break;
                    case "width":
                        width = Acc.parse_float_or_int_to_val(vals, pwidth);
                        break;
                    case "depth":
                        depth = Acc.parse_float_or_int_to_val(vals, pheight);
                        break;
                }
            }
        }

    }
    public class SeaProperties
    {
        public int num_make;
        public int depth;
        public int width;
        public string seabed_block;
        public int seabed_depth;
        public string beach_block;
        public int beach_width;

        public SeaProperties()
        {

        }

        public SeaProperties(string vals, int pwidth, int pheight)
        {
            num_make = 1;
            depth = 100;
            width = 200;
            seabed_block = "Sand";
            seabed_depth = 10;
            beach_block = "Sand";
            beach_width = 60;

            string[] sep = vals.Split(';');
            for (int x = 0; x < sep.Length; x++)
            {
                string type = Acc.script_remove_content_of_outer_parenthesis(sep[x]).ToLower().Trim();
                vals = Acc.script_remove_outer_parentheses(sep[x]);
                switch (type)
                {
                    case "numseas":
                        num_make = Acc.parse_float_or_int_to_val(vals, 5);
                        break;
                    case "seadepth":
                        depth = Acc.parse_float_or_int_to_val(vals, pheight);
                        break;
                    case "seawidth":
                        width = Acc.parse_float_or_int_to_val(vals, pwidth);
                        break;
                    case "seabed":
                        string[] temp = vals.Split(',');
                        seabed_block = temp[0].Trim();
                        seabed_depth = Acc.parse_float_or_int_to_val(temp[1], pheight);
                        break;
                    case "beach":
                        string[] temp2 = vals.Split(',');
                        beach_block = temp2[0].Trim();
                        beach_width = Acc.parse_float_or_int_to_val(temp2[1], pwidth);
                        break;
                }
            }
        }

    }
    public class Resource
    {
        public string block_name;
        public int lodesize;
        public int depth_lodesize_modifier;
        public int top_lodesize_modifier;
        public int min_depth;
        public int max_depth;
        public int depth_concentration_modifier;
        public int top_concentration_modifier;
        public int incidence;
        public int priority;

        public Resource()
        {

        }

        public Resource(string vals, int width, int height)
        {
            block_name = "Dirt";
            lodesize = 140;
            depth_lodesize_modifier = 0;
            top_lodesize_modifier = 0;
            min_depth = 0;
            max_depth = height;
            depth_concentration_modifier = 0;
            top_concentration_modifier = 0;
            incidence = width * height;
            priority = 0;

            string[] spl = vals.Split(';');
            block_name = spl[0].Trim();
            for (int x = 1; x < spl.Length; x++)
            {
                string comp = Acc.script_remove_content_of_outer_parenthesis(spl[x]).ToLower().Trim();
                vals = Acc.script_remove_outer_parentheses(spl[x]).Trim();
                switch (comp)
                {
                    case "lodesize":
                        if (vals.Contains(','))
                        {
                            string[] temp = vals.Split(',');
                            lodesize = Acc.parse_float_or_int_to_val(temp[0], 500);
                            temp[1] = temp[1].ToLower().Trim();
                            if (temp[1] == "moredepth")
                            {
                                depth_lodesize_modifier = Acc.parse_float_or_int_to_val(temp[2], 5);
                            }
                            else
                            {
                                top_lodesize_modifier = Acc.parse_float_or_int_to_val(temp[2], 5);
                            }
                        }
                        else
                        {
                            lodesize = Acc.parse_float_or_int_to_val(vals, 500);
                        }
                        break;
                    case "mindepth":
                        min_depth = Acc.parse_float_or_int_to_val(vals, height);
                        break;
                    case "maxdepth":
                        max_depth = Acc.parse_float_or_int_to_val(vals, height);
                        break;
                    case "concentration":
                        if (vals.ToLower() == "linear")
                        {

                        }
                        else
                        {
                            string[] temp = vals.Split(',');
                            temp[0] = temp[0].ToLower().Trim();
                            if (temp[0] == "low")
                            {
                                top_concentration_modifier = Acc.parse_float_or_int_to_val(temp[1], 5);
                            }
                            else
                            {
                                depth_concentration_modifier = Acc.parse_float_or_int_to_val(temp[1], 5);
                            }
                        }
                        break;
                    case "incidence":
                        incidence = Acc.parse_float_or_int_to_val(vals, width * height);
                        break;
                    case "priority":
                        priority = Acc.parse_float_or_int_to_val(vals, 1000);
                        break;
                    default:
                        Exilania.text_stream.WriteLine("Unhandled Resource type: " + comp);
                        break;
                }
            }
        }
    }
    public class VarianceReverseChance
    {
        public int chance;
        public int threshold;

        public VarianceReverseChance()
        {

        }

        public VarianceReverseChance(string vals, int width, int height)
        {
            //get something like .1f,.1f
            string[] spl = vals.Split(',');
            chance = Acc.parse_float_or_int_to_val(spl[0], 100);
            threshold = Acc.parse_float_or_int_to_val(spl[1], height);
        }

        public override string ToString()
        {
            return "C: " + chance + ", T: " + threshold;
        }
    }
    public class GroundVariance
    {
        public int chance;
        public int difference;
        public int num_runs;

        public GroundVariance()
        {

        }

        public GroundVariance(string vals,int width,int height)
        {
            //vals will look like 12,12 or 12,12,12
            string[] spl = vals.Split(',');
            chance = Acc.parse_float_or_int_to_val(spl[0], 1000);
            difference = Acc.parse_float_or_int_to_val(spl[1], height);
            if (spl.Length == 3)
            {
                num_runs = Acc.parse_float_or_int_to_val(spl[2], width);
            }
            else
            {
                num_runs = 1;
            }
        }

    }
    public class DefinitionWorld
    {
        string name;
        string description;
        public int width;
        public int height;
        int day_seconds;
        float night_percent_length;
        float world_gravity;
        string base_ground;
        string base_liquid;
        string base_backround;
        string base_ground_top;
        int atmosphere_start;
        int sealevel;
        int underground_one;
        int underground_two;
        int core;
        public int ground_smooth_iterations;
        int maxgroundvariance;
        List<GroundVariance> topography;
        List<VarianceReverseChance> topography_reverse_chance;
        int topography_modulus_reverse_num;
        List<Resource> world_resources;
        List<SeaProperties> seas;
        List<PassBiome> biomes;
        List<PassLake> lakes;
        List<PassCave> caves;
        int[] ground_variance;
        long timer_start;
        int tot_time_used;

        //for generating topography
        int cur_height;
        int safe_passes;
        bool increasing;
        int dy;
        int cur_smooth;
        int cur_val;
        int resource_cur_num;
        Point sea_init;
        double total_time = 0;

        public DefinitionWorld()
        {

        }

        public DefinitionWorld(System.IO.StreamReader r)
        {
            name = "Temp";
            description = "no description given... this could be bad.";
            width = 2000;
            height = 1000;
            day_seconds = 600;
            night_percent_length = .4f;
            world_gravity = 1f;
            base_ground = "Dirt";
            base_backround = "Dirt";
            base_liquid = "Water";
            base_ground_top = "Grass";
            sealevel = 250;
            ground_smooth_iterations = 1;
            atmosphere_start = 150;
            maxgroundvariance = 125;
            topography = new List<GroundVariance>();
            topography_reverse_chance = new List<VarianceReverseChance>();
            topography_modulus_reverse_num = 10;
            world_resources = new List<Resource>();
            seas = new List<SeaProperties>();
            biomes = new List<PassBiome>();
            lakes = new List<PassLake>();
            caves = new List<PassCave>();

            string line = "";
            bool cont = true;
            string[] read = new string[0];
            while (cont && !r.EndOfStream)
            {
                line = r.ReadLine().Trim();
                if (line == "" || line[0] == '#')
                {
                    //comment or empty line, ignore!
                }
                else
                {
                    read = line.Split(':');
                    read[0] = read[0].ToLower().Trim();
                    switch (read[0])
                    {
                        case "worldtypename":
                            name = read[1].Trim();
                            break;
                        case "worldgravity":
                            world_gravity = float.Parse(read[1].Replace("f", "").Trim());
                            break;
                        case "description":
                            description = read[1].Trim();
                            break;
                        case "width":
                            width = Acc.parse_float_or_int_to_val(read[1], ushort.MaxValue);
                            break;
                        case "height":
                            height = Acc.parse_float_or_int_to_val(read[1], ushort.MaxValue);
                            break;
                        case "daylength":
                            day_seconds = Acc.parse_float_or_int_to_val(read[1], 86400);
                            break;
                        case "nightpercentlength":
                            night_percent_length = Acc.parse_float_or_int_to_val(read[1], day_seconds);
                            night_percent_length = night_percent_length / (float)day_seconds;
                            break;
                        case "baseground":
                            base_ground = read[1].Trim();
                            break;
                        case "basebackround":
                            base_backround = read[1].Trim();
                            break;
                        case "basegroundtop":
                            base_ground_top = read[1].Trim();
                            break;
                        case "baseliquid":
                            base_liquid = read[1].Trim();
                            break;
                        case "atmospherestart":
                            atmosphere_start = Acc.parse_float_or_int_to_val(read[1], height);
                            Exilania.display.add_message("@05AS: " + atmosphere_start);
                            break;
                        case "sealevel":
                            sealevel = Acc.parse_float_or_int_to_val(read[1],height);
                            break;
                        case "underground1":
                            underground_one = Acc.parse_float_or_int_to_val(read[1], height);
                            break;
                        case "underground2":
                            underground_two = Acc.parse_float_or_int_to_val(read[1], height);
                            break;
                        case "core":
                            core = Acc.parse_float_or_int_to_val(read[1], height);
                            break;
                        case "groundsmoothiterations":
                            ground_smooth_iterations = Acc.parse_float_or_int_to_val(read[1], 10);
                            break;
                        case "groundvariance":
                            maxgroundvariance = Acc.parse_float_or_int_to_val(read[1], height);
                            break;
                        case "groundvariancespeed":
                            read[1] = Acc.script_remove_outer_parentheses(read[1]).Trim();
                            read = read[1].Split('|');
                            for (int x = 0; x < read.Length; x++)
                            {
                                topography.Add(new GroundVariance(read[x], width, height));
                            }
                            break;
                        case "groundreversechancethreshold":
                            read[1] = Acc.script_remove_outer_parentheses(read[1]).Trim();
                            read = read[1].Split('|');
                            for (int x = 0; x < read.Length; x++)
                            {
                                topography_reverse_chance.Add(new VarianceReverseChance(read[x], width, maxgroundvariance));
                            }
                            break;
                        case "modtestgroundreversechance":
                            topography_modulus_reverse_num = Acc.parse_float_or_int_to_val(read[1], 100);
                            break;
                        case "resource":
                            world_resources.Add(new Resource(read[1], width, height));
                            break;
                        case "seaproperties":
                            seas.Add(new SeaProperties(read[1], width, height));
                            break;
                        case "biome":
                            biomes.Add(new PassBiome(read[1], width, height));
                            break;
                        case "lakes":
                            lakes.Add(new PassLake(read[1], width, height));
                            break;
                        case "caves":
                            caves.Add(new PassCave(read[1], width, height));
                            break;
                        case "endworldtype":
                            cont = false;
                            break;
                        default:
                            Exilania.text_stream.WriteLine("There was an un-handled worldtype line: " + line + ".");
                            break;
                    }
                }
            }
        }


        public string creating_world(ref World pass, ref int cur_state, ref int cur_fine_state)
        {
            string ret = "Initiating.";
            timer_start = System.DateTime.Now.Ticks / 10000;
            tot_time_used = 0;
            int init_x = 0;
            switch (cur_state)
            {
                case 0:
                    pass.world_height_points[(int)HeightVals.SPACE] = atmosphere_start;
                    pass.world_height_points[(int)HeightVals.SEALEVEL] = sealevel;
                    pass.world_height_points[(int)HeightVals.UNDERGROUNDONE] = underground_one;
                    pass.world_height_points[(int)HeightVals.UNDERGROUNDTWO] = underground_two;
                    pass.world_height_points[(int)HeightVals.CORE] = core;

                    if (cur_fine_state == 0)
                    {
                        pass.map = new Voxel[width, height];
                    }
                    init_x = cur_fine_state / height;
                    for (int x = init_x; x < pass.map.GetLength(0); x++)
                    {
                        init_x = 0;
                        tot_time_used = (int)((System.DateTime.Now.Ticks / 10000) - timer_start);
                        if (tot_time_used > WorldCreator.max_time)
                        {
                            return "Initializing (" +Math.Round((float)cur_fine_state * 100f /((float)width * (float)height),2) + "%)";
                        }
                        for (int y = 0; y < pass.map.GetLength(1); y++)
                        {
                            cur_fine_state++;
                            pass.map[x, y] = new Voxel();
                        }
                    }
                    pass.day_length = day_seconds;
                    pass.night_percent_length = night_percent_length;
                    pass.world_gravity = world_gravity;
                    pass.atmosphere_start = atmosphere_start;
                    ret = "Generating terrain";
                    cur_fine_state = 0;
                    cur_state = 1;
                    break;
                case 1: //ground variance and basic flood filling of the world.
                    if (cur_fine_state == 0)
                    { //initialize
                        create_ground_Variance(cur_fine_state);
                    }
                    init_x = cur_fine_state / height;
                    sbyte desired_block = (sbyte)Exilania.block_types.get_block_by_name(base_ground);
                    sbyte desired_Bkd = Exilania.block_types.get_block_by_name(base_backround);
                    for (int x = init_x; x < pass.map.GetLength(0); x++)
                    {
                        init_x = 0;
                        tot_time_used = (int)((System.DateTime.Now.Ticks / 10000) - timer_start);
                        if (tot_time_used > WorldCreator.max_time)
                        {
                            return "Filling world with "+base_ground+ " (" + Math.Round((float)cur_fine_state * 100f / ((float)width * (float)height), 2) + "%)";
                        }
                        for (int y = 0; y < pass.map.GetLength(1); y++)
                        {
                            cur_fine_state++;
                            if (y >= ground_variance[x])
                            {
                                pass.place_block(new Point(x, y), desired_block, false, true);
                                pass.place_block(new Point(x, y), desired_Bkd, true, true);
                            }
                        }
                    }
                    cur_state = 2;
                    cur_fine_state = 0;
                    resource_cur_num = 0;
                    break;
                case 2: //sea
                    create_sea(pass);
                    cur_state = 3;
                    cur_fine_state = 0;
                    ret = "Placing Sea.";
                    break;
                case 3: //resources
                    int runs = 0;
                    for (int x = resource_cur_num; x < world_resources.Count; x++)
                    {
                        while (cur_fine_state < world_resources[x].incidence-5)
                        {
                            runs++;
                            if (runs % 100 == 0)
                            {
                                tot_time_used = (int)((System.DateTime.Now.Ticks / 10000) - timer_start);
                                if (tot_time_used > WorldCreator.max_time)
                                {
                                    return "Placing " + world_resources[x].block_name + " (" + Math.Round((float)cur_fine_state * 100f / (world_resources[x].incidence), 2) + "%)";
                                }
                            }
                            cur_fine_state += send_out_resources(x, cur_fine_state, pass);
                        }
                        cur_fine_state = 0;
                        resource_cur_num++;
                    }
                    cur_state = 4;
                    cur_fine_state = 0;
                    break;
                case 4://caves
                    cur_state = 29;
                    cur_fine_state = 0;
                    create_caves(pass);
                    ret = "Building Caves";
                    cur_fine_state = 0;
                    cur_smooth = 0;
                    break;
                case 29: //finish and smooth
                    for (int x = cur_smooth; x < ground_smooth_iterations; x++)
                    {
                        ret = smooth_terrain(pass,ref cur_fine_state);
                        if (ret != "Finished.")
                            return ret;
                        Exilania.display.add_message("@00Got to this spot.");
                        cur_smooth++;
                    }
                    cur_fine_state = 0;
                    cur_state = 30;
                    break;
                case 30:
                    ret = final_touchup(pass,ref cur_fine_state);
                    if (ret != "Finished.")
                        return ret;
                    cur_state = 31;
                    cur_fine_state = 0;
                    break;
                case 31:
                    Exilania.display.add_message("@05Sand capac: " + (pass.flowing_overflow.Count + pass.flowing_sand.Count));
                        ret = set_sand_to_rest(pass,ref cur_fine_state);
                    if (ret != "Finished.")
                        return ret;
                    cur_state = 32;
                    cur_fine_state = 0;
                    pass.disable_connectivity = false;
                    break;
                case 32: //resting the sand
                    if (cur_fine_state == 0)
                    {
                        cur_val = 0;
                        cur_fine_state++;
                        pass.liquid_simulator = new LiquidSimulator();
                        pass.liquid_simulator.init_liquid_cells(pass);
                        pass.world_ending = true;
                        total_time = 0;
                    }
                    while ((pass.flowing_overflow.Count + pass.flowing_sand.Count) > 0 || cur_val > 1000)
                    {
                        cur_fine_state++;
                        tot_time_used = (int)((System.DateTime.Now.Ticks / 10000) - timer_start);
                        if (tot_time_used > WorldCreator.max_time)
                        {
                            total_time += tot_time_used;
                            return "Doing Sand and Liquid. Current pass: " + (cur_fine_state-1) + " Active: " + (pass.flowing_overflow.Count + pass.flowing_sand.Count) 
                                + "/" + cur_val;
                        }
                        pass.freeflowing_blocks();
                        cur_val = pass.liquid_simulator.update(pass, 1f / 60f, true);
                    }
                    total_time += tot_time_used;
                    Exilania.display.add_message("@05Took " + cur_fine_state + " passes to complete. in " + total_time + " msec");
                    cur_state = 37;
                    cur_fine_state = 0;
                    break;
                case 37://finish the world
                    flow_sea(pass);
                    cur_state = 50;
                    ret = "Saving Map to File.";
                    break;
            }
            return ret;
        }

        public string set_sand_to_rest(World pass,ref int cur_fine_state)
        {
            sbyte sand_id = Exilania.block_types.get_block_by_name("Sand");
            int init_x = cur_fine_state / height;
            for (int x = init_x; x < pass.map.GetLength(0); x++)
            {
                init_x = 0;
                tot_time_used = (int)((System.DateTime.Now.Ticks / 10000) - timer_start);
                if (tot_time_used > WorldCreator.max_time)
                {
                    return "Initializing Sand Fall ["+(pass.flowing_overflow.Count + pass.flowing_sand.Count)+"] (" + Math.Round((float)cur_fine_state * 100f / ((float)width * (float)height), 2) + "%)";
                }
                for (int y = 1; y < pass.map.GetLength(1); y++)
                {
                    cur_fine_state++;
                    if (pass.map[x, y].fgd_block == sand_id)
                    {
                        if (pass.has_empty_block_foreground_neighbors(new Point(x, y)))
                        {
                            pass.add_freeflowing(new Point(x, y));
                        }
                    }
                }
            }
            return "Finished.";
        }

        public void create_caves(World pass)
        {
            Point cur_at = new Point();
            sbyte sand_block = Exilania.block_types.get_block_by_name("Sand");
            for (int i = 0; i < caves.Count; i++)
            {
                for (int j = 0; j < caves[i].num_make; j++)
                {
                    //create point at random point above the ground
                    cur_at = new Point(Exilania.rand.Next(j * (pass.map.GetLength(0) / caves[i].num_make), (j+1) * (pass.map.GetLength(0) / caves[i].num_make)), 0);
                    int runs = 0;
                    while (pass.map[cur_at.X, cur_at.Y].fgd_block == -1 && pass.map[cur_at.X, cur_at.Y].fgd_block != sand_block && runs < 50000)
                    {
                        runs++;
                        if (pass.map[cur_at.X, cur_at.Y].liquid_level > 0)
                            cur_at = new Point(Exilania.rand.Next(j * (pass.map.GetLength(0) / caves[i].num_make), (j + 1) * (pass.map.GetLength(0) / caves[i].num_make)), 0);
                        else
                            cur_at.Y++;
                    }
                    if (runs >= 50000)
                    {
                        runs = 0;
                        continue;
                    }
                    else
                        drunk_walk_cave(caves[i], cur_at, Exilania.rand.Next(caves[i].lode_size, pass.map.GetLength(1) * 2), pass);
                }
            }
        }

        public void drunk_walk_cave(PassCave c, Point start, int cave_length, World pass)
        {
            bool left = true;
            Point dloc = new Point(0, 1);
            Exilania.display.add_message("@00Size: " + c.min_width + " - " + c.max_width);
            int time_left_in_loc = 50;
            int bore_radius = Exilania.rand.Next(c.min_width,c.max_width+1);
            int count = 0;
            while (count < cave_length)
            {
                bore_radius = get_new_bore_radius(bore_radius, c);
                bore_out_area(pass, start, dloc, bore_radius);
                start.X += dloc.X;
                start.X = pass.wraparound_x(start.X);
                start.Y += dloc.Y;
                count++;
                time_left_in_loc--;
                if (time_left_in_loc <= 0)
                {
                    start.X += dloc.X;
                    start.X = pass.wraparound_x(start.X);
                    start.Y += dloc.Y;
                    dloc = switch_directions(dloc, c);
                    time_left_in_loc = Exilania.rand.Next(15, 20);
                    left = Exilania.rand.Next(0, 2) == 1;
                }
                else
                {
                    if (dloc.X == 0 && Exilania.rand.Next(0,3)==0)
                    {
                        if (left)
                            start.X = pass.wraparound_x(start.X - 1);
                        else
                            start.X = pass.wraparound_x(start.X + 1);
                    }
                }
                if (start.Y >= pass.map.GetLength(1) || (pass.map[start.X, start.Y].fgd_block < 0 && Exilania.rand.Next(0,14)==0))
                    count = cave_length;
            }
        }

        public int get_new_bore_radius(int old_radius, PassCave c)
        {
            int new_radius = Exilania.rand.Next(c.min_width, c.max_width+1);
            while(Math.Abs(new_radius - old_radius) > 1 )
            {
                new_radius = Exilania.rand.Next(c.min_width, c.max_width + 1);
            }
            return new_radius;
        }

        public Point switch_directions(Point now, PassCave c)
        {
            if (now.X != 0)
            { //currently an x is running.. only a y can run.
                now.X = 0;
                //now.Y = (Exilania.rand.Next(0, 2) * 2) - 1;
                //if (Exilania.rand.Next(0, 4) < 3)
                    now.Y = 1;
            }
            else
            {
                if (Exilania.rand.Next(0, 3) == 0)
                {
                    now.Y = 0;
                    now.X = (Exilania.rand.Next(0, 2) * 2) - 1;
                }
            }
            return now;
        }

        public void bore_out_area(World pass, Point orig, Point dloc, int bore_radius)
        {
            if (dloc.X == 0)
            { //currently moving left or right... bore out above and below
                for (int x = orig.X - bore_radius; x < orig.X + bore_radius; x++)
                {
                    //pass.place_block(new Point(pass.wraparound_x(x), orig.Y),4,false,true);
                    pass.remove_block(new Point(pass.wraparound_x(x), orig.Y), false);
                }
            }
            else
            { //currently moving up or down.. bore out left and right
                for (int y = orig.Y - bore_radius; y < orig.Y + bore_radius; y++)
                {
                    if (y > -1 && y < pass.map.GetLength(1))
                    {
                        //pass.place_block(new Point(orig.X, y), 4, false, true);
                        pass.remove_block(new Point(orig.X,y),false);
                    }
                }
            }
        }

        public void create_sea(World pass)
        {
            for (int j = 0; j < seas.Count; j++)
            {
                for (int i = 0; i < seas[j].num_make; i++)
                {
                    Point loc_do = new Point();
                    bool accepted = false;
                    //find a spot at sea level that doesn't have anything in it.
                    while (!accepted)
                    {
                        loc_do = new Point(Exilania.rand.Next(0, width), sealevel);
                        accepted = true;
                        if (pass.map[loc_do.X, loc_do.Y].liquid_level > 0 || pass.map[loc_do.X, loc_do.Y].fgd_block != -1)
                            accepted = false;
                    }
                    //expand left and right until can't anymore.
                    int min_x = loc_do.X;
                    int max_x = loc_do.X;
                    while (pass.map[pass.wraparound_x(min_x), sealevel].fgd_block == -1)
                    {
                        min_x--;
                        if (min_x <= pass.map.GetLength(0) * -1)
                            return;
                    }
                    min_x = pass.wraparound_x(min_x + 1);
                    //expand the sea eastward until can't anymore, or until width is reached.
                    while (pass.map[pass.wraparound_x(max_x), sealevel].fgd_block == -1 && max_x - min_x < seas[j].width)
                    {
                        max_x++;
                        if (min_x >= pass.map.GetLength(0) )
                            return;
                    }
                    max_x = pass.wraparound_x(max_x - 1);
                    int width_sea = max_x - min_x;
                    float sea_percent = 0f;
                    float cur_percent = 0f;
                    float cur_depth = 0f;
                    sbyte chosen_liquid = Exilania.block_types.get_block_by_name(base_liquid);
                    sbyte chosen_beach = Exilania.block_types.get_block_by_name(seas[i].beach_block);
                    sbyte chosen_seabed = Exilania.block_types.get_block_by_name(seas[i].seabed_block);
                    sbyte default_bkd = Exilania.block_types.get_block_by_name(base_backround);

                    int use_x = 0;
                    for (int x = min_x; x < max_x; x++)
                    {
                        sea_percent = (float)(x - min_x) / (width_sea);
                        use_x = pass.wraparound_x(x);
                        for (int y = sealevel; y < sealevel + seas[j].depth; y++)
                        {
                            cur_depth = (float)(y - sealevel) / (float)seas[j].depth;
                            if (sea_percent <= .35f)
                            { //left bank of the sea
                                cur_percent = sea_percent / .35f; //set to percent of incline
                                if (cur_depth <= cur_percent)
                                { //make the liquid of choice!
                                    pass.remove_block(new Point(use_x, y), false);
                                    pass.remove_block(new Point(use_x, y), true);
                                    //pass.place_block(new Point(use_x, y), chosen_liquid, false, true);
                                }
                            }
                            else if (sea_percent >= .65f)
                            { //right bank of the sea
                                cur_percent = (1f - sea_percent) / .35f; //set to percent of inlcine
                                if (cur_depth <= cur_percent)
                                {
                                    pass.remove_block(new Point(use_x, y), false);
                                    pass.remove_block(new Point(use_x, y), true);
                                    //pass.place_block(new Point(use_x, y), chosen_liquid, false, true);
                                }
                               
                            }
                            else
                            { //middle depths of the sea
                                pass.remove_block(new Point(use_x, y), false);
                                pass.remove_block(new Point(use_x, y), true);
                                //pass.place_block(new Point(use_x, y), chosen_liquid, false, true);
                            }
                        }
                    }
                    int num_added = 0;

                    for (int x = min_x - seas[j].beach_width; x < max_x + seas[j].beach_width; x++)
                    {
                        use_x = pass.wraparound_x(x);
                        for (int y = sealevel; y < sealevel + seas[j].depth + seas[j].seabed_depth; y++)
                        {
                            if (x < min_x || x > max_x)
                            {
                                if (y < pass.map.GetLength(1) && y < sealevel + seas[j].seabed_depth &&
                                    ( pass.map[use_x, y].liquid_id == 0))
                                {
                                    num_added++;
                                    pass.place_block(new Point(use_x, y), chosen_beach, false, true);
                                    pass.place_block(new Point(use_x, y), default_bkd, true, true);

                                }
                            }
                            else if (y < pass.map.GetLength(1) && (pass.map[use_x, y].liquid_id == 0))
                            {
                                num_added++;
                                pass.place_block(new Point(use_x, y), chosen_seabed, false, true);
                                pass.place_block(new Point(use_x, y), default_bkd, true, true);
                            }
                        }
                    }
                    sea_init = new Point(pass.wraparound_x(max_x - min_x), sealevel);
                }
            }
        }

        public void flow_sea(World pass)
        {
            sbyte chosen_liquid = Exilania.block_types.get_block_by_name(base_liquid);
            List<Point> open_points = new List<Point>();
            open_points.Add(sea_init);
            while (open_points.Count > 0)
            {
                if (open_points[0].Y >= sealevel && pass.map[open_points[0].X, open_points[0].Y].fgd_block == -1 && pass.map[open_points[0].X, open_points[0].Y].liquid_level < 100)
                {
                    pass.place_block(open_points[0], chosen_liquid, false, true);
                    pass.remove_block(open_points[0], true);
                    if (open_points[0].Y > sealevel)
                    {
                        open_points.Add(new Point(open_points[0].X, open_points[0].Y - 1));
                    }
                    open_points.Add(new Point(pass.wraparound_x(open_points[0].X - 1), open_points[0].Y));
                    open_points.Add(new Point(pass.wraparound_x(open_points[0].X + 1), open_points[0].Y));
                    if (open_points[0].Y < pass.map.GetLength(1) - 1)
                        open_points.Add(new Point(open_points[0].X, open_points[0].Y + 1));

                }
                open_points.RemoveAt(0);
            }
        }

        public string smooth_terrain(World pass, ref int cur_fine_state)
        {
            bool is_first = false;
            int init_y = cur_fine_state / width;
            for (int y = init_y; y < pass.map.GetLength(1); y++)
            {
                tot_time_used = (int)((System.DateTime.Now.Ticks / 10000) - timer_start);
                if (tot_time_used > WorldCreator.max_time)
                {
                    return "Smooth Terrain [" + cur_smooth + "/" + ground_smooth_iterations + "] - (" + Math.Round((float)cur_fine_state * 100f / ((float)width * (float)height), 2) + "%)";
                }
                is_first = true;
               
                for (int x = 0; x < pass.map.GetLength(0); x++)
                {
                    cur_fine_state++;
                    if (pass.number_of_walls_next_to(x, y) <= 3)
                    {
                        if (!pass.map[x, y].passable)
                        {
                            pass.remove_block(new Point(x, y), false);
                            if (is_first)
                                pass.remove_block(new Point(x, y), true);
                            is_first = false;
                        }
                    }
                    else if (!pass.map[x, y].passable)
                    {
                        is_first = false;
                    }
                }
            }
            return "Finished.";
        }

        public string final_touchup(World pass, ref int cur_fine_state)
        {
            sbyte grass_block = (sbyte)Exilania.block_types.get_block_by_name(base_ground_top);
            sbyte dirt_block = (sbyte)Exilania.block_types.get_block_by_name("Dirt");
            int init_x = cur_fine_state / height;
            for (int x = init_x; x < pass.map.GetLength(0); x++)
            {
                tot_time_used = (int)((System.DateTime.Now.Ticks / 10000) - timer_start);
                if (tot_time_used > WorldCreator.max_time)
                {
                    return "Final Touchup (" + Math.Round((float)cur_fine_state * 100f / ((float)width * (float)height), 2) + "%)";
                }
                bool done_top = false;
                for (int y = 0; y < pass.map.GetLength(1); y++)
                {
                    if (pass.map[x, y].liquid_level > 0)
                    {
                        pass.remove_block(new Point(x, y), true);
                    }
                    cur_fine_state++;
                    if (!done_top && !pass.map[x, y].passable || !pass.map[x,y].bkd_transparent)
                    {
                        done_top = true;
                        pass.remove_block(new Point(x, y), true);
                        if(pass.map[x,y].fgd_block == dirt_block)
                            pass.place_block(new Point(x, y), grass_block, false, true);
                        y = pass.map.GetLength(1);
                    }
                }
            }
            return "Finished.";
            /*for (int x = 0; x < pass.map.GetLength(0); x++)
            {
                
                for (int y = 0; y < pass.map.GetLength(1); y++)
                {
                    if (!done_top && !pass.map[x, y].passable)
                    {
                        done_top = true;
                        pass.remove_block(new Point(x, y), true);
                        if(pass.map[x,y].fgd_block == dirt_block)
                            pass.place_block(new Point(x, y), grass_block, false, true);
                        y = pass.map.GetLength(1);
                    }
                    else if (done_top)
                    {
                       // pass.place_block(new Point(x, y), grass_block, true, true);
                    }
                }
            }*/
             
        }

        public int send_out_resources(int x, int cur_fine_state,World w)
        {
            //choose a random spot
            //populate the empty spots with the resource
            //adjust cur_fine_state by returning the number of resources made, but don't make too many!
            int place_id = Exilania.block_types.get_block_by_name(world_resources[x].block_name);
            int x_post = Exilania.rand.Next(0, width);
            int[] y_pos = new int[world_resources[x].top_concentration_modifier + world_resources[x].depth_concentration_modifier];
            if (y_pos.Length == 0)
                y_pos = new int[1];
            int y_post = height;
            for (int i = 0; i < y_pos.Length; i++)
            {
                y_pos[i] = Exilania.rand.Next(world_resources[x].min_depth, world_resources[x].max_depth);
            }
            if (world_resources[x].top_concentration_modifier > 0)
            {
                for (int i = 0; i < y_pos.Length; i++)
                {
                    if (y_pos[i] < y_post)
                        y_post = y_pos[i];
                }
            }
            else if (world_resources[x].depth_concentration_modifier > 0)
            {
                for (int i = 0; i < y_pos.Length; i++)
                {
                    if (y_pos[i] > y_post)
                        y_post = y_pos[i];
                }
            }
            else
            {
                y_post = y_pos[0];
            }
            int lode_size = world_resources[x].lodesize;
            float spot = (float)(y_post-world_resources[x].min_depth) / ((float)world_resources[x].max_depth - (float)world_resources[x].min_depth);
            if (world_resources[x].depth_lodesize_modifier > 0)
            {
                lode_size = (int)((float)lode_size * (spot * (float)world_resources[x].depth_lodesize_modifier));
            }
            else if (world_resources[x].top_lodesize_modifier > 0)
            {
                //spot = 1f - spot;
                lode_size = (int)((float)lode_size / (spot * (float)world_resources[x].top_lodesize_modifier));
            }
            if (cur_fine_state + lode_size > world_resources[x].incidence)
            {
                lode_size = world_resources[x].incidence - cur_fine_state;
            }
            int num_placed = 0;
            int iterations = 0;
            int max_iterations = lode_size * 3;
            while (lode_size > 0 && iterations < max_iterations)
            {
                iterations++;
                if (w.map[x_post, y_post].fgd_block != -1)
                {
                    lode_size--;
                    num_placed++;
                    if (place_id > -1)
                        w.place_block(new Point(x_post, y_post), (sbyte)place_id, false, true);
                    else
                        w.remove_block(new Point(x_post, y_post), false);
                }
                switch (Exilania.rand.Next(0, 3))
                {
                    case 0://do dx;
                        x_post += (Exilania.rand.Next(0, 2) * 2) - 1;
                        break;
                    case 1://do dy;
                        y_post += (Exilania.rand.Next(0, 2) * 2) - 1;
                        break;
                    case 2://do both
                        y_post += (Exilania.rand.Next(0, 2) * 2) - 1;
                        x_post += (Exilania.rand.Next(0, 2) * 2) - 1;
                        break;
                }
                x_post = w.wraparound_x(x_post);
                if (y_post < world_resources[x].min_depth)
                    y_post = world_resources[x].min_depth;
                if (y_post >= world_resources[x].max_depth-1)
                    y_post = world_resources[x].max_depth-1;

            }
            return num_placed;
        }

        public void create_ground_Variance(int cur_fine_state)
        {
            if (cur_fine_state == 0)
            { //initialize
                cur_height = sealevel;
                safe_passes = 0;
                increasing = true;
                dy = get_rand_ground_var();
            }
            
            ground_variance = new int[width];
            ground_variance[0] = sealevel;
           
            int min_height = sealevel - maxgroundvariance;
            int max_height = sealevel + maxgroundvariance;
            for (int x = 1; x < width; x++)
            {
                if (increasing)
                    ground_variance[x] = ground_variance[x - 1] + dy;
                else
                    ground_variance[x] = ground_variance[x - 1] - dy;
                if (x % topography_modulus_reverse_num == 0)
                {
                    if (reverse_direction(ground_variance[x]))
                    {
                        increasing = !increasing;
                        safe_passes = 0;
                        dy = get_rand_ground_var();
                    }
                }
                if (ground_variance[x] < min_height)
                {
                    ground_variance[x] = min_height;
                    increasing = true;
                }
                else if (ground_variance[x] > max_height)
                {
                    ground_variance[x] = max_height;
                    increasing = false;
                }
                safe_passes--;
                if (safe_passes <= 0)
                    dy = get_rand_ground_var();
            }
        }
        public bool reverse_direction(int cur_height)
        {
            int dist_off = cur_height - sealevel;
            if (increasing)
            {
                //check for greater values, then.
                if (dist_off > 0)
                {
                    int highest = -1;
                    for (int x = 0; x < topography_reverse_chance.Count; x++)
                    {
                        if (topography_reverse_chance[x].threshold <= dist_off && (highest == -1 || topography_reverse_chance[highest].threshold < topography_reverse_chance[x].threshold))
                        {
                            highest = x;
                        }
                    }
                    if (highest != -1)
                    {
                        if (Exilania.rand.Next(0, 100) <= topography_reverse_chance[highest].chance)
                            return true;
                    }
                }
            }
            else
            {
                dist_off *= -1;
                //check for less than values, then.
                if (dist_off > 0)
                {
                    int highest = -1;
                    for (int x = 0; x < topography_reverse_chance.Count; x++)
                    {
                        if (topography_reverse_chance[x].threshold <= dist_off && (highest == -1 || topography_reverse_chance[highest].threshold < topography_reverse_chance[x].threshold))
                        {
                            highest = x;
                        }
                    }
                    if (highest != -1)
                    {
                        if (Exilania.rand.Next(0, 100) <= topography_reverse_chance[highest].chance)
                            return true;
                    }
                }
            }
            return false;
        }
        public int get_rand_ground_var()
        {
            int total_vals = 0;
            for (int x = 0; x < topography.Count; x++)
            {
                total_vals += topography[x].chance;
            }
            int chc = Exilania.rand.Next(0, total_vals);
            total_vals = 0;
            for (int x = 0; x < topography.Count; x++)
            {
                total_vals += topography[x].chance;
                if (total_vals >= chc)
                {
                    safe_passes = topography[x].num_runs;
                    return topography[x].difference;
                }
            }
            return 0;
        }
        public static void FullWorldTypeReader(ref List<DefinitionWorld> worlds)
        {
            if (System.IO.File.Exists(@"world_creation_properties.txt"))
            {
                worlds = new List<DefinitionWorld>();
                System.IO.StreamReader r = new System.IO.StreamReader(@"world_creation_properties.txt");
                while (!r.EndOfStream)
                {
                    worlds.Add(new DefinitionWorld(r));
                }
                r.Close();
                worlds.RemoveAt(worlds.Count - 1);
                //Exilania.debug = "Num Worlds: " + worlds.Count;
            }
            else
            {
                Exilania.text_stream.WriteLine("Found no world_creation_properties.txt... aborting.");
            }
        }
        public override string ToString()
        {
            return name;
        }
    }
}
