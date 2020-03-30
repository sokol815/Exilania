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

namespace Exilania
{
    public class ParentBrassPin
    {
        public Point parent_loc;
        public string child_name;

    }

    public struct ChildPinPicture
    {
        public Rectangle image;
        public Point child_loc;
    }
    public class BodypartTemplate
    {
        public string name;

        public int angle_offset;
        public bool angle_follow_mouse;
        public bool click_active_mouse;
        public bool angle_follow_parent;
        public int angle_walking_id; //positive/0 indicates follow the walking id. -1 indicates ignore
        public List<ChildPinPicture> images;
        public List<ParentBrassPin> children;
        public int draw_order;
        public string parent_name;
        public bool no_color;

        public BodypartTemplate()
        {
            images = new List<ChildPinPicture>();
            children = new List<ParentBrassPin>();
            name = "";
            angle_offset = 0;
            angle_follow_mouse = false;
            click_active_mouse = false;
            angle_follow_parent = false;
            no_color = false;
            angle_walking_id = -1;
            draw_order = 0;
            parent_name = "";
        }
    }

    public class BodyTemplate
    {
        public List<BodypartTemplate> parts_list;
        public string template_name;
        public Rectangle size_body;
        int num_torsos;
        /// <summary>
        /// this is for when a body is being reconstructed from file.
        /// </summary>
        int cur_part_id_reading_from_file;
        int leg_chosen_pic = 0;
        int foot_chosen_pic = 0;
        public int body_template_id = 0;
        public List<Color> skins;
        public int num_parts = 0;

        public BodyTemplate()
        {
            skins = new List<Color>();
        }

        public BodyTemplate(System.IO.StreamReader r,int template_num)
        {
            body_template_id = template_num;
            BodypartTemplate temp = new BodypartTemplate();
            parts_list = new List<BodypartTemplate>();
            skins = new List<Color>();
            string line = "";
            bool cont = true;
            while (cont)
            {
                line = r.ReadLine();
                if (line.Trim() == "" || line[0] == '#')
                {
                    //skip this line
                }
                else
                {
                    string[] items = line.Split(':');
                    switch (items[0].ToLower())
                    {
                        case "color":
                            if (items[1].ToLower() == "none")
                            {
                                temp.no_color = true;
                            }
                            break;
                        case "skins":
                            string[] colors = items[1].Split(';');
                            for(int i = 0; i < colors.Length;i++)
                            {
                                string[] comps = colors[i].Split(','); 
                                skins.Add(Color.FromNonPremultiplied(byte.Parse(comps[0]),byte.Parse(comps[1]),byte.Parse(comps[2]),255));
                            }
                             break;
                        case "body":
                            template_name = items[1];
                            break;
                        case "size":
                            string[] tdim = Acc.script_remove_outer_parentheses(items[1]).Split(',');
                            size_body = new Rectangle(int.Parse(tdim[0]) / -2, int.Parse(tdim[1]) / -2, int.Parse(tdim[0]), int.Parse(tdim[1])); 
                            break;
                        case "num-torsos":
                            num_torsos = Int32.Parse(items[1]);
                            break;
                        case "endbody":
                            parts_list.Add(temp);
                            num_parts++;
                            cont = false;
                            break;
                        case "part":
                            if (temp.name == "")
                            { //first time... already default
                                temp.name = items[1];
                            }
                            else
                            {//not the first time, add to stack, then default the temp and give it it's name.
                                parts_list.Add(temp);
                                num_parts++;
                                temp = new BodypartTemplate();
                                temp.name = items[1];
                            }
                            break;
                        case "angle":
                            if (items[1].Contains("CLICKMOUSE"))
                            {
                                temp.click_active_mouse = true;
                                temp.angle_follow_mouse = true;
                                if (items[1].Length > 10)
                                {
                                    items[1] = items[1].Remove(items[1].IndexOf("CLICKMOUSE"), 10);
                                    temp.angle_offset = Acc.resolve_die_roll(items[1], 0, 0);
                                }
                            }
                            else if (items[1].Contains("MOUSE"))
                            {
                                temp.angle_follow_mouse = true;
                                if (items[1].Length > 5)
                                {
                                    items[1] = items[1].Remove(items[1].IndexOf("MOUSE"), 5);
                                    temp.angle_offset = Acc.resolve_die_roll(items[1], 0, 0);
                                }
                            }
                            else if (items[1].Contains("PARENT"))
                            {
                                temp.angle_follow_parent = true;
                                if (items[1].Length > 6)
                                {
                                    items[1] = items[1].Remove(items[1].IndexOf("PARENT"), 6);
                                    temp.angle_offset = Acc.resolve_die_roll(items[1], 0, 0);
                                }
                            }
                            else if (items[1].Contains("WALKING"))
                            {
                                char[] split = { '+', '-' };
                                string[] codes = items[1].Split(split);
                                temp.angle_walking_id = Int32.Parse(codes[0].Substring(codes[0].IndexOf("WALKING") + 7));
                                if (codes.Length > 1)
                                    temp.angle_offset = Acc.resolve_die_roll(codes[1], 0, 0);
                            }
                            else
                            {
                                temp.angle_offset = Acc.resolve_die_roll(items[1], 0, 0);
                            }
                            break;
                        case "draw-order":
                            temp.draw_order = Int32.Parse(items[1]);
                            break;
                        case "images":
                            items = items[1].Split(';');
                            Rectangle temp_rect = new Rectangle();
                            ChildPinPicture t = new ChildPinPicture();
                            for (int x = 0; x < items.Length; x++)
                            {
                                if (items[x].Contains('='))
                                { //this is setup to be a child, it has an attachment point.

                                    string[] sub_inner = items[x].Split('=');
                                    string[] inner = Acc.get_inner_parenthesis(sub_inner[0]).Split(',');
                                    temp_rect = new Rectangle(Int32.Parse(inner[0]), Int32.Parse(inner[1]), Int32.Parse(inner[2]), Int32.Parse(inner[3]));
                                    sub_inner = Acc.get_inner_parenthesis(sub_inner[1]).Split(',');
                                    t.image = temp_rect;
                                    t.child_loc = new Point(Int32.Parse(sub_inner[0]), Int32.Parse(sub_inner[1]));
                                    temp.images.Add(t);
                                }
                                else
                                { //this is not a child, no attachment point.
                                    string[] inner = Acc.get_inner_parenthesis(items[x]).Split(',');
                                    temp_rect = new Rectangle(Int32.Parse(inner[0]), Int32.Parse(inner[1]), Int32.Parse(inner[2]), Int32.Parse(inner[3]));
                                    t.image = temp_rect;
                                    t.child_loc = new Point(0, 0);
                                    temp.images.Add(t);
                                }
                            }
                            break;
                        case "child":
                            items = items[1].Split(';'); //Left Arm;attach(14,5)
                            ParentBrassPin temp_child = new ParentBrassPin();
                            temp_child.child_name = items[0];
                            items = Acc.get_inner_parenthesis(items[1]).Split(',');
                            temp_child.parent_loc = new Point(Int32.Parse(items[0]), Int32.Parse(items[1]));
                            temp.children.Add(temp_child);
                            break;
                    }
                }
                if (r.EndOfStream)
                    cont = false;
            }
        }

