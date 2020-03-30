using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;

namespace Exilania
{
   
    public class BodyPart
    {
        public float rotation;
        public float magnification;
        public Vector2 center;
        public Vector2 parent_attachment_point;
        public Vector2 my_attachment_point;
        public Vector2 grid_location;
        public Rectangle picture;
        public bool use_hashcode;
        public int pic_hashcode;
        public string name;
        public Color draw_color;
        public string parent_name;
        public int parent_id;
        public List<int> child_ids;
        public List<string> child_names;
        public bool follow_parent;
        public bool follow_mouse;
        public bool click_active_mouse;
        public int walking;
        public int rotation_offset;
        public int draw_order;
        public int original_order;
        public Color resultant_color;
        public sbyte skin_id;
        public int body_type_id;

        public BodyPart()
        {
            rotation = 0;
            magnification = 1f;
            center = new Vector2();
            parent_attachment_point = new Vector2();
            my_attachment_point = new Vector2();
            grid_location = new Vector2();
            picture = new Rectangle();
            use_hashcode = false;
            pic_hashcode = -1;
            name = "";
            draw_color = Color.White;
            parent_name = "";
            parent_id = -1;
            child_ids = new List<int>();
            child_names = new List<string>();
            follow_mouse = false;
            follow_parent = false;
            click_active_mouse = false;
            walking = -1;
            rotation_offset = 0;
            draw_order = 0;
            original_order = 0;
            resultant_color = Color.White;
            skin_id = -1;
            body_type_id = -1;
        }

        public void draw(SpriteBatch s, float mag, Display d, bool facing_right)
        {
            /*if (skin_id > -1)
            {
                Exilania.display.add_message("@05Arachnid with color " + skin_id);
                Color mult = Exilania.body_templates[body_type_id].skins[skin_id];
                resultant_color.R *= (byte)(((float)mult.R) / 255f * (float)resultant_color.R);
                resultant_color.G *= (byte)(((float)mult.G) / 255f * (float)resultant_color.G);
                resultant_color.B *= (byte)(((float)mult.B) / 255f * (float)resultant_color.B);

            }*/
            mag = Math.Max(mag, magnification);
            if (facing_right)
            {
                if (use_hashcode)
                    s.Draw(Exilania.item_manager.custom_item_images[pic_hashcode], grid_location, Exilania.item_manager.custom_item_images[pic_hashcode].Bounds,
                        resultant_color, rotation, center, mag, SpriteEffects.None, 0);
                else
                    s.Draw(d.sprites, grid_location, picture, resultant_color, rotation, center, mag, SpriteEffects.None, 0);
            }
            else
            {
                if (use_hashcode)
                    s.Draw(Exilania.item_manager.custom_item_images[pic_hashcode], grid_location, Exilania.item_manager.custom_item_images[pic_hashcode].Bounds,
                        resultant_color, rotation - (float)Math.PI, center, mag, SpriteEffects.FlipHorizontally, 0);
                else
                    s.Draw(d.sprites, grid_location, picture, resultant_color, rotation - (float)Math.PI, center, mag, SpriteEffects.FlipHorizontally, 0);
            }
        }

        public override string ToString()
        {
            return Display.color_code + "01Named: " + name + " Location: " + grid_location.ToString() + " Rotation: " + rotation + " orig_order: " + original_order.ToString(); 
        }
    }
    public class scheduled_movement
    {
        public string movement_state;
        public float movement_frame_start;

        public scheduled_movement(float time, string mov)
        {
            movement_state = mov;
            movement_frame_start = time;
        }

        public scheduled_movement(Lidgren.Network.NetIncomingMessage i)
        {
            movement_frame_start = i.ReadSingle();
            movement_state = i.ReadString();
        }

        public void send_movement(Lidgren.Network.NetOutgoingMessage m)
        {
            m.Write(movement_frame_start);
            m.Write(movement_state);
        }

        public override string ToString()
        {
            return movement_frame_start + " [" + movement_state + "]";
        }
    }

    public class Actor_Input
    {
        public bool hold_left_mouse_click;
        public bool hold_right_mouse_click;
        public long start_left_click;
        public bool initial_left_click;
        public bool end_left_click;
        public long start_right_click;
        public Point mouse_loc;
        public Vector2 screen_size;
        public List<scheduled_movement> movement_schedule;
        public int cur_selection;
        public string pressed_keys;
        public string old_pressed_keys;
        public string server_says_pressed_keys;
        public float time_elapsed_check_mouse;
        public static float max_time_elapsed_check_mouse = 0.15f;
        public static float cur_server_roundtrip_time = 0f;

        public Actor_Input()
        {
            pressed_keys = "";
            old_pressed_keys = "";
            server_says_pressed_keys = "";
            hold_left_mouse_click = false;
            hold_right_mouse_click = false;
            mouse_loc = new Point();
            movement_schedule = new List<scheduled_movement>();
            screen_size = new Vector2(Exilania.screen_size.X,Exilania.screen_size.Y);
            cur_selection = 0;

        }

        /// <summary>
        /// this is the update that is used when the current client input is being updated for the current player.
        /// </summary>
        /// <param name="i"></param>
        public void update_input(float time, Input i, Display d, World w, Exilania e, bool is_player)
        {
            if (is_player)
            {
                cur_server_roundtrip_time = Exilania.network_client.udp_client.ServerConnection.AverageRoundtripTime + .05f;
            }

            if (i.mouse_now.X > -1 && i.mouse_now.X < Exilania.screen_size.X &&
                i.mouse_now.Y > -1 && i.mouse_now.Y < Exilania.screen_size.Y && e.IsActive)
            {
                if (i.mouse_now.LeftButton == ButtonState.Pressed && !hold_left_mouse_click)
                {
                    start_left_click = System.DateTime.Now.Ticks / 10000;
                    hold_left_mouse_click = true;
                    initial_left_click = true;
                    end_left_click = false;
                }
                else if (hold_left_mouse_click && i.mouse_now.LeftButton == ButtonState.Released)
                {
                    long end_time = System.DateTime.Now.Ticks / 10000;
                    end_time -= start_left_click;
                    start_left_click = end_time;
                    //d.add_message("Left Clicked for " + end_time + "ms.");
                    hold_left_mouse_click = false;
                    end_left_click = true;
                    initial_left_click = false;
                }
                else
                {
                    initial_left_click = false;
                    end_left_click = false;
                }

                if (i.mouse_now.RightButton == ButtonState.Pressed && !hold_right_mouse_click)
                {
                    start_right_click = System.DateTime.Now.Ticks / 10000;
                    hold_right_mouse_click = true;
                }
                else if (hold_right_mouse_click && i.mouse_now.RightButton == ButtonState.Released)
                {
                    long end_time = System.DateTime.Now.Ticks / 10000;
                    end_time -= start_right_click;
                    start_right_click = end_time;
                    //d.add_message("Right Clicked for " + end_time + "ms.");
                    hold_right_mouse_click = false;
                }
            }
            mouse_loc = new Point((int)w.top_left.X + i.mouse_now.X, (int)w.top_left.Y + i.mouse_now.Y);
            
            old_pressed_keys = pressed_keys;
            if (Exilania.input.current_input_type != InputConsume.Normal)
            {
                pressed_keys = "";
            }
            else
            {
                pressed_keys = i.collection;
            }

            if (is_player)
            {
                time_elapsed_check_mouse += time;
                if (time_elapsed_check_mouse > max_time_elapsed_check_mouse)
                { //send mouse coords to everyone
                    Exilania.network_client.send_mouse_coordinates(mouse_loc,w.players[Exilania.game_my_user_id].avatar);
                    time_elapsed_check_mouse = 0f;
                }
                if (old_pressed_keys != pressed_keys)
                { //send a packet detailing info about pressed keys.

                    movement_schedule.Add(new scheduled_movement(w.world_time + cur_server_roundtrip_time, pressed_keys));
                    Exilania.network_client.send_key_commands(movement_schedule[movement_schedule.Count-1]);
                    movement_schedule[movement_schedule.Count - 1].movement_frame_start = w.world_time + .05f;
                }
            }
        }

        public void fine_mesh_update_input(float world_start_time, Actor a)
        {
            movement_schedule.Sort((x, y) => x.movement_frame_start.CompareTo(y.movement_frame_start));
            while (movement_schedule.Count > 0 && movement_schedule[0].movement_frame_start <= world_start_time)
            {
                server_says_pressed_keys = movement_schedule[0].movement_state;
                movement_schedule.RemoveAt(0);
            }
        }

        public void update_input_ai(Display d, World w, Actor a)
        {
            if (pressed_keys.Contains('D'))
                a.facing_right = true;
            else if(pressed_keys.Contains('A'))
                a.facing_right = false;
            mouse_loc = new Point((int)a.world_loc.X,(int)a.world_loc.Y);
            if (a.facing_right)
                mouse_loc.X += 200;
            else
                mouse_loc.X -= 200;
            mouse_loc.Y -= 200;

            if (Exilania.rand.Next(0, 200) == 0)
            {
                if (pressed_keys.Contains('D'))
                    pressed_keys = "A";
                else
                    pressed_keys = "D";

            }
            if (Exilania.rand.Next(0, 200) == 0)
                pressed_keys += " ";
            server_says_pressed_keys = pressed_keys;
        }
    }

    public class Actor
    {
        public static int[] foot_angles = new int[] {-35,-26,-17,-9,0,9,17,26,35};
        public static float min_sec_down_fall = .25f; //in world defined
        public float block_place_wait_time = .15f;//constant...
        public float time_at_change_foot_State = .03f; //in world defined
        public static float liquid_speed_reduction = .3f;
        public static float breath_recover_multiplier = 3f;
       
        /// <summary>
        /// this contains the chosen pieces to be able to reconstruct the character's body.
        /// </summary>
        public List<int> piece_choices;
        public List<Color> piece_colors;
        public string name;
        public List<BodyPart> body; //you will be reconstructed from the piece choices list.
        public Rectangle bounding_box = new Rectangle(-18, -36, 36, 72);
        public bool empty = false;
        public Inventory items;
        public ActorStats stats;
        public List<DamageMove> damages;
        public DamageMove last_damage;
       
        //the below items are not written into the file, but defined by the reinitialization.
        public Vector2 screen_loc = new Vector2(); //in world defined
        public Vector2 inertia = new Vector2(); //in world defined
        public Vector2 world_loc = new Vector2(); //in world defined
        public Vector2 dloc_server = new Vector2();//in world defined
        public float total_off = 0f;
        public float last_button_push = 0f;
        public bool facing_right = true; //in world defined
        public bool hold_breath = false;
        public Actor_Input input; //in world defined
        public float block_place_wait_so_far = .4f; //in world defined
        public float time_in_foot_state = 0f; //in world defined
        public bool foot_state_increase = true; //in world defined
        public int foot_state = 2; //in world defined
        public int platform_fall_y = 0; //in world defined
        public float cur_sec_down_fall = 0f; //in world defined
        public bool step_up_left = false;
        public int last_step_up_left_x = -1;
        public bool step_up_right = false;
        public int last_Step_up_right_x = -1;
        public int fall_start_loc = -1;
        public bool is_falling = false;
        public bool action_swinging_right = false;
        public bool action_swinging = false;
        public float action_start = (float)Math.PI * 1.625f;
        public float action_end = (float)Math.PI * 2.50f;
        public float action_swing_time = 0.2f;
        public float cur_action_swing_time = 0f;
        public bool mouse_in_construction_zone = false;
        
