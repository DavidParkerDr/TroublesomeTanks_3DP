using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace Tankontroller.Managers
{
    public class SoundManager
    {
        private Dictionary<string, SoundEffect> mSoundEffects = new Dictionary<string, SoundEffect>();

        private SoundEffectInstance mCurrentMusic;
        private string mCurrentMusicName;

        static SoundManager mInstance = new SoundManager();
        public static SoundManager Instance
        {
            get { return mInstance; }
        }
        private SoundManager() { }

        public void Add(string pName)
        {
            SoundEffect soundEffect = Tankontroller.Instance().CM().Load<SoundEffect>(pName);
            mSoundEffects.Add(pName, soundEffect);
        }

        public SoundEffectInstance GetSoundEffectInstance(string pName)
        {
            return mSoundEffects[pName].CreateInstance();
        }
        public SoundEffectInstance GetLoopableSoundEffectInstance(string pName)
        {
            SoundEffectInstance instance = GetSoundEffectInstance(pName);
            instance.IsLooped = true;
            return instance;
        }

        public SoundEffectInstance ReplaceCurrentMusicInstance(string pName, bool pLoopable)
        {
            if (mCurrentMusicName != pName)
            {
                if (mCurrentMusic != null)
                {
                    mCurrentMusic.Stop();
                }
                mCurrentMusicName = pName;
                SoundEffectInstance replacement = GetSoundEffectInstance(pName);
                replacement.IsLooped = pLoopable;
                mCurrentMusic = replacement;
                mCurrentMusic.Play();
            }
            mCurrentMusic.IsLooped = pLoopable;

            return mCurrentMusic;
        }
    }
}