        public int get_bodypart_template_id_by_name(string name)
        {
            for (int x = 0; x < parts_list.Count; x++)
            {
                if (parts_list[x].name == name)
                {
                    return x;
                }
            }
            return -1;
        }

        public List<BodyPart> add_children_to_list(List<BodyPart> body, int parent_id, List<int> c, List<Color> colors)
        {
            BodyPart temp_part = new BodyPart();
            temp_part.body_type_id = body_template_id;
            int child_id = -1;
            for (int x = 0; x < parts_list[parent_id].children.Count; x++)
            {
                child_id = get_bodypart_template_id_by_name(parts_list[parent_id].children[x].child_name);
                temp_part = new BodyPart();
                temp_part.body_type_id = body_template_id;

                temp_part.name = parts_list[child_id].name;
                temp_part.parent_name = parts_list[parent_id].name;
                int child_image_id = Exilania.rand.Next(parts_list[child_id].images.Count);
                if (temp_part.name == "Right Leg")
                    child_image_id = leg_chosen_pic;
                if (temp_part.name == "Right Foot")
                {
                    child_image_id = foot_chosen_pic;
                }
                c.Add(child_image_id);
                if (temp_part.name == "Left Leg")
                {
                    leg_chosen_pic = child_image_id;
                }
                else if (temp_part.name == "Left Foot")
                {
                    foot_chosen_pic = child_image_id;
                }
                temp_part.picture = parts_list[child_id].images[child_image_id].image;
                temp_part.center = new Vector2(((float)temp_part.picture.Width / 2f), ((float)temp_part.picture.Height / 2f));
                temp_part.parent_attachment_point = new Vector2(parts_list[parent_id].children[x].parent_loc.X,parts_list[parent_id].children[x].parent_loc.Y);

                temp_part.my_attachment_point = new Vector2(parts_list[child_id].images[child_image_id].child_loc.X, parts_list[child_id].images[child_image_id].child_loc.Y);
                temp_part.follow_mouse = parts_list[child_id].angle_follow_mouse;
                temp_part.follow_parent = parts_list[child_id].angle_follow_parent;
                temp_part.click_active_mouse = parts_list[child_id].click_active_mouse;
                temp_part.rotation_offset = parts_list[child_id].angle_offset;
                temp_part.walking = parts_list[child_id].angle_walking_id;
                temp_part.draw_order = parts_list[child_id].draw_order;
                //temp_part.draw_color = Acc.int_to_col(Exilania.rand.Next(0, 31));
                colors.Add(temp_part.draw_color);
                body.Add(temp_part);
                if (parts_list[child_id].children.Count > 0)
                {
                    body = add_children_to_list(body, child_id, c, colors);
                }

            }
            return body;
        }