        public Color light_level = Color.White;
        /// <summary>
        /// light source is in RGB format with actual light values being 25 * number for R, G, and B seperately.
        /// </summary>
        public byte[] light_source =  new byte[] {0,0,0};
        public bool in_water = false; //in world defined.
        public bool foot_water = false;
        public double mouse_angle;
        public int break_block_swing = 0;
        public int break_block_swings_needed = 0;
        public Point last_break_block_loc = new Point();
        public bool swinging_arms = false;
        public List<ItemConnector> connections;
        public Point furn_start_click;
        public bool is_furn_clicking;
        public int active_chest_id = -1;
        
        public Actor()
        {
        }

        public Actor(BodyTemplate b, bool is_player)
        {
            is_furn_clicking = false;
            piece_choices = new List<int>();
            piece_colors = new List<Color>();
            connections = new List<ItemConnector>();
            damages = new List<DamageMove>();
            items = new Inventory(is_player);
            input = new Actor_Input();
           
            name = b.template_name;
            body = b.create_actor_body(piece_choices, piece_colors);
            if (b.skins.Count > 0)
            {
                int skin_choice = Exilania.rand.Next(0, b.skins.Count);
                for (int i = 0; i < body.Count; i++)
                {
                    if(!Exilania.body_templates[body[i].body_type_id].parts_list[Exilania.body_templates[body[i].body_type_id].get_bodypart_template_id_by_name(body[i].name)].no_color)
                        body[i].draw_color = b.skins[skin_choice];
                }
            }
           
            stats = new ActorStats();
            world_loc = new Vector2(700, 300);
            bounding_box = b.size_body;
            stats.pvp = true;
            for (int x = 0; x < body.Count; x++)
            {
                body[x].original_order = x;
            }
            body.Sort((x, y) => y.draw_order.CompareTo(x.draw_order));
            for (int x = 0; x < body.Count; x++)
            {
                for (int y = 0; y < body.Count; y++)
                {
                    if (body[y].parent_name == body[x].name)
                    {
                        body[y].parent_id = x;
                        body[x].child_ids.Add(y);
                        body[x].child_names.Add(body[y].name);
                    }
                }
            }
        }

        public Actor(System.IO.BinaryReader r)
        {
            is_furn_clicking = false;
            BodyTemplate b = Exilania.body_templates[0];
            connections = new List<ItemConnector>();
            name = r.ReadString();
            int num_parts = r.ReadInt32();
            piece_colors = new List<Color>();
            damages = new List<DamageMove>();
            piece_choices = new List<int>();
            for (int x = 0; x < num_parts; x++)
            {
                piece_choices.Add(r.ReadInt32());
                Color temp = new Color();
                temp.PackedValue = r.ReadUInt32();
                piece_colors.Add(temp);
            }
            body = b.create_body_from_list(piece_choices, piece_colors);
            for (int x = 0; x < body.Count; x++)
            {
                body[x].original_order = x;
            }
            body.Sort((x, y) => y.draw_order.CompareTo(x.draw_order));
            for (int x = 0; x < body.Count; x++)
            {
                for (int y = 0; y < body.Count; y++)
                {
                    if (body[y].parent_name == body[x].name)
                    {
                        body[y].parent_id = x;
                        body[x].child_ids.Add(y);
                        body[x].child_names.Add(body[y].name);
                    }
                }
            }
            bounding_box = new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
            items = new Inventory(r,this,Exilania.display);
            input = new Actor_Input();
            stats = new ActorStats(r);
        }

        public Actor(NetIncomingMessage r)
        {
            is_furn_clicking = false;
            BodyTemplate b = Exilania.body_templates[0];
            connections = new List<ItemConnector>();
            name = r.ReadString();
            int num_parts = r.ReadInt32();
            damages = new List<DamageMove>();
            piece_colors = new List<Color>();
            piece_choices = new List<int>();
            for (int x = 0; x < num_parts; x++)
            {
                piece_choices.Add(r.ReadInt32());
                Color temp = new Color();
                temp.PackedValue = r.ReadUInt32();
                piece_colors.Add(temp);
            }
            body = b.create_body_from_list(piece_choices, piece_colors);
            for (int x = 0; x < body.Count; x++)
            {
                body[x].original_order = x;
            }
            body.Sort((x, y) => y.draw_order.CompareTo(x.draw_order));
            for (int x = 0; x < body.Count; x++)
            {
                for (int y = 0; y < body.Count; y++)
                {
                    if (body[y].parent_name == body[x].name)
                    {
                        body[y].parent_id = x;
                        body[x].child_ids.Add(y);
                        body[x].child_names.Add(body[y].name);
                    }
                }
            }
            bounding_box = new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
            items = new Inventory(r,this,Exilania.display);
            input = new Actor_Input();
            stats = new ActorStats(r);
        }

        public void write_actor(System.IO.BinaryWriter w)
        {
            w.Write(name);
            w.Write(piece_choices.Count);
            for (int x = 0; x < piece_choices.Count; x++)
            {
                w.Write(piece_choices[x]);
                w.Write(piece_colors[x].PackedValue);
            }
            w.Write(bounding_box.X);
            w.Write(bounding_box.Y);
            w.Write(bounding_box.Width);
            w.Write(bounding_box.Height);
            items.write_inventory(w);
            stats.write_ActorStats(w);
        }

        public void send_actor(NetOutgoingMessage m)
        {
            m.Write(name);
            m.Write(piece_choices.Count);
            for (int x = 0; x < piece_choices.Count; x++)
            {
                m.Write(piece_choices[x]);
                m.Write(piece_colors[x].PackedValue);
            }
            m.Write(bounding_box.X);
            m.Write(bounding_box.Y);
            m.Write(bounding_box.Width);
            m.Write(bounding_box.Height);
            items.send_inventory(m);
            stats.send_ActorStats(m);
        }


        /// <summary>
        /// clears piece_colors to properly set it.
        /// </summary>
        public void reset_colors()
        {
            piece_colors = new List<Color>();
            for (int x = 0; x < body.Count; x++)
            {
                piece_colors.Add(body[x].draw_color);
            }
        }

        public int get_parent_part_id()
        {
            for (int x = 0; x < body.Count; x++)
            {
                if (body[x].parent_id == -1)
                    return x;
            }
            return -1;
        }

        private void update_facing(Actor_Input i)
        {
            bool fac_before = facing_right;
            if (input.mouse_loc.X > world_loc.X)
            {
                facing_right = true;
            }
            else
                facing_right = false;
            if (facing_right != fac_before)
            {
                //not really necessary?
            }
        }

        public Vector2 focus_actor(World w)
        {
            Vector2 new_loc = new Vector2(Exilania.screen_size.X / 2, Exilania.screen_size.Y / 2);

            if (world_loc.Y < Exilania.screen_size.Y / 2)
            {
                new_loc.Y = world_loc.Y;
            }
            else if (world_loc.Y > w.map.GetLength(1) * 24f - (Exilania.screen_size.Y / 2 ))
            {
                new_loc.Y = Exilania.screen_size.Y - (w.map.GetLength(1) * 24f - world_loc.Y);
            }
            return new_loc;
        }

