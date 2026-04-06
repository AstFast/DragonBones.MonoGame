using Microsoft.Xna.Framework.Graphics;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameTextureAtlasData : TextureAtlasData
    {
        public Texture2D texture;

        protected override void _OnClear()
        {
            base._OnClear();
            texture = null;
        }

        public override TextureData CreateTexture()
        {
            return BaseObject.BorrowObject<MonoGameTextureData>();
        }
    }
}
