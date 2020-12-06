using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenBN.BattleElements;

namespace OpenBN.Interfaces
{
    public class CustomWindowButtons : IBattleChip
    {
        private CustomWindow.ButtonDelegate ExecutionDelegate;

        public Texture2D Image { get; set; }
        public Texture2D Icon { get; set; }

        public string DisplayName { get; set; }
        public int Damage { get; set; }
        public ChipElements Element { get; set; }
        public char Code { get; set; }
        public SpriteBatch SB { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public ContentManager Content { get; set; }

        public bool IsSelected { get; set; }
        public Point SlotRowCol { get; set; }

        int setStatus = 0;
        public int SetStatus {
            get
            {
                return setStatus;
            }
            set
            {
                var chipimage = "";
                setStatus = value;
                switch (setStatus)
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


        public virtual void Execute(CustomWindow CW, Battle BT, Stage ST)
        {
            ExecutionDelegate();
        }

        public CustomWindowButtons(int id, ContentManager Content, CustomWindow.ButtonDelegate ButtonsDelegate) 
        {
            var chipimage = "";
            ExecutionDelegate = ButtonsDelegate;
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
            Image = Content.Load<Texture2D>("BC/" + chipimage);
            DisplayName = "";
            Element = ChipElements.NONE;
        }
    }
}