        /// <summary>
        /// called by update_movement. only callable by the current player on their own machine.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="is_left_click"></param>
        public void perform_click(World w, bool is_left_click, Display d, int actor_id)
        {
            if (items.is_player && block_place_wait_so_far >= block_place_wait_time)
            { //this is a player and they can place items.
                Point equiv_loc = new Point((int)(input.mouse_loc.X / 24f), (int)(input.mouse_loc.Y / 24f)); //physical click location
                //wraparound needed
                if (input.mouse_loc.X < 0)
                {
                    equiv_loc.X = (int)Math.Floor(input.mouse_loc.X / 24f);
                }
                equiv_loc.X = w.wraparound_x(equiv_loc.X);
                //reset block place timer
                block_place_wait_so_far = 0f;
                if((w.map[equiv_loc.X,equiv_loc.Y].fgd_block > -1 && Exilania.block_types.blocks[w.map[equiv_loc.X,equiv_loc.Y].fgd_block].is_hangar) ||
                    (w.map[equiv_loc.X, equiv_loc.Y].bkd_block > -1 && Exilania.block_types.blocks[w.map[equiv_loc.X, equiv_loc.Y].bkd_block].is_hangar))
                {
                    mouse_in_construction_zone = true;
                }
                else
                {
                    mouse_in_construction_zone = false;
                }
                bool use_bkd = input.pressed_keys.Contains('`'); //this character is invoked by pressing shift or tilde.
                //are we breaking or placing a block?
                sbyte block_type = w.get_block_at(equiv_loc, use_bkd);
                bool furniture = false;
                bool plant = false;

                //Exilania.debug = "Check breaking block count: " + break_block_swing;
                switch (items.cubby_type(is_left_click,(block_type!=-1?true:false)))
                {
                    case -1: //undefined, or empty;
                        break;
                    case 0: //block
                        if ((Exilania.input.alt || block_type == -1 || Exilania.block_types.blocks[block_type].block_group == 5) && (use_bkd ||
                            (w.map[equiv_loc.X, equiv_loc.Y].furniture_index == -1 && w.map[equiv_loc.X, equiv_loc.Y].plant_index == -1 &&
                            w.collision_table.can_place_at(equiv_loc, 24, 24, w, new Rectangle((int)world_loc.X + bounding_box.X, 
                                (int)world_loc.Y + bounding_box.Y, bounding_box.Width, bounding_box.Height)))))
                        {
                            block_place_wait_time = .05f;
                            if (Exilania.input.alt)
                            {
                                Point loc_use = new Point();
                                for (int x = 0; x < 2; x++)
                                {
                                    for (int y = 0; y < 2; y++)
                                    {
                                        loc_use = new Point(w.wraparound_x(equiv_loc.X + x), equiv_loc.Y + y);
                                        if( loc_use.Y > -1 && loc_use.Y < w.map.GetLength(1) && (w.get_block_at(loc_use,use_bkd) == -1 || 
                                            Exilania.block_types.blocks[w.get_block_at(loc_use,use_bkd)].block_group == 5))
                                            place_block(new Point(w.wraparound_x(equiv_loc.X + x),equiv_loc.Y + y), w, is_left_click);
                                    }
                                }
                            }
                            else
                                place_block(equiv_loc, w, is_left_click);
                        }
                        break;
                    case 1: //item
                        #region item_actions
                        bool attacking = false;
                        if (!use_bkd && w.map[equiv_loc.X, equiv_loc.Y].furniture_index != -1)
                        {
                            furniture = true;
                        }
                        else if (!use_bkd && w.map[equiv_loc.X, equiv_loc.Y].plant_index != -1)
                        {
                            plant = true;
                            block_type = (sbyte)BlockData.block_enum[Exilania.plant_manager.plants[w.plants[w.map[equiv_loc.X, equiv_loc.Y].plant_index].plant_index].material];
                            
                        }
                        bool plant_above = w.can_break(equiv_loc);
                        if (items.is_block_breaker(is_left_click, block_type, furniture) && (use_bkd || furniture || plant || w.can_break(equiv_loc)))
                        {
                            if (is_left_click)
                            {
                                if ((!items.temporary.is_empty && items.temporary.phys_item != null && items.temporary.phys_item.can_swing) ||
                                    (items.cur_left_hand != -1 && items.hotbar_items[items.cur_left_hand].phys_item != null && items.hotbar_items[items.cur_left_hand].phys_item.can_swing))
                                {
                                    if (!items.temporary.is_empty && items.temporary.phys_item != null && items.temporary.phys_item.can_swing)
                                    {
                                        Exilania.network_client.send_swing_arms(true);
                                        block_place_wait_time = items.temporary.phys_item.swing_speed;
                                        action_swinging_right = false;
                                        action_swinging = true;
                                        cur_action_swing_time = 0;
                                        action_swing_time = items.temporary.phys_item.swing_speed;
                                    }
                                    else
                                    {
                                        attacking = true;
                                        Exilania.network_client.send_swing_arms(true);
                                        block_place_wait_time = items.hotbar_items[items.cur_left_hand].phys_item.swing_speed;
                                        action_swinging_right = false;
                                        action_swinging = true;
                                        cur_action_swing_time = 0;
                                        action_swing_time = items.hotbar_items[items.cur_left_hand].phys_item.swing_speed;
                                    }
                                }
                            } //end left click (includes left hand and in-mouse items)
                            else
                            {
                                if (items.cur_right_hand != -1 && items.hotbar_items[items.cur_right_hand].phys_item != null && 
                                    items.hotbar_items[items.cur_right_hand].phys_item.can_swing)
                                {
                                    attacking = true;
                                    Exilania.network_client.send_swing_arms(false);
                                    block_place_wait_time = items.hotbar_items[items.cur_right_hand].phys_item.swing_speed;
                                    action_swinging_right = true;
                                    action_swinging = true;
                                    cur_action_swing_time = 0;
                                    action_swing_time = items.hotbar_items[items.cur_right_hand].phys_item.swing_speed;
                                }
                            } //end right click
                            //swinging something that can break a block of the type we are at.
                            if (equiv_loc.X != last_break_block_loc.X || equiv_loc.Y != last_break_block_loc.Y)
                            { //resetting the break block counter
                                last_break_block_loc.X = equiv_loc.X;
                                last_break_block_loc.Y = equiv_loc.Y;
                                break_block_swing = 0;
                                break_block_swings_needed = items.strikes_to_break(is_left_click, block_type, furniture,plant);
                            } //end hitting at a new place
                            break_block_swing++;
                            //Exilania.sounds.finish_hit_block.Play(.02f, (Exilania.rand.Next(0, 201) - 100) * .01f, Math.Min(Exilania.screen_size.X/2,-1 * (world_loc.X - (equiv_loc.X * 24)))/(Exilania.screen_size.X/2 + 20));
                            if (break_block_swing >= break_block_swings_needed)
                            {
                                break_block_swing = 0;
                                if (furniture)
                                {
                                    int furniture_id = w.map[equiv_loc.X, equiv_loc.Y].furniture_index;
                                    if ((w.furniture[furniture_id].owner == name || w.furniture[furniture_id].owner.Contains(name)) &&
                                        (!w.furniture[furniture_id].flags[(int)FFLAGS.IS_CHEST] ||ItemChest.is_empty(w,furniture_id)))
                                    {
                                        Exilania.display.fading_text.Add(new FadeText("@00+1 " + Exilania.furniture_manager.furniture[w.furniture[furniture_id].furniture_id].name,
                                            Display.default_msec_show_fade_text/2, (int)w.players[Exilania.game_my_user_id].avatar.world_loc.X + 18, (int)w.players[Exilania.game_my_user_id].avatar.world_loc.Y - 40, true, true));
                                        items.pickup_furniture(equiv_loc, w, Exilania.game_my_user_id);
                                        Exilania.network_client.send_remove_furniture(equiv_loc);
                                    }
                                } //furniture end
                                else if (plant)
                                {
                                    int plant_id = w.map[equiv_loc.X, equiv_loc.Y].plant_index;
                                    Exilania.network_client.send_plant_attack(equiv_loc);
                                    if (w.plants[plant_id].destroy_give_items(equiv_loc, w, this,plant_id))
                                    {
                                        w.plants[plant_id].unset_map(w);
                                        for (int x = plant_id + 1; x < w.plants.Count; x++)
                                        {
                                            w.plants[x].unset_map(w);
                                            w.plants[x].reset_map(w, x - 1);
                                        }
                                        w.plants.RemoveAt(plant_id);
                                    }
                                } //plant end
                                else if (plant_above && w.map[equiv_loc.X, equiv_loc.Y - 1].plant_index != -1)
                                {
                                    equiv_loc.Y--;
                                    int plant_id = w.map[equiv_loc.X, equiv_loc.Y].plant_index;
                                    Exilania.network_client.send_plant_attack(equiv_loc);
                                    if (w.plants[plant_id].destroy_give_items(equiv_loc, w, this, plant_id))
                                    {
                                        w.plants[plant_id].unset_map(w);
                                        for (int x = plant_id + 1; x < w.plants.Count; x++)
                                        {
                                            w.plants[x].unset_map(w);
                                            w.plants[x].reset_map(w, x - 1);
                                        }
                                        w.plants.RemoveAt(plant_id);
                                    }
                                    equiv_loc.Y++;
                                    sbyte last_state = w.remove_block(equiv_loc, use_bkd);
                                    if (last_state != -1)
                                    {
                                        Exilania.network_client.send_remove_block(equiv_loc, use_bkd, w);
                                    }
                                    if ((use_bkd && Exilania.block_types.blocks[last_state].place_wall) || !use_bkd)
                                    {
                                        if(last_state > -1)
                                        Exilania.display.fading_text.Add(new FadeText("@00+1 " + Exilania.block_types.blocks[last_state].name,
                                            Display.default_msec_show_fade_text/2, (int)w.players[Exilania.game_my_user_id].avatar.world_loc.X + 18, (int)w.players[Exilania.game_my_user_id].avatar.world_loc.Y - 40, true, true));
                                        items.pickup_block(last_state);
                                    }
                                } //plant above block end
                                else
                                {
                                    sbyte last_state = w.remove_block(equiv_loc, use_bkd);
                                    if (last_state != -1)
                                    {
                                        Exilania.network_client.send_remove_block(equiv_loc, use_bkd, w);
                                    }
                                    if (last_state != -2 && ((use_bkd && Exilania.block_types.blocks[last_state].place_wall) || !use_bkd))
                                    {
                                        if (last_state > -1)
                                            Exilania.display.fading_text.Add(new FadeText("@00+1 " + Exilania.block_types.blocks[last_state].name,
                                                Display.default_msec_show_fade_text/2, (int)w.players[Exilania.game_my_user_id].avatar.world_loc.X + 18, (int)w.players[Exilania.game_my_user_id].avatar.world_loc.Y - 40, true, true));
                                        items.pickup_block(last_state);
                                    }
                                } //just a normal block end
                            } //end if finished swings to break block
                        } //end if blockbreaker and can break current object
                        else if (items.can_swing_item(is_left_click))
                        {
                            if (is_left_click)
                            {
                                if (items.hotbar_items[items.cur_left_hand].phys_item != null && items.hotbar_items[items.cur_left_hand].phys_item.can_swing)
                                {
                                    attacking = true;
                                    Exilania.network_client.send_swing_arms(true);
                                    block_place_wait_time = items.hotbar_items[items.cur_left_hand].phys_item.swing_speed;
                                    action_swinging_right = false;
                                    action_swinging = true;
                                    cur_action_swing_time = 0;
                                    action_swing_time = items.hotbar_items[items.cur_left_hand].phys_item.swing_speed;
                                }
                            }
                            else
                            {
                                attacking = true;
                                Exilania.network_client.send_swing_arms(false);
                                block_place_wait_time = items.hotbar_items[items.cur_right_hand].phys_item.swing_speed;
                                action_swinging_right = true;
                                action_swinging = true;
                                cur_action_swing_time = 0;
                                action_swing_time = items.hotbar_items[items.cur_right_hand].phys_item.swing_speed;
                            }
                        }
                        if (attacking)
                        { //you are now doing a melee attack!
                            Point swing_spot = new Point();
                            for (int i = 0; i < body.Count; i++)
                            {
                                if (body[i].name == "Left Arm")
                                    swing_spot = new Point((int)(body[i].grid_location.X + w.top_left.X), (int)(body[i].grid_location.Y + w.top_left.Y));
                            }
                            List<DamageMove> queue = new List<DamageMove>();
                            ushort melee_range = (ushort)items.hotbar_items[(is_left_click ? items.cur_left_hand : items.cur_right_hand)].phys_item.melee_range;
                            List<int> enemies_can_hit = new List<int>();
                            List<int> plants_can_hit = new List<int>();
                            if (stats.pvp)
                            {
                                enemies_can_hit = w.collision_table.get_players_in_range(swing_spot, melee_range, w);
                                enemies_can_hit.Remove(actor_id);
                                for (int i = 0; i < enemies_can_hit.Count; i++)
                                {
                                    DamageMove t = new DamageMove(w.world_time + Actor_Input.cur_server_roundtrip_time, (ushort)actor_id, TargetType.Player, (ushort)enemies_can_hit[i],
                                        TargetType.Player, items.get_swinging_damage(is_left_click, this, w.players[enemies_can_hit[i]].avatar), CauseDamage.Sliced,melee_range);
                                    queue.Add(t);
                                }
                            }
                            plants_can_hit = w.collision_table.get_plants_in_range(swing_spot, w, melee_range, false, facing_right);
                            plants_can_hit.Sort();
                            for (int i = plants_can_hit.Count-1; i >-1; i--)
                            {
                                int plant_id = plants_can_hit[i];

                                if (!Exilania.plant_manager.plants[w.plants[plant_id].plant_index].material.ToLower().Contains("misc"))
                                {
                                    Point hit_point = w.plants[plant_id].top_left;
                                    Exilania.network_client.send_plant_attack(hit_point);
                                    if (w.plants[plant_id].destroy_give_items(hit_point, w, this, plant_id))
                                    {
                                        w.plants[plant_id].unset_map(w);
                                        for (int x = plant_id + 1; x < w.plants.Count; x++)
                                        {
                                            w.plants[x].unset_map(w);
                                            w.plants[x].reset_map(w, x - 1);
                                        }
                                        w.plants.RemoveAt(plant_id);
                                    }
                                }
                            }
                            enemies_can_hit = w.collision_table.get_npcs_in_range(swing_spot, melee_range, w, facing_right);
                            for (int i = 0; i < enemies_can_hit.Count; i++)
                            {
                                if (w.npcs[enemies_can_hit[i]].stats.pvp)
                                {
                                    DamageMove t = new DamageMove(w.world_time + Actor_Input.cur_server_roundtrip_time, (ushort)actor_id, TargetType.Player, (ushort)enemies_can_hit[i],
                                        TargetType.NPC, items.get_swinging_damage(is_left_click, this, w.npcs[enemies_can_hit[i]]), CauseDamage.Sliced, melee_range);
                                    queue.Add(t);
                                }
                            }
                            if (queue.Count > 0)
                            {
                                Exilania.network_client.send_damage_notice(queue);
                                queue.Clear();
                            }
                        }
#endregion
                        break;
                    case 2: //furniture
                        items.try_place_furniture(is_left_click, equiv_loc, w, this, d);
                        break;
                    case 3://materials
                        items.try_use_material(is_left_click, equiv_loc, w, this, d);
                        break;
                }
            }
        }

