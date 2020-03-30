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
    public class ItemPiece
    {
        public Rectangle image;
        public string name;
        public bool has_hand_attach_point;
        public Point hand_attach_point;
        public List<Point> item_attach_points;
        public int complexity;
        public Dictionary<string, string> data;
        public Dictionary<int, int> break_block;
        public Dictionary<string, int> materials;
        public string click_action;
        public int worth;
        public string craft_require;

        public ItemPiece()
        {
            image = new Rectangle();
            name = "";
            has_hand_attach_point = false;
            hand_attach_point = new Point();
            item_attach_points = new List<Point>();
            complexity = 0;
            data = new Dictionary<string, string>();
            break_block = new Dictionary<int, int>();
            materials = new Dictionary<string, int>();
            click_action = "";
            worth = 0;
            craft_require = "";
        }
    }
}
