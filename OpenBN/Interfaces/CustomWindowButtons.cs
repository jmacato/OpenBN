using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenBN.BattleElements;

namespace OpenBN.Interfaces
{
    public class CustomWindowButtons : IBattleChip
    {
        private CustomWindow.ButtonDelegate _executionDelegate;

        public Texture2D Image { get; set; }
        public Texture2D Icon { get; set; }

        public string DisplayName { get; set; }
        public int Damage { get; set; }
        public ChipElements Element { get; set; }
        public char Code { get; set; }
        public SpriteBatch Sb { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public ContentManager Content { get; set; }

        public bool IsSelected { get; set; }
        public Point SlotRowCol { get; set; }

        int _setStatus = 0;
        public int SetStatus {
            get
            {
                return _setStatus;
            }
            set
            {
                var chipimage = "";
                _setStatus = value;
                switch (_setStatus)
                {
                    case 0:
                        chipimage = "info_nodata";
                        break;
                    case 1:
                        chipimage = "info_sendChip";
                        break;
                }
                Image = Content.Load<Texture2D>("BC/" + chipimage);
            }
        }


        public virtual void Execute(CustomWindow cw, Battle bt, Stage st)
        {
            _executionDelegate();
        }

        public CustomWindowButtons(int id, ContentManager content, CustomWindow.ButtonDelegate buttonsDelegate) 
        {
            var chipimage = "";
            _executionDelegate = buttonsDelegate;
            switch (id)
            {
                case 0:
                    chipimage = "info_nodata";
                    break;
                case 1:
                    chipimage = "info_sendChip";
                    break;
            }

            this.Content = content;
            Image = content.Load<Texture2D>("BC/" + chipimage);
            DisplayName = "";
            Element = ChipElements.None;
        }
    }
}