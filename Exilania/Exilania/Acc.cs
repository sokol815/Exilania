using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public enum AccColors
    {
        White = 0,
        Blue = 1,
        LightBlue = 2,
        LimeGreen = 3,
        Lime = 4,
        Yellow = 5,
        Orange = 6,
        OrangeRed = 7,
        Red = 8,
        PaleVioletRed = 9,
        Cyan = 10,
        Brown = 11,
        Peach = 12,
        DarkGray = 13,
        DarkGreen = 14,
        HotPink = 15,
        DarkCyan = 16,
        Green = 17,
        GreenYellow = 18,
        Iron = 19,
        Brass = 20,
        Steel = 21,
        Bronze = 22,
        Gold = 23,
        Mithril = 24,
        Adamantium = 25,
        Electrite = 26,
        Frigisium = 27,
        Ignisite = 28,
        Venenium = 29,
        Silver = 30,
        Black = 31,
        MidGray = 32,
        BlueRedChameleon = 33,
        Random = 34
    }
    public struct item_descriptor
    {
        public ItemType item_type;
        public int item_id;
    }
    public class Circle
    {
        Point Center;
        float r;

        public Circle(Point c, float rad)
        {
            Center = c;
            r = rad;
        }

        public bool contains_point(Point loc)
        {
            return Acc.get_distance(loc, Center) <= r;
        }

        public bool intersects_rectangle(Rectangle i)
        {
            Point offset = new Point(Math.Abs(Center.X - i.Center.X), Math.Abs(Center.Y - i.Center.Y));
            if (offset.X > (i.Width / 2) + r || offset.Y > (i.Height / 2) + r) return false;
            if (offset.X <= i.Width / 2 || offset.Y <= i.Height / 2) return true;
            //compute distance from center of circle to edge of reectangle
            float tot_dist = ((offset.X - i.Width / 2) * (offset.X - i.Width / 2)) + ((offset.Y - i.Height / 2) * (offset.Y - i.Height / 2));
            //pythagorean theorom states that squared distance of the hypotenuse is equal to the squared sum of the other two parts... in other words
            //if the r squared is <= the total, it is in the square.
            return tot_dist <= r*r;
        }

        public override string ToString()
        {
            return "Center: " + Center.ToString() + " Radius: " + r;
        }
    }
   
    class Acc
    {
        /// <summary>
        /// 0 is no light, 10 is full light
        /// </summary>
        public static Color[] brightnesses = new Color[] { 
            Color.FromNonPremultiplied(0,0,0,255),
            Color.FromNonPremultiplied(25,25,25,255),
            Color.FromNonPremultiplied(51,51,51,255),
            Color.FromNonPremultiplied(76,76,76,255),
            Color.FromNonPremultiplied(102,102,102,255),
            Color.FromNonPremultiplied(127,127,127,255),
            Color.FromNonPremultiplied(153,153,153,255),
            Color.FromNonPremultiplied(180,180,180,255),
            Color.FromNonPremultiplied(205,205,205,255),     
            Color.FromNonPremultiplied(230,230,230,255),
            Color.FromNonPremultiplied(255,255,255,255)
        };
        public static Color[] bkd_brightnesses = new Color[] { 
            Color.FromNonPremultiplied(0,0,0,255),
            Color.FromNonPremultiplied(12,12,12,255),
            Color.FromNonPremultiplied(25,25,25,255),
            Color.FromNonPremultiplied(38,38,38,255),
            Color.FromNonPremultiplied(51,51,51,255),
            Color.FromNonPremultiplied(64,64,64,255),
            Color.FromNonPremultiplied(77,77,77,255),
            Color.FromNonPremultiplied(90,90,90,255),
            Color.FromNonPremultiplied(103,103,103,255),     
            Color.FromNonPremultiplied(116,116,116,255),
            Color.FromNonPremultiplied(128,128,128,255)
        };
        public static Color visible = Color.White;
        public static Color fog_of_war = Color.FromNonPremultiplied(64, 64, 64, 255);
        public static Color dark_bkd = Color.FromNonPremultiplied(128, 128, 128, 255);
        public static Color glo1 = Color.FromNonPremultiplied(0, 155, 255, 255);
        public static Color glo2 = Color.FromNonPremultiplied(255,0,0, 255);

        public static float glow_speed = 5f;
        public static long calc_33_color_tstamp = 0;

        public Acc()
        {
        }

        public static Color int_to_col(int id)
        {
            switch (id)
            {
                case 0: return Color.White;
                case 1: return Color.Blue;
                case 2: return Color.CornflowerBlue;
                case 3: return Color.LimeGreen;
                case 4: return Color.Lime;
                case 5: return Color.Yellow;
                case 6: return Color.Orange;
                case 7: return Color.OrangeRed;
                case 8: return Color.Red;
                case 9: return Color.PaleVioletRed;
                case 10: return Color.Cyan;
                case 11: return Color.Brown;
                case 12: return Color.PeachPuff;
                case 13: return Color.DarkGray;
                case 14: return Color.DarkGreen;
                case 15: return Color.HotPink;
                case 16: return Color.DarkCyan;
                case 17: return Color.Green;
                case 18: return Color.GreenYellow;
                case 19: return Color.FromNonPremultiplied(142, 28, 28, 255); //iron
                case 20: return Color.FromNonPremultiplied(193, 141, 62, 255);//brass
                case 21: return Color.FromNonPremultiplied(102, 111, 116, 255);//steel
                case 22: return Color.FromNonPremultiplied(111, 89, 68, 255); //bronze
                case 23: return Color.FromNonPremultiplied(236, 214, 82, 255); //gold
                case 24: return Color.FromNonPremultiplied(57, 94, 114, 255); //mithril
                case 25: return Color.FromNonPremultiplied(35, 122, 68, 255); //adamantium
                case 26: return Color.FromNonPremultiplied(0, 163, 147, 255); //electrite
                case 27: return Color.FromNonPremultiplied(119, 132, 206, 255); //frigisium
                case 28: return Color.FromNonPremultiplied(204, 89, 0, 255); //ignisite
                case 29: return Color.FromNonPremultiplied(0, 61, 0, 255); //venenium
                case 30: return Color.FromNonPremultiplied(196, 196, 196, 255); //silver
                case 31: return Color.Black;
                case 32: return Color.FromNonPremultiplied(128, 128, 128, 255); // mid gray.
                case 33:
                    long p2 = System.DateTime.Now.Ticks / 10000;
                    float pos = (float)(p2 % (int)(glow_speed * 1000)); //milliseconds passed total
                    pos /= 1000f * glow_speed;

                    if (pos > .5f)
                    {
                        pos = 1f - pos;
                    }
                    pos *= 2f;
                    Color col = new Color();
                    col.R = (byte)(((float)glo1.R * pos) + ((float)glo2.R * (1f - pos)));
                    col.G = (byte)(((float)glo1.G * pos) + ((float)glo2.G * (1f - pos)));
                    col.B = (byte)(((float)glo1.B * pos) + ((float)glo2.B * (1f - pos)));
                    col.A = (byte)(((float)glo1.A * pos) + ((float)glo2.A * (1f - pos)));
                    return col;
                default:
                    if (Exilania.settings.allow_strobe)
                        return int_to_col(Exilania.rand.Next(33));
                    else
                        return Color.White;
            }
        }

        public static Color multiply_colors(Color a, Color b)
        {
            Color r = Color.White;
            r.R = (byte)((float)a.R / 255f * (float)b.R);
            r.G = (byte)((float)a.G / 255f * (float)b.G);
            r.B = (byte)((float)a.B / 255f * (float)b.B);
            return r;
        }

        public static Color multiply_colors_opacity(Color a, float opacity)
        {
            Color r = Color.White;
            r.R = (byte)((float)a.R * opacity);
            r.G = (byte)((float)a.G * opacity);
            r.B = (byte)((float)a.B * opacity);
            r.A = (byte)((float)a.A * opacity);

            return r;
        }
      
        public static int get_property_value_int(string attrib_String, string property)
        {
            attrib_String = attrib_String.Substring(5).Trim();
            if (attrib_String.Contains(';')) //does it have multiple attributes?
            {
                string[] data = attrib_String.Split(';');
                for (int x = 0; x < data.Length; x++)
                {
                    if (data[x].Split(':')[0] == property)
                    {
                        return Int32.Parse(data[x].Split(':')[1]);
                    }
                }
            }
            else
            { //only 1... parse it!
                return Int32.Parse(attrib_String.Split(':')[1]);
            }

            return 0;
        }

        public static string sanitize_text_color(string san)
        {
            string[] items = san.Split('@');
            string combined = "";
            for (int x = 0; x < items.Length; x++)
            {
                if (items[x].Length >= 2)
                {
                    if ((byte)items[x][0] >= 48 && (byte)items[x][0] <= 57 && (byte)items[x][1] >= 48 && (byte)items[x][1] <= 57)
                    { //we have a color code... remove it
                        if (items[x].Length > 2)
                            combined += items[x].Substring(2);
                    }
                    else
                    { //not a color code, display the @ symbol
                        combined += "@" + items[x];
                    }
                }
                else if(x!= 0)
                    combined += "@" + items[x];
            }
            return combined;
        }

        public static string[] split_csv(string line)
        {

            List<string> collection = new List<string>();
            if (line.Contains('"'))
            {
                string[] contents = line.Split('"');
                for (int x = 0; x < contents.Length; x++)
                {
                    if (contents[x].Length > 0)
                    {
                        if (x % 2 == 0)
                        {
                            string[] sub_content = contents[x].Split(',');
                            for (int i = 0; i < sub_content.Length; i++)
                            {
                                if (sub_content[i].Length > 0)
                                    collection.Add(sub_content[i]);
                            }
                        }
                        else
                        {
                            collection.Add(contents[x]);
                        }
                    }
                    else
                    {
                        //there was a " at the beginning or the end of the string.
                    }

                }
            }
            else
            {
                return line.Split(',');
            }
            string[] c = new string[collection.Count];
            for (int x = 0; x < c.Length; x++)
            {
                c[x] = collection[x];
            }
            return c;
        }

        /// <summary>
        /// returns the contents of the innermost parenthesis of the first set it finishes....
        /// </summary>
        /// <param name="testing_on"></param>
        /// <returns></returns>
        public static string get_inner_parenthesis(string testing_on)
        {
            int last_open_paren = -1;
            int first_close_paren = -1;
            for (int x = 0; x < testing_on.Length; x++)
            {
                if (testing_on[x] == ')' && first_close_paren == -1)
                    first_close_paren = x;
                if (testing_on[x] == '(' && first_close_paren == -1)
                    last_open_paren = x;
            }
            if (last_open_paren < first_close_paren)
            {
                return testing_on.Substring(last_open_paren + 1, first_close_paren - last_open_paren - 1);
            }
            else
                return testing_on;
        }
        /// <summary>
        /// returns the passed in string without the parenthesis or anything that was in them.
        /// </summary>
        /// <param name="testing_on"></param>
        /// <returns></returns>
        public static string remove_inner_parenthesis(string testing_on)
        {
            int last_open_paren = -1;
            int first_close_paren = -1;
            for (int x = 0; x < testing_on.Length; x++)
            {
                if (testing_on[x] == '(')
                    last_open_paren = x;
                if (testing_on[x] == ')' && first_close_paren == -1)
                    first_close_paren = x;
            }
            if (last_open_paren < first_close_paren)
            {
                return testing_on.Substring(0, last_open_paren) + testing_on.Substring(first_close_paren + 1);
            }
            else
                return testing_on;
        }

        /// <summary>
        /// this function takes in a string and returns the content outside of the parenthesis. symplistic
        /// E.G. 'Friend(GAG)' returns 'Friend'
        /// </summary>
        /// <param name="testing_on"></param>
        /// <returns></returns>
        public static string script_remove_content_of_outer_parenthesis(string testing_on)
        {
            int spos = testing_on.IndexOf('(');
            int rpos = testing_on.LastIndexOf(')');
            if (spos == -1 || rpos == -1)
                return testing_on;
            return testing_on.Remove(spos,rpos-spos+1);
        }

        /// <summary>
        /// this function determines if the set has outer parenthesis that apply to the whole thing
        /// </summary>
        /// <param name="testing_on"></param>
        /// <returns></returns>
        public static bool script_has_outer_parenthesis(string testing_on)
        {
            if(script_remove_content_of_outer_parenthesis(testing_on).Contains(";"))
                return false;
            return true;
        }

        /// <summary>
        /// get only contents of outermost parentheses.
        /// e.g. "Friend(BLABLABLA5)" becomes "BLABLABLA5"
        /// </summary>
        /// <param name="testing_on"></param>
        /// <returns></returns>
        public static string script_remove_outer_parentheses(string testing_on)
        {
            int spos = testing_on.IndexOf('(');
            int rpos = testing_on.LastIndexOf(')');
            if (spos == -1 || rpos == -1)
                return testing_on;
            return testing_on.Substring(spos+1, rpos - spos - 1);
        }

        public static Point return_die_range(string roll)
        {
            Point p = new Point();
            if (roll.Contains('d'))
            {
                string[] parts = roll.Split('d');
                int num_die = Int16.Parse(parts[0]);
                int modifier = 0;
                int num_sides = 0;
                if (parts[1].Contains('+'))
                {
                    string[] sub_parts = parts[1].Split('+');
                    modifier = Int16.Parse(sub_parts[1]);
                    num_sides = Int16.Parse(sub_parts[0]);
                }
                else if (parts[1].Contains('-'))
                {
                    string[] sub_parts = parts[1].Split('-');
                    modifier = Int16.Parse(sub_parts[1]) * -1;
                    num_sides = Int16.Parse(sub_parts[0]);
                }
                else
                {
                    modifier = 0;
                    num_sides = Int16.Parse(parts[1]);
                }
                p.X = num_die + modifier;
                p.Y = (num_die * num_sides) + modifier;
                return p;
            }
            else
            {
                p.X = Int32.Parse(roll);
                p.Y = Int32.Parse(roll);
            }
            return p;
        }
        public static int resolve_die_roll(string roll, double reroll_low, double reroll_high)
        {
            if (reroll_high > 0 && reroll_low > 0)
            {
                if (reroll_low > reroll_high)
                {
                    reroll_low -= reroll_high;
                    reroll_high = 0;
                }
                else if (reroll_high > reroll_low)
                {
                    reroll_high -= reroll_low;
                    reroll_low = 0;
                }
                else
                {
                    reroll_low = 0;
                    reroll_high = 0;
                }
            }
            if (roll.Contains(' '))
            {
                string[] smalls = roll.Split(' ');
                roll = "";
                for (int x = 0; x < smalls.GetLength(0); x++)
                {
                    roll += smalls[x];
                }
            }
            while (roll.Contains('('))
            {
                string inner = get_inner_parenthesis(roll);
                int start_loc = roll.IndexOf(inner) - 1;
                int end_loc = start_loc + 2 + inner.Length;
                string first_half = roll.Substring(0, start_loc);
                string second_half = roll.Substring(end_loc);
                roll = first_half + resolve_die_roll(inner, reroll_low, reroll_high).ToString() + second_half;

            }
            if (roll.Contains('+'))
            {
                int tot = 0;
                string[] pieces = roll.Split('+');
                for (int x = 0; x < pieces.Length; x++)
                {
                    tot += resolve_die_roll(pieces[x], reroll_low, reroll_high);
                }
                return tot;
            }
            if (roll.Contains('-'))
            {
                int tot = 0;
                string[] pieces = roll.Split('-');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                        tot -= resolve_die_roll(pieces[x], reroll_low, reroll_high);
                    else
                        tot = resolve_die_roll(pieces[x], reroll_low, reroll_high);
                }
                return tot;
            }
            if (roll.Contains('*'))
            {
                int tot = 0;
                string[] pieces = roll.Split('*');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                        tot *= resolve_die_roll(pieces[x], reroll_low, reroll_high);
                    else
                        tot = resolve_die_roll(pieces[x], reroll_low, reroll_high);
                }
                return tot;
            }
            if (roll.Contains('/'))
            {
                int tot = 0;
                string[] pieces = roll.Split('/');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                    {
                        if (resolve_die_roll(pieces[x], reroll_low, reroll_high) > 0)
                            tot /= resolve_die_roll(pieces[x], reroll_low, reroll_high);
                        else
                            tot = 0;
                    }
                    else
                        tot = resolve_die_roll(pieces[x], reroll_low, reroll_high);
                }
                return tot;
            }
            if (roll.Contains('%'))
            {
                int tot = 0;
                string[] pieces = roll.Split('%');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                    {
                        if (resolve_die_roll(pieces[x], reroll_low, reroll_high) > 0)
                            tot %= resolve_die_roll(pieces[x], reroll_low, reroll_high);
                        else
                            tot = 0;
                    }
                    else
                        tot = resolve_die_roll(pieces[x], reroll_low, reroll_high);
                }
                return tot;
            }
            if (roll.Contains('^'))
            {
                int tot = 0;
                string[] pieces = roll.Split('^');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                    {
                        tot = (int)Math.Pow(tot, resolve_die_roll(pieces[x], reroll_low, reroll_high));
                    }
                    else
                        tot = resolve_die_roll(pieces[x], reroll_low, reroll_high);
                }
                return tot;
            }
            //this could be something like 3d2-5 or 1d2-1 or 10d10+39
            if (roll.Contains('d'))
            {
                string[] parts = roll.Split('d');
                int num_die = Int16.Parse(parts[0]);
                int modifier = 0;
                int num_sides = 0;
                if (parts[1].Contains('+'))
                {
                    string[] sub_parts = parts[1].Split('+');
                    modifier = Int16.Parse(sub_parts[1]);
                    num_sides = Int16.Parse(sub_parts[0]);
                }
                else if (parts[1].Contains('-'))
                {
                    string[] sub_parts = parts[1].Split('-');
                    modifier = Int16.Parse(sub_parts[1]) * -1;
                    num_sides = Int16.Parse(sub_parts[0]);
                }
                else
                {
                    modifier = 0;
                    num_sides = Int16.Parse(parts[1]);
                }
                int num_dice_roll = (int)((double)num_die + ((double)num_die * reroll_high) + ((double)num_die * reroll_low));
                List<int> rolls = new List<int>();
                for (int x = 0; x < num_dice_roll; x++)
                {
                    rolls.Add(Exilania.rand.Next(1, num_sides + 1));
                }
                if (reroll_high > 0)
                {
                    rolls.Sort((y, x) => x.CompareTo(y));
                    for (int x = 0; x < num_die; x++)
                    {
                        modifier += rolls[x];
                    }
                }
                else if (reroll_low > 0)
                {
                    rolls.Sort((y, x) => y.CompareTo(x));
                    for (int x = 0; x < num_die; x++)
                    {
                        modifier += rolls[x];
                    }
                }
                else
                {
                    for (int x = 0; x < rolls.Count; x++)
                    {
                        modifier += rolls[x];
                    }
                }
                return modifier;
            }
            else
            {
                if (roll.Length > 0)
                    return Int16.Parse(roll);
                else return 0;
            }
        }

        public static string int_to_ordinal(int num)
        {
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "th";
            }
            switch (num % 10)
            {
                case 0: return "th";
                case 1: return "st";
                case 2: return "nd";
                case 3: return "rd";
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 4: return "th";
            }
            return "th";
        }

        /// <summary>
        /// this function will take 2 x values and find the minimu
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="world_width"></param>
        /// <returns></returns>
        public static int get_min_horizontal_distance(int x1, int x2, int world_width)
        {
            //bring everything within the bounds of the current world... always n >= 0 && n < world_width
            if (x1 < 0)
                x1 += world_width;
            else if (x1 > world_width)
                x1 -= world_width;
            if (x2 < 0)
                x2 += world_width;
            else if (x2 > world_width)
                x2 -= world_width;
            //make x2 always greater than or equal to x1
            if (x2 < x1)
            {
                int t3 = x2;
                x2 = x1;
                x1 = t3;
            }
            if (x2 - x1 > world_width / 2)
            {
                return x1 + world_width - x2;
            }
            else
            {
                return x2 - x1;
            }
        }

        /// <summary>
        /// this function will take 2 x values and find the minimu
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="world_width"></param>
        /// <returns></returns>
        public static int get_signed_min_horizontal_distance(int x1, int x2, int world_width)
        {
            int sign_change = 1;
            //bring everything within the bounds of the current world... always n >= 0 && n < world_width
            if (x1 < 0)
                x1 += world_width;
            else if (x1 > world_width)
                x1 -= world_width;
            if (x2 < 0)
                x2 += world_width;
            else if (x2 > world_width)
                x2 -= world_width;
            //make x2 always greater than or equal to x1
            if (x2 < x1)
            {
                sign_change = -1;
                int t3 = x2;
                x2 = x1;
                x1 = t3;
            }
            if (x2 - x1 > world_width / 2)
            {
                return (x1 + world_width - x2) * sign_change;
            }
            else
            {
                return x2 - x1 * sign_change;
            }
        }

        public static double get_distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((Math.Pow((double)Math.Abs(x1 - x2), 2) + Math.Pow((double)Math.Abs(y1 - y2), 2)));
        }

        public static double get_distance(Point a, Point b)
        {
            return Math.Sqrt((Math.Pow((double)Math.Abs(a.X - b.X), 2) + Math.Pow((double)Math.Abs(a.Y - b.Y), 2)));

        }

        public static double get_distance(Vector2 a, Point b)
        {
            return Math.Sqrt((Math.Pow((double)Math.Abs((int)a.X - b.X), 2) + Math.Pow((double)Math.Abs((int)a.Y - b.Y), 2)));

        }

        public static double get_distance(Vector2 a, Vector2 b)
        {
            return Math.Sqrt((Math.Pow((double)Math.Abs(a.X - b.X), 2) + Math.Pow((double)Math.Abs(a.Y - b.Y), 2)));

        }

        public static float DegreeToRadian(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static Vector2 get_vector_off_points(Vector2 p1, Vector2 p2)
        {
            Double store = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            return new Vector2((float)Math.Cos(store), (float)Math.Sin(store));
        }

        public static bool parse_is_float(string parsing)
        {
            if (parsing.Contains('f')||parsing.Contains('.')) 
                return true;
            return false;
        }

        public static int parse_float_or_int_to_val(string parsing, int max)
        {
            if (parse_is_float(parsing))
            {
                parsing = parsing.Replace("f", "");
                int ret = max;
                float temp = float.Parse(parsing);
                ret = (int)((float)ret * temp);
                return ret;
            }
            else
            {
                int p = int.Parse(parsing);
                if (p > max)
                    p = max;
                return p;
            }
        }

        /// <summary>
        /// returns an item by name in the matching sets of 
        /// item_pieces
        /// furniture
        /// materials
        /// blocks
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static item_descriptor get_item_by_name(string name)
        {
            name = name.Trim().ToLower();
            item_descriptor t = new item_descriptor();
            for (int x = 0; x < Exilania.block_types.blocks.Count; x++)
            {
                if (Exilania.block_types.blocks[x].name.Trim().ToLower() == name)
                {
                    t.item_type = ItemType.Block;
                    t.item_id = x;
                    return t;
                }
            }
            for (int x = 0; x < Exilania.furniture_manager.furniture.Count; x++)
            {
                if (Exilania.furniture_manager.furniture[x].name.Trim().ToLower() == name)
                {
                    t.item_id = x;
                    t.item_type = ItemType.Furniture;
                    return t;
                }
            }
            for (int x = 0; x < Exilania.item_manager.item_pieces.Count; x++)
            {
                if (Exilania.item_manager.item_pieces[x].name.Trim().ToLower() == name)
                {
                    t.item_type = ItemType.ItemPiece;
                    t.item_id = x;
                    return t;
                }
            }
            for (int x = 0; x < Exilania.material_manager.materials.Count; x++)
            {
                if (Exilania.material_manager.materials[x].name.Trim().ToLower() == name)
                {
                    t.item_id = x;
                    t.item_type = ItemType.Material;
                    return t;
                }
            }
            t.item_type = ItemType.Empty;
            t.item_id = -1;
            return t;
        }

    }
}
