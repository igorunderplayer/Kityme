using System.Collections.Generic;

namespace Kityme.Entities
{
    public class Filter
    {
        public List<Equalizer> equalizer = new();
        public Timescale timescale = new();
        public Tremolo tremolo = new();

        public Filter (List<Equalizer> eqs, Timescale timescale, Tremolo tremolo)
        {
            this.equalizer = eqs;
            this.timescale = timescale;
            this.tremolo = tremolo;
        }
    }

    public class Equalizer
    {
        public float band = 1f;
        public float gain = 1f;

        public Equalizer (float _band, float _gain)
        {
            this.band = _band;
            this.gain = _gain;
        }
    }

    public class Timescale
    {
        public float pitch = 1f;
        public float rate = 1f;
        public float speed = 1f;

        public Timescale(float p = 1f, float r = 1f, float s = 1f)
        {
            this.pitch = p;
            this.rate = r;
            this.speed = s;
        }
    }

    public class Tremolo
    {
        public float depth = 0.5f;
        public float frequency = 2.0f;

        public Tremolo (float d = 0.5f, float f = 2.0f)
        {
            this.depth = d;
            this.frequency = f;
        }
    }
}