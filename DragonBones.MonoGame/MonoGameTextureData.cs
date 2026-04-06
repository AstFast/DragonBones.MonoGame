using Microsoft.Xna.Framework.Graphics;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameTextureData : TextureData
    {
        public Texture2D texture;

        protected override void _OnClear()
        {
            base._OnClear();
            texture = null;
        }
    }
}
