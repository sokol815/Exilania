using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Exilania
{
    public enum InputConsume
    {
        /// <summary>
        /// default input state
        /// </summary>
        Normal = 1,
        /// <summary>
        /// typing a message
        /// </summary>
        Message = 2,
        /// <summary>
        /// something like naming a new world, naming a character
        /// </summary>
        ActiveState = 3,
        /// <summary>
        /// editing permissions on a furniture item.
        /// </summary>
        FurnitureEdit = 4

    }
    public class Input
    {
        public List<Keys> keys_previous = new List<Keys>();
        public List<Keys> keys_now = new List<Keys>();
        MouseState mouse_previous = new MouseState();
        public MouseState mouse_now = new MouseState();
        public bool left_clicked = false;
        public Point left_clicked_spot;
        public Point mouse_cur_spot;
        long msec_now = (DateTime.Now.Ticks / 10000);
        long msec_last = (DateTime.Now.Ticks / 10000);
        int msec_elapsed = 0;
        public int key_wait_time = 150;
        public int init_key_wait_time = 200;
        public int rocker_state = 0;
        public string pressed = "";
        /// <summary>
        /// this is used for typing messages, basically. Use this for all string input within the game.
        /// </summary>
        public string key_input = ""; 
        public string collection = "";
        public string cur_input = "";
        public string cur_down = "";
        bool caps_loc = false;
        bool shift = false;
        public bool ctrl = false;
        public bool alt = false;
        public bool enter = false;
        public Point dx_dy;
        public Vector2 saved_delta;
        public int last_ctrl_press = 0;
        /// <summary>
        /// 0 = unpressed; 1 = use_held_state
        /// </summary>
        public int ctrl_state = 0;
        public InputConsume current_input_type = InputConsume.Normal;

        public Input()
        {

        }

        public int process_input(Exilania e, int ms_elapsed)
        {
            if (!e.IsActive)
                return 0;
            mouse_previous = mouse_now;
            mouse_now = Mouse.GetState();
            rocker_state = mouse_now.ScrollWheelValue;
            mouse_cur_spot = new Point(mouse_now.X, mouse_now.Y);
            if (mouse_now.LeftButton == ButtonState.Pressed && mouse_previous.LeftButton == ButtonState.Released &&
                mouse_now.X > -1 && mouse_now.X < Exilania.screen_size.X && mouse_now.Y > -1 && mouse_now.Y < Exilania.screen_size.Y &&
                e.IsActive)
            {

                left_clicked = true;
                left_clicked_spot = new Point(mouse_now.X, mouse_now.Y);
            }

            int code = 0;
            if (e.IsActive)
                live_buttons_down();
            else
                collection = "";

            dx_dy = new Point(0, 0);
            msec_now = (DateTime.Now.Ticks / 10000);
            msec_elapsed += (int)(msec_now - msec_last);
            msec_last = msec_now;
            keys_now = new List<Keys>(Keyboard.GetState().GetPressedKeys());

            if (keys_now.Contains(Keys.LeftShift) || keys_now.Contains(Keys.RightShift))
                shift = true;
            else
                shift = false;

            if (keys_now.Contains(Keys.LeftControl) || keys_now.Contains(Keys.RightControl))
            {
                if (last_ctrl_press < 1000 && last_ctrl_press > 0)
                { //double hit the keyboard within 1 second
                    ctrl_state = 0;
                    last_ctrl_press = -1000;
                    saved_delta = new Vector2();
                }
                else if(last_ctrl_press > -1)
                {
                    ctrl_state = 1;
                    last_ctrl_press = 0;
                }
                ctrl = true;
            }
            else
            {
                last_ctrl_press += msec_elapsed;
                ctrl = false;
            }
            if (Exilania.gstate == 100 && ctrl == true && ctrl_state == 1)
            {
                Vector2 center = new Vector2(e.world.players[Exilania.game_my_user_id].avatar.world_loc.X, e.world.players[Exilania.game_my_user_id].avatar.world_loc.Y);
                saved_delta = new Vector2((int)center.X - ((int)(center.X + e.world.players[Exilania.game_my_user_id].avatar.input.mouse_loc.X) / 2),
                    (int)center.Y - ((int)(center.Y + e.world.players[Exilania.game_my_user_id].avatar.input.mouse_loc.Y) / 2));
                if (Math.Abs(saved_delta.X) > Exilania.screen_size.X / 2)
                {
                    if (saved_delta.X < 0)
                        saved_delta.X = -Exilania.screen_size.X / 2;
                    else
                        saved_delta.X = Exilania.screen_size.X / 2;
                }
                if (Math.Abs(saved_delta.Y) > Exilania.screen_size.Y / 2)
                {
                    if (saved_delta.Y < 0)
                        saved_delta.Y = -Exilania.screen_size.Y / 2;
                    else
                        saved_delta.Y = Exilania.screen_size.Y / 2;
                }
            }
            if (keys_now.Contains(Keys.LeftAlt) || keys_now.Contains(Keys.RightAlt))
            {
                alt = true;
            }
            else
            {
                alt = false;
            }
            /* if (keys_now.Contains(Keys.Back))
            {
                if (key_input.Length > 1)
                {
                    key_input = key_input.Substring(0, key_input.Length - 1);
                }
                else if (key_input.Length == 1)
                    key_input = "";
            }*/
            if (keys_now.Contains(Keys.Enter) && !keys_previous.Contains(Keys.Enter))
            {
                enter = true;
            }
            bool cont_down = false;
            bool repeated = false;
            pressed = "";
            foreach (Keys p in keys_now)
            {
                if (keys_previous.Contains(p))
                {
                    cont_down = true;
                }
                if (!keys_previous.Contains(p) || (keys_previous.Contains(p) && msec_elapsed > key_wait_time))
                {

                    if (p.ToString().Length < 2)
                    {
                        if ((shift && !caps_loc) || (!shift && caps_loc))
                        {
                            pressed += p.ToString();
                        }
                        else
                            pressed += p.ToString().ToLower();
                    }
                    repeated = true;
                    switch (p)
                    {
                        case Keys.CapsLock:
                            if (caps_loc)
                                caps_loc = false;
                            else
                                caps_loc = true;
                            break;
                        case Keys.Escape:
                            code = -1;
                            break;
                        case Keys.F12:
                            code = -2;
                            break;
                        case Keys.F11:
                            code = 1;
                            break;
                        case Keys.F10:
                            code = 5;
                            break;
                        case Keys.F2:
                            code = 2;
                            break;
                        case Keys.F3:
                            code = 4;
                            break;
                        case Keys.M:
                            code = 10;
                            break;
                        case Keys.F1:
                            code = 20;
                            break;
                        case Keys.Up:
                            dx_dy.Y = -1;
                            break;
                        case Keys.Down:
                            dx_dy.Y = 1;
                            break;
                        case Keys.Left:
                            dx_dy.X = -1;
                            break;
                        case Keys.Right:
                            dx_dy.X = 1;
                            break;
                        case Keys.PageUp:
                            dx_dy.X = 1;
                            dx_dy.Y = -1;
                            break;
                        case Keys.PageDown:
                            dx_dy.X = 1;
                            dx_dy.Y = 1;
                            break;
                        case Keys.Home:
                            dx_dy.X = -1;
                            dx_dy.Y = -1;
                            break;
                        case Keys.End:
                            dx_dy.X = -1;
                            dx_dy.Y = 1;
                            break;
                        case Keys.Space:
                            key_input += " ";
                            break;
                        case Keys.Enter:
                            code = 3;
                            break;
                        case Keys.OemPeriod:
                            if (shift)
                            {
                                key_input += ">";
                            }
                            else
                            {
                                key_input += ".";
                            }
                            break;
                        case Keys.OemComma:
                            if (shift)
                            {
                                key_input += "<";
                            }
                            else
                            {
                                key_input += ",";
                            }
                            break;
                        case Keys.NumPad1:
                        case Keys.D1:
                            if (shift)
                            {
                                key_input += "!";
                            }
                            else
                            {
                                key_input += "1";
                            }
                            dx_dy.X = -1;
                            dx_dy.Y = 1;
                            break;
                        case Keys.NumPad2:
                        case Keys.D2:
                            if (shift)
                            {
                                key_input += "@";
                            }
                            else
                            {
                                key_input += "2";
                            }
                            dx_dy.Y = 1;
                            break;
                        case Keys.NumPad3:
                        case Keys.D3:
                            if (shift)
                            {
                                key_input += "#";
                            }
                            else
                            {
                                key_input += "3";
                            }
                            dx_dy.X = 1;
                            dx_dy.Y = 1;
                            break;
                        case Keys.D4:
                        case Keys.NumPad4:
                            if (shift)
                            {
                                key_input += "$";
                            }
                            else
                            {
                                key_input += "4";
                            }
                            dx_dy.X = -1;
                            break;
                        case Keys.NumPad5:
                        case Keys.D5:
                            if (shift)
                            {
                                key_input += "%";
                            }
                            else
                            {
                                key_input += "5";
                            }
                            break;
                        case Keys.NumPad6:
                        case Keys.D6:
                            if (shift)
                            {
                                key_input += "^";
                            }
                            else
                            {
                                key_input += "6";
                            }
                            dx_dy.X = 1;
                            break;
                        case Keys.NumPad7:
                        case Keys.D7:
                            if (shift)
                            {
                                key_input += "&";
                            }
                            else
                            {
                                key_input += "7";
                            }
                            dx_dy.X = -1;
                            dx_dy.Y = -1;
                            break;
                        case Keys.NumPad8:
                        case Keys.D8:
                            if (shift)
                            {
                                key_input += "*";
                            }
                            else
                            {
                                key_input += "8";
                            }
                            dx_dy.Y = -1;
                            break;
                        case Keys.NumPad9:
                        case Keys.D9:
                            if (shift)
                            {
                                key_input += "(";
                            }
                            else
                            {
                                key_input += "9";
                            }
                            dx_dy.Y = -1;
                            dx_dy.X = 1;
                            break;
                        case Keys.NumPad0:
                        case Keys.D0:
                            if (shift)
                            {
                                key_input += ")";
                            }
                            else
                            {
                                key_input += "0";
                            }
                            break;
                        case Keys.OemPipe:
                            if (shift)
                            {
                                key_input += "|";
                            }
                            else
                            {
                                key_input += "\\";
                            }
                            break;
                        case Keys.OemOpenBrackets:
                            if (shift)
                            {
                                key_input += "{";
                            }
                            else
                            {
                                key_input += "[";
                            }
                            break;
                        case Keys.OemCloseBrackets:
                            if (shift)
                            {
                                key_input += "}";
                            }
                            else
                            {
                                key_input += "]";
                            }
                            break;
                        case Keys.OemQuestion:
                            if (shift)
                            {
                                key_input += "?";
                            }
                            else
                            {
                                key_input += "/";
                            }
                            break;
                        case Keys.OemPlus:
                            if (shift)
                            {
                                key_input += "+";
                            }
                            else
                            {
                                key_input += "=";
                            }
                            break;
                        case Keys.Add:
                            key_input += "+";
                            break;
                        case Keys.Subtract:
                            key_input += "-";
                            break;
                        case Keys.Divide:
                            key_input += "/";
                            break;
                        case Keys.Multiply:
                            key_input += "*";
                            break;
                        case Keys.Decimal:
                            key_input += ".";
                            break;
                        case Keys.OemQuotes:
                            if (shift)
                            {
                                key_input += "\"";
                            }
                            else
                            {
                                key_input += "'";
                            }
                            break;
                        case Keys.OemMinus:
                        
                            if (!shift)
                            {
                                key_input += "-";
                            }
                            else
                            {
                                key_input += "_";
                            }
                            break;
                        case Keys.Back:
                            if (key_input.Length > 1)
                            {
                                key_input = key_input.Substring(0, key_input.Length - 1);
                            }
                            else if (key_input.Length == 1)
                                key_input = "";
                            break;
                        case Keys.OemTilde:
                            if (!shift)
                            {
                                key_input = "";
                            }
                            else
                            {
                                key_input += "~";
                            }
                            break;
                        case Keys.OemSemicolon:
                            if (shift)
                            {
                                key_input += ":";
                            }
                            else
                            {
                                key_input += ";";
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (repeated)
            {
                key_input += pressed;
                msec_elapsed = 0;
            }
            if (cont_down)
            {
                key_wait_time--;
                if (key_wait_time < 150)
                    key_wait_time = 150;
            }
            else
                key_wait_time = init_key_wait_time;

            return code;
        }

        public void finish_input()
        {
            keys_previous = new List<Keys>(keys_now);
        }

        public void live_buttons_down()
        {
            List<Keys> keys_now = new List<Keys>(Keyboard.GetState().GetPressedKeys());
            collection = "";
            foreach (Keys p in keys_now)
            {
              
                    if (p.ToString().Length < 2)
                    {
                     
                        collection += p.ToString();
                    }
                    else
                    switch (p)
                    {
                        case Keys.LeftShift:
                        case Keys.RightShift:
                            collection += "`";
                            break;
                        case Keys.Space:
                            collection += " ";
                            break;
                        case Keys.OemPeriod:
                            if (shift)
                                collection += ">";
                            else
                                collection += ".";
                            break;
                        case Keys.OemComma:
                            if (shift)
                                collection += "<";
                            else
                                collection += ",";
                            break;
                        case Keys.NumPad1:
                        case Keys.D1:
                            if (shift)
                                collection += "!";
                            else
                                collection += "1";
                            break;
                        case Keys.NumPad2:
                        case Keys.D2:
                            if (shift)
                                collection += "@";
                            else
                                collection += "2";
                            break;
                        case Keys.NumPad3:
                        case Keys.D3:
                            if (shift)
                                collection += "#";
                            else
                                collection += "3";
                            break;
                        case Keys.D4:
                        case Keys.NumPad4:
                            if (shift)
                                collection += "$";
                            else
                                collection += "4";
                            break;
                        case Keys.NumPad5:
                        case Keys.D5:
                            if (shift)
                                collection += "%";
                            else
                                collection += "5";
                            break;
                        case Keys.NumPad6:
                        case Keys.D6:
                            if (shift)
                                collection += "^";
                            else
                                collection += "6";
                            break;
                        case Keys.NumPad7:
                        case Keys.D7:
                            if (shift)
                                collection += "&";
                            else
                                collection += "7";
                            break;
                        case Keys.NumPad8:
                        case Keys.D8:
                            if (shift)
                                collection += "*";
                            else
                                collection += "8";
                            break;
                        case Keys.NumPad9:
                        case Keys.D9:
                            if (shift)
                                collection += "(";
                            else
                                collection += "9";
                            break;
                        case Keys.NumPad0:
                        case Keys.D0:
                            if (shift)
                                collection += ")";
                            else
                                collection += "0";
                            break;
                        case Keys.OemPipe:
                            if (shift)
                                collection += "|";
                            else
                                collection += "\\";
                            break;
                        case Keys.OemOpenBrackets:
                            if (shift)
                                collection += "{";
                            else
                                collection += "[";
                            break;
                        case Keys.OemCloseBrackets:
                            if (shift)
                                collection += "}";
                            else
                                collection += "]";
                            break;
                        case Keys.OemQuestion:
                            if (shift)
                                collection += "?";
                            else
                                collection += "/";
                            break;
                        case Keys.OemPlus:
                        case Keys.Add:
                            collection += "+";
                            break;
                        case Keys.OemMinus:
                        case Keys.Subtract:
                            collection += "-";
                            break;
                        case Keys.Back:

                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
