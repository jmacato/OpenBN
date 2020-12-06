using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OpenBN.Interfaces
{
    public class ChipIconProvider
    {

        //Fields
        Texture2D _iconTexture;
        ContentManager _content;
        GraphicsDevice _graphics;
        int _rowCount, _colCount, _totalTiles;
        public List<Texture2D> Icons;

        //Properties
        public int RowCount
        {
            get { return _rowCount; }
            private set { _rowCount = value; }
        }

        public int ColCount
        {
            get => _colCount;
            private set { _colCount = value; }
        }

        public int TotalTiles
        {
            get { return _totalTiles; }
            private set { _totalTiles = value; }
        }

        //Methods
        public ChipIconProvider(ContentManager content, GraphicsDevice graphics)
        {
            Icons = new List<Texture2D>();
            this._graphics = graphics;
            this._content = content;
            var magentacnt = 0;

            _iconTexture = content.Load<Texture2D>("BC/ChipIcons");
            var colorData = new Color[_iconTexture.Width * _iconTexture.Height];
            _iconTexture.GetData<Color>(colorData);

            //Get the number of empty pixels
            foreach (var clr in colorData)
            {
                if (clr == Color.FromNonPremultiplied(0, 240, 240, 255))
                {
                    magentacnt++;
                }
            }

            var i = _iconTexture.Height / 14;
            var j = _iconTexture.Width / 14;

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
            Icons.Add(new Texture2D(graphics, 14, 14));
            for (var j1 = 0; j1 < _rowCount; j1++)
            {
                for (var i1 = 0; i1 < _colCount; i1++)
                {
                    var rX = i1 * 14;
                    var rY = j1 * 14;
                    var srcrect = new Rectangle(rX, rY, 14, 14);
                    var dstrect = new Rectangle(0, 0, 14, 14);

                    var trgt = new Texture2D(graphics, 14, 14);
                    var colors = new Color[14 * 14];

                    _iconTexture.GetData<Color>(0, srcrect, colors, 0, 14 * 14);
                    trgt.SetData<Color>(colors);
                    Icons.Add(trgt);
                }

            }
        }
    }
}