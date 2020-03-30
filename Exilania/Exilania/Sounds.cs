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
    public class Sounds
    {
        public SoundEffect finish_hit_block;
        public SoundEffect hit_block;
        public SoundEffect door_change;
        public float master_volume = 1f;

        public Sounds()
        {
            master_volume = Exilania.settings.mastervolume;
        }

        public void load_content(ContentManager content)
        {
            hit_block = content.Load<SoundEffect>(@"Sounds\hit");
            finish_hit_block = content.Load<SoundEffect>(@"Sounds\finish_hit");
            door_change = content.Load<SoundEffect>(@"Sounds\door_change");

        }
    }
}