        public void place_block(Point equiv_loc, World w, bool is_left_click)
        {
            bool use_bkd = input.pressed_keys.Contains('`');
            sbyte placing_id = -1;
            if (is_left_click) //using left button.
            {
                if (!items.temporary.is_empty && items.temporary.is_block && !items.click_consumed)
                    placing_id = items.temporary.block_id;
                else if (items.cur_left_hand > -1 && !items.click_consumed && items.hotbar_items[items.cur_left_hand].is_block)
                    placing_id = items.hotbar_items[items.cur_left_hand].block_id;
                else
                    return;
            }
            else if (items.cur_right_hand > -1 && items.hotbar_items[items.cur_right_hand].is_block)
            {
                placing_id = items.hotbar_items[items.cur_right_hand].block_id;
            }
            else
                return;
            sbyte res = w.place_block(equiv_loc, placing_id, use_bkd, false);
            if ( res == -1 || Exilania.block_types.blocks[res].block_group == 5)
            {
                Exilania.network_client.send_place_block(equiv_loc, use_bkd, (byte)placing_id, w);
                if (is_left_click)
                {
                    if (!items.temporary.is_empty && items.temporary.is_block && !items.click_consumed)
                        items.temporary.use_block();
                    else if (items.cur_left_hand > -1 && !items.click_consumed)
                    {
                        items.hotbar_items[items.cur_left_hand].use_block();
                        if (items.hotbar_items[items.cur_left_hand].is_empty)
                        {
                            Exilania.network_client.send_changed_item_in_hands(items.hotbar_items[items.cur_left_hand], true);
                            items.equip_item(this, Exilania.display, true, false);
                        }
                    }
                    else
                        return;
                }
                else if (items.cur_right_hand > -1)
                {
                    items.hotbar_items[items.cur_right_hand].use_block();
                    if (items.hotbar_items[items.cur_right_hand].is_empty)
                    {
                        Exilania.network_client.send_changed_item_in_hands(items.hotbar_items[items.cur_right_hand], false);
                        items.equip_item(this, Exilania.display, false, true);
                    }
                }
            }

        }

        public void set_torso_rotation(int amt)
        {
            int bid = 0;
            while (body[bid].parent_id != -1)
            {
                bid = body[bid].parent_id;
            }
            body[bid].rotation_offset = amt;
        }

        /// <summary>
        /// called by update_body
        /// </summary>
        /// <param name="sec_fraction"></param>
        /// <param name="d"></param>
        /// <param name="i"></param>
        /// <param name="w"></param>
        public bool update_movement(float sec_fraction, Display d,  World w, int actor_id)
        {
            if (action_swinging)
            {
                cur_action_swing_time += sec_fraction;
                if (cur_action_swing_time >= action_swing_time)
                    action_swinging = false;
            }
            damages.Sort((a, b) => a.time_execute.CompareTo(b.time_execute));
            for (int i = 0; i < damages.Count; i++)
            {
                if (damages[i].time_execute <= w.world_time)
                {
                    last_damage = damages[i];
                    stats.life.change_val(-damages[i].damage);
                    d.fading_text.Add(new FadeText("@08-" + damages[i].damage, 800, (int)world_loc.X + (Exilania.rand.Next(0, 40) - 20), (int)world_loc.Y - (Exilania.rand.Next(20, 40) ), true, true));
                    damages.RemoveAt(i);
                    i--;
                }
            }
            if (stats.life.cur_val <= 0)
            {
                if (Exilania.game_server)
                {
                    Exilania.network_server.send_death_notice(last_damage,w);
                }
                if (!items.is_player)
                {
                    return true;
                }
            }

            Point top_left = new Point((int)world_loc.X + bounding_box.X, (int)world_loc.Y + bounding_box.Y);
            top_left.X /= 24;
            top_left.X = w.wraparound_x(top_left.X);
            top_left.Y /= 24;
            top_left.Y++;
            if (top_left.Y > 0 && w.map[top_left.X, top_left.Y - 1].liquid_level > 50)
            {
                hold_breath = true;
                stats.breath.change_val(-sec_fraction);
                if (stats.breath.cur_val <= 0f)
                {
                    stats.breath.change_val(1f);
                    if (Exilania.game_server)
                    {
                        Exilania.network_server.damages.Add(new DamageMove(w.world_time + .5f, (ushort)ushort.MaxValue, TargetType.World, (ushort)actor_id, 
                            (items.is_player ? TargetType.Player : TargetType.NPC), 5, CauseDamage.Drowned,0));
                    }
                }
            }
            else
            {
                stats.breath.change_val(sec_fraction * breath_recover_multiplier);
                hold_breath = false;
            }
            if (w.map[top_left.X, top_left.Y].liquid_level > 50)
            {
                in_water = true;
            }
            else
            {
                in_water = false;
            }
            if (top_left.Y < w.map.GetLength(1) -1 && w.map[top_left.X, top_left.Y + 1].liquid_level > 50)
            {
                foot_water = true;
            }
            else
            {
                foot_water = false;
            }
            if (input.server_says_pressed_keys.Contains('X') && block_place_wait_so_far >= block_place_wait_time)
            {
                block_place_wait_so_far = 0f;
                w.remove_line_of_blocks(world_loc, (float)mouse_angle, (float)Acc.get_distance(world_loc,input.mouse_loc), this, input.pressed_keys.Contains('`'));
                Particle.initialize_particle(w.bullets, Exilania.particle_manager.get_particle_id_by_name("Red Laser"), 5f, mouse_angle, (int)world_loc.X, (int)world_loc.Y, 1, false, d);
            }
            if (is_falling && !stats.jump_can_resume && !in_water)
            {
                stats.jump_cur_time = stats.jump_max_time;
            }
            if ((!is_falling || stats.jump_can_resume) && stats.jump_cur_time < stats.jump_max_time)
            { //still have jump power left.
                if (input.server_says_pressed_keys.Contains(' ')) //space is pressed and jumping is less than jump_fuel... still have fuel left in the tank.
                {
                    if ((stats.jump_max_time - stats.jump_cur_time) - sec_fraction > 0.0f) //if I have more than this current second fraction's time to jump left
                    {
                        if (stats.jump_cur_time == 0.0f)
                        {
                            inertia.Y = -1 * stats.jump_speed * (1f / w.world_gravity) * (in_water ? stats.water_jump_proficiency : 1f);
                            stats.jump_cur_time += sec_fraction;
                        }
                        else
                        {
                            stats.jump_cur_time += sec_fraction;
                            if (inertia.Y > 0)
                                inertia.Y = -.5f * stats.jump_speed * (1f / w.world_gravity) * (in_water ? stats.water_jump_proficiency : 1f);
                            inertia.Y -= stats.jump_speed * sec_fraction * (1f / w.world_gravity);
                        }
                    }
                    else //I have less than the current seconds fraction to jump left... use what I have left!
                    {
                        float jump_use = stats.jump_max_time - stats.jump_cur_time;
                        stats.jump_cur_time = stats.jump_max_time;
                        inertia.Y -= stats.jump_speed * jump_use * (1f / w.world_gravity) * (in_water ? stats.water_jump_proficiency : 1f);
                    }
                }
                else
                { //I have jumping fuel left, but I am not currently pressing spacebar to jump
                    if (stats.jump_cur_time > 0f && !stats.jump_can_resume && (!in_water || stats.can_swim)) 
                    { //if I have a partially used jump and I can't reuse it, destroy the rest of the current jump
                        stats.jump_cur_time = stats.jump_max_time;
                    }
                }
            }
            if (stats.boyant && in_water)
            {
                if (!input.server_says_pressed_keys.Contains('S'))
                {
                    float inertia_boyancy = 50 + (w.map[top_left.X,top_left.Y].liquid_level * 2);
                    stats.jump_cur_time = 0;
                    if (inertia.Y > -inertia_boyancy)
                        inertia.Y -= w.gravity_constant * w.fall_speed * w.world_gravity * sec_fraction;
                    if (inertia.Y < -inertia_boyancy)
                        inertia.Y = -inertia_boyancy;
                }
                else
                {
                    inertia.Y += w.gravity_constant * w.fall_speed * w.world_gravity * sec_fraction * liquid_speed_reduction;
                    if (inertia.Y > w.gravity_constant * w.world_gravity * (!in_water ? 1f : liquid_speed_reduction))
                        inertia.Y = w.gravity_constant * w.world_gravity * (!in_water ? 1f : liquid_speed_reduction);
                }
                platform_fall_y = 0;
            }
            else if (stats.can_swim && in_water)
            {
                if ((!input.server_says_pressed_keys.Contains(' ')) && w.map[top_left.X, top_left.Y + 1].liquid_level > 0 &&
                    w.liquid_simulator.liquid_cells[w.map[top_left.X, top_left.Y + 1].liquid_cell_id].tot_liquid_level +
                    w.liquid_simulator.liquid_cells[w.map[top_left.X, top_left.Y + 1].liquid_cell_id].pressure >= 400)
                {
                    stats.jump_cur_time = 0.0f;
                }
            }
            if (input.server_says_pressed_keys.Contains(' ') && stats.jump_cur_time < stats.jump_max_time)
            {
                platform_fall_y = 0;
            }
            else if(!(in_water && stats.boyant))
            { //not in water and or not boyant if both are true, don't do the below.
               inertia.Y += w.gravity_constant * w.fall_speed * w.world_gravity * sec_fraction;
               if (inertia.Y > w.gravity_constant * w.world_gravity * (!in_water ? 1f : liquid_speed_reduction))
                   inertia.Y = w.gravity_constant * w.world_gravity * (!in_water ? 1f : liquid_speed_reduction);
            }
            if (input.server_says_pressed_keys.Contains('D') && !input.server_says_pressed_keys.Contains('A'))
            {
                if (inertia.X < 0 || inertia.X == 0)
                    inertia.X = stats.run_speed * .2f;
                inertia.X += stats.run_speed * sec_fraction;
                if (inertia.X > stats.run_speed * (!in_water ? 1f : liquid_speed_reduction))
                    inertia.X = stats.run_speed * (!in_water ? 1f : liquid_speed_reduction);
                time_in_foot_state += sec_fraction;
                if (time_in_foot_state > time_at_change_foot_State)
                {
                    time_in_foot_state = 0;
                    if (foot_state_increase)
                        foot_state++;
                    else
                        foot_state--;
                    if (foot_state <= 0 || foot_state >= foot_angles.GetLength(0)-1)
                    {
                        if (foot_state < 0)
                            foot_state = 0;
                        else if (foot_state >= foot_angles.GetLength(0))
                            foot_state = foot_angles.GetLength(0) - 1;
                        foot_state_increase = !foot_state_increase;
                    }
                }
            }
            if (input.server_says_pressed_keys.Contains('A') && !input.server_says_pressed_keys.Contains('D'))
            {
                if (inertia.X > 0 || inertia.X == 0)
                    inertia.X = stats.run_speed * -.2f;
                inertia.X -= stats.run_speed * sec_fraction;
                if (inertia.X < stats.run_speed * (!in_water ? -1f : -liquid_speed_reduction))
                    inertia.X = stats.run_speed * (!in_water ? -1f : -liquid_speed_reduction);
                time_in_foot_state += sec_fraction;
                if (time_in_foot_state > time_at_change_foot_State)
                {
                    time_in_foot_state = 0;
                    if (foot_state_increase)
                        foot_state++;
                    else
                        foot_state--;
                    if (foot_state <= 0 || foot_state >= foot_angles.GetLength(0) - 1)
                    {
                        if (foot_state < 0)
                            foot_state = 0;
                        else if (foot_state >= foot_angles.GetLength(0))
                            foot_state = foot_angles.GetLength(0) - 1;
                        foot_state_increase = !foot_state_increase;
                    }
                   
                }
            }
            if (input.server_says_pressed_keys.Contains('D') == input.server_says_pressed_keys.Contains('A'))
            {
                if (Math.Abs(inertia.X) > 25)
                    inertia.X *= .90f;
                else
                    inertia.X = 0f;
                foot_state = (foot_angles.GetLength(0)-1)/2;
            }

            cur_sec_down_fall += sec_fraction;
            if ((!input.server_says_pressed_keys.Contains('S') && platform_fall_y - (int)((world_loc.Y - bounding_box.Y)) < 3) || min_sec_down_fall < cur_sec_down_fall)
            {
                cur_sec_down_fall = 0;
                platform_fall_y = 0;
            }
            if (input.server_says_pressed_keys.Contains('S') && platform_fall_y == 0)
            {
                platform_fall_y = (int)((world_loc.Y - bounding_box.Y) + 25);
            }


            block_place_wait_so_far += sec_fraction;

            if (active_chest_id > -1)
            {
                if (Acc.get_distance(world_loc, w.furniture[w.chests[active_chest_id].furniture_id].get_rect().Center) > ItemChest.max_range)
                    active_chest_id = -1;
            }

            do_furniture_clicking(w);
            if (is_furn_clicking)
            {
                if (w.wraparound_x(input.mouse_loc.X / 24) == furn_start_click.X && input.mouse_loc.Y / 24 == furn_start_click.Y && !items.click_consumed)
                {
                    perform_click(w, true, d,actor_id);
                }
            }
            else
            {
                Point mouse_screen_loc = new Point(input.mouse_loc.X - (int)w.top_left.X, input.mouse_loc.Y - (int)w.top_left.Y);
                if (active_chest_id > -1 && ItemChest.bounding_box.Contains(mouse_screen_loc))
                {
                    //treat this as if you have actually clicked on it.
                    if (ItemChest.allow_click && input.initial_left_click)
                    {
                        w.chests[active_chest_id].do_input(w, this, mouse_screen_loc);
                    }
                    else if (Exilania.input.mouse_now.LeftButton == ButtonState.Released)
                        ItemChest.allow_click = true;
                }
                else
                {
                    if (input.hold_left_mouse_click && !items.click_consumed)
                    {
                        perform_click(w, true, d, actor_id);
                    }
                    if (input.hold_right_mouse_click)
                    {
                        perform_click(w, false, d, actor_id);
                    }
                }
            }

            do_left_right_collision(w, sec_fraction);
            do_above_below_collision(w, sec_fraction);
            
            world_loc.X += (inertia.X * sec_fraction);
            world_loc.Y += (inertia.Y * sec_fraction);
            if (inertia.Y > 0)
            {
                if (!is_falling)
                {
                    
                    fall_start_loc = (int)world_loc.Y;
                }
                //you are currently falling!
                is_falling = true;

            }
            else
            {
                if (is_falling)
                {
                    is_falling = false;
                    int tot_fall_Dist = (int)world_loc.Y - fall_start_loc;
                    if (tot_fall_Dist > stats.max_safe_fall && !in_water)
                    {
                        int tot_dmg = (tot_fall_Dist - stats.max_safe_fall)/8;
                        if (tot_dmg > 100)
                            tot_dmg = 100;
                        if (tot_dmg > 0)
                        {
                            if (Exilania.game_server)
                            {
                                Exilania.network_server.damages.Add(new DamageMove(w.world_time + .0f, (ushort)ushort.MaxValue, TargetType.World, (ushort)actor_id,
                                    (items.is_player ? TargetType.Player : TargetType.NPC), (short)tot_dmg, CauseDamage.Fell, 0));
                            }
                            Exilania.display.add_message("@05Total fall damage: " + tot_dmg);
                        }

                        
                    }
                    fall_start_loc = -1;
                }
            }
            //move me in the world according to my inertia... which has been made sure not to be bad for me.
            if (world_loc.X < 0)
            {
                world_loc.X += w.map.GetLength(0) * 24;
            }
            else if (world_loc.X > w.map.GetLength(0) * 24)
            {
                world_loc.X -= w.map.GetLength(0) * 24;
            }
            if (world_loc.Y < bounding_box.Top * -1)
            {
                world_loc.Y = bounding_box.Top * -1;
                inertia.Y = 0;
            }
            else if (world_loc.Y > w.map.GetLength(1) * 24 + bounding_box.Top)
            {
                world_loc.Y = w.map.GetLength(1) * 24 + bounding_box.Top;
                inertia.Y = 0;
            }
            //if (inertia.Y != 0)
            //    foot_state = 0;
            //true is returned above if the actor/player has died.
            return false;
        }

