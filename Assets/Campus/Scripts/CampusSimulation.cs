using System;
using System.Collections.Generic;

namespace CampusEngineer
{
    [Serializable]
    public class ControlRules
    {
        public bool requireOccupied = true;
        public bool hysteresis;
        public float fanThreshold = 28;
        public float lightThreshold = 300;
        public bool forceOff;
        public bool forceOn;
        public ControlRules Copy() { return (ControlRules)MemberwiseClone(); }
    }

    [Serializable]
    public class SensorFrame
    {
        public int minute;
        public bool occupied;
        public float temperature;
        public float lux;
        public SensorFrame(int m, bool o, float t, float l)
        { minute = m; occupied = o; temperature = t; lux = l; }
    }

    [Serializable]
    public class ExperimentTick
    {
        public SensorFrame sensor;
        public bool fan, light, expectedFan, expectedLight;
        public bool Correct { get { return fan == expectedFan && light == expectedLight; } }
    }

    [Serializable]
    public class ExperimentResult
    {
        public string scenario;
        public List<ExperimentTick> ticks = new List<ExperimentTick>();
        public int errors, switches, occupiedFrames, servicedFrames, emptyActiveFrames;
        public double kwh, baselineKwh;
        public bool Passed { get { return errors == 0; } }
        public double Savings { get { return baselineKwh > 0 ? 1-kwh/baselineKwh : 0; } }
    }

    public static class CampusSimulation
    {
        public const float FanWatts = 25;
        public const float LightWatts = 10;

        public static List<SensorFrame> Scenario(int id)
        {
            var frames = new List<SensorFrame>();
            for (int i=0; i<40; i++)
            {
                if (id == 0)
                    frames.Add(new SensorFrame(i,i>=10 && i<30,i<10 || (i>=20 && i<30)?26:29,i<10 || (i>=20 && i<30)?600:250));
                else if (id == 1)
                    frames.Add(new SensorFrame(i,i>=4 && i<36,29,new float[]{275,295,305,290,315,325,310,285}[i%8]));
                else
                    frames.Add(new SensorFrame(i,i>=7 && i<33,new float[]{27,28,29,26,30}[i%5],new float[]{300,250,600,299,450}[i%5]));
            }
            return frames;
        }

        public static ExperimentResult Run(ControlRules rule, int scenarioId)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (float.IsNaN(rule.fanThreshold) || float.IsInfinity(rule.fanThreshold) || float.IsNaN(rule.lightThreshold) || float.IsInfinity(rule.lightThreshold))
                throw new ArgumentException("Thresholds must be finite");
            var result = new ExperimentResult { scenario = "classroom-"+scenarioId };
            bool lastFan=false,lastLight=false,referenceLight=false;
            foreach(var input in Scenario(scenarioId))
            {
                bool occupiedGate=!rule.requireOccupied || input.occupied;
                bool fan=occupiedGate && input.temperature>=rule.fanThreshold;
                bool light=occupiedGate && input.lux<rule.lightThreshold;
                if(rule.hysteresis && occupiedGate)
                    light=input.lux<=rule.lightThreshold-20 || (input.lux<rule.lightThreshold+20 && lastLight);
                if(rule.forceOff)fan=light=false;
                if(rule.forceOn)fan=light=true;
                bool expectedFan=input.occupied && input.temperature>=28;
                bool expectedLight=input.occupied && input.lux<300;
                if(scenarioId==1)
                    expectedLight=input.occupied && (input.lux<=280 || (input.lux<320 && referenceLight));
                referenceLight=expectedLight;
                var tick=new ExperimentTick{sensor=input,fan=fan,light=light,expectedFan=expectedFan,expectedLight=expectedLight};
                result.ticks.Add(tick);
                if(!tick.Correct)result.errors++;
                if(input.occupied){result.occupiedFrames++;if(tick.Correct)result.servicedFrames++;}
                else if(fan || light)result.emptyActiveFrames++;
                if(fan!=lastFan)result.switches++;
                if(light!=lastLight)result.switches++;
                result.kwh+=((fan?FanWatts:0)+(light?LightWatts:0))/60000.0;
                result.baselineKwh+=(FanWatts+LightWatts)/60000.0;
                lastFan=fan;lastLight=light;
            }
            return result;
        }

        public static string PythonPreview(ControlRules r)
        {
            string gate=r.requireOccupied?"occupied and ":"";
            if(r.forceOff)return "# 全關方案  用反例檢驗它\nfan = False\nlight = False";
            if(r.forceOn)return "# 全開方案\nfan = True\nlight = True";
            string text="fan = "+gate+"temperature >= "+r.fanThreshold.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture);
            text+="\nlight = "+gate+"lux < "+r.lightThreshold.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture);
            if(r.hysteresis)text+="\n# 雙閾值模式會參考上一次 light 狀態";
            return text;
        }
    }
}