        public List<BodyPart> create_actor_body(List<int> c, List<Color> colors)
        {
            int parent_id = 0;
            List<BodyPart> body = new List<BodyPart>();
            BodyPart temp_part = new BodyPart();
            temp_part.body_type_id = body_template_id;
            temp_part.name = parts_list[parent_id].name;
            int chose_id = Exilania.rand.Next(parts_list[parent_id].images.Count);
            c.Add(chose_id); //this contains the chosen picture so the body can be reconstructed later.
            temp_part.picture = parts_list[parent_id].images[chose_id].image;
            temp_part.center = new Vector2(temp_part.picture.Width / 2, temp_part.picture.Height / 2);
            temp_part.rotation_offset = parts_list[parent_id].angle_offset;
            temp_part.follow_mouse = parts_list[parent_id].angle_follow_mouse;
            temp_part.follow_parent = false;
            temp_part.click_active_mouse = parts_list[parent_id].click_active_mouse;
            temp_part.walking = parts_list[parent_id].angle_walking_id;
            temp_part.draw_order = parts_list[parent_id].draw_order;
            //temp_part.draw_color = Acc.int_to_col(Exilania.rand.Next(0, 31));
            colors.Add(temp_part.draw_color);
            body.Add(temp_part);
            body = add_children_to_list(body, parent_id, c, colors);
            return body;
        }

        public List<BodyPart> add_children_to_list_from_list(List<BodyPart> body, int parent_id, List<int> c, List<Color> colors)
        {
            BodyPart temp_part = new BodyPart();
            temp_part.body_type_id = body_template_id;
            int child_id = -1;
            for (int x = 0; x < parts_list[parent_id].children.Count; x++)
            {
                child_id = get_bodypart_template_id_by_name(parts_list[parent_id].children[x].child_name);
                temp_part = new BodyPart();
                temp_part.body_type_id = body_template_id;
                temp_part.name = parts_list[child_id].name;
                temp_part.parent_name = parts_list[parent_id].name;
                int child_image_id = c[cur_part_id_reading_from_file];
               
                temp_part.picture = parts_list[child_id].images[child_image_id].image;
                temp_part.center = new Vector2(((float)temp_part.picture.Width / 2f), ((float)temp_part.picture.Height / 2f));
                temp_part.parent_attachment_point = new Vector2(parts_list[parent_id].children[x].parent_loc.X, parts_list[parent_id].children[x].parent_loc.Y);

                temp_part.my_attachment_point = new Vector2(parts_list[child_id].images[child_image_id].child_loc.X, parts_list[child_id].images[child_image_id].child_loc.Y);
                temp_part.follow_mouse = parts_list[child_id].angle_follow_mouse;
                temp_part.follow_parent = parts_list[child_id].angle_follow_parent;
                temp_part.click_active_mouse = parts_list[child_id].click_active_mouse;
                temp_part.rotation_offset = parts_list[child_id].angle_offset;
                temp_part.walking = parts_list[child_id].angle_walking_id;
                temp_part.draw_order = parts_list[child_id].draw_order;


                //temp_part.draw_color = Acc.int_to_col(Exilania.rand.Next(0, 31));
                temp_part.draw_color = colors[cur_part_id_reading_from_file];
                cur_part_id_reading_from_file++;

                body.Add(temp_part);
                if (parts_list[child_id].children.Count > 0)
                {
                    body = add_children_to_list_from_list(body, child_id, c, colors);
                }

            }
            return body;
        }

        public List<BodyPart> create_body_from_list(List<int> c, List<Color> colors)
        {
            int parent_id = 0;
            List<BodyPart> body = new List<BodyPart>();
            BodyPart temp_part = new BodyPart();
            temp_part.body_type_id = body_template_id;
            temp_part.name = parts_list[parent_id].name;

            int chose_id = c[0];
            cur_part_id_reading_from_file = 1;

            temp_part.picture = parts_list[parent_id].images[chose_id].image;
            temp_part.center = new Vector2(temp_part.picture.Width / 2, temp_part.picture.Height / 2);
            temp_part.rotation_offset = parts_list[parent_id].angle_offset;
            temp_part.follow_mouse = parts_list[parent_id].angle_follow_mouse;
            temp_part.follow_parent = false;
            temp_part.click_active_mouse = parts_list[parent_id].click_active_mouse;
            temp_part.walking = parts_list[parent_id].angle_walking_id;
            temp_part.draw_order = parts_list[parent_id].draw_order;

            //temp_part.draw_color = Acc.int_to_col(Exilania.rand.Next(0, 31));
            temp_part.draw_color = colors[0];

            body.Add(temp_part);
            body = add_children_to_list_from_list(body, parent_id, c, colors);
            return body;
        }



        public override string ToString()
        {
            return template_name + ": " + parts_list.Count + " body parts present. "; 
        }
    }
}
