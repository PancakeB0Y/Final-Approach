using GXPEngine;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TiledMapParser;

namespace gxpengine_template.MyClasses
{

    public class MyGame : Game
    {
        public Level CurrentScene { get; private set; }
        private string _newSceneName = null;

        public MyGame() : base(500, 800, false, false)
        {
            targetFps = 60;
            LoadScene("level1.tmx");
            OnAfterStep += LoadSceneIfNotNull;
        }

        static void Main()
        {
            new MyGame().Start();
        }
        void Update()
        {
            if (Input.GetKeyDown(Key.R))
            {
                ReloadScene();
            }
        }
        private void LoadSceneIfNotNull()
        {
            if (_newSceneName == null) return;
            DestroyAll();
            var level = new Level(_newSceneName);
            CurrentScene = level;
            AddChild(level);
            level.Init();

            _newSceneName = null;
        }

        public void LoadScene(string sceneName)
        {
            _newSceneName = sceneName;
        }

        public void ReloadScene()
        {
            _newSceneName = CurrentScene.Name;
        }

        protected override void OnDestroy()
        {
            OnAfterStep -= LoadSceneIfNotNull;
        }
        private void DestroyAll()
        {
            foreach (var child in GetChildren())
            {

                child.LateDestroy();

            }
        }
    }
}