        public void do_furniture_clicking(World w)
        {
            if (input.start_left_click < 200 && input.start_left_click > 0)
            {
                input.start_left_click = 0;
                block_place_wait_time = .1f;
                Point equiv_loc = new Point(w.wraparound_x((int)(input.mouse_loc.X / 24f)), (int)(input.mouse_loc.Y / 24f));
                if (w.map[equiv_loc.X, equiv_loc.Y].furniture_index != -1)
                {
                    int furn_index = w.map[equiv_loc.X, equiv_loc.Y].furniture_index;
                    if (w.furniture[furn_index].run_click_function(this, w))
                    {
                        Exilania.network_client.send_run_trigger(furn_index, 0);
                        last_break_block_loc.X = -1;
                    }
                }
            }
            if (input.initial_left_click)
            {
                furn_start_click = new Point(w.wraparound_x(input.mouse_loc.X / 24), input.mouse_loc.Y / 24);
                if (w.map[furn_start_click.X, furn_start_click.Y].furniture_index != -1)
                {
                    is_furn_clicking = true;
                    last_break_block_loc.X = -1;
                    break_block_swing = -1;
                }
            }
            if (input.end_left_click && is_furn_clicking)
            {
                Point start_pos = furn_start_click;
                is_furn_clicking = false;
                if (furn_start_click.X != -1)
                {
                    furn_start_click.X = -1;
                    Point furn_end_click = new Point(w.wraparound_x(input.mouse_loc.X / 24), input.mouse_loc.Y / 24);
                    if (w.map[furn_end_click.X, furn_end_click.Y].furniture_index != -1)
                    {
                        //there is furniture here... are the two furnitures matching?
                        int furn_start_index = w.map[start_pos.X, start_pos.Y].furniture_index;
                        int furn_end_index = w.map[furn_end_click.X, furn_end_click.Y].furniture_index;
                        if (furn_end_index != -1 && furn_start_index != -1 && furn_end_index != furn_start_index)
                        {
                            //not empty, and 2 different furnitures, connect them!
                            int x_dist = Acc.get_min_horizontal_distance(w.furniture[furn_start_index].get_rect().Center.X, w.furniture[furn_end_index].get_rect().Center.X, w.map.GetLength(0) * 24);
                            double dist = Acc.get_distance(0,w.furniture[furn_start_index].get_rect().Center.Y,x_dist, w.furniture[furn_end_index].get_rect().Center.Y);
                            Exilania.display.add_message("@05 x_dist: " + x_dist + "; actual dist: " + dist +  "; world cell size: " + CollisionHashTable.world_size_cell + ";");
                            //dist = Acc.get_distance(w.furniture[furn_start_index].get_rect().Center, w.furniture[furn_end_index].get_rect().Center);
                            if (dist <= CollisionHashTable.world_size_cell)
                            {
                                bool good = true;
                                for (int x = 0; x < w.furniture[furn_start_index].connections.Count; x++)
                                {
                                    if (w.furniture[furn_start_index].connections[x].target_type == TargetType.Furniture && w.furniture[furn_start_index].connections[x].target_id == furn_end_index)
                                    {
                                        good = false;
                                    }
                                }
                                if (good)
                                {
                                    if (w.furniture[furn_start_index].share_power < 255)
                                    {
                                        Exilania.network_client.send_furniture_connection(w.furniture[furn_start_index].top_left, w.furniture[furn_end_index].top_left, ItemConnectionType.PowerReceiving);
                                        Exilania.network_client.send_furniture_connection(w.furniture[furn_end_index].top_left, w.furniture[furn_start_index].top_left, ItemConnectionType.PowerCharging);
                                        w.furniture[furn_start_index].connections.Add(new ItemConnector(furn_end_index, TargetType.Furniture, ItemConnectionType.PowerReceiving, 0, 0));
                                        w.furniture[furn_end_index].connections.Add(new ItemConnector(furn_start_index, TargetType.Furniture, ItemConnectionType.PowerCharging, 0, 0));
                                    }
                                    else if (Exilania.furniture_manager.furniture[w.furniture[furn_start_index].furniture_id].default_connection_make != ItemConnectionType.Default)
                                    {
                                        Exilania.network_client.send_furniture_connection(w.furniture[furn_start_index].top_left, w.furniture[furn_end_index].top_left, 
                                            Exilania.furniture_manager.furniture[w.furniture[furn_start_index].furniture_id].default_connection_make);
                                        w.furniture[furn_start_index].connections.Add(new ItemConnector(furn_end_index, 
                                            TargetType.Furniture, Exilania.furniture_manager.furniture[w.furniture[furn_start_index].furniture_id].default_connection_make, 0, 0));
                                        if (Exilania.furniture_manager.furniture[w.furniture[furn_end_index].furniture_id].default_connection_make != ItemConnectionType.Default)
                                        {
                                            Exilania.network_client.send_furniture_connection(w.furniture[furn_end_index].top_left, w.furniture[furn_start_index].top_left,
                                            Exilania.furniture_manager.furniture[w.furniture[furn_end_index].furniture_id].default_connection_make);
                                            w.furniture[furn_end_index].connections.Add(new ItemConnector(furn_start_index,
                                                TargetType.Furniture, Exilania.furniture_manager.furniture[w.furniture[furn_end_index].furniture_id].default_connection_make, 0, 0));
                                        }
                                    }
                                }
                                else
                                {
                                    Exilania.network_client.send_furniture_disconnect(w.furniture[furn_start_index].top_left, w.furniture[furn_end_index].top_left);
                                    Exilania.network_client.send_furniture_disconnect(w.furniture[furn_end_index].top_left, w.furniture[furn_start_index].top_left);
                                    if (input.pressed_keys.Contains('Q') || input.pressed_keys.Contains('q'))
                                    {

                                        for (int x = 0; x < w.furniture[furn_start_index].connections.Count; x++)
                                        {
                                            if (w.furniture[furn_start_index].connections[x].target_type == TargetType.Furniture && w.furniture[furn_start_index].connections[x].target_id == furn_end_index)
                                            {
                                                w.furniture[furn_start_index].connections.RemoveAt(x);
                                                x--;
                                            }
                                        }
                                        for (int x = 0; x < w.furniture[furn_end_index].connections.Count; x++)
                                        {
                                            if (w.furniture[furn_end_index].connections[x].target_type == TargetType.Furniture && w.furniture[furn_end_index].connections[x].target_id == furn_start_index)
                                            {
                                                w.furniture[furn_end_index].connections.RemoveAt(x);
                                                x--;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void do_above_below_collision(World w, float sec_fraction)
        {
            bool works_one = true;
            bool works_two = true;
            Point return_to;
            int height_char = (int)Math.Ceiling(((float)bounding_box.Height) / 24f);
            int width_char = (int)Math.Ceiling(((float)bounding_box.Width) / 24f) - 1;

            Vector2 top_left = new Vector2(world_loc.X + bounding_box.X + (inertia.X * sec_fraction), world_loc.Y + bounding_box.Y + (inertia.Y * sec_fraction));
            Vector2 bottom_right = new Vector2(world_loc.X - bounding_box.X + (inertia.X * sec_fraction), world_loc.Y - bounding_box.Y + (inertia.Y * sec_fraction));


            //right bottom and top calculations
            return_to = new Point((int)top_left.X / 24, (int)bottom_right.Y / 24);
            for (int x = 0; x <= width_char; x++)
            {
                if (return_to.Y >= w.map.GetLength(1) || !w.map[w.wraparound_x(return_to.X + x), return_to.Y].passable ||
                    (w.map[w.wraparound_x(return_to.X + x), return_to.Y].fgd_block_type == 3 && platform_fall_y < return_to.Y * 24) 
                    || (w.map[w.wraparound_x(return_to.X + x), return_to.Y].furniture_index != -1 &&
                    !w.furniture[w.map[w.wraparound_x(return_to.X + x), return_to.Y].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    works_one = false;
                if (return_to.Y - height_char < 0 || !w.map[w.wraparound_x(return_to.X + x), return_to.Y - height_char].passable
                    || (w.map[w.wraparound_x(return_to.X + x), return_to.Y - height_char].furniture_index != -1 &&
                    !w.furniture[w.map[w.wraparound_x(return_to.X + x), return_to.Y - height_char].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    works_two = false;

            }
            //left bottom and top calculations
            return_to = new Point((int)Math.Floor((top_left.X + (float)bounding_box.Width - 1) / 24f), (int)bottom_right.Y / 24);
            for (int x = 0; x <= width_char; x++)
            {
                if (return_to.Y >= w.map.GetLength(1) || !w.map[w.wraparound_x(return_to.X - x), return_to.Y].passable ||
                    (w.map[w.wraparound_x(return_to.X - x), return_to.Y].fgd_block_type == 3 && platform_fall_y < return_to.Y * 24)
                     || (w.map[w.wraparound_x(return_to.X - x), return_to.Y].furniture_index != -1 &&
                    !w.furniture[w.map[w.wraparound_x(return_to.X - x), return_to.Y].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    works_one = false;
                if (return_to.Y - height_char < 0 || !w.map[w.wraparound_x(return_to.X - x), return_to.Y - height_char].passable 
                    || (w.map[w.wraparound_x(return_to.X - x), return_to.Y - height_char].furniture_index != -1 &&
                    !w.furniture[w.map[w.wraparound_x(return_to.X - x), return_to.Y - height_char].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    works_two = false;

            }
            if (!works_one && inertia.Y > 0)
            {
                world_loc.Y = (return_to.Y * 24) + bounding_box.Y;
                if (!input.server_says_pressed_keys.Contains(' '))
                    stats.jump_cur_time = 0f;
                inertia.Y = 0;
                set_torso_rotation(0);
            }
            else if (works_one)
            {
                if (foot_water)
                {
                    set_torso_rotation(30);
                }
                else
                {
                    set_torso_rotation(0);
                    foot_state = 0;
                }
                /*if (!stats.jump_can_resume && !input.server_says_pressed_keys.Contains(' '))
                {
                    stats.jump_cur_time = stats.jump_max_time;
                }*/
            }
            if (!works_two && inertia.Y < 0)
            {
                inertia.Y = 0;
                world_loc.Y = (return_to.Y * 24) + bounding_box.Y + 24;
            }
        }

        public void do_left_right_collision(World w, float sec_fraction)
        {
            ///
            /// true indicates you are allowed to move left
            ///
            bool can_move_left = true;
            ///
            /// true indicates you are allowed to move right
            ///
            bool can_move_right = true;
            Vector2 top_left = new Vector2(world_loc.X + bounding_box.X + (inertia.X * sec_fraction), world_loc.Y + bounding_box.Y);
            Vector2 bottom_right = new Vector2(world_loc.X - bounding_box.X + (inertia.X * sec_fraction), world_loc.Y - bounding_box.Y + (inertia.Y * sec_fraction));
            Point return_to;
            int height_char = (int)Math.Ceiling(((float)bounding_box.Height) / 24f);
            int width_char = (int)Math.Ceiling(((float)bounding_box.Width) / 24f) - 1;

            //left collision
            return_to = new Point((int)Math.Floor(top_left.X / 24f), (int)Math.Floor(top_left.Y / 24f));
            step_up_left = false;
            for (int y = 0; y < height_char; y++)
            {
                if (y == height_char - 1 && can_move_left && inertia.X < 0)
                    step_up_left = true;
                if (  (!w.map[w.wraparound_x(return_to.X), return_to.Y + y].passable
                    || (y == height_char-1 && 
                    w.map[w.wraparound_x(return_to.X), return_to.Y + y].fgd_block > -1 &&
                    Exilania.block_types.blocks[w.map[w.wraparound_x(return_to.X), return_to.Y + y].fgd_block].platform &&
                    last_step_up_left_x != (int)world_loc.X / 24 && input.pressed_keys.Contains('W'))
                    ) || 
                    (w.map[w.wraparound_x(return_to.X), return_to.Y + y].furniture_index!= -1 && 
                    !w.furniture[w.map[w.wraparound_x(return_to.X), return_to.Y + y].furniture_index].flags[(int)FFLAGS.PASSABLE] ))
                    can_move_left = false;
                //else
                //    step_up_left = false;

            }
            //if there are blocks above the players head, they may not move up!
            if (step_up_left)
            {
                for (int x = 0; x < width_char + 2; x++)
                {
                    if (return_to.Y-1 < 0 || !w.map[w.wraparound_x(return_to.X + x), return_to.Y-1].passable ||
                        (w.map[w.wraparound_x(return_to.X + x), return_to.Y -1].furniture_index != -1 &&
                    !w.furniture[w.map[w.wraparound_x(return_to.X + x), return_to.Y-1].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    {
                        step_up_left = false;
                    }
                }
            }
            //right collision
            return_to = new Point((int)bottom_right.X / 24, (int)top_left.Y / 24);
            step_up_right = false;
            for (int y = 0; y < height_char; y++)
            {
                if (y == height_char - 1 && can_move_right && inertia.X > 0)
                    step_up_right = true;
                if ((!w.map[w.wraparound_x(return_to.X), return_to.Y + y].passable || (y == height_char-1 && 
                    w.map[w.wraparound_x(return_to.X), return_to.Y + y].fgd_block > -1 && 
                    Exilania.block_types.blocks[w.map[w.wraparound_x(return_to.X), return_to.Y + y].fgd_block].platform && 
                    input.pressed_keys.Contains('W') && last_Step_up_right_x != (int)world_loc.X / 24)
                    )||
                    (w.map[w.wraparound_x(return_to.X), return_to.Y + y].furniture_index != -1 &&
                    !w.furniture[w.map[w.wraparound_x(return_to.X), return_to.Y + y].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    can_move_right = false;
               // else
               //     step_up_right = false;
            }
            //if there are blocks above the players head, they may not move up!
            if (step_up_right)
            {
              
                for (int x = -2; x < width_char; x++)
                {
                    if (return_to.Y - 1 < 0 || !w.map[w.wraparound_x(return_to.X + x), return_to.Y - 1].passable ||
                        (w.map[w.wraparound_x(return_to.X + x), return_to.Y - 1].furniture_index != -1 &&
                    !w.furniture[w.map[w.wraparound_x(return_to.X + x), return_to.Y - 1].furniture_index].flags[(int)FFLAGS.PASSABLE]))
                    {
                        step_up_right = false;
                    }
                }
            }
            //left collision
            if (!can_move_left && inertia.X < 0)
            {
                if (step_up_left)
                {
                    last_step_up_left_x = (int)world_loc.X / 24;
                    //you are allowed to move left!
                    if (!in_water || (in_water && !stats.boyant))
                        world_loc.Y -= 4;
                    else
                        world_loc.Y -= 24;
                    //world_loc.X -= 4;
                    inertia.X *= .8f;
                }
                else
                {
                    inertia.X = 0;
                    //if (can_move_right)
                    //    world_loc.X = ((top_left.X/24) * 24) + (bounding_box.Width/2) + 1f;
                }
            }
            //right collision
            if (!can_move_right && inertia.X > 0)
            {
                if (step_up_right)
                {
                    last_Step_up_right_x = (int)world_loc.X / 24;
                    //you are allowed to move right!
                    if (!in_water || (in_water && !stats.boyant))
                        world_loc.Y -= 4;
                    else
                        world_loc.Y -= 24;
                   // world_loc.X += 4;
                    inertia.X *= .8f;
                }
                else
                {
                    inertia.X = 0;
                    if (can_move_left)
                        world_loc.X = (return_to.X * 24) + bounding_box.X;
                }
            }
        }


        public void update_body(float sec_fraction, float world_start_time, Display d, Input i, World w, bool is_client_character, bool is_player, int actor_id)
        {
            if (input.server_says_pressed_keys == "")
            {
                last_button_push += sec_fraction;
            }
            else
                last_button_push = 0f;
            if (!Exilania.game_server && last_button_push > Actor_Input.cur_server_roundtrip_time && !(dloc_server.X == 0 && dloc_server.Y == 0))
            {
                total_off += (float)Acc.get_distance(dloc_server, world_loc);
                if (total_off > 100)
                {
                    world_loc = dloc_server;
                    dloc_server = new Vector2(0, 0);
                    total_off = 0;
                    last_button_push = 0f;
                }
            }
            //addup the difference each frame, start displaying it, that will give me a threshold!
            /*if (dloc_server.X > 0)
            {
                if (Math.Abs((((float)dloc_server.X - world_loc.X) * sec_fraction * 5f)) < 1)
                {
                    world_loc.X = dloc_server.X;
                    dloc_server.X = 0;
                }
                else
                    world_loc.X += (((float)dloc_server.X - world_loc.X) * sec_fraction * 5f);
            }
            if(dloc_server.Y > 0)
            {
                if (Math.Abs((((float)dloc_server.Y - world_loc.Y) * sec_fraction * 5f)) < 1)
                {
                    //world_loc.Y += Math.Sign(((float)dloc_server.Y - (float)world_loc.Y));
                    world_loc.Y = dloc_server.Y;
                    dloc_server.Y = 0;
                }
                else
                    world_loc.Y += (((float)dloc_server.Y - world_loc.Y) * sec_fraction * 5f); 
            }*/
            w.collision_table.modify_actor_in_table(this, is_player, actor_id, false);
           
            float amt = 0f;
            for (amt = 0f; amt < sec_fraction; amt += 1f/180f)
            {
                world_start_time += 1f / 180f;
                input.fine_mesh_update_input(world_start_time, this);
                update_movement(1f / 180f, d, w,actor_id);
            }
            if (sec_fraction - amt > 0f)
            {
                world_start_time += sec_fraction - amt;
                input.fine_mesh_update_input(world_start_time, this);
                update_movement(sec_fraction - amt, d, w, actor_id);
            }
            w.collision_table.modify_actor_in_table(this, is_player, actor_id, true);
            if (is_client_character)
            {
                screen_loc = focus_actor(w);
                Vector2 center = new Vector2(world_loc.X, world_loc.Y);
                if (i.ctrl_state == 1)
                {
                    center -= i.saved_delta;
                    Vector2 top_left = new Vector2(center.X - (Exilania.screen_size.X / 2), center.Y - (Exilania.screen_size.Y / 2));
                    if (top_left.Y < 0)
                        top_left.Y = 0;
                    else if (top_left.Y > w.map.GetLength(1) * 24 - (Exilania.screen_size.Y))
                    {
                        top_left.Y = w.map.GetLength(1) * 24 - Exilania.screen_size.Y;
                    }
                    screen_loc = new Vector2(world_loc.X - top_left.X, world_loc.Y - top_left.Y);
                }
            }
            else
            {
                if (Exilania.game_my_user_id != -1)
                {
                    screen_loc.X = w.players[Exilania.game_my_user_id].avatar.screen_loc.X + (world_loc.X - w.players[Exilania.game_my_user_id].avatar.world_loc.X);
                    screen_loc.Y = w.players[Exilania.game_my_user_id].avatar.screen_loc.Y + (world_loc.Y - w.players[Exilania.game_my_user_id].avatar.world_loc.Y);
                }
                else
                {
                    screen_loc.X = w.players[0].avatar.screen_loc.X + (world_loc.X - w.players[0].avatar.world_loc.X);
                    screen_loc.Y = w.players[0].avatar.screen_loc.Y + (world_loc.Y - w.players[0].avatar.world_loc.Y);
                }
                if (screen_loc.X + 100 > w.map.GetLength(0) * 24f)
                {
                    screen_loc.X -= w.map.GetLength(0) * 24f;
                }
                else if (screen_loc.X < 0 && screen_loc.X + w.map.GetLength(0) * 24f < Exilania.screen_size.X + 100)
                {
                    screen_loc.X += w.map.GetLength(0) * 24f;
                }
            }
            update_facing(input);
            int parent_id = get_parent_part_id();
            if (parent_id != -1)
            {
                body[parent_id].grid_location = screen_loc;

                mouse_angle = Math.Atan2(input.mouse_loc.Y - world_loc.Y, input.mouse_loc.X - world_loc.X);
                body[parent_id].rotation = 0;
                if (body[parent_id].follow_mouse)
                {
                    if (body[parent_id].click_active_mouse)
                    {
                        if (action_swinging && 
                            (
                                (action_swinging_right && body[parent_id].name.Contains("Right"))
                                ||
                                (!action_swinging_right && body[parent_id].name.Contains("Left"))
                           ))
                        {
                            body[parent_id].rotation = action_start + (float)(action_end - action_start) * (cur_action_swing_time/action_swing_time);
                            if (!facing_right)
                                body[parent_id].rotation = (float)Math.PI - body[parent_id].rotation;
                        }
                        else
                            body[parent_id].rotation = (float)mouse_angle;
                    }
                    else
                        body[parent_id].rotation = (float)mouse_angle;
                }
                if (facing_right)
                    body[parent_id].rotation += Acc.DegreeToRadian(body[parent_id].rotation_offset);
                else
                {
                    body[parent_id].rotation += (float)(Math.PI) -  Acc.DegreeToRadian(body[parent_id].rotation_offset);
                }
                for (int x = 0; x < body[parent_id].child_ids.Count; x++)
                {
                    update_children( body[parent_id].child_ids[x]);
                }
            }
            if (screen_loc.X > -100 && screen_loc.X < Exilania.screen_size.X + 100 &&
                screen_loc.Y > -100 && screen_loc.Y < Exilania.screen_size.Y + 100)
            light_actor(w);
        }

        private void update_children(int child_id)
        {

            Double mouse_angle = Math.Atan2(input.mouse_loc.Y - world_loc.Y, input.mouse_loc.X - world_loc.X);
            body[child_id].rotation = 0;
            if (body[child_id].follow_mouse)
            {
                if (body[child_id].click_active_mouse)
                {
                    if (action_swinging &&
                            (
                                (action_swinging_right && body[child_id].name.Contains("Right"))
                                ||
                                (!action_swinging_right && body[child_id].name.Contains("Left"))
                           ))
                    {
                        body[child_id].rotation = action_start + (float)(action_end - action_start) * (cur_action_swing_time / action_swing_time);
                        if (!facing_right)
                            body[child_id].rotation = (float)Math.PI - body[child_id].rotation;
                    }
                    else
                        body[child_id].rotation = (float)mouse_angle;
                }
                else
                    body[child_id].rotation = (float)mouse_angle;
                if (!facing_right)
                    body[child_id].rotation += (float)(Math.PI);
            }
            if (body[child_id].follow_parent)
            {
                if (facing_right)
                    body[child_id].rotation = body[body[child_id].parent_id].rotation;
                else
                    body[child_id].rotation = body[body[child_id].parent_id].rotation + (float)(Math.PI);
            }
            else if (body[child_id].walking != -1)
            {
                if (facing_right)
                    body[child_id].rotation = body[body[child_id].parent_id].rotation;
                else
                    body[child_id].rotation = body[body[child_id].parent_id].rotation + (float)(Math.PI);
                switch (body[child_id].walking)
                {
                    case 0:
                        if (facing_right)
                            body[child_id].rotation += Acc.DegreeToRadian(foot_angles[foot_state]);
                        else
                            body[child_id].rotation += Acc.DegreeToRadian(foot_angles[foot_angles.GetLength(0) - 1 - foot_state]);
                        break;
                    case 1:
                        if (facing_right)
                            body[child_id].rotation += Acc.DegreeToRadian(foot_angles[foot_angles.GetLength(0) - 1 - foot_state]);
                        else
                            body[child_id].rotation += Acc.DegreeToRadian(foot_angles[foot_state]);
                        break;
                    case 2:
                        if (facing_right)
                            body[child_id].rotation += Acc.DegreeToRadian(foot_angles[foot_angles.GetLength(0) - 1 - foot_state] / 2);
                        else
                            body[child_id].rotation += Acc.DegreeToRadian(foot_angles[foot_state] / 2);
                        break;
                }
            }
            if (facing_right)
                body[child_id].rotation += Acc.DegreeToRadian(body[child_id].rotation_offset);
            else
            {
                body[child_id].rotation -= Acc.DegreeToRadian(body[child_id].rotation_offset) + (float)(Math.PI * 1f);
            }

            double base_angle;
            double distance;
            Vector2 parent_attach_loc;
            Vector2 child_attach_loc;

            if (facing_right)
            {
                // 1. Calculate the angle from the center of the parent to the attachment point on the parent. (Atan2)
                base_angle = Math.Atan2(body[child_id].parent_attachment_point.Y - body[body[child_id].parent_id].center.Y,
                    body[child_id].parent_attachment_point.X - body[body[child_id].parent_id].center.X);
                // 1a. distance = the distance between the center and the attachment point
                distance = Acc.get_distance(body[child_id].parent_attachment_point, body[body[child_id].parent_id].center);
                // 2. Add to that angle, the rotation of the parent (in radians as well, of course)
                base_angle += body[body[child_id].parent_id].rotation;
                // 3. use Cos(angle) * distance to get the new X
                parent_attach_loc = new Vector2();
                parent_attach_loc.X = (float)(Math.Cos(base_angle) * distance * body[child_id].magnification);
                // 4. use Sin(angle) * distance to get the new Y
                parent_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * body[child_id].magnification);
                // 5. normalize results by rounding them.(very optional, probably better if you don't)

                base_angle = Math.Atan2(body[child_id].center.Y - body[child_id].my_attachment_point.Y, body[child_id].center.X - body[child_id].my_attachment_point.X);
                distance = Acc.get_distance(body[child_id].my_attachment_point, body[child_id].center);
                base_angle += body[child_id].rotation;
                child_attach_loc = new Vector2();
                child_attach_loc.X = (float)(Math.Cos(base_angle) * distance * body[child_id].magnification);
                child_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * body[child_id].magnification);
                body[child_id].grid_location = body[body[child_id].parent_id].grid_location + parent_attach_loc + child_attach_loc;// +(body[child_id].my_attachment_point * (float)d.magnification);

            }
            else
            { //facing left version... a wee bit frustrating.
                // 1. Calculate the angle from the center of the parent to the attachment point on the parent. (Atan2)
                base_angle = (Math.PI * 2) - Math.Atan2(body[child_id].parent_attachment_point.Y - body[body[child_id].parent_id].center.Y,
                    body[child_id].parent_attachment_point.X - body[body[child_id].parent_id].center.X);
                // 1a. distance = the distance between the center and the attachment point
                distance = Acc.get_distance(body[child_id].parent_attachment_point, body[body[child_id].parent_id].center);
                // 2. Add to that angle, the rotation of the parent (in radians as well, of course)
                base_angle += body[body[child_id].parent_id].rotation;
                // 3. use Cos(angle) * distance to get the new X
                parent_attach_loc = new Vector2();
                parent_attach_loc.X = (float)(Math.Cos(base_angle) * distance * body[child_id].magnification);
                // 4. use Sin(angle) * distance to get the new Y
                parent_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * body[child_id].magnification);
                // 5. normalize results by rounding them.(very optional, probably better if you don't)

                base_angle = (Math.PI * 2) - Math.Atan2(body[child_id].center.Y - body[child_id].my_attachment_point.Y, body[child_id].center.X - body[child_id].my_attachment_point.X);
                distance = Acc.get_distance(body[child_id].my_attachment_point, body[child_id].center);
                base_angle += body[child_id].rotation;
                child_attach_loc = new Vector2();
                child_attach_loc.X = (float)(Math.Cos(base_angle) * distance * body[child_id].magnification);
                child_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * body[child_id].magnification);
                body[child_id].grid_location = body[body[child_id].parent_id].grid_location + parent_attach_loc + child_attach_loc;
            }
            for (int x = 0; x < body[child_id].child_ids.Count; x++)
            {
                update_children(body[child_id].child_ids[x]);
            }
        }

        public void update_static_body(float mag)
        {
            update_facing(input);
            int parent_id = get_parent_part_id();
            if (parent_id != -1)
            {
                body[parent_id].grid_location = screen_loc;

                Double mouse_angle = Math.Atan2(input.mouse_loc.Y - world_loc.Y, input.mouse_loc.X - world_loc.X);
                body[parent_id].rotation = 0;
                if (body[parent_id].follow_mouse)
                {
                    body[parent_id].rotation = (float)mouse_angle;
                }
                if (facing_right)
                    body[parent_id].rotation += Acc.DegreeToRadian(body[parent_id].rotation_offset);
                else
                {
                    body[parent_id].rotation += (float)(Math.PI) - Acc.DegreeToRadian(body[parent_id].rotation_offset);
                }
                for (int x = 0; x < body[parent_id].child_ids.Count; x++)
                {
                    update_children(body[parent_id].child_ids[x], mag);
                }
            }
        }

        private void update_children(int child_id, float mag)
        {

            Double mouse_angle = Math.Atan2(input.mouse_loc.Y - world_loc.Y, input.mouse_loc.X - world_loc.X);
            body[child_id].rotation = 0;
            if (body[child_id].follow_mouse)
            {
                if (facing_right)
                    body[child_id].rotation = (float)mouse_angle;
                else
                    body[child_id].rotation = (float)mouse_angle + (float)(Math.PI);
            }
            if (body[child_id].follow_parent)
            {
                if (facing_right)
                    body[child_id].rotation = body[body[child_id].parent_id].rotation;
                else
                    body[child_id].rotation = body[body[child_id].parent_id].rotation + (float)(Math.PI);
            }
            else if (body[child_id].walking != -1)
            {
                switch (body[child_id].walking)
                {
                    case 0:
                        if (facing_right)
                            body[child_id].rotation = Acc.DegreeToRadian(foot_angles[foot_state]);
                        else
                            body[child_id].rotation = Acc.DegreeToRadian(foot_angles[foot_angles.GetLength(0) - 1 - foot_state]);
                        break;
                    case 1:
                        if (facing_right)
                            body[child_id].rotation = Acc.DegreeToRadian(foot_angles[foot_angles.GetLength(0) - 1 - foot_state]);
                        else
                            body[child_id].rotation = Acc.DegreeToRadian(foot_angles[foot_state]);
                        break;
                    case 2:
                        if (facing_right)
                            body[child_id].rotation = Acc.DegreeToRadian(foot_angles[foot_angles.GetLength(0) - 1 - foot_state] / 2);
                        else
                            body[child_id].rotation = Acc.DegreeToRadian(foot_angles[foot_state] / 2);
                        break;
                }
            }
            if (facing_right)
                body[child_id].rotation += Acc.DegreeToRadian(body[child_id].rotation_offset);
            else
            {
                body[child_id].rotation -= Acc.DegreeToRadian(body[child_id].rotation_offset) + (float)(Math.PI * 1f);
            }

            double base_angle;
            double distance;
            Vector2 parent_attach_loc;
            Vector2 child_attach_loc;

            if (facing_right)
            {
                // 1. Calculate the angle from the center of the parent to the attachment point on the parent. (Atan2)
                base_angle = Math.Atan2(body[child_id].parent_attachment_point.Y - body[body[child_id].parent_id].center.Y,
                    body[child_id].parent_attachment_point.X - body[body[child_id].parent_id].center.X);
                // 1a. distance = the distance between the center and the attachment point
                distance = Acc.get_distance(body[child_id].parent_attachment_point, body[body[child_id].parent_id].center);
                // 2. Add to that angle, the rotation of the parent (in radians as well, of course)
                base_angle += body[body[child_id].parent_id].rotation;
                // 3. use Cos(angle) * distance to get the new X
                parent_attach_loc = new Vector2();
                parent_attach_loc.X = (float)(Math.Cos(base_angle) * distance * mag);
                // 4. use Sin(angle) * distance to get the new Y
                parent_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * mag);
                // 5. normalize results by rounding them.(very optional, probably better if you don't)

                base_angle = Math.Atan2(body[child_id].center.Y - body[child_id].my_attachment_point.Y, body[child_id].center.X - body[child_id].my_attachment_point.X);
                distance = Acc.get_distance(body[child_id].my_attachment_point, body[child_id].center);
                base_angle += body[child_id].rotation;
                child_attach_loc = new Vector2();
                child_attach_loc.X = (float)(Math.Cos(base_angle) * distance * mag);
                child_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * mag);
                body[child_id].grid_location = body[body[child_id].parent_id].grid_location + parent_attach_loc + child_attach_loc;// +(body[child_id].my_attachment_point * (float)d.magnification);

            }
            else
            { //facing left version... a wee bit frustrating.
                // 1. Calculate the angle from the center of the parent to the attachment point on the parent. (Atan2)
                base_angle = (Math.PI * 2) - Math.Atan2(body[child_id].parent_attachment_point.Y - body[body[child_id].parent_id].center.Y,
                    body[child_id].parent_attachment_point.X - body[body[child_id].parent_id].center.X);
                // 1a. distance = the distance between the center and the attachment point
                distance = Acc.get_distance(body[child_id].parent_attachment_point, body[body[child_id].parent_id].center);
                // 2. Add to that angle, the rotation of the parent (in radians as well, of course)
                base_angle += body[body[child_id].parent_id].rotation;
                // 3. use Cos(angle) * distance to get the new X
                parent_attach_loc = new Vector2();
                parent_attach_loc.X = (float)(Math.Cos(base_angle) * distance * mag);
                // 4. use Sin(angle) * distance to get the new Y
                parent_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * mag);
                // 5. normalize results by rounding them.(very optional, probably better if you don't)

                base_angle = (Math.PI * 2) - Math.Atan2(body[child_id].center.Y - body[child_id].my_attachment_point.Y, body[child_id].center.X - body[child_id].my_attachment_point.X);
                distance = Acc.get_distance(body[child_id].my_attachment_point, body[child_id].center);
                base_angle += body[child_id].rotation;
                child_attach_loc = new Vector2();
                child_attach_loc.X = (float)(Math.Cos(base_angle) * distance * mag);
                child_attach_loc.Y = (float)(Math.Sin(base_angle) * distance * mag);
                body[child_id].grid_location = body[body[child_id].parent_id].grid_location + parent_attach_loc + child_attach_loc;
            }
            for (int x = 0; x < body[child_id].child_ids.Count; x++)
            {
                update_children(body[child_id].child_ids[x], mag);
            }
        }

        public void light_actor(World w)
        {
            Point t_loc = new Point();
            for (int x = 0; x < body.Count; x++)
            {
               
                if (w == null)
                {
                    light_level = Color.White;
                    body[x].resultant_color = body[x].draw_color;
                }
                else
                { 
                    t_loc = new Point(w.wraparound_x(((int)w.top_left.X + (int)body[x].grid_location.X) / 24), ((int)w.top_left.Y + (int)body[x].grid_location.Y) / 24);
                    if (t_loc.Y >= w.map.GetLength(1))
                        t_loc.Y = w.map.GetLength(1) - 1;
                    body[x].resultant_color = Acc.multiply_colors(w.map[t_loc.X, t_loc.Y].light_level, body[x].draw_color);
                    if (items != null)
                    {
                        body[x].resultant_color = Acc.multiply_colors_opacity(body[x].resultant_color, Exilania.cur_showing);
                    }
                    /*body[x].resultant_color.R = (byte)((float)w.map[t_loc.X, t_loc.Y].light_level.R / 255f * (float)body[x].draw_color.R);
                    body[x].resultant_color.G = (byte)((float)w.map[t_loc.X, t_loc.Y].light_level.G / 255f * (float)body[x].draw_color.G);
                    body[x].resultant_color.B = (byte)((float)w.map[t_loc.X, t_loc.Y].light_level.B / 255f * (float)body[x].draw_color.B);*/
                    if (x == 0)
                    {
                        light_level = w.map[t_loc.X, t_loc.Y].light_level;
                    }
                }
            }
        }

        public void draw_actor(SpriteBatch s, float mag, Display d)
        {
            if (screen_loc.X > -100 && screen_loc.X < Exilania.screen_size.X + 100 &&
                screen_loc.Y > -100 && screen_loc.Y < Exilania.screen_size.Y + 100)
            {
                for (int x = 0; x < body.Count; x++)
                {
                    body[x].draw(s, mag, d, facing_right);
                }
                if (Exilania.draw_debug)
                {
                    //s.Draw(d.sprites, new Rectangle((int)screen_loc.X + bounding_box.X, (int)screen_loc.Y + bounding_box.Y, bounding_box.Width, bounding_box.Height), new Rectangle(1139, 64, 7, 13), Color.White);
                }
               
            }
        }

        public void draw_hud(SpriteBatch s, Display d, bool show_name, bool show_life)
        {
            if (show_name)
            {
                Vector2 string_size = d.small_font.MeasureString(name);
                d.draw_text_with_outline(s, d.small_font, "@00" + name, (int)screen_loc.X - ((int)string_size.X / 2), (int)screen_loc.Y - 71, 600, AccColors.Blue);
            }
            if (show_life)
            {
                stats.life.draw_at(s, new Point((int)screen_loc.X - 40, (int)screen_loc.Y - 48), 80, 10, 0);
            }
        }

        public void draw_headsupdisplay(SpriteBatch s, Display d)
        {
            //696 33 24x24
            //769 149 72,72

        }

        public override string ToString()
        {
            return //world_loc.ToString() + " Server: " + platform_fall_y.ToString() + " " + 
                (step_up_left ? "@04" : "@07") + "L" + 
                (step_up_right ? "@04" : "@07") + "R" +
                (in_water ? "@04" : "@07") + "W" +
                (foot_water ? "@04" : "@07") + "F" +
                (hold_breath ? "@04" : "@07") + "B" +
                (is_falling ? "@04" : "@07") + "Fa" + 
                stats.jump_cur_time;
        }
    }
}
