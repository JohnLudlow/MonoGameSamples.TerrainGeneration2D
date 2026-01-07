using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Audio;

public class AudioController : IDisposable
{
  private readonly List<SoundEffectInstance> _activeSoundEffects = [];

  private float _previousSongVolume = 1.0f;

  private float _previousSoundEffectsVolume = 1.0f;
  public bool IsMuted { get; private set; } = false;
  public bool IsDisposed { get; private set; } = false;

  public float SongVolume
  {
    get => IsMuted ? 0.0f : MediaPlayer.Volume;
    set
    {
      if (IsMuted) return;

      MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
    }
  }

  public float SoundEffectVolume
  {
    get => IsMuted ? 0.0f : SoundEffect.MasterVolume;
    set
    {
      if (IsMuted) return;

      SoundEffect.MasterVolume = Math.Clamp(value, 0.0f, 1.0f);
    }
  }

  public void Update()
  {
    for (var i = _activeSoundEffects.Count - 1; i >= 0; i--)
    {
      var soundEffectInstance = _activeSoundEffects[i];
      if (soundEffectInstance.State == SoundState.Stopped)
      {
        if (!soundEffectInstance.IsDisposed)
          soundEffectInstance.Dispose();

        _activeSoundEffects.RemoveAt(i);
      }
    }
  }

  public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect)
  {
    return PlaySoundEffect(soundEffect, 1.0f, 0.0f, 0.0f, false);
  }

  public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan, bool isLooped)
  {
    ArgumentNullException.ThrowIfNull(soundEffect);

    var soundEffectInstance = soundEffect.CreateInstance();
    soundEffectInstance.Volume = volume;
    soundEffectInstance.Pitch = pitch;
    soundEffectInstance.Pan = pan;
    soundEffectInstance.IsLooped = isLooped;

    soundEffectInstance.Play();
    _activeSoundEffects.Add(soundEffectInstance);

    return soundEffectInstance;
  }

  public void PlaySong(Song song, bool isRepeating = true)
  {
    if (MediaPlayer.State == MediaState.Playing)
    {
      MediaPlayer.Stop();
    }

    MediaPlayer.Play(song);
    MediaPlayer.IsRepeating = isRepeating;
  }

  public void PauseAudio()
  {
    MediaPlayer.Pause();

    foreach (var soundEffectInstance in _activeSoundEffects)
    {
      soundEffectInstance.Pause();
    }
  }

  public void ResumeAudio()
  {
    MediaPlayer.Resume();

    foreach (var soundEffectInstance in _activeSoundEffects)
    {
      soundEffectInstance.Resume();
    }
  }

  public void ToggleMute()
  {
    if (IsMuted)
    {
      UnmuteAudio();
    }
    else
    {
      MuteAudio();
    }
  }

  public void MuteAudio()
  {
    _previousSongVolume = MediaPlayer.Volume;
    _previousSoundEffectsVolume = SoundEffect.MasterVolume;

    MediaPlayer.Volume = 0;
    SoundEffect.MasterVolume = 0;

    IsMuted = true;
  }

  public void UnmuteAudio()
  {
    MediaPlayer.Volume = _previousSongVolume;
    SoundEffect.MasterVolume = _previousSoundEffectsVolume;

    IsMuted = false;
  }

  ~AudioController() => Dispose(false);

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!IsDisposed)
    {
      if (disposing)
      {
        foreach (var soundEffectInstance in _activeSoundEffects)
        {
          soundEffectInstance.Dispose();
        }
        _activeSoundEffects.Clear();
      }      

      IsDisposed = true;
    }
  }
}