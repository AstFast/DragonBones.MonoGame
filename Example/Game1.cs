using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DragonBones;
using DragonBones.MonoGame;
using System.Diagnostics;

namespace DragonBonesMonoGameExample
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private MonoGameFactory _factory;
        private MonoGameDragonBones _dragonBones;
        private MonoGameArmature _armature;
        private List<string> _animationNames = new List<string>();
        private int _currentAnimationIndex = 0;
        private KeyboardState _prevKeyboardState;
        private SpriteFont _font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            
            var eventDispatcher = new MonoGameEventDispatcher();
            _dragonBones = new MonoGameDragonBones(eventDispatcher);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Font");

            _factory = new MonoGameFactory(GraphicsDevice);
            _factory._dragonBones = _dragonBones;
            _factory.autoSearch = true;

            try
            {
                var texture = Content.Load<Texture2D>("library/weapon_1000_tex");
                var dragonBonesData = DragonBonesLoader.LoadDragonBonesData(Content, "library/weapon_1000_ske", _factory, "weapon_1000");
                var textureAtlasData = DragonBonesLoader.LoadTextureAtlasData(Content, "library/weapon_1000_tex", texture, _factory, "weapon_1000");
                
                _armature = _factory.BuildArmature("weapon_1005d", "weapon_1000") as MonoGameArmature;
                
                if (_armature != null)
                {
                    _armature.SpriteBatch = _spriteBatch;
                    _armature.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    _armature.Scale = new Vector2(1.0f, 1.0f);
                    
                    _animationNames = _armature.animation.animationNames;
                    
                    if (_animationNames.Count > 0)
                    {
                        foreach (var name in _animationNames)
                        {
                            if (name != "idle")
                            {
                                _armature.animation.Play(name);
                                break;
                            }
                        }
                    }
                    
                    _dragonBones.clock.Add(_armature);
                    var slots = _armature.GetSlots();
                    Debug.WriteLine($"Slots count: {slots.Count}");
                    foreach (var slot in slots)
                    {
                        Debug.WriteLine($"Slot name: {slot.name}, Visible: {slot.visible}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();

            if (_animationNames.Count > 0)
            {
                if (keyboardState.IsKeyDown(Keys.Left) && !_prevKeyboardState.IsKeyDown(Keys.Left))
                {
                    _currentAnimationIndex = (_currentAnimationIndex - 1 + _animationNames.Count) % _animationNames.Count;
                    _armature?.animation.Play(_animationNames[_currentAnimationIndex]);
                }
                else if (keyboardState.IsKeyDown(Keys.Right) && !_prevKeyboardState.IsKeyDown(Keys.Right))
                {
                    _currentAnimationIndex = (_currentAnimationIndex + 1) % _animationNames.Count;
                    _armature?.animation.Play(_animationNames[_currentAnimationIndex]);
                }

                for (int i = 0; i < Math.Min(_animationNames.Count, 9); i++)
                {
                    var key = (Keys)((int)Keys.D1 + i);
                    if (keyboardState.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key))
                    {
                        _currentAnimationIndex = i;
                        _armature?.animation.Play(_animationNames[i]);
                    }
                }
            }

            _prevKeyboardState = keyboardState;

            if (_dragonBones != null)
            {
                _dragonBones.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            
            if (_armature != null)
            {
                _armature.Render();
            }
            
            _spriteBatch.End();

            _spriteBatch.Begin();
            
            if (_animationNames.Count > 0)
            {
                var infoText = $"Animation: {_animationNames[_currentAnimationIndex]} ({_currentAnimationIndex + 1}/{_animationNames.Count})\n";
                infoText += "Press Left/Right or 1-9 to switch animation\n";
                infoText += "Available animations:\n";
                for (int i = 0; i < _animationNames.Count; i++)
                {
                    var prefix = i == _currentAnimationIndex ? "> " : "  ";
                    infoText += $"{prefix}{i + 1}. {_animationNames[i]}\n";
                }
                
                _spriteBatch.DrawString(_font, infoText, new Vector2(10, 10), Color.White);
            }
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
