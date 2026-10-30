using UnityEngine;

namespace CampusEngineer
{
    public class CampusAudio:MonoBehaviour
    {
        AudioSource source;
        AudioClip step,click,success;
        float nextStep;
        AudioClip[] notes=new AudioClip[8];AudioClip swoosh,water,bird;
        void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();source.spatialBlend=0;source.volume=.32f;
            step=MakeTone("Original synthesized footstep",.11f,95,true);
            click=MakeTone("Original synthesized interface click",.07f,620,false);
            success=MakeTone("Original synthesized completion tone",.5f,740,false);
            int[] semitones={0,2,4,5,7,9,11,12};
            for(int n=0;n<8;n++)notes[n]=Pluck(261.63f*Mathf.Pow(2,semitones[n]/12f));
            swoosh=MakeTone("Paper plane air",.7f,80,true);water=MakeTone("Water trickle",1.5f,170,true);
            var chirps=new float[22050];for(int i=0;i<chirps.Length;i++){float t=i/22050f;float a=t%.19f;chirps[i]=Mathf.Sin(2*Mathf.PI*(2100*t+220*Mathf.Sin(t*18)))*Mathf.Exp(-a*25)*.1f*(t<.7f?1:0);}
            bird=AudioClip.Create("Original soft garden bird response",chirps.Length,1,22050,false);bird.SetData(chirps,0);
        }
        AudioClip MakeTone(string name,float seconds,float frequency,bool noise)
        {
            const int rate=22050;var data=new float[Mathf.CeilToInt(seconds*rate)];var random=new System.Random(203);
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate,fade=Mathf.Exp(-t*(noise?35:8));
                data[i]=(noise?(float)(random.NextDouble()*2-1)*.35f+Mathf.Sin(t*frequency*2*Mathf.PI)*.4f:Mathf.Sin(t*frequency*2*Mathf.PI)*.3f)*fade;
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        public void Walk(bool moving){if(moving && Time.time>=nextStep){source.PlayOneShot(step,.6f);nextStep=Time.time+.43f;}}
        public void Interact(){source.PlayOneShot(click,.4f);}
        public void Success(){source.PlayOneShot(success,.45f);}
        public void Note(int note){source.PlayOneShot(notes[Mathf.Clamp(note,0,7)],.8f);}
        public void Whoosh(){source.PlayOneShot(swoosh,.3f);}
        public void Water(){source.PlayOneShot(water,.22f);}
        public void Chirp(){source.PlayOneShot(bird,.3f);}
        AudioClip Pluck(float frequency)
        {
            int rate=22050;var data=new float[rate];
            for(int i=0;i<data.Length;i++){float t=i/(float)rate;data[i]=(.55f*Mathf.Sin(t*frequency*2*Mathf.PI)+.22f*Mathf.Sin(t*frequency*5.52f*Mathf.PI)*Mathf.Exp(-t*9)+.10f*Mathf.Sin(t*frequency*11.1f*Mathf.PI)*Mathf.Exp(-t*14))*Mathf.Exp(-t*5)*(1-Mathf.Exp(-t*200));}
            var clip=AudioClip.Create("Original wooden key "+frequency,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
    }
}
