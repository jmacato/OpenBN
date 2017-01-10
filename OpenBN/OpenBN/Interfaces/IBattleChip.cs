using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenBN
{
    public interface IBattleChip
    {
        Texture2D Image { get; set; }
        string DisplayName { get; set; }
        int Damage { get; set; }
        ChipElements Element { get; set; }
        char Code { get; set; }
        Texture2D Icon { get; set; }


        void Execute(CustomWindow CW, Battle BT, Stage ST);

        SpriteBatch SB { get; set; }
        GraphicsDevice Graphics { get; set; }
        ContentManager Content { get; set; }
    }

    public enum ChipElements
    {
        FIRE, AQUA, THUNDER, WOOD, SWORD, WIND,
        TARGET, BLOCK, MODIFIER, WRECK, NULL, NONE
    }

    public class TestBattleChip : IBattleChip
    {
        public Texture2D Image { get; set; }
        public Texture2D Icon { get; set; }

        public string DisplayName { get; set; }
        public int Damage { get; set; }
        public ChipElements Element { get; set; }
        public char Code { get; set; }
        public SpriteBatch SB { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public ContentManager Content { get; set; }

        public void Execute(CustomWindow CW, Battle BT, Stage ST)
        {

        }

        public TestBattleChip(int id, ChipIconProvider iconprov, ContentManager Content, string dspn, int dmg, ChipElements elem, char code)
        {

            this.Content = Content;
            Image = Content.Load<Texture2D>("BC/schip" + id.ToString().PadLeft(3, '0'));
            DisplayName = dspn;
            Damage = dmg;
            Element = elem;
            Code = code;
            Icon = iconprov.Icons[id];
        }
    }

    public class ChipIconProvider
    {

        //Fields
        Texture2D IconTexture;
        ContentManager Content;
        GraphicsDevice Graphics;
        int rowCount, colCount, totalTiles;
        public List<Texture2D> Icons;

        //Properties
        public int RowCount
        {
            get { return rowCount; }
            private set { rowCount = value; }
        }

        public int ColCount
        {
            get { return colCount; }
            private set { colCount = value; }
        }

        public int TotalTiles
        {
            get { return totalTiles; }
            private set { totalTiles = value; }
        }

        //Methods
        public ChipIconProvider(ContentManager Content, GraphicsDevice Graphics)
        {
            Icons = new List<Texture2D>();
            this.Graphics = Graphics;
            this.Content = Content;
            int magentacnt = 0;

            IconTexture = Content.Load<Texture2D>("BC/ChipIcons");
            Color[] ColorData = new Color[IconTexture.Width * IconTexture.Height];
            IconTexture.GetData<Color>(ColorData);

            //Get the number of empty pixels
            foreach (Color clr in ColorData)
            {
                if (clr == Color.FromNonPremultiplied(0, 240, 240,255))
                {
                    magentacnt++;
                }
            }

            var i = (IconTexture.Height / 14);
            var j = (IconTexture.Width / 14);
            
            RowCount = i;
            ColCount = j;

            if (magentacnt != 0)
            {
                var empty = magentacnt / (14 * 14);
                TotalTiles = i * j - empty;
            } else
            {
                TotalTiles = i * j;
            }
            Icons.Add(new Texture2D(Graphics, 14, 14));
            for (int j1 = 0; j1 < rowCount; j1++)
            {
                for (int i1 = 0; i1 < colCount; i1++)
                {
                    var r_x = (i1 ) * 14;
                    var r_y = (j1 ) * 14;
                    var srcrect = new Rectangle(r_x, r_y, 14, 14);
                    var dstrect = new Rectangle(0, 0, 14, 14);

                    Texture2D Trgt = new Texture2D(Graphics, 14, 14);
                    Color[] colors = new Color[14 * 14];

                    IconTexture.GetData<Color>(0, srcrect, colors, 0, 14 * 14);
                    Trgt.SetData<Color>(colors);
                    Icons.Add(Trgt);
                }

            }
        }
    }


    public class CustomStatusBattleChip : IBattleChip
    {
        public Texture2D Image { get; set; }
        public Texture2D Icon { get; set; }

        public string DisplayName { get; set; }
        public int Damage { get; set; }
        public ChipElements Element { get; set; }
        public char Code { get; set; }
        public SpriteBatch SB { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public ContentManager Content { get; set; }

        public virtual void Execute(CustomWindow CW, Battle BT, Stage ST)
        {

        }

        public CustomStatusBattleChip(int id, ContentManager Content)
        {
            var chipimage = "";

            switch (id)
            {
                case 0:
                    chipimage = "info_nodata";
                    break;
                case 1:
                    chipimage = "info_sendChip";
                    break;
            }

            this.Content = Content;
            Image = Content.Load<Texture2D>("BC/"+chipimage);
            DisplayName = "";
            Element = ChipElements.NONE;
        }
    }


}
