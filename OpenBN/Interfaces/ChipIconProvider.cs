using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN.Interfaces
{
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
            get => colCount;
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
            var magentacnt = 0;

            IconTexture = Content.Load<Texture2D>("BC/ChipIcons");
            var ColorData = new Color[IconTexture.Width * IconTexture.Height];
            IconTexture.GetData<Color>(ColorData);

            //Get the number of empty pixels
            foreach (var clr in ColorData)
            {
                if (clr == Color.FromNonPremultiplied(0, 240, 240, 255))
                {
                    magentacnt++;
                }
            }

            var i = IconTexture.Height / 14;
            var j = IconTexture.Width / 14;

            RowCount = i;
            ColCount = j;

            if (magentacnt != 0)
            {
                var empty = magentacnt / (14 * 14);
                TotalTiles = i * j - empty;
            }
            else
            {
                TotalTiles = i * j;
            }
            Icons.Add(new Texture2D(Graphics, 14, 14));
            for (var j1 = 0; j1 < rowCount; j1++)
            {
                for (var i1 = 0; i1 < colCount; i1++)
                {
                    var r_x = i1 * 14;
                    var r_y = j1 * 14;
                    var srcrect = new Rectangle(r_x, r_y, 14, 14);
                    var dstrect = new Rectangle(0, 0, 14, 14);

                    var Trgt = new Texture2D(Graphics, 14, 14);
                    var colors = new Color[14 * 14];

                    IconTexture.GetData<Color>(0, srcrect, colors, 0, 14 * 14);
                    Trgt.SetData<Color>(colors);
                    Icons.Add(Trgt);
                }

            }
        }
    }
}